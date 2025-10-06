final class API: ObservableObject {
    static let shared = API()

    @Published var messagesBySender: [String:[Message]] = [:]
    private var lastFetch: TimeInterval?

    func fetchMessages() async {
        let url = URL(string: "http://localhost:5000/fetch")!
        var req = URLRequest(url: url)
        // append user_id & signature params here

        do {
            let (data, _) = try await URLSession.shared.data(for: req)
            let msgs = try JSONDecoder().decode([Message].self, from: data)
            DispatchQueue.main.async {
                self.messagesBySender = Dictionary(grouping: msgs, by: \.sender)
            }
        } catch {
            print("Fetch failed:", error)
        }
    }

    func sendMessage(to receiver: String, content: String) async {
        guard let url = URL(string: "http://localhost:5000/send") else { return }
        var req = URLRequest(url: url)
        req.httpMethod = "POST"
        req.httpBody = "receiver_id=\(receiver)&message=\(content)"
            .data(using: .utf8)
        // add user_id & signature
        _ = try? await URLSession.shared.data(for: req)
    }
}
