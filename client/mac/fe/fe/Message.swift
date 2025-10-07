//
//  Message.swift
//  fe
//
//  Created by Nico Schönmeyer on 06.10.25.
//

import Foundation


struct Message: Identifiable, Codable, Equatable {
    let id: Int
    let sender_id: String
    let receiver_id: String
    let timestamp: Int
    let file_name: String
    let file_type: String
    let file_contents: String?
    let queue_deletion: Int
    
    static func == (lhs: Message, rhs: Message) -> Bool {
        return lhs.id == rhs.id
    }
}
