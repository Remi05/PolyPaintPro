import UIKit
import ScalingCarousel

class FeedCollectionViewCell: ScalingCarouselCell {
    @IBOutlet weak var thumbnailPhoto: UIImageView!
    @IBOutlet weak var likes: UILabel!
    @IBOutlet weak var timestamp: UILabel!
    @IBOutlet weak var profilePicture: UIImageView!
    @IBOutlet weak var userName: UILabel!
    @IBOutlet weak var likeImage: UIImageView!
    @IBOutlet weak var nsfw: UIImageView!
    @IBOutlet weak var likeContainer: UIView!
    @IBOutlet weak var nsfwContainer: UIView!
    
    var isLiked: Bool = false
    var drawingId: String = ""
    var userId: String = ""
    var drawingService = DrawingService()
    var isNsfw: Bool = false
    var toggleImage: UIImage = UIImage()
    
    override func awakeFromNib() {
        super.awakeFromNib()
        self.profilePicture.layer.cornerRadius = self.profilePicture.frame.size.width / 2
        self.profilePicture.clipsToBounds = true
        self.thumbnailPhoto.layer.masksToBounds = true
        self.thumbnailPhoto.layer.borderWidth = 1
        self.thumbnailPhoto.layer.borderColor = UIColor.lightGray.cgColor
        self.nsfwContainer.layer.backgroundColor = UIColor(white: 1, alpha: 0).cgColor
        
        let like = UITapGestureRecognizer(target: self, action: #selector(tapLike))
        self.likeContainer.addGestureRecognizer(like)
        
        let gesture = UITapGestureRecognizer(target: self, action: #selector(tapNsfw))
        self.nsfwContainer.addGestureRecognizer(gesture)
    }
    
    func setNsfw() {
        self.nsfw.isHidden = false
        self.nsfw.transform = CGAffineTransform.identity.rotated(by: .pi / -4)
        self.toggleImage = thumbnailPhoto.image!
        let imageToBlur = CIImage(image: thumbnailPhoto.image!)
        let blurFilter = CIFilter(name: "CIGaussianBlur")
        blurFilter!.setValue(imageToBlur, forKey: kCIInputImageKey)
        blurFilter!.setValue(4, forKey: kCIInputRadiusKey)
        thumbnailPhoto.image = UIImage(ciImage: blurFilter!.outputImage!)
    }
    
    func resetNsfw() {
        self.nsfw.isHidden = true
    }
    
    @objc func tapLike() {
        if self.isLiked {
            drawingService.unlikeDrawing(drawingId: self.drawingId, userId: self.userId)
            self.likeImage.image = UIImage(named: "GrayHeart")
            self.isLiked = false
        } else {
            drawingService.likeDrawing(drawingId: self.drawingId, userId: self.userId)
            self.likeImage.image = UIImage(named: "RedHeart")
            self.isLiked = true
        }
        self.drawingService.numberOfLike(drawindId: self.drawingId, userId: self.userId, callback:  { (count) in
            self.likes.text = String(count) + " J'aime(s)"
        })
    }
    
    @objc func tapNsfw() {
        if self.isNsfw && self.nsfw.isHidden {
            setNsfw()
        } else if self.isNsfw {
            resetNsfw()
            self.thumbnailPhoto.image = self.toggleImage
        }
    }
}
