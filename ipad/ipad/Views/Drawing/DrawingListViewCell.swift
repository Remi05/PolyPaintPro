import UIKit

class DrawingListViewCell: UICollectionViewCell {
    @IBOutlet weak var image: UIImageView!
    @IBOutlet weak var nsfw: UIImageView!
    
    override func awakeFromNib() {
        super.awakeFromNib()
        self.image.layer.masksToBounds = true
        self.image.layer.borderWidth = 1
        self.image.layer.borderColor = UIColor.lightGray.cgColor
    }
    
    func setNsfw() {
        self.nsfw.isHidden = false
        self.nsfw.transform = CGAffineTransform.identity.rotated(by: .pi / -4)
        let imageToBlur = CIImage(image: image.image!)
        let blurFilter = CIFilter(name: "CIGaussianBlur")
        blurFilter!.setValue(imageToBlur, forKey: kCIInputImageKey)
        blurFilter!.setValue(4, forKey: kCIInputRadiusKey)
        image.image = UIImage(ciImage: blurFilter!.outputImage!)
    }
    
    func resetNsfw() {
        self.nsfw.isHidden = true
    }
}
