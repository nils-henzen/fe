struct Message: Identifiable, Codable {
    let id: Int
    let sender: String
    let text: String
    let isFile: Bool
    let timestamp: Date
}
