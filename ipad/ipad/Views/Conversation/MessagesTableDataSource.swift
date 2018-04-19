import UIKit

class MessagesTableDataSource: NSObject, UITableViewDataSource, UITableViewDelegate {
    var messages = [MessageModel]()
    var heights = [Float]()
    var tableView: UITableView
    
    let messagingService = MessagingService()
    
    init(_ tableView: UITableView, _ conversationId: String) {
        self.tableView = tableView
        
        super.init()
        self.tableView.rowHeight = UITableViewAutomaticDimension
        self.tableView.estimatedRowHeight = 140
        self.tableView.delegate = self
        self.tableView.dataSource = self
        
        DispatchQueue.global(qos: .userInteractive).async {
            self.observeConversation(conversationId)
        }
    }
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return self.messages.count
    }
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let message = messages[indexPath.row]

        var cellIdentifier =  message.isSentBySelf ? "MessageSentBySelf" : "MessageSentByOther"
        
        if  self.wasPreviousMessageFromSameUser(forMessageAtIndex: indexPath) {
            cellIdentifier +=  "Bis"
        }
        
        guard let cell = tableView.dequeueReusableCell(withIdentifier: cellIdentifier, for: indexPath) as? MessageTableViewCell  else {
            fatalError("The dequeued cell is not an instance of ConversationTableViewCell.")
        }
        
        cell.content.text = message.text
        cell.authorName?.text = message.senderName
        
        let dateFormatter = DateFormatter()
        dateFormatter.locale = Locale(identifier: "fr_CA")
        dateFormatter.setLocalizedDateFormatFromTemplate("MMMMdhms")
        
        cell.timestamp?.text = dateFormatter.string(from: Formatter.date(date: message.timestamp))

        return cell
    }
    
    public func observeConversation(_ id: String) {
        self.messages = []
        DispatchQueue.main.async {
            self.tableView.reloadData()
        }
        self.messagingService.getMessages(id, callback: { (message) in
            self.messages.append(message)
            self.tableView.reloadData()
            let indexPath = NSIndexPath(item: self.messages.count - 1, section: 0)
            self.tableView.scrollToRow(at: indexPath as IndexPath, at: UITableViewScrollPosition.top, animated: false)
        })
    }
    
    private func wasPreviousMessageFromSameUser(forMessageAtIndex indexPath: IndexPath) -> Bool {
        let message = messages[indexPath.row]
        return indexPath.row > 0 && messages[indexPath.row - 1].senderId == message.senderId
    }
}

