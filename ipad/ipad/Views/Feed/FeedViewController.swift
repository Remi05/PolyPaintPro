import UIKit
import FirebaseAuth
import ScalingCarousel

class FeedViewController: UIViewController, UICollectionViewDataSource, UICollectionViewDelegate, UIScrollViewDelegate {
    private var usersService: UsersService?
    private var drawingsService: DrawingService?
    private var feeds: [FeedModel]?

    @IBOutlet weak var scalingCarousel: ScalingCarouselView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        self.usersService = UsersService()
        self.drawingsService = DrawingService()
        self.view.contentMode = .scaleToFill
        self.scalingCarousel.contentMode = .scaleToFill

        DispatchQueue.global(qos: .userInteractive).async {
            self.getFeed()
        }
        
        DispatchQueue.global(qos: .background).async {
            self.drawingsService?.syncData()
        }
    }
    
    func collectionView(_ collectionView: UICollectionView, numberOfItemsInSection section: Int) -> Int {
        return self.feeds?.count ?? 0
    }
    
    func collectionView(_ collectionView: UICollectionView, cellForItemAt indexPath: IndexPath) -> UICollectionViewCell {
        let cellIdentifier = "FeedCollectionViewCell"
        guard let cell = collectionView.dequeueReusableCell(withReuseIdentifier: cellIdentifier, for: indexPath) as? FeedCollectionViewCell else {
            fatalError("The dequeued cell is not an instance of FeedCollectionViewCell.")
        }
        
        let feed = self.feeds![indexPath.row]
        cell.drawingId = feed.drawingId
        cell.userId = (Auth.auth().currentUser?.uid)!
        
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
        
        if feed.userPhotoUrl == "" {
            cell.profilePicture.image = UIImage(named: "Anonymous")
        } else {
            cell.profilePicture.downloadedFrom(url: URL(string: feed.userPhotoUrl)!, completion: { (sucess) in })
        }
        
        cell.userName.text = feed.displayName
        
        let dateFormatter = DateFormatter()
        dateFormatter.locale = Locale(identifier: "fr_CA")
        dateFormatter.setLocalizedDateFormatFromTemplate("YYYYMMMMdhms")
        cell.timestamp.text = dateFormatter.string(from: feed.timestamp)
        cell.timestamp.textAlignment = .right
        
        if (feed.thumbnailPhotoUrl != "") {
            let defaults = UserDefaults.standard
            let data = defaults.data(forKey: feed.drawingId + "_thumbnail")
            
            if data != nil {
                cell.thumbnailPhoto.image = UIImage(data: data!)
            } else {
                cell.thumbnailPhoto.image = UIImage(named: "No-T")
            }
            
            if feed.nsfw {
                cell.setNsfw()
                cell.isNsfw = true
            } else {
                cell.resetNsfw()
                cell.isNsfw = false
            }
        }
        
        return cell
    }
    
    func getFeed() {
        let currentUserId = Auth.auth().currentUser?.uid
        self.usersService?.getFollowingsIds(uid: currentUserId!, callback: { (uids) in
            self.feeds = []
            for uid in uids {
                self.usersService?.getUserDrawings(uid: uid, callback: { (drawings) in
                    for (key, _) in drawings {
                        self.drawingsService!.getDrawingInfo(id: key, callback: { (info) in
                            var exist = false
                            for feed in self.feeds! {
                                if feed.drawingId == info.id {
                                    feed.thumbnailPhotoUrl = info.previewUrl
                                    feed.timestamp = info.timestamp
                                    feed.nsfw = info.nsfw
                                    exist = true
                                    break
                                }
                            }
                            
                            if !exist {
                                self.drawingsService?.getDrawing(name: key, callback: { (drawing) in
                                    if drawing.isPublic {
                                        self.usersService?.getUserInfo(uid: uid, callback: { (userInfo) in
                                            self.feeds?.append(FeedModel(displayName: (userInfo?.displayName)!, userPhotoUrl: (userInfo?.photoUrl)!, timestamp: (info.timestamp), drawingId: info.id, thumbnailPhotoUrl: info.thumbnailUrl, nsfw: info.nsfw))

                                            self.feeds = self.feeds?.sorted(by: { (feed1, feed2) -> Bool in
                                                return feed1.timestamp > feed2.timestamp
                                            })
                                            self.scalingCarousel.reloadData()
                                        })
                                    }
                                })
                            }
                        })
                    }
                })
            }
        })
    }
    
    func getPublic() {
        self.drawingsService?.getDrawings(callback: { (data: [DrawingModel]) in
            self.feeds = []
            for d in data {
                self.drawingsService!.getDrawingInfo(id: d.id, callback: { (info: DrawingInfoModel) in
                    var exist = false
                    for feed in self.feeds! {
                        if feed.drawingId == info.id {
                            feed.thumbnailPhotoUrl = info.thumbnailUrl
                            exist = true
                            break
                        }
                    }
                    if !exist {
                        self.drawingsService?.getDrawing(name: d.id, callback: { (drawing) in
                            if info.owner != "" {
                                self.usersService?.getUserInfo(uid: info.owner, callback: { (userInfo) in
                                    self.feeds?.append(FeedModel(displayName: (userInfo?.displayName)!, userPhotoUrl: (userInfo?.photoUrl)!, timestamp: (info.timestamp), drawingId: info.id, thumbnailPhotoUrl: info.thumbnailUrl, nsfw: info.nsfw))
                                    
                                    self.feeds = self.feeds?.sorted(by: { (feed1, feed2) -> Bool in
                                        return feed1.timestamp > feed2.timestamp
                                    })
                                    self.scalingCarousel.reloadData()
                                })
                            }
                        })
                    }
                })
            }
        })
    }
}
