// AppConfig.swift
import Foundation

struct AppConfig: Codable {
    var userId: String
    var signature: String
    var apiBaseURL: URL
    var sendBaseURL: URL

    static let shared: AppConfig = {
        // 1) Try app support JSON: ~/Library/Application Support/fe/AppConfig.json
        let fm = FileManager.default
        if let appSupport = fm.urls(for: .applicationSupportDirectory, in: .userDomainMask).first {
            let dir = appSupport.appendingPathComponent("fe", isDirectory: true)
            let file = dir.appendingPathComponent("AppConfig.json")
            if let data = try? Data(contentsOf: file),
               let cfg = try? JSONDecoder().decode(AppConfig.self, from: data) {
                return cfg.mergingEnv()
            }
        }
        // 2) Try bundled AppConfig.json
        if let url = Bundle.main.url(forResource: "AppConfig", withExtension: "json"),
           let data = try? Data(contentsOf: url),
           let cfg = try? JSONDecoder().decode(AppConfig.self, from: data) {
            return cfg.mergingEnv()
        }
        // 3) Fallback defaults (can be overridden via env)
        let fallback = AppConfig(
            userId: "changeme",
            signature: "changeme",
            apiBaseURL: URL(string: "http://127.0.0.1:26834")!,
            sendBaseURL: URL(string: "http://127.0.0.1:5000")!
        )
        return fallback.mergingEnv()
    }()

    // Allow environment overrides (useful for local testing/CI)
    private func mergingEnv() -> AppConfig {
        var copy = self
        let env = ProcessInfo.processInfo.environment

        if let v = env["FE_USER_ID"], !v.isEmpty { copy.userId = v }
        if let v = env["FE_SIGNATURE"], !v.isEmpty { copy.signature = v }
        if let v = env["FE_API_BASE_URL"], let u = URL(string: v) { copy.apiBaseURL = u }
        if let v = env["FE_SEND_BASE_URL"], let u = URL(string: v) { copy.sendBaseURL = u }

        return copy
    }
}
