using System.Configuration;
using Firebase.Storage;

namespace PolyPaint.Services.Storage
{
    public class FirebaseStorageService : IStorageService
    {
        private string StorageBucket { get { return ConfigurationManager.AppSettings.Get("StorageBucket"); } }

        private FirebaseStorage Storage { get; }

        public FirebaseStorageService()
        {
            Storage = new FirebaseStorage(StorageBucket);
        }

        public IStorageReference Ref(string path)
        {
            return new FirebaseStorageReference(Storage.Child(path));
        }
    }
}
