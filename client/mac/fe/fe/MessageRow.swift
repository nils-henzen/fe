struct MessageRow: View {
    let msg: Message
    let openFile: (String) -> Void
    let openFullMessage: (String) -> Void

    var body: some View {
        VStack(alignment: .leading, spacing: 4) {
            Text(AppDelegate.getDateFromUnixInt(timestamp: msg.timestamp))
                .font(.footnote)
                .fontWeight(.thin)
                .foregroundStyle(.secondary)
                .padding(.horizontal, 16)
                .frame(maxWidth: .infinity, alignment: .trailing)
                .padding(.top, 5)
                .id(msg.id)

            if msg.file_type != "FETXT" {
                Button(action: { openFile(msg.file_contents ?? "") }) {
                    Text(msg.file_name)
                        .foregroundColor(.primary)
                        .frame(maxWidth: .infinity, alignment: .leading)
                        .padding(8)
                        .background(.thinMaterial, in: RoundedRectangle(cornerRadius: 8))
                }
                .buttonStyle(.plain)
                .id("file-\(msg.id)")
            } else {
                Button(action: { openFullMessage(msg.id) }) {
                    Text(msg.file_name)
                        .padding(8)
                        .frame(maxWidth: .infinity, alignment: .leading)
                        .background(.thinMaterial, in: RoundedRectangle(cornerRadius: 8))
                        .id("text-\(msg.id)")
                }
                .buttonStyle(.plain)
            }
        }
    }
}
