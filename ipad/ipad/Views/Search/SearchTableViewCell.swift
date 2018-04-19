import UIKit

class SearchTableViewCell: UITableViewCell {
    @IBOutlet weak var name: UILabel!
    @IBOutlet weak var photo: UIImageView!
    
    override func awakeFromNib() {
        super.awakeFromNib()
        photo.layer.borderWidth = 1
        photo.layer.masksToBounds = false
        photo.layer.borderWidth = 0;
        photo.layer.cornerRadius = photo.frame.height / 2
        photo.clipsToBounds = true
    }
}
