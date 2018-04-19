class DrawingModel {
    var id: String
    var width: Int
    var height: Int
    var selectedStrokes: [String:String]
    var isProtected: Bool
    var isPublic: Bool
    var password: String
    
    init (data: [String:Any]?) {
        self.id = data?["id"] as? String ?? ""
        self.width = data?["width"] as? Int ?? 0
        self.height = data?["height"] as? Int ?? 0
        self.selectedStrokes = data?["selectedStrokes"] as? [String:String] ?? [:]
        self.isProtected = data?["isProtected"] as? Bool ?? false
        self.isPublic = data?["isPublic"] as? Bool ?? true
        self.password = data?["password"] as? String ?? ""
    }
    
    init(id: String, width: Int, height: Int, selectedStrokes: [String:String]) {
        self.id = id
        self.width = width
        self.height = height
        self.selectedStrokes = selectedStrokes
        self.isPublic = true
        self.isProtected = false
        self.password = ""
    }
    
    init(id: String, width: Int, height: Int, selectedStrokes: [String:String], isPublic: Bool) {
        self.id = id
        self.width = width
        self.height = height
        self.selectedStrokes = selectedStrokes
        self.isPublic = isPublic
        self.isProtected = false
        self.password = ""
    }
    
    init(id: String, width: Int, height: Int, selectedStrokes: [String:String], isPublic: Bool, isProtected: Bool, password: String) {
        self.id = id
        self.width = width
        self.height = height
        self.selectedStrokes = selectedStrokes
        self.isPublic = isPublic
        self.isProtected = isProtected
        self.password = password
    }
    
    func toDictionnary() -> [String:Any] {
        return [
            "id": id,
            "width": width,
            "height": height,
            "selectedStrokes": selectedStrokes,
            "isPublic": isPublic,
            "isProtected": isProtected,
            "password": password
            ] as [String:Any]
    }
}
