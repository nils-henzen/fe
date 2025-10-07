import Foundation
import Combine

final class API: ObservableObject {
    static let shared = API()

    // Grouped by "peer" (the other participant relative to the current user)
    @Published var messagesByPeer: [String:[Message]] = [:]
    @Published var isFetching: Bool = false
    @Published var isSending: Bool = false

    private var pollingTimer: Timer?

    init() {
        // If you implement config hot-reload, post Notification.Name("AppConfigDidReload")
        // and we’ll refetch with the new settings.
        NotificationCenter.default.addObserver(forName: Notification.Name("AppConfigDidReload"), object: nil, queue: .main) { [weak self] _ in
            guard let self else { return }
            Task { await self.fetchMessages() }
        }
    }

    // MARK: - Fetch

    // Fetch messages (network off-main; publish on main)
    func fetchMessages() async {
        let cfg = AppConfig.shared

        await MainActor.run { self.isFetching = true }
        defer { Task { await MainActor.run { self.isFetching = false } } }

        var components = URLComponents(url: cfg.apiBaseURL.appendingPathComponent("fetch"), resolvingAgainstBaseURL: false)!
        components.queryItems = [
            URLQueryItem(name: "sender_id", value: cfg.userId),
            URLQueryItem(name: "signature", value: cfg.signature)
        ]
        guard let url = components.url else { return }

        var req = URLRequest(url: url)
        req.httpMethod = "GET"

        do {
            let (data, _) = try await URLSession.shared.data(for: req)
            let msgs = try JSONDecoder().decode([Message].self, from: data)

            // Sort by timestamp ascending, then group by "peer" (other participant)
            let sorted = msgs.sorted { $0.timestamp < $1.timestamp }
            let current = cfg.userId
            let grouped = Dictionary(grouping: sorted) { (m: Message) -> String in
                return m.sender_id == current ? m.receiver_id : m.sender_id
            }

            await MainActor.run {
                self.messagesByPeer = grouped
            }
        } catch {
            print("Fetch failed:", error)
        }
    }

    // MARK: - Send (JSON, Flask)

    // Backwards-compatible name used by your UI; now posts JSON to /send_message
    func sendMessage(to receiver: String, content: String) async {
        await sendTextMessage(to: receiver, text: content)
    }

    // Send a text message to /send_message with JSON body
    func sendTextMessage(to receiver: String, text: String) async {
        let cfg = AppConfig.shared
        let url = cfg.sendBaseURL.appendingPathComponent("send_message")

        await MainActor.run { self.isSending = true }
        defer { Task { await MainActor.run { self.isSending = false } } }

        struct Payload: Encodable {
            let signature: String
            let sender_id: String
            let receiver_id: String
            let message_text: String
        }

        let payload = Payload(
            signature: cfg.signature,
            sender_id: cfg.userId,
            receiver_id: receiver,
            message_text: text
        )

        do {
            _ = try await postJSON(payload, to: url)
        } catch {
            print("sendTextMessage failed:", error)
        }
    }

    // Send a file to /send_file with JSON body. Encodes file content as Base64.
    func sendFile(to receiver: String, fileURL: URL) async {
        let cfg = AppConfig.shared
        let url = cfg.sendBaseURL.appendingPathComponent("send_file")

        await MainActor.run { self.isSending = true }
        defer { Task { await MainActor.run { self.isSending = false } } }

        guard let data = try? Data(contentsOf: fileURL) else {
            print("sendFile: failed to read file:", fileURL.path)
            return
        }
        let base64 = data.base64EncodedString()
        let name = fileURL.lastPathComponent
        let type = fileTypeString(for: fileURL)

        struct Payload: Encodable {
            let signature: String
            let sender_id: String
            let receiver_id: String
            let file_name: String
            let file_type: String
            let file_content: String
        }

        let payload = Payload(
            signature: cfg.signature,
            sender_id: cfg.userId,
            receiver_id: receiver,
            file_name: name,
            file_type: type,
            file_content: base64
        )

        do {
            _ = try await postJSON(payload, to: url)
        } catch {
            print("sendFile failed:", error)
        }
    }

    // MARK: - Read one

    func readMessage(id: Int) async throws -> Message {
        let cfg = AppConfig.shared

        var components = URLComponents(url: cfg.apiBaseURL.appendingPathComponent("read"), resolvingAgainstBaseURL: false)!
        components.queryItems = [
            URLQueryItem(name: "message_id", value: String(id)),
            URLQueryItem(name: "sender_id", value: cfg.userId),
            URLQueryItem(name: "signature", value: cfg.signature)
        ]
        guard let url = components.url else { throw URLError(.badURL) }

        var request = URLRequest(url: url)
        request.httpMethod = "GET"

        let (data, response) = try await URLSession.shared.data(for: request)
        if let http = response as? HTTPURLResponse, !(200...299).contains(http.statusCode) {
            throw URLError(.badServerResponse)
        }
        return try JSONDecoder().decode(Message.self, from: data)
    }

    // MARK: - Polling

    @MainActor
    func startPolling() {
        pollingTimer?.invalidate()
        pollingTimer = Timer.scheduledTimer(withTimeInterval: 5, repeats: true) { [weak self] _ in
            guard let self else { return }
            Task { await self.fetchMessages() }
        }
    }

    // MARK: - Helpers

    private func postJSON<T: Encodable>(_ payload: T, to url: URL) async throws -> (Data, URLResponse) {
        var req = URLRequest(url: url)
        req.httpMethod = "POST"
        req.setValue("application/json; charset=utf-8", forHTTPHeaderField: "Content-Type")
        req.httpBody = try JSONEncoder().encode(payload)
        return try await URLSession.shared.data(for: req)
    }

    private func fileTypeString(for url: URL) -> String {
        let ext = url.pathExtension
        if ext.isEmpty { return "BIN" }
        return ext.uppercased()
    }
}
