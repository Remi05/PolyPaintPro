import UIKit

class ConversationTableViewCell: UITableViewCell {
    //MARK: Properties
    @IBOutlet weak var photo: UIImageView!
    @IBOutlet weak var name: UILabel!
    var activeConversationId: String = ""
    
    override func awakeFromNib() {
        super.awakeFromNib()
        photo.image = UIImage(named: "Anonymous")
        photo.layer.borderWidth = 1
        photo.layer.masksToBounds = false
        photo.layer.borderWidth = 0;
        photo.layer.cornerRadius = photo.frame.height/2
        photo.clipsToBounds = true
    }
    
    override func setSelected(_ selected: Bool, animated: Bool) {
        super.setSelected(selected, animated: animated)
    }
}
