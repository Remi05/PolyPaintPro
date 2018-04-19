import UIKit
import FirebaseAuth
import FBSDKLoginKit
import GoogleSignIn
import ScalingCarousel

class ProfileViewController: UIViewController, UICollectionViewDataSource, UICollectionViewDelegate, UIScrollViewDelegate {
    @IBOutlet weak var imageProfile: UIImageView!
    @IBOutlet weak var nbDrawings: UILabel!
    @IBOutlet weak var nbFollowers: UILabel!
    @IBOutlet weak var nbFollowings: UILabel!
    @IBOutlet weak var displayName: UILabel!
    @IBOutlet weak var followButton: UIButton!
    @IBOutlet weak var logoutButton: UIButton!
    @IBOutlet weak var scalingCarousel: ScalingCarouselView!
    
    private var usersService: UsersService?
    private var drawingsService: DrawingService?
    private var drawings: [DrawingInfoModel]?
    public var uid: String = (Auth.auth().currentUser?.uid)!
    let loginManager = FBSDKLoginManager()
    
    override func viewDidLoad() {
        super.viewDidLoad()
        self.usersService = UsersService()
        self.drawingsService = DrawingService()
        self.followButton.isHidden = self.uid == Auth.auth().currentUser?.uid
        self.logoutButton.isHidden = self.uid != Auth.auth().currentUser?.uid
        self.followButton.layer.cornerRadius = 5
        self.logoutButton.layer.cornerRadius = 5
        self.displayName.font = self.displayName.font.withSize(20)
        
        DispatchQueue.global(qos: .userInteractive).async {
            self.getUserInfo()
            self.getUserDrawings()
        }
    }
    
    func setupOfflineMode() {
        var views: [UIViewController] = []
        let auth = ViewHelper.getView(.AuthenticationViewController)
        views.append(auth)
        views.append((self.tabBarController?.viewControllers![1])!)
        self.tabBarController?.setViewControllers(views, animated: true)
    }
    
    func collectionView(_ collectionView: UICollectionView, numberOfItemsInSection section: Int) -> Int {
        return self.drawings?.count ?? 0
    }
    
    func collectionView(_ collectionView: UICollectionView, cellForItemAt indexPath: IndexPath) -> UICollectionViewCell {
        let cellIdentifier = "DrawingsCollectionViewCell"
        guard let cell = collectionView.dequeueReusableCell(withReuseIdentifier: cellIdentifier, for: indexPath) as? DrawingsCollectionViewCell else {
            fatalError("The dequeued cell is not an instance of DrawingsCollectionViewCell.")
        }
        
        let drawing = self.drawings![indexPath.row]
        if(drawing.thumbnailUrl != "") {
            let defaults = UserDefaults.standard
            let data = defaults.data(forKey: drawing.id + "_thumbnail")
            
            if data != nil {
                cell.thumbnailPhoto.image = UIImage(data: data!)
            } else {
                cell.thumbnailPhoto.image = UIImage(named: "No-T")
            }
            
            if drawing.nsfw {
                cell.setNsfw()
                cell.isNsfw = true
            } else if !drawing.nsfw {
                cell.resetNsfw()
                cell.isNsfw = false
            }
        }
        
        cell.drawingId = drawing.id
        cell.userId = self.uid
        
        self.drawingsService?.isLike(drawingId: cell.drawingId, userId: cell.userId, callback: {(isLiked) in
            cell.isLiked = isLiked!
            if cell.isLiked {
                cell.likeImage.image = UIImage(named: "RedHeart")
            } else {
                cell.likeImage.image = UIImage(named: "GrayHeart")
            }
        })
        
        self.drawingsService?.numberOfLike(drawindId: cell.drawingId, userId: cell.userId, callback: { (count) in
            cell.likes.text = String(count) + " J'aime(s)"
        })
        
        return cell
    }
    
    private func getUserDrawings() {
        self.usersService?.getUserDrawings(uid: self.uid, callback: { (data: [String:String]) in
            self.drawings = []
            for (key, _) in data {
                self.drawingsService!.getDrawingInfo(id: key, callback: { (info: DrawingInfoModel) in
                    self.drawingsService?.getDrawing(name: key, callback: { (model) in
                        if model.isPublic {
                            var exist = false
                            for drawing in self.drawings! {
                                if drawing.id == info.id {
                                    drawing.thumbnailUrl = info.thumbnailUrl
                                    drawing.nsfw = info.nsfw
                                    drawing.timestamp = info.timestamp
                                    exist = true
                                    break
                                }
                            }
                            if !exist {
                                self.drawings?.append(info)
                            }
                            self.drawings = self.drawings?.sorted(by: { (drawing1, drawing2) -> Bool in
                                return drawing1.timestamp > drawing2.timestamp
                            })
                            self.scalingCarousel.reloadData()
                        }
                    })
                })
            }
        })
    }
    
    private func getUserInfo() {
        self.usersService?.getUserDrawingsCount(uid: self.uid, callback: { (count) in
            self.nbDrawings.text = String(count)
        })
        
        self.usersService?.getFollowersCount(uid: self.uid, callback: { (count) in
            self.nbFollowers.text = String(count)
        })
        
        self.usersService?.getFollowingsCount(uid: self.uid, callback: { (count) in
            self.nbFollowings.text = String(count)
        })
        
        DispatchQueue.main.async {
            if (!self.followButton.isHidden) {
                self.usersService?.isFollower(currentUid: (Auth.auth().currentUser?.uid)!, otherUserUid: self.uid, callback: { (isFollower) in
                    if isFollower! {
                        self.followButton.setTitle("Abonné ✓", for: .normal)
                        self.followButton.backgroundColor = UIColor.gray
                    } else {
                        self.followButton.setTitle("Suivre", for: .normal)
                        self.followButton.backgroundColor = UIColor(red: 0, green: 150 / 255, blue: 1, alpha: 1)
                    }
                })
            }
        }
        
        self.usersService?.getUserInfo(uid: self.uid, callback: { (info) in
            if (info?.photoUrl == "") {
                self.imageProfile.image = UIImage(named: "Anonymous")
            } else {
                self.imageProfile.downloadedFrom(url: URL(string: (info?.photoUrl)!)!, completion: { (success) in })
            }
            self.imageProfile.layer.cornerRadius = self.imageProfile.frame.size.width / 2
            self.imageProfile.clipsToBounds = true
            
            if info?.displayName == "" {
                self.usersService?.getUserDisplayName(uid: self.uid, callback: { (name) in
                    self.displayName?.text = name
                })
            }
            self.displayName?.text = info?.displayName
        })
    }
    
    @IBAction func logout(_ sender: Any) {
        do {
            try Auth.auth().signOut()
            loginManager.logOut()
            FBSDKAccessToken.setCurrent(nil)
            FBSDKProfile.setCurrent(nil)
            GIDSignIn.sharedInstance().signOut()
            self.setupOfflineMode()
        } catch let error as NSError {
            let alertController = ViewHelper.getAlert(error.localizedDescription)
            self.present(alertController, animated: true, completion: nil)
        }
    }
    
    @IBAction func followUser(_ sender: Any) {
        let currentUserUid = Auth.auth().currentUser?.uid
        self.usersService?.isFollower(currentUid: (Auth.auth().currentUser?.uid)!, otherUserUid: self.uid, callback: { (isFollower) in
            if isFollower! {
                self.usersService?.unfollowUser(currentUserUid: currentUserUid!, unfollowedUserUid: self.uid)
                self.getUserInfo()
            } else {
                self.usersService?.followUser(currentUserUid: currentUserUid!, followedUserUid: self.uid)
                self.getUserInfo()
            }
        })
    }
}
