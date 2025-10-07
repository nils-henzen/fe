//
//  AppDelegate.swift
//  fe
//

import AppKit
import SwiftUI
import UserNotifications

final class AppDelegate: NSObject, NSApplicationDelegate, NSWindowDelegate, UNUserNotificationCenterDelegate {
    static var shared: AppDelegate?
    
    var statusItem: NSStatusItem!
    var popover = NSPopover()
    private var messageWindow: NSWindow?
    
    func applicationDidFinishLaunching(_ notification: Notification) {
        AppDelegate.shared = self
        NSApp.setActivationPolicy(.accessory)

        setupStatusBar()
        setupPopover()
        setupNotifications()
        
        Task { await API.shared.startPolling() }
    }
    
    // MARK: - Setup

    private func setupStatusBar() {
        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.squareLength)
        if let button = statusItem.button {
            button.image = NSImage(systemSymbolName: "envelope.fill", accessibilityDescription: nil)
            button.action = #selector(togglePopover)
        }
    }

    private func setupPopover() {
        popover.contentSize = NSSize(width: 320, height: 400)
        popover.behavior = .transient
        popover.animates = false
        popover.contentViewController = NSHostingController(rootView: FEOverviewView())
    }

    private func setupNotifications() {
        let center = UNUserNotificationCenter.current()
        center.delegate = self
        
        center.requestAuthorization(options: [.alert, .sound, .badge]) { granted, error in
            if let error = error {
                print("Notification auth error:", error)
            } else if granted {
                print("Notification permission granted")
            } else {
                print("Notification permission denied")
            }
        }
    }

    // MARK: - Popover

    @objc func togglePopover() {
        if popover.isShown {
            popover.performClose(nil)
        } else if let button = statusItem.button {
            popover.show(relativeTo: button.bounds, of: button, preferredEdge: .minY)
            NSApp.activate(ignoringOtherApps: true)
        }
    }

    // MARK: - Message Window

    func showMessageWindow(messageID: Int) {
        if popover.isShown { popover.performClose(nil) }

        DispatchQueue.main.async {
            // Ensure app and window come to front
            NSRunningApplication.current.activate(options: [.activateIgnoringOtherApps])
            NSApp.activate(ignoringOtherApps: true)

            let content = NSHostingController(rootView: FEMessageView(messageID: messageID))

            if let window = self.messageWindow {
                window.contentViewController = content
                if window.isMiniaturized { window.deminiaturize(nil) }
                window.orderFrontRegardless()
                window.makeKeyAndOrderFront(nil)
                window.center()
            } else {
                let window = NSWindow(
                    contentRect: NSRect(x: 0, y: 0, width: 500, height: 600),
                    styleMask: [.titled, .closable, .resizable],
                    backing: .buffered,
                    defer: false
                )
                window.center()
                window.isReleasedWhenClosed = false
                window.collectionBehavior.insert(.moveToActiveSpace)
                window.contentViewController = content
                self.messageWindow = window
                window.delegate = self
                window.orderFrontRegardless()
                window.makeKeyAndOrderFront(nil)
            }
        }
    }

    func hideMessageWindow(reopenPopover: Bool = true) {
        messageWindow?.orderOut(nil)
        if reopenPopover { togglePopover() }
    }

    // MARK: - NSWindowDelegate

    func windowShouldClose(_ sender: NSWindow) -> Bool {
        sender.orderOut(nil)
        return false
    }

    func windowWillClose(_ notification: Notification) {
        if let window = notification.object as? NSWindow, window == messageWindow {
            messageWindow = nil
        }
    }

    // MARK: - Notifications

    func userNotificationCenter(
        _ center: UNUserNotificationCenter,
        didReceive response: UNNotificationResponse,
        withCompletionHandler completionHandler: @escaping () -> Void
    ) {
        if let messageID = response.notification.request.content.userInfo["messageID"] as? Int {
            showMessageWindow(messageID: messageID)
        }
        completionHandler()
    }

    func sendNotification(for peer: String, messageID: Int) {
        let content = UNMutableNotificationContent()
        content.title = "New message from \(peer)"
        content.body = "Click to open the message."
        content.sound = .default
        content.userInfo = ["messageID": messageID]

        let request = UNNotificationRequest(
            identifier: UUID().uuidString,
            content: content,
            trigger: nil
        )

        UNUserNotificationCenter.current().add(request) { error in
            if let error = error { print("Failed to send notification:", error) }
        }
    }

    // MARK: - Utilities

    public static func getDateFromUnixInt(timestamp: Int) -> String {
        let formatter = DateFormatter()
        formatter.dateStyle = .short
        formatter.timeStyle = .short
        return formatter.string(from: Date(timeIntervalSince1970: Double(timestamp)))
    }
}
