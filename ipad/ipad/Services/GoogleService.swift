import Foundation
import UIKit
import GoogleAPIClientForREST
import GoogleSignIn
import GTMSessionFetcher

class GoogleService {
    private let service: GTLRDriveService?
    
    init() {
        self.service = GTLRDriveService()
        self.service?.authorizer = GIDSignIn.sharedInstance().currentUser?.authentication.fetcherAuthorizer()
    }
    
    func uploadFileToGoogleDrive(image: UIImage, callback: @escaping ((Bool) -> Void)) {
        let metadata = GTLRDrive_File()
        metadata.mimeType = "image/png"
        metadata.name = "PolyPainPro-" + String(Date().timeIntervalSince1970) + ".png"
        
        let data = UIImagePNGRepresentation(image) as Data?
        let uploadParameters = GTLRUploadParameters(data: data!, mimeType: metadata.mimeType ?? "image/png")
        let query = GTLRDriveQuery_FilesCreate.query(withObject: metadata, uploadParameters: uploadParameters)
        self.service?.executeQuery(query) { (ticket: GTLRServiceTicket, obj, error: Error?) in
            if error != nil {
                print(error.debugDescription)
                callback(false)
            } else {
                callback(true)
            }
        }
    }
}
