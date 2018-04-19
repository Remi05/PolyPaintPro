import Foundation
import Firebase
import FirebaseDatabase
import FirebaseAuth
import GTMSessionFetcher

class UsersService {
    private let ref: DatabaseReference!
    private let usersPath: String = "users"
    private let displayNamePath: String = "displayNames"
    private let profilePath: String = "profile"
    private let drawingsPath: String = "drawings"
    private let followersPath: String = "followers"
    private let followingsPath: String = "followings"

    init() {
        ref = Database.database().reference()
    }
    
    public func getUsers(callback: @escaping ((Array<UserModel>) -> Void)) {
        guard let uid = Auth.auth().currentUser?.uid else { return }
        ref.child(self.displayNamePath).observeSingleEvent(of: .value, with: { (snapshot) in
            let value = snapshot.value as? [String:String] ?? [:]
            var users: Array<UserModel> = []
            for (key, value) in value {
                if (key == uid) { continue }
                users.append(UserModel(
                    displayName: value,
                    uid: key,
                    isSelected: false,
                    photoUrl: "",
                    hasSeenIpadTutorial: true))
            }
            callback(users)
        }) { (error) in
            print(error.localizedDescription)
        }
    }
    
    public func getUserDisplayName(uid: String, callback: @escaping ((String) -> Void)) {
        ref.child(self.displayNamePath).child(uid).observeSingleEvent(of: .value, with: { (snapshot) in
            let displayName = snapshot.value as? String ?? ""
            callback(displayName)
        }) { (error) in
            print(error.localizedDescription)
        }
    }
    
    public func setupUser(username: String, imageUrl: String, callback: @escaping ((Error?) -> Void)) {
        guard let user = Auth.auth().currentUser else { return }
        
        let changeRequest = user.createProfileChangeRequest()
        changeRequest.displayName = username
        changeRequest.commitChanges() { error in
            if error != nil {
                callback(error)
            }
        }
        
        var post = [ (user.uid): username ] as [String : Any]
        ref.child(self.displayNamePath).updateChildValues(post)
        post = [ "displayName": username,
                 "photoUrl": imageUrl,
                 "hasSeenIpadTutorial": false] as [String : Any]
        ref.child(self.usersPath).child(user.uid).child(self.profilePath).updateChildValues(post)
    }
    
    public func setIpadTutorialSeen(uid: String, callback: @escaping ((String) -> Void)) {
        guard let user = Auth.auth().currentUser else { return }
        let post = ["hasSeenIpadTutorial": true] as [String : Any]
        ref.child(self.usersPath).child(user.uid).child(self.profilePath).updateChildValues(post)
    }
    
    
    public func userIsSetup(uid: String, callback: @escaping ((Bool?) -> Void)) {
        ref.child(self.displayNamePath).child(uid).observeSingleEvent(of: .value, with: { (snapshot) in
            callback(snapshot.exists())
        })
    }
    
    public func getUserInfo(uid: String, callback: @escaping ((UserModel?) -> Void)) {
        ref.child(self.usersPath).child(uid).child(self.profilePath).observeSingleEvent(of: .value, with: { (snapshot) in
            let value = snapshot.value as? [String:AnyObject] ?? [:]
            let info = UserModel(
                displayName: value["displayName"] as? String ?? "",
                uid: uid,
                isSelected: false,
                photoUrl: value["photoUrl"] as? String ?? "",
                hasSeenIpadTutorial: value["hasSeenIpadTutorial"] as? Bool ?? false
            )
            callback(info)
        }) { (error) in
            print(error.localizedDescription)
        }
    }
    
    public func getUserPhotoUrl(uid: String, callback: @escaping ((String) -> Void)) {
        ref.child(self.usersPath).child(uid).child(self.profilePath).observeSingleEvent(of: .value, with: { (snapshot) in
            let value = snapshot.value as? [String:AnyObject] ?? [:]
            let photoUrl = value["photoUrl"] as? String ?? ""
            callback(photoUrl)
        }) { (error) in
            print(error.localizedDescription)
        }
    }
    
    public func getFollowersCount(uid: String, callback: @escaping ((Int) -> Void)) {
        ref.child(self.usersPath).child(uid).child(self.followersPath).observeSingleEvent(of: .value, with: { (snapshot) in
            let value = snapshot.value as? [String:Bool] ?? [:]
            callback(value.count)
        }) { (error) in
            print(error.localizedDescription)
        }
    }
    
    public func getFollowingsCount(uid: String, callback: @escaping ((Int) -> Void)) {
        ref.child(self.usersPath).child(uid).child(self.followingsPath).observeSingleEvent(of: .value, with: { (snapshot) in
            let value = snapshot.value as? [String:Bool] ?? [:]
            callback(value.count)
        }) { (error) in
            print(error.localizedDescription)
        }
    }
    
    public func getFollowingsIds(uid:String, callback: @escaping ((Array<String>) -> Void)) {
        ref.child(self.usersPath).child(uid).child(self.followingsPath).observe(.value, with: { (snapshot) in
            let value = snapshot.value as? [String:Bool] ?? [:]
            
            var uids: Array<String> = []
            for (key, _) in value {
                uids.append(key)
            }
            
            callback(uids)
        }) { (error) in
            print(error.localizedDescription)
        }
    }
    
    public func getUserDrawingsCount(uid: String, callback: @escaping ((Int) -> Void)) {
        ref.child(self.usersPath).child(uid).child(self.drawingsPath).observeSingleEvent(of: .value, with: { (snapshot) in
            let value = snapshot.value as? [String:String] ?? [:]
            callback(value.count)
        }) { (error) in
            print(error.localizedDescription)
        }
    }
    
    public func getUserDrawings(uid: String, callback: @escaping (([String:String]) -> Void)) {
        ref.child(self.usersPath).child(uid).child(self.drawingsPath).observe(.value, with: { (snapshot) in
            let value = snapshot.value as? [String:String] ?? [:]
            callback(value)
        }) { (error) in
            print(error.localizedDescription)
        }
    }
    
    public func followUser(currentUserUid: String, followedUserUid: String) {
        var post = [ currentUserUid: true ] as [String : Bool]
        ref.child(self.usersPath).child(followedUserUid).child(self.followersPath).updateChildValues(post)
        post = [ followedUserUid: true] as [String : Bool]
        ref.child(self.usersPath).child(currentUserUid).child(self.followingsPath).updateChildValues(post)
    }
    
    public func unfollowUser(currentUserUid: String, unfollowedUserUid: String) {
        ref.child(self.usersPath).child(unfollowedUserUid).child(self.followersPath).child(currentUserUid).setValue(nil)
        ref.child(self.usersPath).child(currentUserUid).child(self.followingsPath).child(unfollowedUserUid).setValue(nil)
    }
    
    public func isFollower(currentUid: String, otherUserUid: String, callback: @escaping ((Bool?) -> Void)) {
        ref.child(self.usersPath).child(otherUserUid).child(self.followersPath).child(currentUid).observeSingleEvent(of: .value, with: { (snapshot) in
            callback(snapshot.exists())
        })
    }
    
    public func addDrawingToUser(drawingId: String) {
        guard let uid = Auth.auth().currentUser?.uid else { return }
        let post = [ drawingId: drawingId] as [String : String]
        ref.child(self.usersPath).child(uid).child(self.drawingsPath).updateChildValues(post)
    }
    
    public func removeDrawingToUser(drawingId: String) {
        guard let uid = Auth.auth().currentUser?.uid else { return }
        ref.child(self.usersPath).child(uid).child(self.drawingsPath).child(drawingId).removeValue()
    }
}
