// AppConfig.swift
import Foundation
import AppKit

struct AppConfig: Codable {
    var userId: String
    var signature: String
    var apiBaseURL: URL
    var sendBaseURL: URL

    // Reloadable singleton
    private static func loadFromSources() -> AppConfig {
        // 1) Try user config: ~/Library/Application Support/fe/AppConfig.json
        if let file = userConfigFileURL,
           let data = try? Data(contentsOf: file),
           let cfg = try? JSONDecoder().decode(AppConfig.self, from: data) {
            return cfg.mergingEnv()
        }
        // 2) Try bundled AppConfig.json
        if let url = Bundle.main.url(forResource: "AppConfig", withExtension: "json"),
           let data = try? Data(contentsOf: url),
           let cfg = try? JSONDecoder().decode(AppConfig.self, from: data) {
            return cfg.mergingEnv()
        }
        // 3) Fallback defaults
        let fallback = AppConfig(
            userId: "changeme",
            signature: "changeme",
            apiBaseURL: URL(string: "http://127.0.0.1:26834")!,
            sendBaseURL: URL(string: "http://127.0.0.1:5000")!
        )
        return fallback.mergingEnv()
    }

    static private(set) var shared: AppConfig = loadFromSources()

    // Call to force a reload from disk/env
    static func reload() {
        // Keep mutation on main to avoid racy writes; most callers read on main.
        if Thread.isMainThread {
            shared = loadFromSources()
        } else {
            DispatchQueue.main.sync {
                shared = loadFromSources()
            }
        }
    }

    // MARK: - Settings helpers

    // ~/Library/Application Support/fe
    static var userConfigDirectoryURL: URL? {
        FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask).first?
            .appendingPathComponent("fe", isDirectory: true)
    }

    // ~/Library/Application Support/fe/AppConfig.json
    static var userConfigFileURL: URL? {
        userConfigDirectoryURL?.appendingPathComponent("AppConfig.json")
    }

    // Ensure directory/file exist; if file is missing, write a template using current values.
    @discardableResult
    static func ensureUserConfigExists() throws -> URL {
        guard let dir = userConfigDirectoryURL, let file = userConfigFileURL else {
            throw NSError(domain: "AppConfig", code: 1, userInfo: [NSLocalizedDescriptionKey: "Unable to resolve Application Support path."])
        }
        let fm = FileManager.default
        if !fm.fileExists(atPath: dir.path) {
            try fm.createDirectory(at: dir, withIntermediateDirectories: true)
        }
        if !fm.fileExists(atPath: file.path) {
            let template = AppConfig.shared
            let encoder = JSONEncoder()
            encoder.outputFormatting = [.prettyPrinted, .withoutEscapingSlashes, .sortedKeys]
            let data = try encoder.encode(template)
            try data.write(to: file, options: .atomic)
        }
        return file
    }

    // Create if needed and open in default editor
    static func openUserConfigInEditor(createIfNeeded: Bool = true) {
        do {
            let url: URL
            if createIfNeeded {
                url = try ensureUserConfigExists()
            } else if let file = userConfigFileURL {
                url = file
            } else {
                throw NSError(domain: "AppConfig", code: 2, userInfo: [NSLocalizedDescriptionKey: "Config file URL could not be determined."])
            }
            NSWorkspace.shared.open(url)
        } catch {
            NSLog("Failed to open config: \(error.localizedDescription)")
        }
    }

    // MARK: - Env overrides

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

// Notification others can observe when config reloads
extension Notification.Name {
    static let appConfigDidReload = Notification.Name("AppConfigDidReload")
}
