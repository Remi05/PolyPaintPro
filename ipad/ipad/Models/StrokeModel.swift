import Foundation

class StrokeModel: Equatable {
    var id: String?
    var authorId: String?
    var drawingAttributes: DrawingAttributesModel?
    var stylusPoints: [StylusPointModel]?
    var createdDate: Date?
    var lastModificationDate: Date?
    var selected = false
    
    init() {
        self.id = UUID().uuidString
        self.authorId = ""
        self.drawingAttributes = DrawingAttributesModel()
        self.stylusPoints = []
        self.createdDate = Date()
        self.lastModificationDate = Date();
    }

    init(data: [String:Any]?) {
        let attr = DrawingAttributesModel.fromAnyObject(value: data?["drawingAttributes"] as? [String:AnyObject])
        self.id = data?["id"] as? String ?? ""
        self.authorId = data?["authorId"] as? String ?? ""
        self.drawingAttributes = attr
        self.stylusPoints = StylusPointModel.fromAnyObjectArray(value: data?["stylusPoints"] as? [[String:AnyObject]], width: attr.width!)
        self.createdDate = Formatter.date(date: data?["createdDate"] as! String)
        self.lastModificationDate = Formatter.date(date: data?["lastModificationDate"] as! String)
    }
    
    init(stroke: StrokeModel) {
        self.id = UUID().uuidString
        self.authorId = stroke.authorId
        self.drawingAttributes = stroke.drawingAttributes
        self.stylusPoints = []
        self.createdDate = stroke.createdDate;
        self.lastModificationDate = Date();
    }
    
    init(stroke: StrokeModel, uuid: Bool) {
        self.id = uuid ? stroke.id : UUID().uuidString
        self.authorId = stroke.authorId
        self.drawingAttributes = stroke.drawingAttributes
        self.stylusPoints = []
        self.createdDate = stroke.createdDate;
        self.lastModificationDate = Date();
    }

    init(id: String, authorId: String, drawingAttributes: DrawingAttributesModel, stylusPoints: [StylusPointModel], createdDate: Date, lastModificationDate: Date) {
        self.id = id
        self.authorId = authorId
        self.drawingAttributes = drawingAttributes
        self.stylusPoints = stylusPoints
        self.createdDate = createdDate;
        self.lastModificationDate = lastModificationDate;
    }
    
    func toDictionnary() -> [String:Any] {
        var points: [[String:Any]] = []
        for point in stylusPoints! {
            points.append(point.toDictionnary())
        }
        
        return [
            "id": id ?? "",
            "authorId": authorId ?? "",
            "drawingAttributes": drawingAttributes?.toDictionnary() ?? [],
            "stylusPoints": points,
            "createdDate": Formatter.dateString(date: createdDate!),
            "lastModificationDate": Formatter.dateString(date: lastModificationDate!)
        ] as [String:Any]
    }
    
    static func == (lhs: StrokeModel, rhs: StrokeModel) -> Bool{
        return (lhs.authorId! == rhs.authorId!) && (lhs.drawingAttributes! == rhs.drawingAttributes!) && (lhs.stylusPoints?.count == rhs.stylusPoints?.count)
    }
}
