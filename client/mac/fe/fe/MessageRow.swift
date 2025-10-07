import SwiftUI

struct MessageRow: View {
    let msg: Message
    let openFullMessage: (Int) -> Void

    @State private var hovering = false

    private var isTextMessage: Bool {
        let t = msg.file_type.lowercased()
        return t == "fetxt" || t == "txt"
    }

    private var isOutgoing: Bool {
        msg.sender_id == AppConfig.shared.userId
    }

    var body: some View {
        VStack(alignment: .leading, spacing: 4) {
            // Align timestamp with the bubble side
            HStack {
                if isOutgoing { Spacer() }
                Text(AppDelegate.getDateFromUnixInt(timestamp: msg.timestamp))
                    .font(.footnote)
                    .foregroundStyle(.secondary)
                if !isOutgoing { Spacer() }
            }

            HStack(alignment: .center, spacing: 8) {
                if isOutgoing { Spacer(minLength: 40) } // push bubble to the right for outgoing

                Button(action: buttonAction) {
                    HStack(alignment: .center, spacing: 10) {
                        Image(systemName: isTextMessage ? "text.alignleft" : "doc")
                            .font(.system(size: 14, weight: .regular))
                            .foregroundStyle(isOutgoing ? .primary : .primary)

                        Text(msg.file_name)
                            .lineLimit(4)
                            .frame(maxWidth: .infinity, alignment: .leading)
                    }
                    .padding(10)
                    .background(bubbleBackground)
                    .overlay(bubbleStroke)
                }
                .buttonStyle(.plain)
                .contentShape(Rectangle())
                .scaleEffect(hovering ? 1.01 : 1)
                .animation(.spring(response: 0.25, dampingFraction: 0.7), value: hovering)
                .onHover { hovering = $0 }

                if !isOutgoing { Spacer(minLength: 40) } // push bubble to the left for incoming
            }
        }
    }

    private var bubbleBackground: some View {
        Group {
            if isOutgoing {
                RoundedRectangle(cornerRadius: 12, style: .continuous)
                    .fill(Color.accentColor.opacity(0.20))
            } else {
                RoundedRectangle(cornerRadius: 12, style: .continuous)
                    .fill(.thinMaterial)
            }
        }
    }

    private var bubbleStroke: some View {
        RoundedRectangle(cornerRadius: 12, style: .continuous)
            .strokeBorder(.white.opacity(0.15), lineWidth: 1)
    }

    private func buttonAction() {
        // Always open the detailed window to trigger a server "read" and show full content
        openFullMessage(msg.id)
    }
}
