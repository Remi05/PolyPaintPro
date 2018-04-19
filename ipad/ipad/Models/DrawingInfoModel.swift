import Foundation

class DrawingInfoModel {
    var id: String
    var previewUrl: String
    var thumbnailUrl: String
    var nsfw: Bool
    var timestamp: Date
    var owner: String
    
    init(data: [String:Any]?) {
        self.id = data?["id"] as? String ?? ""
        self.previewUrl = data?["previewUrl"] as? String ?? ""
        self.thumbnailUrl = data?["thumbnailUrl"] as? String ?? ""
        self.nsfw = data?["nsfw"] as? Bool ?? false
        let date = data?["last_modified_on"] as? String ?? ""
        let timestamp = date == "" ? Date(timeIntervalSinceReferenceDate: -123456789.0) : Formatter.date(date: date)
        self.timestamp = timestamp
        self.owner = data?["owner"] as? String ?? ""
    }
    
    init(id: String, previewUrl: String, thumbnailUrl: String, nsfw: Bool, timestamp: Date, owner: String) {
        self.id = id
        self.previewUrl = previewUrl
        self.nsfw = nsfw
        self.thumbnailUrl = thumbnailUrl
        self.timestamp = timestamp
        self.owner = owner
    }
    
    func toDictionnary() -> [String:Any] {
        return [
            "id": id,
            "previewUrl": previewUrl,
            "thumbnailUrl": thumbnailUrl,
            "nsfw": nsfw,
            "last_modified_on": Formatter.dateString(date: timestamp),
            "owner": owner
            ] as [String:Any]
    }
}
