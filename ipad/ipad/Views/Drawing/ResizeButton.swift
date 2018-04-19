import Foundation
import UIKit

class ResizeButton: UIView {
    var mainView: UIView?
    var drawView: DrawView?
    
    func setMainView(view: UIView, drawView: DrawView) {
        self.mainView = view
        self.drawView = drawView
        
        self.updatePosition()
    }
    
    func updatePosition() {
        self.frame.origin.x = CGFloat((self.drawView?.frame.width)! + 10)
        self.frame.origin.y = CGFloat((self.drawView?.frame.height)! + 140)
    }
    
    override func touchesMoved(_ touches: Set<UITouch>, with event: UIEvent?) {
        let newPoint = touches.first?.location(in: self.mainView!)
        self.frame.origin = newPoint!
        self.drawView?.updateSize(width: Int(newPoint!.x), height: Int(newPoint!.y))
    }
    
    override func touchesEnded(_ touches: Set<UITouch>, with event: UIEvent?) {
        self.drawView?.syncModel()
        self.isHidden = true
    }
}
