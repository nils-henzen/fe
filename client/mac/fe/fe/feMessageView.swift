import SwiftUI

struct FEMessageView: View {
    let messageID: Int
    @State private var message: Message?
    @State private var isLoading = true
    @State private var errorText: String?

    var body: some View {
        VStack(spacing: 0) {
            header
            Divider()
            content
        }
        .frame(minWidth: 700, minHeight: 450)
        .background(.background)
        .task(id: messageID, loadMessage)
    }

    // MARK: - Header

    private var header: some View {
        HStack(spacing: 12) {
            Button {
                AppDelegate.shared?.hideMessageWindow(reopenPopover: true)
            } label: {
                Label("Back", systemImage: "chevron.left")
            }
            .buttonStyle(.link)

            Divider().frame(height: 18)

            Text(titleText)
                .font(.headline)

            Spacer()
        }
        .padding(.horizontal, 12)
        .padding(.vertical, 8)
    }

    // MARK: - Content

    private var content: some View {
        ZStack {
            if isLoading {
                LoadingView().transition(.opacity).id("loading")
            } else if let msg = message {
                ContentView(msg: msg).transition(.opacity).id("content-\(msg.id)")
            } else {
                ErrorView(text: errorText ?? "Failed to load message.")
                    .transition(.opacity)
                    .id("error")
            }
        }
        .frame(maxWidth: .infinity, maxHeight: .infinity)
        .padding(12)
        .animation(.easeInOut(duration: 0.2), value: isLoading)
        .animation(.easeInOut(duration: 0.2), value: message?.id)
    }

    // MARK: - Async Logic

    private func loadMessage() async {
        await MainActor.run {
            isLoading = true
            errorText = nil
            message = nil
        }
        do {
            let msg = try await API.shared.readMessage(id: messageID)
            try Task.checkCancellation()
            await MainActor.run {
                message = msg
                isLoading = false
            }
        } catch is CancellationError {
            // ignore
        } catch {
            await MainActor.run {
                isLoading = false
                errorText = "Failed to load message."
            }
        }
    }

    // MARK: - Title

    private var titleText: String {
        if let msg = message {
            let current = AppConfig.shared.userId
            return msg.sender_id == current ? msg.receiver_id : msg.sender_id
        }
        return isLoading ? "Loading…" : "Message"
    }
}

private struct LoadingView: View {
    var body: some View {
        VStack(spacing: 12) {
            ProgressView().controlSize(.large)
            Text("Loading…").foregroundStyle(.secondary)
        }
        .messageCard()
    }
}

private struct ErrorView: View {
    let text: String
    var body: some View {
        VStack(spacing: 8) {
            Image(systemName: "exclamationmark.triangle.fill")
                .foregroundStyle(.yellow)
                .font(.system(size: 28, weight: .semibold))
            Text(text).foregroundStyle(.secondary)
        }
        .messageCard()
    }
}

// MARK: - ContentView
private struct ContentView: View {
    let msg: Message
    @State private var textPreview: String?

    var body: some View {
        ScrollView {
            VStack(alignment: .leading, spacing: 12) {
                dateText

                if msg.file_type.uppercased() == "FETXT" {
                    textMessageView
                } else {
                    fileMessageView
                }
            }
            .padding(4)
            .task { await loadPreviewIfNeeded() }
        }
        .scrollContentBackground(.hidden)
    }

    // MARK: - Date

    private var dateText: some View {
        Text(AppDelegate.getDateFromUnixInt(timestamp: msg.timestamp))
            .font(.footnote)
            .foregroundStyle(.secondary)
            .frame(maxWidth: .infinity, alignment: .trailing)
    }

    // MARK: - Text Message

    private var textMessageView: some View {
        VStack(alignment: .leading, spacing: 10) {
            Text(msg.file_name)
                .textSelection(.enabled)
                .frame(maxWidth: .infinity, alignment: .leading)

            HStack(spacing: 8) {
                Button {
                    NSPasteboard.general.clearContents()
                    NSPasteboard.general.setString(msg.file_name, forType: .string)
                } label: {
                    Label("Copy", systemImage: "doc.on.doc")
                }

                Button {
                    let encoded = msg.file_name.addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed) ?? ""
                    if let url = URL(string: "https://www.google.com/search?q=\(encoded)") {
                        NSWorkspace.shared.open(url)
                    }
                } label: {
                    Label("Web Lookup", systemImage: "magnifyingglass")
                }
            }
            .buttonStyle(.borderedProminent)
        }
        .messageCard()
    }

    // MARK: - File Message

    private var fileMessageView: some View {
        VStack(alignment: .leading, spacing: 10) {
            // Header
            HStack(spacing: 10) {
                Image(systemName: icon(for: msg.file_name))
                    .font(.system(size: 20))
                    .foregroundStyle(.primary)
                Text(msg.file_name)
                    .font(.headline)
                    .lineLimit(2)
                    .frame(maxWidth: .infinity, alignment: .leading)
            }

            // Optional Preview
            if let preview = textPreview {
                ScrollView {
                    Text(preview)
                        .font(.body)
                        .textSelection(.enabled)
                        .padding(8)
                        .frame(maxWidth: .infinity, alignment: .leading)
                }
                .frame(maxHeight: 200)
                .background(RoundedRectangle(cornerRadius: 8).fill(.ultraThinMaterial))
            }

            // File Buttons
            HStack(spacing: 8) {
                Button { openFileMessage(msg) } label: {
                    Label("Open", systemImage: "arrow.up.right.square")
                }

                Button {
                    if let url = resolveFileURL(for: msg) {
                        NSWorkspace.shared.activateFileViewerSelecting([url])
                    }
                } label: {
                    Label("Reveal", systemImage: "folder")
                }

                Button {
                    if let url = resolveFileURL(for: msg) {
                        NSPasteboard.general.clearContents()
                        NSPasteboard.general.setString(url.path, forType: .string)
                    }
                } label: {
                    Label("Copy Path", systemImage: "doc.on.doc")
                }
            }
            .buttonStyle(.borderedProminent)
        }
        .messageCard()
    }

    // MARK: - File Handling

    private func icon(for name: String) -> String {
        switch (name as NSString).pathExtension.lowercased() {
        case "png", "jpg", "jpeg", "gif", "heic": return "photo"
        case "pdf": return "doc.richtext"
        case "txt", "md", "rtf": return "doc.text"
        case "zip", "tar", "gz": return "archivebox"
        case "mp4", "mov": return "film"
        case "mp3", "wav", "aiff": return "waveform"
        default: return "doc"
        }
    }

    private func openFileMessage(_ msg: Message) {
        guard let url = resolveFileURL(for: msg) else { return }
        NSWorkspace.shared.open(url)
    }

    private func resolveFileURL(for msg: Message) -> URL? {
        guard msg.file_type.uppercased() != "FETXT",
              let contents = msg.file_contents, !contents.isEmpty else { return nil }

        if contents.hasPrefix("file://"), let url = URL(string: contents) {
            return url
        }

        let pathURL = URL(fileURLWithPath: contents)
        if FileManager.default.fileExists(atPath: pathURL.path) { return pathURL }

        if let data = Data(base64Encoded: contents) {
            do {
                let dir = try ensureReceivedDir()
                let fileURL = dir.appendingPathComponent(msg.file_name)
                try data.write(to: fileURL, options: .atomic)
                return fileURL
            } catch { print("Failed to write decoded file:", error) }
        }
        return nil
    }

    private func ensureReceivedDir() throws -> URL {
        let base = try FileManager.default.url(for: .cachesDirectory, in: .userDomainMask, appropriateFor: nil, create: true)
        let dir = base.appendingPathComponent("fe/Received", isDirectory: true)
        if !FileManager.default.fileExists(atPath: dir.path) {
            try FileManager.default.createDirectory(at: dir, withIntermediateDirectories: true)
        }
        return dir
    }

    // MARK: - Preview Loading

    private func loadPreviewIfNeeded() async {
        guard textPreview == nil,
              ["txt", "md", "rtf"].contains((msg.file_name as NSString).pathExtension.lowercased()),
              let url = resolveFileURL(for: msg) else { return }

        do {
            let data = try Data(contentsOf: url)
            if let text = String(data: data, encoding: .utf8) {
                let words = text.split(whereSeparator: \.isWhitespace)
                let preview = words.joined(separator: " ")
                await MainActor.run { textPreview = preview }
            }
        } catch {
            print("Preview load failed for \(msg.file_name):", error)
        }
    }
}

// MARK: - Common Modifiers

private extension View {
    func messageCard() -> some View {
        self
            .frame(maxWidth: .infinity, maxHeight: .infinity, alignment: .center)
            .padding(24)
            .background(RoundedRectangle(cornerRadius: 14, style: .continuous).fill(.thinMaterial))
            .overlay(RoundedRectangle(cornerRadius: 14, style: .continuous)
                .strokeBorder(.white.opacity(0.15), lineWidth: 1))
            .shadow(color: Color.black.opacity(0.12), radius: 12, x: 0, y: 6)
    }
}
