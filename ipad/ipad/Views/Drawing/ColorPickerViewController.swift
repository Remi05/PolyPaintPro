import UIKit
import ChromaColorPicker

protocol ColorPickerViewControllerDelegate {
    func setColor(selectedColor : UIColor)
}

class ColorPickerViewController: UIViewController {
    
    let colorPicker = ChromaColorPicker(frame: CGRect(x: 0, y: 30, width: 380, height: 380))
    var delegate: ColorPickerViewControllerDelegate?
    var currentColor : UIColor!

    
    override func viewDidLoad() {
        super.viewDidLoad()
        self.initColorPicker()
        colorPicker.delegate = self
    }
    
    func initColorPicker() {
        self.colorPicker.padding = 5
        self.colorPicker.stroke = 3
        self.colorPicker.supportsShadesOfGray = true
    
        if(currentColor == nil){
            self.colorPicker.adjustToColor(UIColor.black)
        } else {
            self.colorPicker.adjustToColor(currentColor)
        }
        view.addSubview(colorPicker)
    }
    
    override func viewWillDisappear(_ animated: Bool) {
        delegate?.setColor(selectedColor : self.colorPicker.currentColor)
    }
}

extension ColorPickerViewController: ChromaColorPickerDelegate {
    func colorPickerDidChooseColor(_ colorPicker: ChromaColorPicker, color: UIColor) {
        delegate?.setColor(selectedColor : self.colorPicker.currentColor)
        self.dismiss(animated: true, completion: nil)
    }
}
