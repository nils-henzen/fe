import SwiftUI
import UserNotifications

struct FEOverviewView: View {
    @StateObject private var api = API.shared
    @State private var expandedUser: String?
    @State private var previousMessageCounts: [String: Int] = [:]

    var body: some View {
        VStack(spacing: 0) {
            header
            Divider()
            content
        }
        .frame(width: 320, height: 400)
        .padding(.bottom, 4)
        .task { await startPollingMessages() }
    }

    // MARK: - Header

    private var header: some View {
        HStack(spacing: 12) {
            if let user = expandedUser {
                Button {
                    withAnimation(.easeInOut(duration: 0.2)) { expandedUser = nil }
                } label: {
                    Label("Back", systemImage: "chevron.left")
                }
                .buttonStyle(.link)

                Divider().frame(height: 18)
                Text(user.lowercased()).font(.headline)
                Spacer()
                if api.isFetching || api.isSending { ProgressView().controlSize(.small) }
            } else {
                Text("Fast Exchange").font(.headline).bold()
                Spacer()
                if api.isFetching { ProgressView().controlSize(.small) }
            }
        }
        .padding(.horizontal, 12)
        .padding(.vertical, 8)
    }

    // MARK: - Content

    private var content: some View {
        ZStack {
            if let user = expandedUser {
                ConversationView(user: user)
                    .transition(.opacity)
                    .id("conv-\(user)")
            } else {
                OverviewListView(
                    peers: sortedPeers,
                    lastByPeer: api.messagesByPeer
                ) { peer in
                    withAnimation(.easeInOut(duration: 0.2)) { expandedUser = peer }
                }
                .transition(.opacity)
                .id("overview")
            }
        }
    }

    private var sortedPeers: [String] {
        api.messagesByPeer
            .map { (peer, msgs) in (peer, msgs.last?.timestamp ?? 0) }
            .sorted { $0.1 > $1.1 }
            .map { $0.0 }
    }

    // MARK: - Polling & Notifications

    @MainActor
    private func startPollingMessages() async {
        api.startPolling()
        previousMessageCounts = api.messagesByPeer.mapValues { $0.count }

        Timer.scheduledTimer(withTimeInterval: 1, repeats: true) { _ in
            Task { await checkForNewMessages() }
        }
    }

    @MainActor
    private func checkForNewMessages() async {
        await api.fetchMessages()
        let currentCounts = api.messagesByPeer.mapValues { $0.count }

        for (peer, newCount) in currentCounts {
            let oldCount = previousMessageCounts[peer] ?? 0
            let diff = newCount - oldCount
            guard diff > 0 else { continue }

            if diff == 1, let msg = api.messagesByPeer[peer]?.last {
                AppDelegate.shared?.sendNotification(for: peer, messageID: msg.id)
            } else {
                sendNotification(for: peer, count: diff)
            }
        }

        previousMessageCounts = currentCounts
    }

    private func sendNotification(for peer: String, count: Int) {
        let content = UNMutableNotificationContent()
        content.title = "New message\(count > 1 ? "s" : "") from \(peer)"
        content.body = "You have \(count) new message\(count > 1 ? "s" : "")."
        content.sound = .default

        let request = UNNotificationRequest(
            identifier: UUID().uuidString,
            content: content,
            trigger: nil
        )

        UNUserNotificationCenter.current().add(request) { error in
            if let error = error { print("Failed to send notification:", error) }
        }
    }

    // MARK: - Subviews

    private struct OverviewListView: View {
        let peers: [String]
        let lastByPeer: [String:[Message]]
        let select: (String) -> Void

        var body: some View {
            ScrollView {
                VStack(spacing: 10) {
                    ForEach(peers, id: \.self) { peer in
                        let lastName = lastByPeer[peer]?.last?.file_name
                        Button { select(peer) } label: {
                            VStack(alignment: .leading, spacing: 4) {
                                Text(peer).bold()
                                if let fileName = lastName {
                                    Text(fileName)
                                        .lineLimit(1)
                                        .foregroundStyle(.secondary)
                                }
                            }
                            .frame(maxWidth: .infinity, alignment: .leading)
                            .padding(12)
                            .cardBackground()
                        }
                        .buttonStyle(.plain)
                        .contentShape(Rectangle())
                        .hoverScale()
                    }
                }
                .padding(12)
            }
            .scrollContentBackground(.hidden)
        }
    }

    private struct ConversationView: View {
        let user: String
        @StateObject private var api = API.shared
        @State private var replyText = ""
        @State private var lastCount: Int = 0
        @State private var isDropTarget = false

        var body: some View {
            VStack(spacing: 0) {
                ScrollViewReader { proxy in
                    ScrollView {
                        VStack(alignment: .leading, spacing: 8) {
                            ForEach(api.messagesByPeer[user] ?? []) { msg in
                                MessageRow(
                                    msg: msg,
                                    openFullMessage: { id in
                                        AppDelegate.shared?.showMessageWindow(messageID: id)
                                    }
                                )
                                .transition(.opacity)
                            }
                            Spacer(minLength: 0).id("bottom")
                        }
                        .padding(12)
                        .background(
                            RoundedRectangle(cornerRadius: 8, style: .continuous)
                                .strokeBorder(Color.accentColor.opacity(isDropTarget ? 0.6 : 0.0), lineWidth: 2)
                                .padding(.horizontal, -4)
                                .padding(.vertical, -4)
                        )
                    }
                    .onChange(of: (api.messagesByPeer[user]?.count) ?? 0) { newCount in
                        if newCount > lastCount { scrollToBottom(proxy) }
                        lastCount = newCount
                    }
                    .onAppear {
                        lastCount = (api.messagesByPeer[user]?.count) ?? 0
                        scrollToBottom(proxy)
                    }
                }

                Divider()

                HStack(spacing: 8) {
                    TextField("Type a reply…", text: $replyText, axis: .vertical)
                        .textFieldStyle(.plain)
                        .lineLimit(1...4)
                        .padding(10)
                        .background(RoundedRectangle(cornerRadius: 10, style: .continuous).fill(.ultraThinMaterial))
                        .overlay(RoundedRectangle(cornerRadius: 10, style: .continuous).strokeBorder(.white.opacity(0.15), lineWidth: 1))
                        .dropDestination(for: URL.self, action: { urls, _ in sendFiles(urls); return true }, isTargeted: { isDropTarget = $0 })

                    Button { pasteFiles() } label: { Image(systemName: "doc.on.clipboard") }.buttonStyle(.bordered)

                    Button {
                        Task { await api.sendMessage(to: user, content: replyText); replyText = "" }
                    } label: {
                        if api.isSending {
                            ProgressView().controlSize(.small)
                        } else {
                            Image(systemName: "paperplane.fill")
                        }
                    }
                    .disabled(api.isSending || replyText.isEmpty)
                    .buttonStyle(.borderedProminent)
                    .controlSize(.small)
                }
                .padding(12)
            }
        }

        @MainActor
        private func scrollToBottom(_ proxy: ScrollViewProxy) {
            guard AppDelegate.shared?.popover.isShown == true else { return }
            DispatchQueue.main.async {
                var txn = Transaction(); txn.disablesAnimations = true
                withTransaction(txn) { proxy.scrollTo("bottom", anchor: .bottom) }
            }
        }

        private func sendFiles(_ urls: [URL]) {
            for url in urls { Task { await api.sendFile(to: user, fileURL: url) } }
        }

        private func pasteFiles() {
            let pb = NSPasteboard.general
            if let urls = pb.readObjects(forClasses: [NSURL.self], options: [.urlReadingFileURLsOnly: true]) as? [URL] {
                sendFiles(urls)
            }
        }
    }
}

// MARK: - Styling helpers

private extension View {
    func cardBackground() -> some View {
        self.background(RoundedRectangle(cornerRadius: 12, style: .continuous).fill(.ultraThinMaterial))
            .overlay(RoundedRectangle(cornerRadius: 12, style: .continuous).strokeBorder(.white.opacity(0.15), lineWidth: 1))
    }

    func hoverScale(_ amount: CGFloat = 0.02) -> some View {
        modifier(HoverScale(amount: amount))
    }
}

private struct HoverScale: ViewModifier {
    @State private var hovering = false
    let amount: CGFloat
    func body(content: Content) -> some View {
        content.scaleEffect(hovering ? 1 + amount : 1)
            .animation(.spring(response: 0.25, dampingFraction: 0.7), value: hovering)
            .onHover { hovering = $0 }
    }
}
