import UIKit
import GoogleSignIn
import ChromaColorPicker
import FBSDKShareKit
import FirebaseAuth
import FontAwesome_swift

class DrawingViewController: UIViewController,UIPopoverPresentationControllerDelegate, UIScrollViewDelegate, FBSDKSharingDelegate, ColorPickerViewControllerDelegate, CreateDrawingViewControllerDelegate, DrawViewDelegate {
    func sharer(_ sharer: FBSDKSharing!, didCompleteWithResults results: [AnyHashable : Any]!) {
        print(results)
    }
    
    func sharer(_ sharer: FBSDKSharing!, didFailWithError error: Error!) {
        print("sharer NSError")
        print(error.localizedDescription)
    }
    
    func sharerDidCancel(_ sharer: FBSDKSharing!) {
        print("sharerDidCancel")

    }
    
    var selectColor: UIColor!
    var drawingService: DrawingService?
    var buttonSelectedColor = UIColor(red: 232 / 255, green: 241 / 255, blue: 1, alpha: 1)

    var drawingId: String?
    var info: DrawingInfoModel?
    @IBOutlet weak var drawView: DrawView!
    @IBOutlet weak var scrollView: UIScrollView!
    @IBOutlet weak var widthSliderChanged: UISlider!
    
    @IBOutlet weak var undoButton: UIButton!
    @IBOutlet weak var redoButton: UIButton!
    @IBOutlet weak var resetButton: UIButton!
    
    @IBOutlet weak var lassoButton: UIButton!
    @IBOutlet weak var pencilButton: UIButton!
    @IBOutlet weak var smallEraseButton: UIButton!
    @IBOutlet weak var bigEraseButton: UIButton!
    @IBOutlet weak var colorButton: UIButton!
    @IBOutlet weak var facebookButton: UIButton!
    @IBOutlet weak var duplicateButton: UIButton!
    @IBOutlet weak var cutButton: UIButton!
    @IBOutlet weak var resizeButton: ResizeButton!
    @IBOutlet weak var driveButton: UIButton!
    @IBOutlet weak var nsfwImage: UIImageView!
    @IBOutlet weak var nsfwContainer: UIView!
    @IBOutlet var panGesture: UIPanGestureRecognizer!
    @IBOutlet weak var settingButton: UIButton!
    
    private var isZoomed: Bool = false
    
    private func supportedInterfaceOrientations() -> UIInterfaceOrientationMask {
        return UIInterfaceOrientationMask.landscapeLeft
    }
    
    private func shouldAutorotate() -> Bool {
        return true
    }
    
    override func viewWillDisappear(_ animated: Bool) {
        let value = UIInterfaceOrientation.portrait.rawValue
        UIDevice.current.setValue(value, forKey: "orientation")
    }
    
    override func viewDidLoad() {
        
        facebookButton.setImage( UIImage.fontAwesomeIcon(name: .facebook, textColor: UIColor.darkGray, size: CGSize(width: 50, height: 50)), for: .normal)
        pencilButton.setImage( UIImage.fontAwesomeIcon(name: .pencil, textColor: UIColor.darkGray, size: CGSize(width: 50, height: 50)), for: .normal)
        lassoButton.setImage( UIImage.fontAwesomeIcon(name: .crop, textColor: UIColor.darkGray, size: CGSize(width: 50, height: 50)), for: .normal)
        driveButton.setImage( UIImage.fontAwesomeIcon(name: .google, textColor: UIColor.darkGray, size: CGSize(width: 50, height: 50)), for: .normal)
        cutButton.setImage( UIImage.fontAwesomeIcon(name: .scissors, textColor: UIColor.darkGray, size: CGSize(width: 50, height: 50)), for: .normal)
        duplicateButton.setImage( UIImage.fontAwesomeIcon(name: .copy, textColor: UIColor.darkGray, size: CGSize(width: 50, height: 50)), for: .normal)
        resetButton.setImage( UIImage.fontAwesomeIcon(name: .trash, textColor: UIColor.darkGray, size: CGSize(width: 50, height: 50)), for: .normal)
        settingButton.setImage( UIImage.fontAwesomeIcon(name: .cog, textColor: UIColor.darkGray, size: CGSize(width: 50, height: 50)), for: .normal)
        redoButton.setImage( UIImage.fontAwesomeIcon(name: .longArrowRight, textColor: UIColor.darkGray, size: CGSize(width: 50, height: 50)), for: .normal)
        undoButton.setImage( UIImage.fontAwesomeIcon(name:.longArrowLeft , textColor: UIColor.darkGray, size: CGSize(width: 50, height: 50)), for: .normal)
        
        colorButton.layer.borderWidth = 2
        colorButton.layer.borderColor = UIColor.gray.cgColor

        let value = UIInterfaceOrientation.landscapeRight.rawValue
        UIDevice.current.setValue(value, forKey: "orientation")
        
        self.drawView.delagate = self
        self.drawView.setDrawingId(id: self.drawingId!)
        
        let tap = UITapGestureRecognizer(target: self, action: #selector(doubleTapped))
        tap.numberOfTapsRequired = 2
        self.drawView.addGestureRecognizer(tap)
        
        let scale = UILongPressGestureRecognizer(target: self, action: #selector(resize))
        scale.numberOfTouchesRequired = 1
        self.drawView.addGestureRecognizer(scale)
        
        resizeButton.setMainView(view: self.view!, drawView: self.drawView)
        self.drawView.setParent(parent: self)
        
        let nsfw = UITapGestureRecognizer(target: self, action: #selector(tapNsfw))
        self.nsfwContainer.addGestureRecognizer(nsfw)
        
        self.drawView.drawingService?.getDrawingInfo(id: self.drawingId!, callback: { (info) in
            if info.nsfw {
                self.nsfwImage.image = UIImage(named: "NSFW-stamp")
            } else {
                self.nsfwImage.image = UIImage(named: "NSFW-stamp-BW")
            }
            self.settingButton.isHidden = info.owner != Auth.auth().currentUser?.uid
        })
        
        if(FBSDKAccessToken.current() == nil){
            facebookButton.isHidden = true
        }
        
        if (GIDSignIn.sharedInstance().currentUser == nil) {
            self.driveButton.isHidden = true
        }
    }
    
    public func setDrawingId(id: String) {
        self.drawingId = id
    }
    
    public func createDrawing(drawing: DrawingModel) {
        self.drawView.drawingService?.updateDrawing(drawing: drawing)
    }

    @IBAction func settingButton(_ sender: UIButton) {
        let createDrawingViewController = ViewHelper.getView(Views.CreateDrawingViewController) as! CreateDrawingViewController
        createDrawingViewController.delegate = self
        createDrawingViewController.setEditMode(drawing: self.drawView.model)
        
        createDrawingViewController.modalPresentationStyle = .popover
        if let popoverController = createDrawingViewController.popoverPresentationController {
            popoverController.sourceView = sender
            popoverController.sourceRect = sender.bounds
            popoverController.permittedArrowDirections = .any
            popoverController.delegate = self
        }
        present(createDrawingViewController, animated: true, completion: nil)
    }
    
    @IBAction func driveButton(_ sender: UIButton) {
        let alertController = UIAlertController(title: "Sauvegarder sur Google Drive?", message: "", preferredStyle: .alert)
        let cancelAction = UIAlertAction(title: "Annuler", style: .default)
        let saveAction = UIAlertAction(title: "Sauvgarder", style: .default) { (action) -> Void in
            self.drawView?.uploadToGoogleDrive()
        }
        alertController.addAction(cancelAction)
        alertController.addAction(saveAction)
        
        self.present(alertController, animated: true, completion: nil)
    }
    
    @IBAction func cutButton(_ sender: UIButton) {
       self.drawView.cutSelection()
    }
    
    @IBAction func duclicateButton(_ sender: UIButton) {
        self.drawView.duplicateSelection()
    }
    
    @IBAction func facebookButton(_ sender: UIButton) {
        self.facebookAlert()
    }
    
    func facebookAlert() {
        let facebookController = UIAlertController(title: "Partager sur Facebook", message: "Ajouter un message:", preferredStyle: .alert)
        facebookController.addTextField { (textField) in
            textField.text = "Votre message"
        }
        let shareAction = UIAlertAction(title: "Partager", style: .default) { (action) -> Void in
            
            self.facebookShare(text: (facebookController.textFields?.first?.text!)!)
        }
        let cancelAction = UIAlertAction(title: "Annuler", style: .default)
        facebookController.addAction(cancelAction)

        facebookController.addAction(shareAction)

        self.present(facebookController, animated: true, completion: nil)
    }
    
    func facebookShare(text: String) {
        UIGraphicsBeginImageContextWithOptions(self.drawView.bounds.size, self.drawView.isOpaque, 0.0)
        self.drawView.drawHierarchy(in: self.drawView.bounds, afterScreenUpdates: false)
        let screenshot = UIGraphicsGetImageFromCurrentImageContext()
        UIGraphicsEndImageContext()
        let sharePhoto = FBSDKSharePhoto()
        sharePhoto.caption = text
        sharePhoto.image = screenshot as UIImage?
        let content = FBSDKSharePhotoContent()
        content.photos = [sharePhoto]
        
        let accesToken = FBSDKAccessToken.current()
        if accesToken != nil {
            FBSDKShareAPI.share(with: content, delegate: self)
        } else {
            print("Not loggedIn")
        }
    }
    @IBAction func resetButton(_ sender: UIButton) {
        self.drawView.clear()
        self.drawView.setNeedsDisplay()
    }
    
    @IBAction func undoButton(_ sender: UIButton) {
        self.drawView.undo()
        self.drawView.setNeedsDisplay()
    }
   
    @IBAction func colorButton(_ sender: UIButton) {
        let colorPickerViewController = storyboard?.instantiateViewController(withIdentifier: "ColorPicker") as! ColorPickerViewController
        
        colorPickerViewController.delegate = self
        
        colorPickerViewController.modalPresentationStyle = .popover
        if let popoverController = colorPickerViewController.popoverPresentationController {
            popoverController.sourceView = sender
            popoverController.sourceRect = sender.bounds
            popoverController.permittedArrowDirections = .any
            popoverController.delegate = self
        }
        colorPickerViewController.currentColor = self.selectColor
        present(colorPickerViewController, animated: true, completion: nil)
    }
    
    func setColor(selectedColor: UIColor) {
        self.selectColor = selectedColor
        drawView.setColor(color: selectedColor)
        self.colorButton.backgroundColor = self.selectColor
    }
    
    func adaptivePresentationStyleForPresentationController(controller: UIPresentationController!) -> UIModalPresentationStyle {
        return .fullScreen
    }
    
    func presentationController(controller: UIPresentationController!, viewControllerForAdaptivePresentationStyle style: UIModalPresentationStyle) -> UIViewController! {
        return UINavigationController(rootViewController: controller.presentedViewController)
    }
    
    @IBAction func redoButton(_ sender: UIButton) {

        self.drawView.redo()
        self.drawView.setNeedsDisplay()
    }
    
    @IBAction func widthSliderChanged(_ sender: UISlider) {
        self.drawView.setWidth(newWidth: sender.value)
    }
    
    @IBAction func toolsButtonUpEvent(_ sender: UIButton) {
        self.clearToolButton();
        self.backgroudToColor(button: sender, color: self.buttonSelectedColor)
        self.drawView.setActiveTool(activeTool: ViewHelper.stringToTool(tool: sender.accessibilityIdentifier!) )
    }

    @IBAction func buttonDownEvent(_ sender: UIButton) {
        self.backgroudToColor(button: sender, color: self.buttonSelectedColor)
    }

    func clearToolButton() {
        self.drawView.clearSelection()
        self.drawView.selection.emptySelection()
        self.drawView.setNeedsDisplay()
        self.backgroudToColor(button: self.lassoButton, color: UIColor.white)
        self.backgroudToColor(button: self.pencilButton, color: UIColor.white)
        self.backgroudToColor(button: self.smallEraseButton, color: UIColor.white)
        self.backgroudToColor(button: self.bigEraseButton, color: UIColor.white)
    }
    
    func backgroudToColor(button: UIButton, color: UIColor) {
        button.backgroundColor = color
    }
    
    @objc func doubleTapped() {
        self.drawView.isInGesture = true
        self.drawView.transform = isZoomed ? self.drawView.transform.scaledBy(x: 0.5, y: 0.5) :self.drawView.transform.scaledBy(x: 2, y: 2)
        if isZoomed {
            self.drawView.frame.origin.x = 11
            self.drawView.frame.origin.y = 141
        }
        isZoomed = !isZoomed
        self.drawView.isInGesture = false
    }
    
    @objc func resize() {
        if !self.drawView.inAction {
            self.resizeButton.updatePosition()
            self.resizeButton.isHidden = false
        }
    }
    
    @objc func tapNsfw() {
        self.drawView.toggleNotSafeForWork()
        if self.drawView.info.nsfw {
            self.nsfwImage.image = UIImage(named: "NSFW-stamp")
        } else {
            self.nsfwImage.image = UIImage(named: "NSFW-stamp-BW")
        }
    }
    
    @IBAction func handlePan(recognizer:UIPanGestureRecognizer) {
        if panGesture.state == .began || panGesture.state == .changed {
            
            let translation = panGesture.translation(in: self.view)
            // note: 'view' is optional and need to be unwrapped
            panGesture.view!.center = CGPoint(x: panGesture.view!.center.x + translation.x, y: panGesture.view!.center.y + translation.y)
            panGesture.setTranslation(CGPoint.zero, in: self.view)
        }
    }
    
    func onDrawingPermissionChanged(model: DrawingModel) {
        self.navigationController?.popToRootViewController(animated: true)
        let alertController = UIAlertController(title: "Le créateur du dessin a changé l'accessibilité du dessin", message: "", preferredStyle: .alert)
        let action = UIAlertAction(title: "Ok", style: .default)
        alertController.addAction(action)
        
        self.parent?.present(alertController, animated: true, completion: nil)
    }
}
