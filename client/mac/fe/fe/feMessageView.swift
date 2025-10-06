import SwiftUI

struct FEMessageView: View {
    let messageID: String
    @State private var message: Message? = nil
    @State private var isLoading = true
    @State private var errorText: String? = nil
    
    var body: some View {
        ZStack {
            if isLoading {
                ProgressView("Loading message...")
            } else if let message {
                ScrollView {
                    VStack(alignment: .leading, spacing: 12) {
                        Text("Message Details")
                            .font(.largeTitle)
                            .bold()
                        
                        Text("From: \(message.sender)")
                            .font(.headline)
                        
                        Text("Date: \(getDateFromUnixInt(timestamp: message.timestamp))")
                            .font(.footnote)
                            .foregroundStyle(.secondary)
                        
                        Divider()
                        
                        if message.file_type == "FETXT" {
                            Text(message.file_name)
                                .font(.body)
                                .padding()
                                .background(.thinMaterial, in: RoundedRectangle(cornerRadius: 12))
                        } else {
                            Button("Open File: \(message.file_name)") {
                                openFile(message.file_contents ?? "")
                            }
                            .buttonStyle(.borderedProminent)
                        }
                        
                        Spacer()
                    }
                    .padding()
                }
            } else if let errorText {
                Text("Error: \(errorText)")
                    .foregroundColor(.red)
            }
        }
        .task {
            await loadMessage()
        }
    }
    
    private func loadMessage() async {
        do {
            let result = try await api.readMessage(id: messageID)
            await MainActor.run {
                self.message = result
                self.isLoading = false
            }
        } catch {
            await MainActor.run {
                self.errorText = error.localizedDescription
                self.isLoading = false
            }
        }
    }
}
