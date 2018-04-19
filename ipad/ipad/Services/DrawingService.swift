import Foundation
import Firebase
import FirebaseDatabase
import FirebaseAuth

class DrawingService {
    private let ref: DatabaseReference!
    private let storageService: StorageService?
    private let userPath: String = "users"
    private let drawingPath: String = "drawings"
    private let drawingInfoPath: String = "drawingInfo"
    private let strokesPath: String = "strokes"
    private let likesPath: String = "likes"
    
    init() {
        ref = Database.database().reference()
        self.storageService = StorageService()
    }
    
    func setDrawingToUpdate(id: String) {
        let defaults = UserDefaults.standard
        var drawing = defaults.dictionary(forKey: "modifiedDrawings") as? [String:Bool] ?? [:]
        drawing[id] = true
        defaults.set(drawing, forKey: "modifiedDrawings")
    }
    
    func downloadThumbnail(drawingId: String, url: String) {
        if url != "" {
            URLSession.shared.dataTask(with: URL(string: url)!, completionHandler: { data, response, error in
                if error == nil {
                    let defaults = UserDefaults.standard
                    defaults.set(data!, forKey: drawingId + "_thumbnail")
                }
            }).resume()
        }
    }
    
    func fetchDrawing() {
        let defaults = UserDefaults.standard
        ref.child(self.drawingPath).observeSingleEvent(of: .value, with: { (snapshot) in
            let value  = snapshot.value as? [String:AnyObject] ?? [:]
            var drawings: [String] = []
            
            for (key, _) in value {
                drawings.append(key)
                self.ref.child(self.drawingPath).child(key).observe(.value, with: { (snapshot) in
                    let data = snapshot.value as? [String:AnyObject] ?? [:]
                    let isPublic = data["isPublic"] as? Bool ?? true
                    let isProtected = data["isProtected"] as? Bool ?? false
                    
                    if isPublic && !isProtected {
                        
                        let model = DrawingModel(
                            id: key,
                            width: data["width"] as! Int ,
                            height: data["height"] as! Int,
                            selectedStrokes: data["selectedStrokes"] as? [String:String] ?? [:],
                            isPublic: isPublic,
                            isProtected: isProtected,
                            password: data["password"] as? String ?? ""
                        )
                        defaults.set(model.toDictionnary(), forKey: key)
                        self.getDrawingInfo(id: key, callback: { (info) in
                            defaults.set(info.toDictionnary(), forKey: key + "_info")
                            self.downloadThumbnail(drawingId: key, url: info.thumbnailUrl)
                        })
                        self.getDrawingStrokes(drawing: key, callback: { (strokes) in
                            var data: [String:Any] = [:]
                            for (key, value) in strokes {
                                data[key] = value.toDictionnary()
                            }
                            defaults.set(data, forKey: key + "_strokes")
                        })
                    }
                })
            }
            defaults.set(drawings, forKey: "drawings")
        }) { (error) in
            print(error.localizedDescription)
        }
    }
    
    func syncData() {
        let defaults = UserDefaults.standard
        let createdDrawings = defaults.array(forKey: "createdDrawings") as? [String] ?? []

        for drawingId in createdDrawings {
            let drawing = DrawingModel(data: defaults.dictionary(forKey: drawingId))
            let info = DrawingInfoModel(data: defaults.dictionary(forKey: drawingId + "_info"))
            let strokes = self.getLocalStrokes(drawingId: drawingId)
            let uid = Auth.auth().currentUser?.uid ?? ""
            self.ref.child(self.userPath).child(uid).child(self.drawingPath).updateChildValues([drawingId:drawingId])
            self.ref.child(self.drawingPath).child(drawingId).updateChildValues(drawing.toDictionnary())
            self.ref.child(self.strokesPath).child(drawingId).updateChildValues(strokes)
            
            let data = defaults.data(forKey: drawingId + "_thumbnail")
            if data != nil {
                self.storageService?.saveThumbnail(data: data!, id: drawingId, callback: { (link) in
                    if link != "" {
                        info.thumbnailUrl = link
                    }
                    self.ref.child(self.drawingInfoPath).child(drawingId).updateChildValues(info.toDictionnary())
                })
            } else {
                self.ref.child(self.drawingInfoPath).child(drawingId).updateChildValues(info.toDictionnary())
            }
        }
        
        defaults.set([], forKey: "createdDrawings")
        
        let modifiedDrawing = defaults.dictionary(forKey: "modifiedDrawings") as? [String:Bool] ?? [:]
        let group = DispatchGroup()
        
        for (drawingId, _) in modifiedDrawing {
            group.enter()
            self.getDrawingInfoOnce(id: drawingId, callback: { (i) in
                let info = DrawingInfoModel(data: defaults.dictionary(forKey: drawingId + "_info") ?? [:])
                if info.timestamp == i.timestamp {
                    let drawing = DrawingModel(data: defaults.dictionary(forKey: drawingId))
                    let strokes = self.getLocalStrokes(drawingId: drawingId)
                    
                    self.ref.child(self.drawingPath).child(drawingId).updateChildValues(drawing.toDictionnary())
                    self.ref.child(self.drawingInfoPath).child(drawingId).updateChildValues(info.toDictionnary())
                    self.ref.child(self.strokesPath).child(drawingId).setValue(strokes)
                    let data = defaults.data(forKey: drawingId + "_thumbnail")
                    self.storageService?.saveThumbnail(data: data!, id: drawingId, callback: { (link) in })
                }
                group.leave()
            })
        }
        
        group.notify(queue: .global(qos: .userInteractive)) {
            self.fetchDrawing()
        }
        
        defaults.set([:], forKey: "modifiedDrawings")
    }
    
    func getLocalStrokes(drawingId: String) -> [String:Any] {
        let defaults = UserDefaults.standard
        let data = defaults.dictionary(forKey: drawingId + "_strokes") ?? [:]
        var strokes: [String:Any] = [:]
        let uid = Auth.auth().currentUser?.uid
        for (key, value) in data {
            let stroke = StrokeModel(data: value as? [String:Any] ?? [:])
            stroke.authorId = uid
            strokes[key] = stroke.toDictionnary()
        }
        return strokes
    }
    
    func createDrawing(drawing: DrawingModel) -> String {
        if Auth.auth().currentUser != nil {
            let key = ref.child(self.drawingPath).childByAutoId().key
            guard let uid = Auth.auth().currentUser?.uid else { return "" }
            ref.child(self.drawingPath).child(key).updateChildValues(drawing.toDictionnary())
            ref.child(self.userPath).child(uid).child(drawingPath).updateChildValues([key:key])
            let info = DrawingInfoModel(id: key, previewUrl: "", thumbnailUrl: "", nsfw: false, timestamp: Date(), owner: (Auth.auth().currentUser?.uid)!)
            ref.child(self.drawingInfoPath).child(key).updateChildValues(info.toDictionnary())
            
            return key
        } else {
            let key = UUID().uuidString
            let defaults = UserDefaults.standard
            var drawings = defaults.array(forKey: "drawings")
            drawings?.append(key)
            defaults.set(drawings, forKey: "drawings")
            
            drawing.id = key
            defaults.set(drawing.toDictionnary(), forKey: key)
            let info = DrawingInfoModel(id: key, previewUrl: "", thumbnailUrl: "", nsfw: false, timestamp: Date(), owner: "")
            defaults.set(info.toDictionnary(), forKey: key + "_info")
            
            var createdDrawing = defaults.array(forKey: "createdDrawings") ?? []
            createdDrawing.append(key)
            defaults.set(createdDrawing, forKey: "createdDrawings")
            
            return key
        }
    }
    
    func getDrawings(callback:@escaping (([DrawingModel]) -> Void)) {
        if (Auth.auth().currentUser != nil) {
            ref.child(self.drawingPath).observe(.value, with: { (snapshot) in
                let value  = snapshot.value as? [String:AnyObject] ?? [:]
                var models: [DrawingModel] = []
                
                for (key, data) in value {
                    models.append(DrawingModel(
                        id: key,
                        width: data["width"] as! Int ,
                        height: data["height"] as! Int,
                        selectedStrokes: data["selectedStrokes"] as? [String:String] ?? [:],
                        isPublic: data["isPublic"] as? Bool ?? true,
                        isProtected: data["isProtected"] as? Bool ?? false,
                        password: data["password"] as? String ?? ""
                    ))
                }
                callback(models)
            }) { (error) in
                print(error.localizedDescription)
            }
        } else {
            let defaults = UserDefaults.standard
            let drawings: [String] = defaults.array(forKey: "drawings") as? [String] ?? []
            var models: [DrawingModel] = []
            
            for id in drawings {
                let data = defaults.dictionary(forKey: id)
                models.append(DrawingModel(data: data))
            }
            callback(models)
        }
    }
    
    func getDrawing(name: String, callback:@escaping ((DrawingModel) -> Void)) {
        if Auth.auth().currentUser != nil {
            ref.child(self.drawingPath).child(name).observe(.value, with: { (snapshot) in
                let value  = snapshot.value as? [String:AnyObject] ?? [:]
                
                let model = DrawingModel(
                    id: snapshot.key as String,
                    width: value["width"] as? Int ?? 500,
                    height: value["height"] as? Int ?? 300,
                    selectedStrokes: value["selectedStrokes"] as? [String:String] ?? [:],
                    isPublic: value["isPublic"] as? Bool ?? true,
                    isProtected: value["isProtected"] as? Bool ?? false,
                    password: value["password"] as? String ?? ""
                )
                callback(model)
            }) { (error) in
                print(error.localizedDescription)
            }
        } else {
            let defaults = UserDefaults.standard
            let model = DrawingModel(data: defaults.dictionary(forKey: name))
            callback(model)
        }
    }
    
    func getDrawingOnce(name: String, callback:@escaping ((DrawingModel) -> Void)) {
        if Auth.auth().currentUser != nil {
            ref.child(self.drawingPath).child(name).observeSingleEvent(of: .value, with: { (snapshot) in
                let value  = snapshot.value as? [String:AnyObject] ?? [:]

                let model = DrawingModel(
                    id: snapshot.key as String,
                    width: value["width"] as! Int ,
                    height: value["height"] as! Int,
                    selectedStrokes: value["selectedStrokes"] as? [String:String] ?? [:],
                    isPublic: value["isPublic"] as? Bool ?? true,
                    isProtected: value["isProtected"] as? Bool ?? false,
                    password: value["password"] as? String ?? ""
                )
                callback(model)
            }) { (error) in
                print(error.localizedDescription)
            }
        } else {
            let defaults = UserDefaults.standard
            let model = DrawingModel(data: defaults.dictionary(forKey: name))
            callback(model)
        }
    }
    
    func updateDrawing(drawing: DrawingModel) {
        if Auth.auth().currentUser != nil {
            ref.child(self.drawingPath).child(drawing.id).updateChildValues(drawing.toDictionnary())
        } else {
            self.setDrawingToUpdate(id: drawing.id)
            let defaults = UserDefaults.standard
            defaults.set(drawing.toDictionnary(), forKey: drawing.id)
        }
    }
    
    func getDrawingInfo(id: String, callback:@escaping ((DrawingInfoModel) -> Void)) {
        if Auth.auth().currentUser != nil {
            ref.child(self.drawingInfoPath).child(id).observe(.value, with: { (snapshot) in
                let value  = snapshot.value as? [String:AnyObject] ?? [:]
                let date = value["last_modified_on"] as? String ?? ""
                let timestamp = date == "" ? Date(timeIntervalSinceReferenceDate: -123456789.0) : Formatter.date(date: date)
                
                let model = DrawingInfoModel(
                    id: value["id"] as? String ?? "",
                    previewUrl: value["previewUrl"] as? String ?? "",
                    thumbnailUrl: value["thumbnailUrl"] as? String ?? "",
                    nsfw: value["nsfw"] as? Bool ?? false,
                    timestamp: timestamp,
                    owner: value["owner"] as? String ?? ""
                )
                callback(model)
            }) { (error) in
                print(error.localizedDescription)
            }
        } else {
            let defaults = UserDefaults.standard
            let info = DrawingInfoModel(data: defaults.dictionary(forKey: id + "_info"))
            callback(info)
        }
    }
    
    func getDrawingInfoOnce(id: String, callback:@escaping ((DrawingInfoModel) -> Void)) {
        ref.child(self.drawingInfoPath).child(id).observeSingleEvent(of: .value, with: { (snapshot) in
            let value  = snapshot.value as? [String:AnyObject] ?? [:]
            let date = value["last_modified_on"] as? String ?? ""
            let timestamp = date == "" ? Date(timeIntervalSinceReferenceDate: -123456789.0) : Formatter.date(date: date)
            
            let model = DrawingInfoModel(
                id: value["id"] as? String ?? "",
                previewUrl: value["previewUrl"] as? String ?? "",
                thumbnailUrl: value["thumbnailUrl"] as? String ?? "",
                nsfw: value["nsfw"] as? Bool ?? false,
                timestamp: timestamp,
                owner: value["owner"] as? String ?? ""
            )
            callback(model)
        }) { (error) in
            print(error.localizedDescription)
        }
    }
    
    func updateDrawingInfo(info: DrawingInfoModel) {
        if Auth.auth().currentUser != nil {
            ref.child(self.drawingInfoPath).child(info.id).updateChildValues(info.toDictionnary())
        } else {
            self.setDrawingToUpdate(id: info.id)
            let defaults = UserDefaults.standard
            defaults.set(info.toDictionnary(), forKey: info.id + "_info")
        }
    }
    
    func getDrawingStrokes(drawing: String, callback:@escaping (([String:StrokeModel]) -> Void)) {
        if Auth.auth().currentUser != nil {
            ref.child(self.strokesPath).child(drawing).observe(.value, with: { (snapshot) in
                guard let value  = snapshot.value as? [String:AnyObject] else { return }
                var strokes: [String:StrokeModel] = [:]
                for (_, value) in value {
                    let attr = DrawingAttributesModel.fromAnyObject(value: value["drawingAttributes"] as? [String:AnyObject])
                    strokes[value["id"] as! String] = StrokeModel(
                        id: value["id"] as? String ?? "",
                        authorId: value["authorId"] as? String ?? "",
                        drawingAttributes: attr,
                        stylusPoints: StylusPointModel.fromAnyObjectArray(value: value["stylusPoints"] as? [[String:AnyObject]], width: attr.width ?? 200),
                        createdDate: Formatter.date(date: value["createdDate"] as! String),
                        lastModificationDate: Formatter.date(date: value["lastModificationDate"] as! String)
                    )
                }
                callback(strokes)
            }) { (error) in
                print(error.localizedDescription)
            }
        } else {
            let defaults = UserDefaults.standard
            let data = defaults.dictionary(forKey: drawing + "_strokes") ?? [:]
            var strokes: [String:StrokeModel] = [:]
            
            for (key, value) in data {
                strokes[key] = StrokeModel(data: value as? [String:Any] ?? [:])
            }
            
            callback(strokes)
        }
    }
    
    func createStroke(drawing: String, stroke: StrokeModel) {
        if stroke.stylusPoints!.count == 0 {
            return
        }
        if Auth.auth().currentUser != nil {
            ref.child(self.strokesPath).child(drawing).child(stroke.id!).updateChildValues(stroke.toDictionnary())
            let post = [ "last_modified_on": Formatter.dateString(date: Date()) ] as [String : String]
            ref.child(self.drawingInfoPath).child(drawing).updateChildValues(post)
        } else {
            self.setDrawingToUpdate(id: drawing)
            let defaults = UserDefaults.standard
            var data = defaults.dictionary(forKey: drawing + "_strokes") ?? [:]
            data[stroke.id!] = stroke.toDictionnary()
            defaults.set(data, forKey: drawing + "_strokes")
        }
    }
    
    func updateStrokes(drawing: String, strokes: [StrokeModel]) {
        if Auth.auth().currentUser != nil {
            var values: [String:Any] = [:]
            
            for stroke in strokes {
                if stroke.stylusPoints!.count > 0 {
                    values[stroke.id!] = stroke.toDictionnary()
                } else {
                    self.removeStroke(drawing: drawing, strokeId: stroke.id!)
                }
            }
            
            ref.child(self.strokesPath).child(drawing).updateChildValues(values)
            let post = [ "last_modified_on": Formatter.dateString(date: Date()) ] as [String : String]
            ref.child(self.drawingInfoPath).child(drawing).updateChildValues(post)
        } else {
            self.setDrawingToUpdate(id: drawing)
            let defaults = UserDefaults.standard
            var data = defaults.dictionary(forKey: drawing + "_strokes") ?? [:]
            for stroke in strokes {
                data[stroke.id!] = stroke.toDictionnary()
            }
            defaults.set(data, forKey: drawing + "_strokes")
        }
    }
    
    func removeStroke(drawing: String, strokeId: String) {
        if Auth.auth().currentUser != nil {
            ref.child(self.strokesPath).child(drawing).child(strokeId).removeValue()
            let post = [ "last_modified_on": Formatter.dateString(date: Date()) ] as [String : String]
            ref.child(self.drawingInfoPath).child(drawing).updateChildValues(post)
        } else {
            self.setDrawingToUpdate(id: drawing)
            let defaults = UserDefaults.standard
            var data = defaults.dictionary(forKey: drawing + "_strokes") ?? [:]
            data.removeValue(forKey: strokeId)
            defaults.set(data, forKey: drawing + "_strokes")
        }
    }
    
    func moveStroke(drawing: String, strokeId: String, newPoints: StrokeModel){
        self.removeStroke(drawing: drawing, strokeId: strokeId)
        self.createStroke(drawing: drawing, stroke: newPoints)
    }
    
    func removeAll(drawing: String) {
        if Auth.auth().currentUser != nil {
            ref.child(self.strokesPath).child(drawing).removeValue()
        let post = [ "last_modified_on": Formatter.dateString(date: Date()) ] as [String : String]
        ref.child(self.drawingInfoPath).child(drawing).updateChildValues(post)
        } else {
            self.setDrawingToUpdate(id: drawing)
            let defaults = UserDefaults.standard
            defaults.set([:], forKey: drawing + "_strokes")
        }
    }
    
    public func likeDrawing(drawingId: String, userId: String) {
        let post = [ userId: true ] as [String : Bool]
        ref.child(self.drawingInfoPath).child(drawingId).child(self.likesPath).updateChildValues(post)
    }
    
    public func unlikeDrawing(drawingId: String, userId: String) {
        ref.child(self.drawingInfoPath).child(drawingId).child(self.likesPath).child(userId).setValue(nil)
    }
    
    public func isLike(drawingId: String, userId: String, callback: @escaping ((Bool?) -> Void)) {
        ref.child(self.drawingInfoPath).child(drawingId).child(self.likesPath).child(userId).observeSingleEvent(of: .value, with: { (snapshot) in
            callback(snapshot.exists())
        })
    }
    
    public func numberOfLike(drawindId: String, userId: String, callback: @escaping ((Int) -> Void)) {
        ref.child(self.drawingInfoPath).child(drawindId).child(self.likesPath).observeSingleEvent(of: .value, with: { (snapshot) in
            let value = snapshot.value as? [String:Bool] ?? [:]
            callback(value.count)
        }) { (error) in
            print(error.localizedDescription)
        }
    }
}
