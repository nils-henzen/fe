//
//  feApp.swift
//  fe
//

import SwiftUI

@main
struct FEApp: App {
    @NSApplicationDelegateAdaptor(AppDelegate.self) var appDelegate

    var body: some Scene {
        Settings { EmptyView() }
        .commands {
            CommandGroup(replacing: .appSettings) {
                Button("Settings…") {
                    AppConfig.openUserConfigInEditor(createIfNeeded: true)
                }
                .keyboardShortcut(",", modifiers: [.command])
            }
        }
    }
}
