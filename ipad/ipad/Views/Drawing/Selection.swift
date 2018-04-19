import Foundation
import UIKit

class Selection {
    var selectedStrokes: [StrokeModel] = []
    var selectionRect: CGRect = CGRect.null
    var resizeRects: [CGRect] = []
    var resizing: Bool = false
    
    init(selectedStrokes: [StrokeModel]) {
        self.selectedStrokes = selectedStrokes
        self.setRect()
    }
    
    func setRect(){
        if(selectedStrokes != []){
            var biggestX = selectedStrokes.first?.stylusPoints?.first!.x
            var biggestY = selectedStrokes.first?.stylusPoints?.first!.y
            var smallestX = selectedStrokes.first?.stylusPoints?.first!.x
            var smallestY = selectedStrokes.first?.stylusPoints?.first!.y
            
            for stroke in selectedStrokes {
                for point in stroke.stylusPoints! {
                    if(point.x > Double(biggestX!)){
                        biggestX = point.x
                    } else if (point.x < Double(smallestX!)){
                        smallestX = point.x
                    }
                    if(point.y > Double(biggestY!)){
                        biggestY = point.y
                    } else if (point.y < Double(smallestY!)){
                        smallestY = point.y
                    }
                }
            }
            
            self.selectionRect = CGRect(x: smallestX! - 30, y: smallestY! - 30, width: biggestX! - smallestX! + 60 , height: biggestY! - smallestY! + 60)
            self.setResizeRects()
        }
    }
    func setResizeRects() {
        self.resizeRects.append(contentsOf: [CGRect(x: selectionRect.minX - 10, y: selectionRect.minY - 10, width: 20, height: 20),
                                             CGRect(x: selectionRect.minX - 10, y: selectionRect.maxY - 10, width: 20, height: 20),
                                             CGRect(x: selectionRect.maxX - 10, y: selectionRect.minY - 10, width: 20, height: 20),
                                             CGRect(x: selectionRect.maxX - 10, y: selectionRect.maxY - 10, width: 20, height: 20)])
        
    }
    
    func emptySelection() {
        self.selectedStrokes = []
        self.selectionRect = CGRect.null
        
    }
    
    func moveSelection(start: CGPoint, end: CGPoint) {
        let offSetX = end.x - start.x
        let offSetY = end.y - start.y
        
        for stroke in selectedStrokes {
            for point in stroke.stylusPoints! {
                point.x += Double(offSetX)
                point.y += Double(offSetY)
            }
        }
        self.updateRect()
    }
    
    func resizeSelection(point: RectPoint, start: CGPoint, end: CGPoint){
        self.resizing = true
        
        let offSetX = (end.x - start.x ) * 0.05
        let offSetY = (end.y - start.y ) * 0.05
        let oldWidth = self.selectionRect.width
        let oldHeight = self.selectionRect.height
        let minWidth:CGFloat = 100
        let minHeight:CGFloat = 1
        switch point{
        case RectPoint.rightUp:
            if(self.selectionRect.width + offSetX > minWidth &&  self.selectionRect.height  - offSetY > minHeight){
                self.selectionRect = CGRect(x: self.selectionRect.minX, y: self.selectionRect.minY + offSetY, width: self.selectionRect.width + offSetX, height: self.selectionRect.height  - offSetY)
            }
            
            break
        case RectPoint.rightDown:
            if(self.selectionRect.width + offSetX > minWidth &&  self.selectionRect.height + offSetY > minHeight){
                self.selectionRect = CGRect(x: selectionRect.minX, y: self.selectionRect.minY, width: self.selectionRect.width + offSetX, height: self.selectionRect.height  + offSetY )
            }
            break
        case RectPoint.leftUp:
            if(self.selectionRect.width - offSetX > minWidth &&  self.selectionRect.height - offSetY > minHeight){
                self.selectionRect = CGRect(x: self.selectionRect.minX + offSetX, y: self.selectionRect.minY + offSetY, width: self.selectionRect.width - offSetX, height: self.selectionRect.height - offSetY)
            }
            break
        case RectPoint.leftDown:
            if(self.selectionRect.width - offSetX > minWidth &&  self.selectionRect.height + offSetY > minHeight){
                self.selectionRect = CGRect(x: self.selectionRect.minX + offSetX, y: self.selectionRect.minY, width: self.selectionRect.width - offSetX , height: self.selectionRect.height  + offSetY)
            }
            break
        }
        self.resizeRects = []
        self.resize(oldW: oldWidth, oldH: oldHeight, newW: self.selectionRect.width, newH: self.selectionRect.height, point: point)
        self.setResizeRects()
        
    }
    
    func resize(oldW: CGFloat, oldH: CGFloat, newW: CGFloat, newH: CGFloat, point: RectPoint) {
        switch point{
        case RectPoint.rightUp:
            for stroke in selectedStrokes {
                for point in stroke.stylusPoints! {
                    point.x = (point.x - Double(selectionRect.minX)) * Double( newW / oldW ) + Double(selectionRect.minX)
                    point.y = (point.y - Double(selectionRect.maxY)) * Double( newH / oldH ) + Double(selectionRect.maxY)
                }
            }
            break
        case RectPoint.rightDown:
            for stroke in selectedStrokes {
                for point in stroke.stylusPoints! {
                    point.x = (point.x - Double(selectionRect.minX)) * Double( newW / oldW ) + Double(selectionRect.minX)
                    point.y = (point.y - Double(selectionRect.minY)) * Double( newH / oldH ) + Double(selectionRect.minY)
                }
            }
            break
        case RectPoint.leftUp:
            for stroke in selectedStrokes {
                for point in stroke.stylusPoints! {
                    point.x = (point.x - Double(selectionRect.maxX)) * Double( newW / oldW ) + Double(selectionRect.maxX)
                    point.y = (point.y - Double(selectionRect.maxY)) * Double( newH / oldH ) + Double(selectionRect.maxY)
                }
            }
            break
        case RectPoint.leftDown:
            for stroke in selectedStrokes {
                for point in stroke.stylusPoints! {
                    point.x = (point.x - Double(selectionRect.maxX)) * Double( newW / oldW ) + Double(selectionRect.maxX)
                    point.y = (point.y - Double(selectionRect.minY)) * Double( newH / oldH ) + Double(selectionRect.minY)
                    
                }
            }
            break
        }
    }
    
    func updateRect() {
        self.selectionRect = CGRect.null
        self.resizeRects = []
        self.setRect()
    }
} 
