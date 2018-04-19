import UIKit
import FirebaseAuth
import FontAwesome_swift
class TutorialViewController: UIViewController {

    @IBOutlet weak var previousButton: UIButton!
    @IBOutlet weak var nextButton: UIButton!
    @IBOutlet weak var finishTutorialButton: UIButton!
    @IBOutlet weak var tutorialImages: UIImageView!
    
    private var userService: UsersService?
    private var drawingService: DrawingService?
    let tutorial: [UIImage] = [#imageLiteral(resourceName: "TUT-INTRO.png"), #imageLiteral(resourceName: "TUT-RES.png"), #imageLiteral(resourceName: "TUT-ZOOM.png"), #imageLiteral(resourceName: "TUT-TRAIT.png"),#imageLiteral(resourceName: "TUT-SELECTION.png"),#imageLiteral(resourceName: "TUT-ORANGE.png"), #imageLiteral(resourceName: "TUT-MOVE.png"), #imageLiteral(resourceName: "TUT-SEL-REZ.png"), #imageLiteral(resourceName: "TUT-DUPLI.png"), #imageLiteral(resourceName: "TUT-ER.png"),#imageLiteral(resourceName: "TUT-DESSIN.png"),#imageLiteral(resourceName: "TUT-SLIDER.png"), #imageLiteral(resourceName: "TUT-COULEUR.png"),#imageLiteral(resourceName: "TUT-NSFW.png"), #imageLiteral(resourceName: "TUT-GOOGLE.png"), #imageLiteral(resourceName: "TUT-FACEBOOK.png")]
    
    var firstTime: Bool = false
    var checkedIndex: [Int] = [0]
    var currentImageIndex = 0
    
    public var drawingModel: DrawingModel? = nil
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        let value = UIInterfaceOrientation.landscapeRight.rawValue
        UIDevice.current.setValue(value, forKey: "orientation")
        
        self.userService = UsersService()
        self.drawingService = DrawingService()
        self.hasSeenTutorial()
        self.updateButton()

        tutorialImages.image = tutorial[currentImageIndex]
        
        previousButton.setImage( UIImage.fontAwesomeIcon(name: .arrowCircleOLeft, textColor: UIColor.darkGray, size: CGSize(width: 120, height: 120)), for: .normal)
        nextButton.setImage( UIImage.fontAwesomeIcon(name: .arrowCircleORight, textColor: UIColor.darkGray, size: CGSize(width: 120, height: 120)), for: .normal)
        }
    
    @IBAction func previousButton(_ sender: Any) {
        currentImageIndex -= 1
        tutorialImages.image = tutorial[currentImageIndex]
        if(!checkedIndex.contains(currentImageIndex)){
            checkedIndex.append(currentImageIndex)
        }

        self.updateButton()
    }
    
    @IBAction func nextButton(_ sender: Any) {
        currentImageIndex += 1
        tutorialImages.image = tutorial[currentImageIndex]
        if(!checkedIndex.contains(currentImageIndex)){
            checkedIndex.append(currentImageIndex)
        }
        self.updateButton()
    }
    
    @IBAction func finishTutorialButton(_ sender: Any) {
        guard let uid = Auth.auth().currentUser?.uid else { return; }
        self.userService?.getUserInfo(uid: uid, callback: { (user) in
            self.userService?.setIpadTutorialSeen(uid: uid, callback: { (user) in })
        })
        self.navigationController?.popViewController(animated: true)
    }
    
    func updateButton() {
        if(currentImageIndex == 0){
            self.previousButton.isHidden = true
            self.nextButton.isHidden = false
        } else if (currentImageIndex == tutorial.count - 1) {
            self.nextButton.isHidden = true
            self.previousButton.isHidden = false
        } else {
            self.previousButton.isHidden = false
            self.nextButton.isHidden = false
        }
        
        if(checkedIndex.count == tutorial.count && !self.firstTime){
            self.finishTutorialButton.isHidden = false
        } else {
            self.finishTutorialButton.isHidden = true
        }
    }
    
    func hasSeenTutorial() {
        guard let uid = Auth.auth().currentUser?.uid else { return; }
        self.userService?.getUserInfo(uid: uid, callback: { (user) in
            self.firstTime = (user?.hasSeenIpadTutorial)!
        })
    }
    
}
