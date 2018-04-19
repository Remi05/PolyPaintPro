import UIKit

class ConversationsViewController: UIViewController, UITextFieldDelegate {
    //MARK: Properties
    @IBOutlet weak var messagesTableView: UITableView!
    @IBOutlet weak var conversationsTableView: UITableView!
    @IBOutlet weak var textField: UITextField!
    @IBOutlet weak var sendButton: UIBarButtonItem!
    @IBOutlet weak var joinButton: UIButton!
    @IBOutlet weak var createButton: UIButton!
    
    private var messagingService: MessagingService?
    private var conversationListDataSource: ConversationListDataSource?

    override func viewDidLoad() {
        super.viewDidLoad()
        self.conversationsTableView.layer.masksToBounds = true
        self.conversationsTableView.layer.borderWidth = 0.5
        self.conversationsTableView.layer.borderColor = UIColor.lightGray.cgColor
        self.messagesTableView.layer.masksToBounds = true
        self.messagesTableView.layer.borderWidth = 0.5
        self.messagesTableView.layer.borderColor = UIColor.lightGray.cgColor
        self.joinButton.layer.masksToBounds = true
        self.joinButton.layer.borderWidth = 0.5
        self.joinButton.layer.borderColor = UIColor.lightGray.cgColor
        self.createButton.layer.masksToBounds = true
        self.createButton.layer.borderWidth = 0.5
        self.createButton.layer.borderColor = UIColor.lightGray.cgColor
        self.textField.delegate = self
        self.messagingService = MessagingService()
        self.conversationListDataSource = ConversationListDataSource(self.conversationsTableView, messagesTableView: self.messagesTableView)
    }
    
    override func viewWillAppear(_ animated:Bool) {
        super.viewWillAppear(animated)
        
        NotificationCenter.default.addObserver(self, selector: #selector(self.keyboardWillShow), name: NSNotification.Name.UIKeyboardWillShow, object: nil)
        
        NotificationCenter.default.addObserver(self, selector: #selector(self.keyboardWillHide), name: NSNotification.Name.UIKeyboardWillHide, object: nil)
    }
    
    override func viewWillDisappear(_ animated: Bool) {
        super.viewWillDisappear(animated)
        NotificationCenter.default.removeObserver(self, name: NSNotification.Name.UIKeyboardWillShow, object: nil)
        NotificationCenter.default.removeObserver(self, name: NSNotification.Name.UIKeyboardWillHide, object: nil)
    }
    
    @objc func keyboardWillShow(notification: NSNotification) {
        if let keyboardSize = (notification.userInfo?[UIKeyboardFrameBeginUserInfoKey] as? NSValue)?.cgRectValue {
           self.view.frame.origin.y -= keyboardSize.height
        }
    }
    
    @objc func keyboardWillHide(notification: NSNotification) {
        self.view.frame.origin.y = 0
    }

    func textFieldShouldReturn(_ textField: UITextField) -> Bool {
        self.sendMessage(textField)
        return true
    }
    
    @IBAction func newConversation(_ sender: Any) {
        let controller = ViewHelper.getView(Views.NewConversationViewController)
        self.navigationController?.pushViewController(controller, animated: true)
    }
    
    @IBAction func joinConversation(_ sender: Any) {
        let controller = ViewHelper.getView(Views.JoinConversationViewController) as! JoinConversationViewController
        controller.alreadyJoinedConversations = (self.conversationListDataSource?.conversations)!
        self.navigationController?.pushViewController(controller, animated: true)
    }
    
    @IBAction func sendMessage(_ sender: Any) {
        let index = conversationsTableView.indexPathForSelectedRow
        let currentConversation = conversationsTableView.cellForRow(at: index!) as? ConversationTableViewCell
        self.messagingService?.sendMessage((currentConversation?.activeConversationId)!, self.textField!.text ?? "")
        self.textField.text = ""
    }
}
