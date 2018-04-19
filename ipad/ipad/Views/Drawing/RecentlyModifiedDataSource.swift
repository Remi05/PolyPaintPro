import UIKit
import ScalingCarousel
import FirebaseAuth

protocol RecentlyModifiedDataSourceDelegate {
    func onDrawingClick(drawing: DrawingModel)
}

class RecentlyModifiedDataSource: NSObject, UICollectionViewDataSource, UICollectionViewDelegate {
    var drawings = [DrawingInfoModel]()
    var collection: ScalingCarouselView
    var delegate: RecentlyModifiedDataSourceDelegate?
    
    let usersService = UsersService()
    let drawingsService = DrawingService()
    
    init(_ collection: ScalingCarouselView) {
        self.collection = collection
        super.init()
        self.collection.delegate = self
        self.collection.dataSource = self
    }
    
    func collectionView(_ collectionView: UICollectionView, numberOfItemsInSection section: Int) -> Int {
        return self.drawings.count
    }
    
    func collectionView(_ collectionView: UICollectionView, cellForItemAt indexPath: IndexPath) -> UICollectionViewCell {
        let cellIdentifier = "RecentlyModifiedViewCell"
        guard let cell = collectionView.dequeueReusableCell(withReuseIdentifier: cellIdentifier, for: indexPath) as? RecentlyModifiedViewCell else {
            fatalError("The dequeued cell is not an instance of RecentlyModifiedViewCell.")
        }
        
        let drawing = self.drawings[indexPath.row]
        if (drawing.thumbnailUrl != "") {
            let defaults = UserDefaults.standard
            let data = defaults.data(forKey: drawing.id + "_thumbnail")
            
            if data != nil {
                cell.image.image = UIImage(data: data!)
            } else {
                cell.image.image = UIImage(named: "No-T")
            }
            
            if drawing.nsfw {
                cell.setNsfw()
            } else if !drawing.nsfw {
                cell.resetNsfw()
            }
        } else {
            cell.image.image = UIImage(named: "No-T")
        }
        
        cell.setNeedsDisplay()
        cell.layoutIfNeeded()
        
        return cell
    }
      
    func collectionView(_ collectionView: UICollectionView, didSelectItemAt indexPath: IndexPath) {
        self.drawingsService.getDrawingOnce(name: drawings[indexPath.row].id, callback: { (drawing) in
            self.delegate?.onDrawingClick(drawing: drawing)
        })
    }
    
    func getDrawings() {
        if (Auth.auth().currentUser?.uid != nil) {
            self.usersService.getUserDrawings(uid: (Auth.auth().currentUser?.uid)!, callback: { (data: [String:String]) in
                for (key, _) in data {
                    self.drawingsService.getDrawingInfo(id: key, callback: { (info: DrawingInfoModel) in
                        var exist = false
                        for drawing in self.drawings {
                            if drawing.id == info.id {
                                drawing.thumbnailUrl = info.thumbnailUrl
                                drawing.nsfw = info.nsfw
                                drawing.timestamp = info.timestamp
                                exist = true
                                break
                            }
                        }
                        if !exist {
                            self.drawings.append(info)
                        }
                        
                        self.drawings = self.drawings.sorted(by: { (drawing1, drawing2) -> Bool in
                            return drawing1.timestamp > drawing2.timestamp
                        })
                        
                        DispatchQueue.main.async {
                            self.collection.reloadData()
                        }
                    })
                }
            })
        } else {
            self.drawings = []
        }
    }
}
