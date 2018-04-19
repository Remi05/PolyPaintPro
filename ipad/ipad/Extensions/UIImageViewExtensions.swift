import UIKit

extension UIImageView {
    func downloadedFrom(url: URL, contentMode mode: UIViewContentMode = .scaleAspectFill, completion: @escaping (Bool) -> ()) {
        contentMode = mode
        URLSession.shared.dataTask(with: url) { data, response, error in
            guard
                let httpURLResponse = response as? HTTPURLResponse, httpURLResponse.statusCode == 200,
                let mimeType = response?.mimeType, mimeType.hasPrefix("image"),
                let data = data, error == nil,
                let image = UIImage(data: data)
                else {
                    completion(false)
                    return
            }
            DispatchQueue.main.async() {
                self.image = image
                completion(true)
            }
            }.resume()
    }
    
    func downloadedFrom(link: String, contentMode mode: UIViewContentMode = .scaleAspectFill, completion: @escaping (Bool) -> ()) {
        guard let url = URL(string: link) else { return }
        downloadedFrom(url: url, contentMode: mode, completion: completion)
    }
}
