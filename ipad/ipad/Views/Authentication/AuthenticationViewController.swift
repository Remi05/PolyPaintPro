import Firebase
import FirebaseAuth
import UIKit
import FacebookLogin
import FBSDKLoginKit
import GoogleSignIn
import GoogleAPIClientForREST
import GTMSessionFetcher

class AuthenticationViewController: UIViewController, FBSDKLoginButtonDelegate, GIDSignInUIDelegate, GIDSignInDelegate {
    private var usersService: UsersService?
    let loginManager = FBSDKLoginManager()
    
    //MARK: Properties
    @IBOutlet weak var usernameTextField: UITextField!
    @IBOutlet weak var passwordTextField: UITextField!
    @IBOutlet weak var fbview: UIStackView!
    @IBOutlet weak var googleview: UIStackView!
    
    override func viewDidLoad() {
        let loginButton = FBSDKLoginButton()
        fbview.addArrangedSubview(loginButton)
        loginButton.delegate = self
        loginButton.publishPermissions = ["publish_actions"]
        
        let googleSignInButton = GIDSignInButton()
        googleview.addArrangedSubview(googleSignInButton)
        GIDSignIn.sharedInstance().uiDelegate = self
        GIDSignIn.sharedInstance().delegate = self
        GIDSignIn.sharedInstance().scopes = [kGTLRAuthScopeDrive]
        
        self.usersService = UsersService()
    }
    
    override func viewWillAppear(_ animated: Bool) {
        if Auth.auth().currentUser != nil {
            self.setupOnlineMode()
        }
    }
    
    func setupOnlineMode() {
        var views: [UIViewController] = []
        views.append(ViewHelper.getView(.FeedViewController))
        views.append((self.tabBarController?.viewControllers![1])!)
        views.append(ViewHelper.getView(.SearchViewController))
        views.append(ViewHelper.getView(.ConversationsViewController))
        views.append(ViewHelper.getView(.ProfileViewController))
        self.tabBarController?.setViewControllers(views, animated: true)
    }
    
    func setupOfflineMode() {
        var views: [UIViewController] = []
        views.append((self.tabBarController?.viewControllers![1])!)
        views.append((self.tabBarController?.viewControllers![4])!)
        self.tabBarController?.setViewControllers(views, animated: true)
    }
    
    func sign(_ signIn: GIDSignIn!, didSignInFor user: GIDGoogleUser!, withError error: Error?) {
        if let error = error {
            print(error.localizedDescription)
            return
        }
        
        guard let authentication = user.authentication else { return }
        let credential = GoogleAuthProvider.credential(withIDToken: authentication.idToken,
                                                       accessToken: authentication.accessToken)

        Auth.auth().signIn(with: credential) { (user, error) in
            if let error = error {
                let alertController = ViewHelper.getAlert(error.localizedDescription)
                self.present(alertController, animated: true, completion: nil)
                return
            }
            
            self.usersService?.userIsSetup(uid: (user?.uid)!, callback: { (userExist) in
                if !userExist! {
                    self.usersService?.setupUser(username: (user?.displayName)!, imageUrl: (user?.photoURL?.absoluteString)!, callback: { (error) in
                        let alertController = ViewHelper.getAlert(error!.localizedDescription)
                        self.present(alertController, animated: true, completion: nil)
                    })
                }
            })
            
            self.validateUserData()
            self.setupOnlineMode()
        }
    }
    
    func sign(_ signIn: GIDSignIn!, didDisconnectWith user: GIDGoogleUser!, withError error: Error!) {
        do {
            try Auth.auth().signOut()
            GIDSignIn.sharedInstance().signOut()
            let controller = ViewHelper.getView(Views.AuthenticationViewController)
            self.present(controller, animated: false, completion: nil)
        } catch let error as NSError {
            let alertController = ViewHelper.getAlert(error.localizedDescription)
            self.present(alertController, animated: true, completion: nil)
        }
    }
    
    func loginButton(_ loginButton: FBSDKLoginButton!, didCompleteWith result: FBSDKLoginManagerLoginResult!, error: Error!) {
        if let error = error {
            print(error.localizedDescription)
            return
        }
        
        if FBSDKAccessToken.current() == nil {
            print("Aucun Utilisateur.")
            return
        }
        
        let credential = FacebookAuthProvider.credential(withAccessToken: FBSDKAccessToken.current().tokenString)
        Auth.auth().signIn(with: credential) { (user, error) in
            if error != nil {
                let alertController = ViewHelper.getAlert(error!.localizedDescription)
                self.present(alertController, animated: true, completion: nil)
                return
            }
            self.usersService?.userIsSetup(uid: (user?.uid)!, callback: { (userExist) in
                if !userExist! {
                    self.usersService?.setupUser(username: (user?.displayName)!, imageUrl: (user?.photoURL?.absoluteString)!, callback: { (error) in
                        let alertController = ViewHelper.getAlert(error!.localizedDescription)
                        self.present(alertController, animated: true, completion: nil)
                    })
                }
            })
            
            self.validateUserData()
            self.setupOnlineMode()
        }
    }
    
    //MARK: Actions
    @IBAction func connectionButton(_ sender: UIButton) {     
        Auth.auth().signIn(withEmail: usernameTextField.text!, password: passwordTextField.text!) { (user, error) in
            if error != nil {
                let alertController = ViewHelper.getAlert(error!.localizedDescription)
                self.present(alertController, animated: true, completion: nil)
                return
            }
            
            self.validateUserData()
            self.setupOnlineMode()
        }
    }
    
    @IBAction func newAccountButton(_ sender: UIButton) {
        let controller = ViewHelper.getView(Views.NewUserViewController)
        self.navigationController?.pushViewController(controller, animated: true)
    }
    
    private func validateUserData() {
        let messagingService = MessagingService()
        guard let uid = Auth.auth().currentUser?.uid else { return; }
        messagingService.joinConversation(userId: uid, conversationWithId: MessagingService.PublicChannelId)
    }
    
    func loginButtonDidLogOut(_ loginButton: FBSDKLoginButton!) {
        do {
            try Auth.auth().signOut()
            loginManager.logOut()
            FBSDKAccessToken.setCurrent(nil)
            FBSDKProfile.setCurrent(nil)
            self.setupOfflineMode()
        } catch let error as NSError {
            let alertController = ViewHelper.getAlert(error.localizedDescription)
            self.present(alertController, animated: true, completion: nil)
        }
    }
}
