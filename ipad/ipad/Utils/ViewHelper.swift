import Foundation
import UIKit

enum Views: String {
    case MainMenuViewController = "MainMenuViewController"
    case NewUserViewController = "NewUserViewController"
    case AuthenticationViewController = "AuthenticationViewController"
    case NavigationController = "NavigationController"
    case ConversationTableViewController = "ConversationTableViewController"
    case NewConversationViewController = "NewConversationViewController"
    case DrawingViewController = "DrawingViewController"
    case ProfileViewController = "ProfileViewController"
    case CreateDrawingViewController = "CreateDrawingViewController"
    case DrawingListViewController = "DrawingListViewController"
    case FeedViewController = "FeedViewController"
    case ConversationsViewController = "ConversationsViewController"
    case SearchViewController = "SearchViewController"
    case JoinConversationViewController = "JoinConversationViewController"
}

enum RectPoint: String {
    case rightUp = "rightUp"
    case rightDown = "rightDown"
    case leftUp = "leftUp"
    case leftDown = "leftDown"
}

enum DrawStyles: String {
    case circle = "circle"
    case square = "square"
    case vertical = "vertical"
    case horizontal = "horizontal"
}

enum DrawTools: String {
    case pencil = "pencil"
    case smallEraser = "smallEraser"
    case bigEraser = "bigEraser"
    case lasso = "lasso"
}

class ViewHelper {
    static func getView(_ view: Views)-> UIViewController {
        let storyboard: UIStoryboard = UIStoryboard(name: "Main", bundle: nil)
        return storyboard.instantiateViewController(withIdentifier: view.rawValue)
    }
    
    static func getAlert(_ sender: String) -> UIViewController {
        let alertController = UIAlertController(title: "Erreur", message: sender, preferredStyle: UIAlertControllerStyle.alert)
        alertController.addAction(UIAlertAction(title: "Dismiss", style: UIAlertActionStyle.default, handler: nil))
        return alertController
    }
    
    static func stringToTool(tool: String) -> DrawTools {
        if (tool == "pencil"){
            return DrawTools.pencil
        } else if (tool == "erase_segment") {
            return DrawTools.smallEraser
        } else if (tool == "lasso") {
            return DrawTools.lasso
        } else {
            return DrawTools.bigEraser
        }
    }
    
    static func stringToStyle(style: String) -> DrawStyles {
        if (style == "circle"){
            return DrawStyles.circle
        } else if (style == "square") {
            return DrawStyles.square
        } else if (style == "vertical"){
            return DrawStyles.vertical
        } else {
            return DrawStyles.horizontal
        }
    }
}
