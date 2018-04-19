import UIKit

class ConversationListDataSource: NSObject, UITableViewDataSource, UITableViewDelegate {
    var conversations = [ConversationModel]()
    var tableView: UITableView
    var messagesTableView: UITableView
    
    let messagingService = MessagingService()
    var activeConversationId: String
    let messagesTableDataSource: MessagesTableDataSource
    
    init(_ tableView: UITableView, messagesTableView: UITableView) {
        self.tableView = tableView
        self.messagesTableView = messagesTableView
        self.activeConversationId = "public"
        self.messagesTableDataSource = MessagesTableDataSource(self.messagesTableView, activeConversationId)
       
        super.init()
        self.tableView.delegate = self
        self.tableView.dataSource = self
        
        observeConversations()
    }
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return self.conversations.count
    }
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let cellIdentifier = "ConversationTableViewCell"
        guard let cell = tableView.dequeueReusableCell(withIdentifier: cellIdentifier, for: indexPath) as? ConversationTableViewCell  else {
            fatalError("The dequeued cell is not an instance of ConversationTableViewCell.")
        }
        
        let conversation = conversations[indexPath.row]
        cell.name.text = conversation.name
        cell.activeConversationId = activeConversationId
        if (cell.activeConversationId == conversation.id) {
            tableView.selectRow(at: indexPath, animated: false, scrollPosition: UITableViewScrollPosition.top)
        }
        
        return cell
    }
    
    public func observeConversations() {
        self.messagingService.getPublicConversation(callback: { (conversation) in
            self.conversations.append(conversation)
            self.tableView.reloadData()
        })
        
        self.messagingService.getConversationList(callback: { (conversation) in
            if self.conversations.isEmpty {
                self.activeConversationId = conversation.id
            }
            self.conversations.append(conversation)
            self.tableView.reloadData()
        })
    }
    
    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        activeConversationId = conversations[indexPath.row].id
        messagesTableDataSource.observeConversation(activeConversationId)
        self.tableView.reloadData()
    }
}
