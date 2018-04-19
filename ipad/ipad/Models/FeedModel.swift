import Foundation

class FeedModel {
    var displayName: String
    var userPhotoUrl: String
    var timestamp: Date
    var drawingId: String
    var thumbnailPhotoUrl: String
    var nsfw: Bool
    
    init(displayName: String, userPhotoUrl: String, timestamp: Date, drawingId: String, thumbnailPhotoUrl: String, nsfw: Bool) {
        self.displayName = displayName
        self.userPhotoUrl = userPhotoUrl
        self.timestamp = timestamp
        self.drawingId = drawingId
        self.thumbnailPhotoUrl = thumbnailPhotoUrl
        self.nsfw = nsfw
    }
}
