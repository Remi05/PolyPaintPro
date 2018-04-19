import UIKit
import MapKit
import Foundation
import Firebase
import FirebaseAuth

protocol DrawViewDelegate {
    func onDrawingPermissionChanged(model: DrawingModel)
}

class DrawView: UIView {
    public var delagate: DrawViewDelegate?
    
    public var isInGesture = false
    public var inAction = false
    var drawingService: DrawingService?
    var storageService: StorageService?
    var googleService: GoogleService?
    var userService: UsersService?
    var parent: DrawingViewController?
    var thumbnailCreation = 5
    var drawingId: String?
    var isOwner: Bool = true
    var info: DrawingInfoModel = DrawingInfoModel(id: "", previewUrl: "", thumbnailUrl: "", nsfw: false, timestamp: Date(), owner: "")
    var model: DrawingModel = DrawingModel(id: "", width: 500, height: 500, selectedStrokes: [:])
    var activeTool = DrawTools.pencil
    
    var style = DrawStyles.circle
    var width: Float = 1
    var color: UIColor = UIColor.black
    
    var selectStroke: [StylusPointModel] = []
    var selection: Selection = Selection(selectedStrokes: [])
    var selectionCut: Selection = Selection(selectedStrokes: [])
    var resizePoint: RectPoint = RectPoint.leftUp
    
    var strokes: [String:StrokeModel] = [:]
    var deletedStrokes: [StrokeModel] = []
    var currentStroke: StrokeModel! = StrokeModel()
    var endPoint: CGPoint = CGPoint.zero
    
    required init?(coder aDecoder: NSCoder) {
        super.init(coder: aDecoder)
        self.updateSize()

        self.drawingService = DrawingService()
        self.storageService = StorageService()
        self.googleService = GoogleService()
        self.userService = UsersService()
    }
    
    public func setParent(parent: DrawingViewController) {
        self.parent = parent
    }
    
    public func setDrawingId(id: String) {
        self.drawingId = id
        self.userService?.addDrawingToUser(drawingId: id)
        self.drawingService?.getDrawingInfo(id: drawingId!, callback: { (info) in
            self.info = info
            self.isOwner = Auth.auth().currentUser?.uid == self.info.owner
        })
        self.drawingService?.getDrawing(name: drawingId!, callback: { (model) in
            self.setModel(model: model)
            self.layer.setNeedsDisplay()
            
            if !model.isPublic && !self.isOwner {
                self.userService?.removeDrawingToUser(drawingId: id)
                self.delagate?.onDrawingPermissionChanged(model: model)
            }
        })
        self.drawingService?.getDrawingStrokes(drawing: drawingId!, callback: { (strokes) in
            self.setStrokes(strokes: strokes)
            self.layer.setNeedsDisplay()
            self.createTumbnail()
        })
    }
    
    override func touchesBegan(_ touches: Set<UITouch>, with event: UIEvent?) {
        if isInGesture {
            return
        }
        
        if let firstTouch = touches.first {
            endPoint = firstTouch.location(in: self)
            switch activeTool {
            case DrawTools.pencil:
                currentStroke = self.createStroke()
                strokes[currentStroke.id!] = currentStroke
            case DrawTools.bigEraser:
                self.deleteLine(touch: endPoint)
            case DrawTools.smallEraser:
                self.deletePoints(touchPoint: StylusPointModel(point: endPoint), it: 0)

            case DrawTools.lasso:
                if((!selection.selectionRect.contains(endPoint)) && selection.selectedStrokes == []) {
                    self.selectStroke = []
                    self.selection.emptySelection()
                    self.selectStroke.append(StylusPointModel(point: endPoint))
                } else if(selection.selectedStrokes != []){
                    
                    if (endPoint.x >= selection.selectionRect.minX - 20 && endPoint.x <= selection.selectionRect.minX + 20  && endPoint.y >= selection.selectionRect.minY - 20 && endPoint.y <= selection.selectionRect.minY + 20 ){
                        self.resizePoint = RectPoint.leftUp
                        self.selection.resizing = true
                    } else if (endPoint.x >= selection.selectionRect.minX - 20 && endPoint.x <= selection.selectionRect.minX + 20  && endPoint.y >= selection.selectionRect.maxY - 20 && endPoint.y <= selection.selectionRect.maxY + 20 ){
                        self.resizePoint = RectPoint.leftDown
                        self.selection.resizing = true
                    } else if (endPoint.x >= selection.selectionRect.maxX - 20 && endPoint.x <= selection.selectionRect.maxX + 20  && endPoint.y >= selection.selectionRect.minY - 20 && endPoint.y <= selection.selectionRect.minY + 20 ){
                        self.resizePoint = RectPoint.rightUp
                        self.selection.resizing = true
                    } else if (endPoint.x >= selection.selectionRect.maxX - 20 && endPoint.x <= selection.selectionRect.maxX + 20  && endPoint.y >= selection.selectionRect.maxY - 20 && endPoint.y <= selection.selectionRect.maxY + 20 ){
                        self.resizePoint = RectPoint.rightDown
                        self.selection.resizing = true
                    } else if !selection.selectionRect.contains(endPoint) {
                        self.clearSelection()
                    }
                }
            }
            self.setNeedsDisplay()
        }
    }
    
    override func touchesMoved(_ touches: Set<UITouch>, with event: UIEvent?) {
        if isInGesture {
            return
        }
        
        if let firstTouch = touches.first {
            self.inAction = true
            let newPoint = firstTouch.location(in: self)
            
            switch activeTool {
            case DrawTools.pencil:
                appendPoint(touchPoint: newPoint)
                endPoint = newPoint
            case DrawTools.bigEraser:
                self.deleteLine(touch: endPoint)
                endPoint = newPoint
            case DrawTools.smallEraser:
                self.deletePoints(touchPoint: StylusPointModel(point: endPoint), it: 0)
                endPoint = newPoint
            case DrawTools.lasso:
                if(selection.selectedStrokes != []){
                    if(selection.resizing){
                        self.selection.resizeSelection(point: self.resizePoint, start: endPoint, end: newPoint)
                    }
                    if(selection.selectionRect.contains(endPoint) && !self.selection.resizing){
                        self.selection.moveSelection(start: endPoint, end: newPoint)
                        endPoint = newPoint
                    } else if (!self.selection.resizing){
                        self.selectStroke.append(StylusPointModel(point: newPoint))
                        endPoint = newPoint
                    }
                } else {
                    self.selectStroke.append(StylusPointModel(point: newPoint))
                    endPoint = newPoint
                }
            }
        }
        self.setNeedsDisplay()
    }
    
    override func touchesEnded(_ touches: Set<UITouch>, with event: UIEvent?) {
        if isInGesture {
            return
        }
        
        self.inAction = false
        
        switch activeTool {
        case .pencil:
            self.drawingService?.createStroke(drawing: drawingId!, stroke: currentStroke)
            currentStroke = StrokeModel()
            
            if (Auth.auth().currentUser == nil) {
                self.createTumbnail()
            }
        case .smallEraser:
            break
        case .bigEraser:
            break
        case .lasso:
            if(selectStroke != []) {
                self.clearSelection()
                self.selection = Selection(selectedStrokes: self.getStrokeInSelection())
                self.selectionCut = Selection(selectedStrokes: [])
                self.updateSelection()

                self.selectStroke = []
            } else if(selection.selectionRect.contains(endPoint) && !self.selection.resizing) {
                self.drawingService?.updateStrokes(drawing: self.drawingId!, strokes: selection.selectedStrokes)
                
                if (Auth.auth().currentUser == nil) {
                    self.createTumbnail()
                }
            }
            if(selection.resizing) {
                selection.resizing = false
                self.resizePoint = RectPoint.leftUp
                self.drawingService?.updateStrokes(drawing: self.drawingId!, strokes: selection.selectedStrokes)
                
                if (Auth.auth().currentUser == nil) {
                    self.createTumbnail()
                }
            }
            break

        }
        self.setNeedsDisplay()
    }
    
    func uploadToGoogleDrive() {
        self.layer.displayIfNeeded()
        UIGraphicsBeginImageContextWithOptions(self.bounds.size, self.isOpaque, 0.0)
        self.drawHierarchy(in: self.bounds, afterScreenUpdates: true)
        guard let screenshot = UIGraphicsGetImageFromCurrentImageContext() else {
            UIGraphicsEndImageContext()
            return
        }
        UIGraphicsEndImageContext()
        self.googleService?.uploadFileToGoogleDrive(image: screenshot, callback: { (success) in
            var text = "Image sauvegardé avec succès"
            if (!success) {
                text = "Une erreur est survenue lors de l'enregistrement de l'image"
            }

            let alertController = UIAlertController(title: text, message: "", preferredStyle: .alert)
            let action = UIAlertAction(title: "Ok", style: .default)
            
            alertController.addAction(action)
            
            self.parent?.present(alertController, animated: true, completion: nil)
        })
    }
    
    func updateSelection() {
        if selection.selectedStrokes.count > 0 {
            for stroke in selection.selectedStrokes {
                self.model.selectedStrokes[stroke.id!] = Auth.auth().currentUser?.uid
            }
            
            self.drawingService?.updateDrawing(drawing: model)
        }
    }
    
    func clearSelection() {
        if selection.selectedStrokes.count > 0 {
            for stroke in selection.selectedStrokes {
                model.selectedStrokes.removeValue(forKey: stroke.id!)
            }
            self.drawingService?.updateDrawing(drawing: model)
        }
    }
    
    func createStroke() -> StrokeModel {
        return StrokeModel(
            id: UUID().uuidString,
            authorId: Auth.auth().currentUser?.uid ?? "",
            drawingAttributes: DrawingAttributesModel(
                color: String("#FF" + self.color.hexCode),
                width: Int(self.width),
                height: Int(self.width)
            ),
            stylusPoints: [],
            createdDate: Date(),
            lastModificationDate: Date()
        )
    }
    
    func duplicateStroke(stroke: StrokeModel) -> StrokeModel{
        var newPoints: [StylusPointModel] = []
        for point in stroke.stylusPoints! {
            newPoints.append(StylusPointModel(x: point.x + 20, y: point.y + 20))
        }
        return StrokeModel(
            id: UUID().uuidString,
            authorId: Auth.auth().currentUser!.uid,
            drawingAttributes: stroke.drawingAttributes!,
            stylusPoints: newPoints,
            createdDate: Date(),
            lastModificationDate: Date()
        )
    }
    
    func appendPoint(touchPoint: CGPoint) {
        let point = StylusPointModel(x: Double(touchPoint.x), y: Double(touchPoint.y))
        currentStroke.stylusPoints?.append(point)
    }
    
    func updateSize() {
        self.frame.size.width = CGFloat(self.model.width)
        self.frame.size.height = CGFloat(self.model.height)
    }
    
    func updateSize(width: Int, height: Int) {
        self.model.width = width - 10
        self.model.height = height - 140
        updateSize()
        self.setNeedsDisplay()
    }
    
    func syncModel() {
        self.drawingService?.updateDrawing(drawing: self.model)
    }
    
    func updateSelectedStrokes() {
        var ids: [String] = []
        var others: [String] = []
        for (key, value) in self.model.selectedStrokes {
            if value == Auth.auth().currentUser?.uid {
                ids.append(key)
            } else {
                others.append(key)
            }
        }
        
        var strokes: [StrokeModel] = []
        for (key, value) in self.strokes {
            if ids.contains(key) {
                strokes.append(value)
            } else if others.contains(key) {
                value.selected = true
            } else {
                value.selected = false
            }
        }
        
        self.selection = Selection(selectedStrokes: strokes)
    }
    
    func setModel(model: DrawingModel) {
        self.model = model
        self.updateSize()
        self.updateSelectedStrokes()
    }
    
    func setColor(color: UIColor) {
        self.color = color
    }
    
    func setStyle(style: DrawStyles) {
        self.style = style
    }
    
    func setWidth(newWidth: Float) {
        self.width = newWidth
    }
    
    func setActiveTool(activeTool: DrawTools) {
        self.activeTool = activeTool
    }
    
    func setStrokes(strokes: [String:StrokeModel]) {
        self.strokes = strokes;
        self.updateSelectedStrokes()
        self.setNeedsDisplay();
    }

    override func draw(_ rect: CGRect) {
        var strokes = self.strokes.sorted{ (stroke1, stroke2) -> Bool in
            return stroke1.value.createdDate! < stroke2.value.createdDate!
        }
        if currentStroke!.stylusPoints!.count > 0 {
            strokes.append(("", currentStroke!))
        }
        
        for s in strokes {
            let stroke = s.value;
            let context = UIGraphicsGetCurrentContext()
            context?.setLineCap(CGLineCap.round)
            context?.beginPath()
            var firstpoint: StylusPointModel = StylusPointModel(x: 0, y: 0)
            
            for point in stroke.stylusPoints! {
                if (point == (stroke.stylusPoints?.first!)!) {
                    firstpoint = point
                }
                context?.move(to: CGPoint(x: firstpoint.x, y: firstpoint.y))
                context?.addLine(to: CGPoint(x: point.x, y: point.y))
                firstpoint = point
            }
            context?.setLineWidth(CGFloat(stroke.drawingAttributes?.height ?? 10))
            context?.setStrokeColor(stroke.drawingAttributes!.getCgColor(selected: stroke.selected))
            context?.strokePath()
        }
        
        if (self.selection.selectionRect != CGRect.null) {
            let context = UIGraphicsGetCurrentContext()
            context?.setLineDash(phase: 1, lengths: [4, 6])
            context?.setLineWidth(CGFloat(2))
            context?.setStrokeColor(UIColor.black.cgColor)
            context?.beginPath()
            context?.addRect(self.selection.selectionRect)
            context?.strokePath()
            
            let smallContext = UIGraphicsGetCurrentContext()
            smallContext?.setLineWidth(CGFloat(4))
            smallContext?.setStrokeColor(UIColor.black.cgColor)
            smallContext?.beginPath()
            for rect in selection.resizeRects {
                smallContext?.fill(rect)
            }
            smallContext?.strokePath()
            
        }
        if(self.activeTool == DrawTools.lasso) {
            let context = UIGraphicsGetCurrentContext()
            context?.setLineCap(CGLineCap.round)
            context?.beginPath()
            var firstpoint: StylusPointModel = StylusPointModel(x: 0, y: 0)
            for point in selectStroke {
                if (point == selectStroke.first) {
                    firstpoint = point
                }
                context?.move(to: CGPoint(x: firstpoint.x, y: firstpoint.y))
                context?.addLine(to: CGPoint(x: point.x, y: point.y))
                firstpoint = point
            }
            context?.setLineDash(phase: 1, lengths: [2, 30])
            context?.setLineWidth(CGFloat(5))
            context?.setStrokeColor(UIColor.orange.cgColor)
            context?.strokePath()
        }

    }

    func deletePoints(touchPoint: StylusPointModel, it: Int) {
        if (it > 20) { return }
        for (_, stroke) in self.strokes {
            if stroke.selected {
                continue
            }
            var min = 10.0;
            if (stroke.drawingAttributes!.width! > 20) {
                min = Double(stroke.drawingAttributes!.width!)
            }

            for (index, point) in stroke.stylusPoints!.enumerated() {
                let distance = touchPoint.distanceBetween(point: point)
                if (distance <= min) {
                    self.splitStroke(split: index, stroke: stroke)
                    self.removeStroke(stroke: stroke)
                    deletePoints(touchPoint: touchPoint, it: it + 1)
                    return
                }
            }
        }
    }
    
    func getStrokeInSelection() -> [StrokeModel]{
        var strokeInSelection: [StrokeModel] = []
        for stroke in strokes {
            if stroke.value.selected {
                continue
            }
            for point in stroke.value.stylusPoints! {

                if(!self.contains(polygon: selectStroke, test: point.point)) {
                    break
                } else if (self.contains(polygon: selectStroke, test: point.point) && point == stroke.value.stylusPoints?.last) {
                    strokeInSelection.append(stroke.value)
                }
            }
        }
        
        return strokeInSelection
    }
    
    func splitStroke(split: Int, stroke: StrokeModel) {
        let newStroke1 = StrokeModel(stroke: stroke)
        let newStroke2 = StrokeModel(stroke: stroke)
        
        if (split == 0) {
            newStroke1.stylusPoints?.append(contentsOf: stroke.stylusPoints![split + 1 ..< stroke.stylusPoints!.count])
            if (newStroke1.stylusPoints!.count > 0) {
                self.strokes[newStroke1.id!] = newStroke1
                drawingService?.createStroke(drawing: drawingId!, stroke: newStroke1)
            }
        } else if (split == stroke.stylusPoints?.count){
            newStroke1.stylusPoints?.append(contentsOf: stroke.stylusPoints![0 ..< split - 1])
            if (newStroke1.stylusPoints!.count > 0) {
                strokes[newStroke1.id!] = newStroke1
                drawingService?.createStroke(drawing: drawingId!, stroke: newStroke1)
            }
        } else {
            newStroke1.stylusPoints?.append(contentsOf: stroke.stylusPoints![0 ..< split - 1])
            if (newStroke1.stylusPoints!.count > 0) {
                strokes[newStroke1.id!] = newStroke1
                drawingService?.createStroke(drawing: drawingId!, stroke: newStroke1)
            }
            
            newStroke2.stylusPoints?.append(contentsOf: stroke.stylusPoints![split + 1 ..< stroke.stylusPoints!.count])
            if (newStroke2.stylusPoints!.count > 0) {
                strokes[newStroke2.id!] = newStroke2
                drawingService?.createStroke(drawing: drawingId!, stroke: newStroke2)
            }
        }
    }
    
    func removeStroke(stroke: StrokeModel){
        if (self.strokes[stroke.id!] != nil) {
            self.strokes.removeValue(forKey: stroke.id!)
            self.drawingService?.removeStroke(drawing: drawingId!, strokeId: stroke.id!);
            
            if (Auth.auth().currentUser == nil) {
                self.createTumbnail()
            }
        }
    }
    
    func deleteLine(touch: CGPoint) {
        for (_, stroke) in self.strokes {
            if stroke.selected {
                continue
            }
            if (self.containsPoint(checkPoint: touch, stroke: stroke)){
                self.removeStroke(stroke: stroke)
            }
        }
    }
    
    func undo() {
        let strokes = self.strokes.sorted { (stroke1, stroke2) -> Bool in
            return stroke1.value.lastModificationDate! > stroke2.value.lastModificationDate!
        }.filter { (key: String, value: StrokeModel) -> Bool in
            return value.authorId == Auth.auth().currentUser?.uid ?? ""
        }
        
        guard let stroke = strokes.first else { return }
        self.removeStroke(stroke: stroke.value)
        self.deletedStrokes.append(stroke.value)
    }
    
    func redo() {
        guard let stroke = self.deletedStrokes.popLast() else { return }
        self.strokes[stroke.id!] = stroke
        self.drawingService?.createStroke(drawing: drawingId!, stroke: stroke)
    }
    
    func clear() {
        self.strokes = [:]
        self.drawingService?.removeAll(drawing: drawingId!)
    }

    func cutSelection() {
        for stroke in selection.selectedStrokes {
            self.removeStroke(stroke: stroke)
            
            if (Auth.auth().currentUser == nil) {
                self.createTumbnail()
            }
        }
        self.selectionCut = self.selection
        self.setNeedsDisplay()
    }
    
    func duplicateSelection() {
        var duplicateStrokes: [StrokeModel] = []
        
        if (selection.selectedStrokes.count > 0) {
            for stroke in self.selection.selectedStrokes {
                let newStroke = self.duplicateStroke(stroke: stroke)
                self.strokes[newStroke.id!] = newStroke
                duplicateStrokes.append(newStroke)
            }
        } else if (selectionCut.selectedStrokes.count > 0) {
            for stroke in self.selectionCut.selectedStrokes {
                let newStroke = self.duplicateStroke(stroke: stroke)
                self.strokes[newStroke.id!] = newStroke
                duplicateStrokes.append(newStroke)
            }
        } else {
            return
        }
        self.clearSelection()
        self.selectStroke.removeAll()
        self.selection.emptySelection()
        self.selection = Selection(selectedStrokes: duplicateStrokes)
        self.drawingService?.updateStrokes(drawing: self.drawingId!, strokes: selection.selectedStrokes)
        
        if (Auth.auth().currentUser == nil) {
            self.createTumbnail()
        }

        self.updateSelection()
        
        self.setNeedsDisplay()
    }
    
    func toggleNotSafeForWork() {
        self.info.nsfw = !self.info.nsfw
        self.drawingService?.updateDrawingInfo(info: self.info)
    }
    
    func containsPoint(checkPoint: CGPoint, stroke: StrokeModel) -> Bool{
        var lineWidth = CGFloat((stroke.drawingAttributes?.width)!)
        if (lineWidth < 4){
            lineWidth *= 2
        }
        for point in stroke.stylusPoints! {
            if(CGFloat(point.x) <= checkPoint.x + lineWidth && CGFloat(point.x) >= checkPoint.x - lineWidth &&
                CGFloat(point.y) <= checkPoint.y + lineWidth && CGFloat(point.y) >= checkPoint.y - lineWidth){
                return true
            }
        }
        return false
    }

    func contains(polygon: [StylusPointModel], test: CGPoint) -> Bool {
        if polygon.count <= 1 {
            return false
        }
        let path = UIBezierPath()
        let firstPoint = polygon[0].point
        path.move(to: firstPoint)
        for index in 1...polygon.count-1 {
            path.addLine(to: polygon[index].point)
        }
        
        path.close()
        return path.contains(test)
    }
    
    func distanceBetweenPoints(point1: StylusPointModel, point2: StylusPointModel) -> Double {
        return sqrt(pow(point1.x - point2.x, 2) + pow(point1.y - point2.y, 2))
    }
    
    func createTumbnail() {
        if self.thumbnailCreation > 1 {
            self.thumbnailCreation += 1
            if self.thumbnailCreation > 5 {
                self.thumbnailCreation = 1
            }
            return
        }
        
        self.layer.displayIfNeeded()
        UIGraphicsBeginImageContextWithOptions(self.bounds.size, self.isOpaque, 0.0)
        self.drawHierarchy(in: self.bounds, afterScreenUpdates: true)
        guard let screenshot = UIGraphicsGetImageFromCurrentImageContext() else {
            UIGraphicsEndImageContext()
            return
        }
        UIGraphicsEndImageContext()
        storageService?.savePreview(image: screenshot, id: self.info.id, callback: { (link) in
            if link != "" && self.info.previewUrl == "" {
                self.info.previewUrl = link
                self.drawingService?.updateDrawingInfo(info: self.info)
            }
        })
        let scale = 128 / screenshot.size.height
        let width = screenshot.size.width * scale
        guard let thumbnail = screenshot.scaleImage(toSize: CGSize(width: width, height: 128)) else { return }

        storageService?.saveThumbnail(image: thumbnail, id: self.info.id, callback: { (link) in
            if link != "" && self.info.thumbnailUrl == "" {
                self.info.thumbnailUrl = link
                self.drawingService?.updateDrawingInfo(info: self.info)
            }
        })
    }
}
