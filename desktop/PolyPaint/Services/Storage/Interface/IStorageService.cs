namespace PolyPaint.Services.Storage
{
    public interface IStorageService
    {
        IStorageReference Ref(string path);
    }
}
