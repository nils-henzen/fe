// ConfigReloader.swift
import Foundation

final class ConfigReloader {
    static let shared = ConfigReloader()

    private var timer: DispatchSourceTimer?
    private var lastExists: Bool?
    private var lastModDate: Date?

    func start(interval: TimeInterval = 5) {
        stop()

        let q = DispatchQueue(label: "ConfigReloader.timer")
        let t = DispatchSource.makeTimerSource(queue: q)
        t.schedule(deadline: .now() + interval, repeating: interval)
        t.setEventHandler { [weak self] in
            self?.check()
        }
        t.resume()
        timer = t
        // Seed baseline immediately
        check()
    }

    func stop() {
        timer?.cancel()
        timer = nil
    }

    private func check() {
        guard let url = AppConfig.userConfigFileURL else { return }
        let fm = FileManager.default
        let exists = fm.fileExists(atPath: url.path)

        var modDate: Date? = nil
        if exists {
            modDate = (try? url.resourceValues(forKeys: [.contentModificationDateKey]))?.contentModificationDate
        }

        let shouldReload: Bool = {
            // If existence flipped from false/nil to true → reload
            if lastExists == false || lastExists == nil, exists { return true }
            // If exists and mod date increased → reload
            if exists, let prev = lastModDate, let now = modDate, now > prev { return true }
            return false
        }()

        lastExists = exists
        if exists { lastModDate = modDate }

        if shouldReload {
            AppConfig.reload()
            DispatchQueue.main.async {
                NotificationCenter.default.post(name: .appConfigDidReload, object: nil)
            }
        }
    }
}
