using System.IO;
using System.Threading.Tasks;

namespace PolyPaint.Services.Storage
{
    public interface IStorageReference
    {
        IStorageReference Child(string path);

        Task Delete();

        Task<string> GetDownloadUrl();

        Task Put(Stream stream);
    }
}
