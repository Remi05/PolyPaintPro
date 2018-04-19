import Foundation
import UIKit

class StylusPointModel: Equatable {
    var x: Double
    var y: Double
    var point: CGPoint
    
    init(x: Double, y: Double) {
        self.x = x;
        self.y = y;
        self.point = CGPoint(x: x, y: y)
    }

    init(point: CGPoint){
        self.point = point
        self.x = Double(point.x)
        self.y = Double(point.y)
    }
    
    
    static func fromAnyObjectArray(value: [[String:AnyObject]]?, width: Int) -> [StylusPointModel] {
        var points: Array<StylusPointModel> = []
        if (value != nil) {
            for value in value! {
                points.append(StylusPointModel(
                    x: value["X"] as? Double ?? 0.0,
                    y: value["Y"] as? Double ?? 0.0
                ))
            }
        }
        return points;
    }
    
    func toDictionnary() -> [String:Any] {
        return [
            "X": x,
            "Y": y,
            "PressureFactor": 0.5
        ] as [String:Any]
    }

    static func == (point1: StylusPointModel, point2: StylusPointModel) -> Bool{
        return (point1.x == point2.x) && (point1.y == point2.y)
    }
    
    static func > (point1: StylusPointModel, point2: StylusPointModel) -> Bool {
        return (point1.x > point2.x) || (point1.y > point2.y)
    }
    
    func distanceBetween(point: StylusPointModel) -> Double {
        return sqrt(pow(self.x - point.x, 2) + pow(self.y - point.y, 2))
    }
}
