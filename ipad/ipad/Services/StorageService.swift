import Foundation
import Firebase
import FirebaseStorage
class StorageService {
    private let ref: StorageReference!
    private let imagePath: String = "images"
    private let thumbnailPath: String = "thumbnails"
    private let profilePicturePath: String = "profilePicture"

    init() {
        ref = Storage.storage().reference()
    }
    
    public func saveImage(image: UIImageView, callback: @escaping ((String) -> Void)) {
        let data = UIImagePNGRepresentation(image.image!) as Data?
        let uuid = UUID().uuidString
        ref.child(self.profilePicturePath).child(uuid).putData(data!, metadata: nil, completion: { (metadata, error) in
            if error != nil {
                callback("")
            }
            callback((metadata?.downloadURL()?.absoluteString)!)
        })
    }
    
    public func savePreview(image: UIImage, id: String, callback: @escaping ((String) -> Void)) {
        let data = UIImageJPEGRepresentation(image, 1) as Data?
        self.savePreview(data: data!, id: id, callback: callback)
    }
    
    public func savePreview(data: Data, id: String, callback: @escaping ((String) -> Void)) {
        let defaults = UserDefaults.standard
        defaults.set(data, forKey: id + "_preview")
        if Auth.auth().currentUser != nil {
            ref.child(self.imagePath).child(id + ".jpg").putData(data, metadata: nil, completion: { (metadata, error) in
                if error != nil {
                    callback("")
                    return
                }
                callback((metadata?.downloadURL()?.absoluteString)!)
            })
        } else {
            callback("")
        }
    }
    
    public func saveThumbnail(image: UIImage, id: String, callback: @escaping ((String) -> Void)) {
        let data = UIImageJPEGRepresentation(image, 1) as Data?
        self.saveThumbnail(data: data!, id: id, callback: callback)
    }
    
    public func saveThumbnail(data: Data, id: String, callback: @escaping ((String) -> Void)) {
        let defaults = UserDefaults.standard
        defaults.set(data, forKey: id + "_thumbnail")
        if Auth.auth().currentUser != nil {
            ref.child(self.thumbnailPath).child(id + "_thumbnail.jpg").putData(data, metadata: nil, completion: { (metadata, error) in
                if error != nil {
                    callback("")
                    return
                }
                callback((metadata?.downloadURL()?.absoluteString)!)
            })
        } else {
            callback("")
        }
    }
}
