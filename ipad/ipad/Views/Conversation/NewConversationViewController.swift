import UIKit
import FirebaseAuth

class NewConversationViewController: UIViewController, UITableViewDataSource, UITableViewDelegate {
    var users = [UserModel]()
    var selectedCount: Int = 0
    let usersService = UsersService()
    let messagingService = MessagingService()
    
    @IBOutlet weak var tableView: UITableView!
    @IBOutlet weak var chatButton: UIButton!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        self.tableView.delegate = self
        self.tableView.dataSource = self
        self.tableView.rowHeight = UITableViewAutomaticDimension
        getUsers()
    }
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return users.count
    }
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let cellIdentifier = "UserTableViewCell"
        guard let cell = tableView.dequeueReusableCell(withIdentifier: cellIdentifier, for: indexPath) as? UserTableViewCell  else {
            fatalError("The dequeued cell is not an instance of UserTableViewCell.")
        }
        
        let user = users[indexPath.row]
        cell.name.text = user.displayName
        cell.checkbox.isHidden = !user.isSelected
        chatButton.isHidden = !(selectedCount > 0)
        
        if user.photoUrl == "" {
            cell.photo.image = UIImage(named: "Profile")
        } else {
            cell.photo.downloadedFrom(url: URL(string: user.photoUrl)!, completion: { (success) in
                
            })
        }
        
        return cell
    }
    
    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        users[indexPath.row].isSelected = !users[indexPath.row].isSelected
        selectedCount = users[indexPath.row].isSelected ? selectedCount + 1 : selectedCount - 1
        
        self.tableView.reloadData()
    }

    private func getUsers() {
        usersService.getUsers(callback: { (users) in
            self.users = users
            for (index, user) in self.users.enumerated() {
                self.usersService.getUserPhotoUrl(uid: user.uid, callback: { (url) in
                    self.users[index].photoUrl = url
                    self.tableView.reloadData()
                })
            }
        })
    }
    
    @IBAction func chat(_ sender: Any) {
        let selectedUsers = users.filter({ (user) -> Bool in
            return user.isSelected
        })
        
        var usernames : Array<String> = []
        var ids: Array<String> = []
        
        let currentUser = Auth.auth().currentUser
        usernames.append((currentUser?.displayName)!)
         ids.append((currentUser?.uid)!)
        
        for user in selectedUsers {
            usernames.append(user.displayName)
            ids.append(user.uid)
        }
        let conversationName = "Nouvelle Conversation"
        
        let alertController = UIAlertController(title: "Créer une nouvelle conversation", message: "", preferredStyle: .alert)
        alertController.addTextField { (textField) in
            textField.placeholder = conversationName
        }
        
        let createAction = UIAlertAction(title: "Créer", style: .default) { (action) -> Void in
            let name = (alertController.textFields?.first?.text?.trimmingCharacters(in: .whitespaces).isEmpty)! ? conversationName : alertController.textFields?.first?.text

            self.createConversation(conversationName: name!, userIds: ids)
        }
        
        let cancelAction = UIAlertAction(title: "Annuler", style: .default)
        
        alertController.addAction(cancelAction)
        alertController.addAction(createAction)
        
        self.present(alertController, animated: true, completion: nil)
    }
    
    func createConversation(conversationName: String, userIds: Array<String>) {
        var conversationId: String = ""
        messagingService.createConversation(conversationName, userIds, callback: { (id) in
            conversationId = id
        })
        
        for id in userIds {
            messagingService.joinConversation(userId: id, conversationWithId: conversationId)
        }
        
        self.navigationController?.popToRootViewController(animated: true)
    }
}
