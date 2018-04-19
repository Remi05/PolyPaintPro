import UIKit

class MessageTableViewCell: UITableViewCell {
    @IBOutlet weak var authorName: UILabel?
    @IBOutlet weak var content: UILabel!
    @IBOutlet weak var container: UIView!
    @IBOutlet weak var timestamp: UILabel?
    
    private var isSelfMessage: Bool = true

    override func awakeFromNib() {
        super.awakeFromNib()
        
        self.container.layer.cornerRadius = 17
    }

    override func setSelected(_ selected: Bool, animated: Bool) {
        super.setSelected(selected, animated: animated)
    }
}
