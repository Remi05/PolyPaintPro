import UIKit
import FirebaseAuth
import ScalingCarousel
import FontAwesome_swift

class DrawingListViewController: UIViewController, UICollectionViewDataSource, UICollectionViewDelegate, UIPopoverPresentationControllerDelegate, CreateDrawingViewControllerDelegate, RecentlyModifiedDataSourceDelegate {
    var drawingService: DrawingService?
    var userService: UsersService?
    var drawings = [DrawingInfoModel]()
    
    @IBOutlet weak var list: UICollectionView!
    @IBOutlet weak var scalingCarousel: ScalingCarouselView!
    @IBOutlet weak var tutorialButton: UIButton!
    var recentlyModifiedDataSource: RecentlyModifiedDataSource?
    
    @IBOutlet weak var recentlyLabel: UILabel!
    @IBOutlet weak var allDrawingLabel: UILabel!
    @IBOutlet weak var allDrawingList: UICollectionView!
    
    
    @IBAction func tutorialButton(_ sender: Any) {
        self.goToTutorial()
    }
    
    func collectionView(_ collectionView: UICollectionView, numberOfItemsInSection section: Int) -> Int {
        return self.drawings.count
    }
    
    func collectionView(_ collectionView: UICollectionView, cellForItemAt indexPath: IndexPath) -> UICollectionViewCell {
        let cellIdentifier = "DrawingListViewCell"
        guard let cell = collectionView.dequeueReusableCell(withReuseIdentifier: cellIdentifier, for: indexPath) as? DrawingListViewCell else {
            fatalError("The dequeued cell is not an instance of DrawingsListViewCell.")
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
        self.drawingService?.getDrawingOnce(name: drawings[indexPath.row].id, callback: { (drawing) in
            if drawing.isProtected {
                let passwordController = UIAlertController(title: "Ce dessin est protégé", message: "Entrer le mot de passe:", preferredStyle: .alert)
                passwordController.addTextField(configurationHandler: { (textField) in
                    textField.isSecureTextEntry = true
                })
                let confirmAction = UIAlertAction(title: "Confirmer", style: .default) { (action) -> Void in
                    if passwordController.textFields?.first?.text! == drawing.password {
                        self.goToDrawingView(id: drawing.id)
                    } else {
                        self.popupInvalidPassword()
                    }
                }
                let cancelAction = UIAlertAction(title: "Annuler", style: .default)
                passwordController.addAction(cancelAction)
                passwordController.addAction(confirmAction)
                
                self.present(passwordController, animated: true, completion: nil)
            } else {
                self.goToDrawingView(id: self.drawings[indexPath.row].id)
            }
        })
    }
    
    func onDrawingClick(drawing: DrawingModel) {
        if drawing.isProtected {
            let passwordController = UIAlertController(title: "Ce dessin est protégé", message: "Entrer le mot de passe:", preferredStyle: .alert)
            passwordController.addTextField(configurationHandler: { (textField) in
                textField.isSecureTextEntry = true
            })
            let confirmAction = UIAlertAction(title: "Confirmer", style: .default) { (action) -> Void in
                if passwordController.textFields?.first?.text! == drawing.password {
                    self.goToDrawingView(id: drawing.id)
                } else {
                    self.popupInvalidPassword()
                }
            }
            let cancelAction = UIAlertAction(title: "Annuler", style: .default)
            passwordController.addAction(cancelAction)
            passwordController.addAction(confirmAction)
            
            self.present(passwordController, animated: true, completion: nil)
        } else {
            self.goToDrawingView(id: drawing.id)
        }
    }
    
    func popupInvalidPassword() {
        let alertController = UIAlertController(title: "Mot de passe invalide", message: "", preferredStyle: .alert)
        let action = UIAlertAction(title: "Ok", style: .default)
        
        alertController.addAction(action)
        
        self.parent?.present(alertController, animated: true, completion: nil)
    }
    
  
    func goToTutorial(){
        let controller = self.storyboard?.instantiateViewController(withIdentifier: "TutorialViewController") as! TutorialViewController
        self.navigationController?.pushViewController(controller, animated: true)
    }
    
    func goToDrawingView(id: String) {
        let controller = ViewHelper.getView(Views.DrawingViewController) as! DrawingViewController
        controller.setDrawingId(id: id)
        
        if self.navigationController != nil {
            self.navigationController?.pushViewController(controller, animated: true)
        }
    }
    
    override func viewDidLoad() {
        drawingService = DrawingService()
        userService = UsersService()
        self.recentlyModifiedDataSource = RecentlyModifiedDataSource(self.scalingCarousel)
        self.recentlyModifiedDataSource?.delegate = self
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(animated)
        if Auth.auth().currentUser == nil {
            self.allDrawingLabel.layer.position = self.recentlyLabel.layer.position
            self.allDrawingList.frame.origin = CGPoint(x: 20, y: 177)
            self.allDrawingList.frame.size = CGSize(width: 728, height: 783)
            self.recentlyLabel.isHidden = true
            self.scalingCarousel.isHidden = true
        } else {
            self.allDrawingLabel.frame.origin = CGPoint(x: 20, y: 375)
            self.allDrawingList.frame.origin = CGPoint(x: 20, y: 411)
            self.allDrawingList.frame.size = CGSize(width: 728, height: 549)
            self.recentlyLabel.isHidden = false
            self.scalingCarousel.isHidden = false
        }
        
        DispatchQueue.global(qos: .userInteractive).async {
            self.getDrawings()
            self.recentlyModifiedDataSource?.getDrawings()
        }
    }
    
    func getDrawings() {
        drawingService?.getDrawings(callback: { (data: [DrawingModel]) in
            for d in data {
                self.drawingService!.getDrawingInfo(id: d.id, callback: { (info: DrawingInfoModel) in
                    if d.isPublic || info.owner == Auth.auth().currentUser?.uid {
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
                        DispatchQueue.main.async {
                            self.list.reloadData()
                        }
                    }
                })
            }
        })
    }
    
    @IBAction func touchCreateDrawing(_ sender: UIButton) {
        if Auth.auth().currentUser != nil {
            self.userService?.getUserInfo(uid: (Auth.auth().currentUser?.uid)!, callback: { (user) in
                if(!((user?.hasSeenIpadTutorial)!)) {
                    self.goToTutorial()
                } else {
                    let createDrawingViewController = ViewHelper.getView(Views.CreateDrawingViewController) as! CreateDrawingViewController
                    createDrawingViewController.delegate = self
                
                    createDrawingViewController.modalPresentationStyle = .popover
                    if let popoverController = createDrawingViewController.popoverPresentationController {
                        popoverController.sourceView = sender
                        popoverController.sourceRect = sender.bounds
                        popoverController.permittedArrowDirections = .any
                        popoverController.delegate = self
                    }
                    self.present(createDrawingViewController, animated: true, completion: nil)
                }
            }) 
        } else {
            self.createDrawing(drawing: DrawingModel(id: "", width: 500, height: 300, selectedStrokes: [:], isPublic: true))
        }
    }
    
    func createDrawing(drawing: DrawingModel) {
        guard let key = drawingService?.createDrawing(drawing: drawing) else { return }
        let controller = ViewHelper.getView(Views.DrawingViewController) as! DrawingViewController
        controller.setDrawingId(id: key)
        self.navigationController?.pushViewController(controller, animated: true)
    }
}
