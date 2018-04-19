import UIKit

struct DrawingAttributesModel {
    var color: String?
    var width: Int?
    var height: Int?
    
    static func fromAnyObject(value: [String:AnyObject]?) -> DrawingAttributesModel {
        return DrawingAttributesModel(
            color: value?["color"] as? String,
            width: value?["width"] as? Int,
            height: value?["height"] as? Int)
    }
    
    func getCgColor(selected: Bool) -> CGColor {
        var color = self.color!
        color.removeFirst()
        if let rgbValue = UInt(color, radix: 16) {
            var a = CGFloat((rgbValue & 0xff000000) >> 24) / 255
            
            if selected {
                a = a / 2
            }
            
            let r = CGFloat((rgbValue & 0x00ff0000) >> 16) / 255
            let g = CGFloat((rgbValue & 0x0000ff00) >> 8) / 255
            let b = CGFloat(rgbValue & 0x000000ff) / 255
            return UIColor(red: r, green: g, blue: b, alpha: a).cgColor
        } else {
            return UIColor.black.cgColor
        }
    }
    
    func toDictionnary() -> [String:Any] {
        return [
            "color": color ?? "",
            "width": width ?? "",
            "height": height ?? "",
            "stylusTip": 1
            ] as [String:Any]
    }
    
    static func == (lhs: DrawingAttributesModel, rhs: DrawingAttributesModel) -> Bool{
        return (lhs.color == rhs.color) && (lhs.color == rhs.color) && (lhs.height == rhs.height)
    }
}
