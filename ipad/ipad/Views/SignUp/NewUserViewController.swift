import Firebase
import FirebaseAuth
import FirebaseDatabase
import UIKit

class NewUserViewController: UIViewController, UIImagePickerControllerDelegate, UINavigationControllerDelegate {
    @IBOutlet weak var emailTextField: UITextField!
    @IBOutlet weak var usernameTextField: UITextField!
    @IBOutlet weak var passwordTextField: UITextField!
    @IBOutlet weak var confirmPasswordTextField: UITextField!
    
    private let displayNamePath: String = "displayNames"
    private let usersService = UsersService()
    private let storageService = StorageService()
    
    @IBAction func confirmUsernameButton(_ sender: UIButton) {
        if passwordTextField.text != confirmPasswordTextField.text {
            let alertController = ViewHelper.getAlert("Les mots de passe ne correspondent pas, s'il vous plait entrez deux mots de passes valides et identiques!")
            self.present(alertController, animated: true, completion: nil)
        }
        
        if (usernameTextField.text?.trimmingCharacters(in: .whitespaces).isEmpty)! {
            let alertController = ViewHelper.getAlert("Le nom d'utilisateur est vide, veuillez en entrer un valide!")
            self.present(alertController, animated: true, completion: nil)
        }
        
        Auth.auth().createUser(withEmail: emailTextField.text!, password: passwordTextField.text!) { (user, error) in
            if error != nil {
                let alertController = ViewHelper.getAlert(error!.localizedDescription)
                self.present(alertController, animated: true, completion: nil)
                return;
            }
            
            self.usersService.setupUser(username: self.usernameTextField.text!, imageUrl: "", callback: { (error) in
                let alertController = ViewHelper.getAlert(error!.localizedDescription)
                self.present(alertController, animated: true, completion: nil)
            })
                
            self.navigationController?.popToRootViewController(animated: true)
        }
    }
}
