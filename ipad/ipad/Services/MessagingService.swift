import Foundation
import Firebase
import FirebaseDatabase
import FirebaseAuth
class MessagingService {
    public static let PublicChannelId = "public"
    
    var ref: DatabaseReference!
    var userPath: String = "users"
    private let conversationPath: String = "conversations"
    private let messagesPath: String = "messages"
    private let membersPath: String = "members"
    
    init() {
        ref = Database.database().reference()
    }
    
    func getConversationList(callback:@escaping ((ConversationModel) -> Void)) {
        guard let userId = Auth.auth().currentUser?.uid else { return; }
        ref.child(self.userPath).child(userId).child(self.conversationPath).observe(DataEventType.childAdded, with: { (snapshot) in
            let conversationId = snapshot.key
            self.ref.child(self.conversationPath).child(conversationId).observeSingleEvent(of: DataEventType.value, with: { (snapshot) in
                let value  = snapshot.value as? [String:AnyObject] ?? [:]
                if (conversationId == "public") { return }
                let conversation = ConversationModel(
                    name: value["name"] as? String ?? "Unnamed conversation",
                    id: conversationId
                )
                
                callback(conversation)
                
            }, withCancel: { (error) in
                print(error.localizedDescription)
            })
        }) { (error) in
            print(error.localizedDescription)
        }
    }
    
    func getOtherConversationList(alreadyJoinedConversationsIds: Array<String>, callback:@escaping ((Array<ConversationModel>) -> Void)) {
        ref.child(self.conversationPath).observeSingleEvent(of: .value, with: { (snapshot) in
            let value = snapshot.value as? [String:AnyObject] ?? [:]
            
            var convos: Array<ConversationModel> = []
            for(key, val) in value {
                let id = val["id"] as? String ?? ""
                let name = val["name"] as? String ?? ""
                if !alreadyJoinedConversationsIds.contains(key) {
                    convos.append(ConversationModel(name: name, id: key))
                }
            }
            callback(convos)
        })
    }
    
    func getPublicConversation(callback :@escaping ((ConversationModel) -> Void)) {
        self.ref.child(self.conversationPath).child("public").observeSingleEvent(of: DataEventType.value, with: { (snapshot) in
            let value  = snapshot.value as? [String:AnyObject] ?? [:]
            let conversation = ConversationModel(
                name: value["name"] as? String ?? "Unnamed conversation",
                id: "public"
            )
            
            callback(conversation)
            
        }, withCancel: { (error) in
            print(error.localizedDescription)
        })
    }
    
    func getMessages(_ cid: String, callback: @escaping ((MessageModel) -> Void)) {
        guard let userId = Auth.auth().currentUser?.uid else { return }
        
        ref.child(self.messagesPath).child(cid).observe(DataEventType.childAdded, with: { (snapshot) in
            let value = snapshot.value as? [String:AnyObject] ?? [:]
            let message = MessageModel(
                text: value["text"] as? String ?? "",
                senderId: value["senderId"] as? String ?? "",
                senderName: value["senderName"] as? String ?? "Unnamed user",
                timestamp: (value["timestamp"] as? String) ?? "",
                isSentBySelf: value["senderId"] as? String == userId
            )
            
            callback(message)
        }) { (error) in
            print(error.localizedDescription)
        }
    }
    
    func joinConversation(userId uid: String, conversationWithId cid: String) {
        var post = [cid: true] as [String : Any]
        ref.child(self.userPath).child(uid).child(self.conversationPath).updateChildValues(post)
    }
    
    func sendMessage(_ cid: String, _ text: String) {
        if text.trimmingCharacters(in: .whitespaces).isEmpty { return }
        let userId = Auth.auth().currentUser?.uid
        let key = ref.child(self.messagesPath).child(cid).childByAutoId().key
        let post = ["text": text,
                    "senderName": Auth.auth().currentUser?.displayName ?? "Unnamed user",
                    "senderId": userId ?? "",
                    "timestamp": Formatter.iso8601 ] as [String : Any]
        ref.child(self.messagesPath).child(cid).child(key).updateChildValues(post)
    }
    
    func createConversation(_ name: String, _ members: Array<String>, callback: @escaping ((String) -> Void)) {
        let key = ref.child(self.conversationPath).childByAutoId().key
        var parsedMembers: [String: Bool] = [:]
        for member in members {
            parsedMembers[member] = true
        }
        let post = ["name": name,
                    "members": parsedMembers,
                    "id": key] as [String : Any]
        
        ref.child(self.conversationPath).child(key).updateChildValues(post)
        callback(key)
    }
}
