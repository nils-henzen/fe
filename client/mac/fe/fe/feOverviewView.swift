//
//  FEView.swift
//  fe
//
//  Created by Nico Schönmeyer on 06.10.25.
//

import SwiftUI

struct FEOverviewView: View {
    @StateObject private var api = API.shared
    @State private var expandedUser: String?
    @State private var replyText = ""
    @State private var hoveringBack = false
    
    var body: some View {
        VStack(alignment: .leading) {
            if let user = expandedUser {
                

                Button(action: { expandedUser = nil }) {
                    HStack(spacing: 0) {
                        Text("⬅︎")
                            .opacity(hoveringBack ? 1 : 0)          // fade in/out
                            .foregroundStyle(.blue)
                            .animation(.easeInOut(duration: 0.2), value: hoveringBack)
                        Text(user.lowercased())
                            .offset(x: hoveringBack ? 4 : -12)       // shift right when "<" appears
                            .foregroundStyle(hoveringBack ? .blue : .primary.opacity(1.0))
                            .animation(.easeInOut(duration: 0.2), value: hoveringBack)
                            
                    }
                    .bold()
                }
                .buttonStyle(.plain) // remove default link style if needed
                .padding(8)
                .contentShape(Rectangle()) // make the whole area hoverable
                .onHover { hovering in
                    hoveringBack = hovering
                }

                    

                ScrollViewReader { proxy in
                    ScrollView {
                        VStack(alignment: .leading, spacing: 4) {
                            ForEach(api.messagesBySender[user] ?? []) { msg in
                                MessageRow(
                                    msg: msg,
                                    openFile: { openFile($0) },
                                    openFullMessage: { id in
                                        AppDelegate.shared?.showMainWindow {
                                            FEMessageView(messageID: id)
                                        }
                                    }
                                )
                            }
                            Spacer(minLength: 0).id("bottom") // extra space at the bottom
                        }
                        .padding(.top, 0)
                        .padding(8)
                    }
                    .onChange(of: api.messagesBySender[user]?.count) { _ in
                        // Scroll to the last message whenever the message list changes
                        withAnimation(.easeInOut(duration: 1.0)) {
                            proxy.scrollTo("bottom", anchor: .bottom)
                        }
                        
                    }
                    .onAppear {
                        // Scroll to bottom when view first appears
                        proxy.scrollTo("bottom", anchor: .bottom)
                    }
                }


                HStack {
                    TextField("...", text: $replyText)
                        .textFieldStyle(.plain)
                        .padding(8)
                        .glassEffect()
                        
                    Button("➢") {
                        Task { await api.sendMessage(to: user, content: replyText) }
                        replyText = ""
                    }
                    .bold()
                    .foregroundStyle(.blue)
                    .buttonStyle(.plain)
                    .padding(8)
                    .glassEffect()
                }
                .frame(minHeight: 16)
                .padding(8)

            } else {
                // Sender list with Liquid Glass buttons
                
                Text("Fast Exchange")
                    .bold()
                    .padding(8)
                
                ScrollView {
                    VStack(spacing: 8) {
                        ForEach(api.messagesBySender.keys.sorted(), id: \.self) { sender in
                            SenderButton(
                                sender: sender,
                                lastFileName: api.messagesBySender[sender]?.last?.file_name
                            ) {
                                expandedUser = sender
                            }
                        }
                    }
                    .padding(8)
                }
                .scrollContentBackground(.hidden) // hide default list background
            }
        }
        .frame(width: 300, height: 280)
        .padding(8)
        .task {
            await api.fetchMessages()
        }
    }

    private func openFile(_ filename: String) {
        let url = URL(fileURLWithPath: filename)
        NSWorkspace.shared.open(url)
    }

    
    struct SenderButton: View {
        let sender: String
        let lastFileName: String?
        let action: () -> Void

        @State private var hovering = false

        var body: some View {
            Button(action: action) {
                VStack(alignment: .leading, spacing: 2) {
                    Text(sender).bold()
                    if let fileName = lastFileName {
                        Text(fileName)
                            .lineLimit(1)
                            .foregroundColor(.secondary)
                    }
                }
                .padding(12)
                .frame(maxWidth: .infinity, alignment: .leading)
                .background(
                    RoundedRectangle(cornerRadius: 12)
                        .fill(.ultraThinMaterial)
                        .shadow(color: Color.black.opacity(0.15), radius: 8, x: 0, y: 4)
                        .compositingGroup()
                        .mask(RoundedRectangle(cornerRadius: 12))
                )

                .overlay(
                    RoundedRectangle(cornerRadius: 12)
                        .stroke(Color.white.opacity(0.2), lineWidth: 1)
                )
                .scaleEffect(hovering ? 1.02 : 1.0)
                .animation(.spring(response: 0.25, dampingFraction: 0.6), value: hovering)
            }
            .buttonStyle(.plain)
            .contentShape(Rectangle())
            .onHover { hovering in
                self.hovering = hovering
            }
        }
    }
}
