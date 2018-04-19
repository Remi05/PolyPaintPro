using System.IO;
using System.Threading.Tasks;

namespace PolyPaint.Services.Storage
{
    class FirebaseStorageReference : IStorageReference
    {
        private Firebase.Storage.FirebaseStorageReference Reference { get; }

        public FirebaseStorageReference(Firebase.Storage.FirebaseStorageReference reference)
        {
            Reference = reference;
        }

        public IStorageReference Child(string path)
        {
            return new FirebaseStorageReference(Reference.Child(path));
        }

        public async Task Delete()
        {
            await Reference.DeleteAsync();
        }

        public async Task<string> GetDownloadUrl()
        {
            return await Reference.GetDownloadUrlAsync();
        }

        public async Task Put(Stream stream)
        {
            await Reference.PutAsync(stream);
        }
    }
}
