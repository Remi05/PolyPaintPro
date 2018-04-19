import UIKit

class SearchViewController: UIViewController, UITableViewDataSource, UITableViewDelegate {
    @IBOutlet weak var tableView: UITableView!
    
    var users = [UserModel]()
    let usersService = UsersService()
    
    override func viewDidLoad() {
        super.viewDidLoad()
        self.tableView.delegate = self
        self.tableView.dataSource = self
        self.tableView.rowHeight = UITableViewAutomaticDimension
        
        DispatchQueue.global(qos: .userInteractive).async {
            self.getUsers()
        }
    }
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return users.count
    }
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let cellIdentifier = "SearchTableViewCell"
        guard let cell = tableView.dequeueReusableCell(withIdentifier: cellIdentifier, for: indexPath) as? SearchTableViewCell  else {
            fatalError("The dequeued cell is not an instance of SearchTableViewCell.")
        }
        
        let user = users[indexPath.row]
        cell.name.text = user.displayName
        if user.photoUrl == "" {
            cell.photo.image = UIImage(named: "Profile")
        } else {
            cell.photo.downloadedFrom(url: URL(string: user.photoUrl)!, completion: { (success) in
                
            })
        }
        
        return cell
    }
    
    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        let controller = ViewHelper.getView(Views.ProfileViewController) as! ProfileViewController
        controller.uid = self.users[indexPath.row].uid
        self.navigationController?.pushViewController(controller, animated: true)
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
}
