import Foundation
import UIKit

protocol CreateDrawingViewControllerDelegate {
    func createDrawing(drawing: DrawingModel)
}

class CreateDrawingViewController: UIViewController {
    var delegate: CreateDrawingViewControllerDelegate?
    
    @IBOutlet weak var protected: UISwitch!
    @IBOutlet weak var publicSwitch: UISwitch!
    @IBOutlet weak var password: UITextField!
    @IBOutlet weak var confirmPassword: UITextField!
    @IBOutlet weak var publicText: UILabel!
    @IBOutlet weak var errorText: UILabel!
    @IBOutlet weak var createButton: UIButton!
    var editMode: Bool = false
    var drawing: DrawingModel?
    
    let pubText = "Votre dessin sera visible pour tous les utilisateurs"
    let privText = "Vous serez le seul utilisateur à voir le dessin"
    
    override func viewDidLoad() {
        if editMode {
            self.createButton.setTitle("Mettre à jour le dessin", for: UIControlState.normal)
            self.protected.isOn = (self.drawing?.isProtected)!
            self.publicSwitch.isOn = (self.drawing?.isPublic)!
            
            if (self.drawing?.isProtected)! {
                self.password.text = self.drawing?.password
                self.confirmPassword.text = self.drawing?.password
            }
        }
    }
    
    func setEditMode(drawing: DrawingModel) {
        self.editMode = true
        self.drawing = drawing
    }
    
    @IBAction func passwordChanged(_ sender: Any) {
        if password.text == "" && confirmPassword.text == "" {
            errorText.isHidden = true
        }
    }
    
    @IBAction func confirmPasswordChanged(_ sender: Any) {
        errorText.isHidden = password.text == confirmPassword.text
    }
    
    @IBAction func publicValueChanged(_ sender: UISwitch) {
        if self.publicSwitch.isOn {
            self.publicText.text = self.pubText
        } else {
            self.publicText.text = self.privText
        }
    }
    
    @IBAction func createDrawing(_ sender: Any) {
        let drawing: DrawingModel?
            
        if self.drawing == nil {
            drawing = DrawingModel(id: "", width: 500, height: 300, selectedStrokes: [:], isPublic: self.publicSwitch.isOn)
        } else {
            drawing = self.drawing
            drawing?.isPublic = self.publicSwitch.isOn
            drawing?.password = ""
            drawing?.isProtected = false
        }
        
        if protected.isOn {
            drawing?.isProtected = true
            if password.text != confirmPassword.text || password.text == "" {
                errorText.isHidden = false
                return
            }
            drawing?.password = password.text!
        }
        
        if delegate != nil {
            delegate?.createDrawing(drawing: drawing!)
        }
        
        self.dismiss(animated: true, completion: nil)
    }
}
