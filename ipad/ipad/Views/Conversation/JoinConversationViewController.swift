import UIKit
import FirebaseAuth

class JoinConversationViewController: UIViewController, UITableViewDataSource, UITableViewDelegate {
    var conversations = [ConversationModel]()
    let messagingService = MessagingService()
    
    @IBOutlet weak var tableview: UITableView!
    public var alreadyJoinedConversations: [ConversationModel] = []
    
    override func viewDidLoad() {
        super.viewDidLoad()
        self.tableview.rowHeight = UITableViewAutomaticDimension
        getConversations()
    }
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return conversations.count
    }
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let cellIdentifier = "ConversationTableViewCell"
        guard let cell = tableView.dequeueReusableCell(withIdentifier: cellIdentifier, for: indexPath) as? ConversationTableViewCell  else {
            fatalError("The dequeued cell is not an instance of ConversationTableViewCell.")
        }
        
        let conversation = conversations[indexPath.row]
        
        cell.name.text = conversation.name
        
        return cell
    }
    
    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        let convo = conversations[indexPath.row]
        let uid = Auth.auth().currentUser?.uid
        
        let alertController = UIAlertController(title: "Joindre la conversation?", message: "", preferredStyle: .alert)
        let joinAction = UIAlertAction(title: "Confirmer", style: .default) { (action) -> Void in
            self.handleJoin(uid: uid!, cid: convo.id)
            
        }
        let cancelAction = UIAlertAction(title: "Annuler", style: .default)
        
        alertController.addAction(cancelAction)
        alertController.addAction(joinAction)
        
        self.present(alertController, animated: true, completion: nil)
    }
    
    func handleJoin(uid: String, cid: String) {
        self.messagingService.joinConversation(userId: uid, conversationWithId: cid)
        self.navigationController?.popToRootViewController(animated: true)
    }
    
    func getConversations() {
        var ids: [String] = []
        for convo in self.alreadyJoinedConversations {
            ids.append(convo.id)
        }
        self.messagingService.getOtherConversationList(alreadyJoinedConversationsIds: ids, callback: { (conversations) in
            self.conversations = conversations
            self.tableview.reloadData()
        })
    }
}
