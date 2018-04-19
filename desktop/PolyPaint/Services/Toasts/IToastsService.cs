namespace PolyPaint.Services.Toasts
{
    public interface IToastsService
    {
        void Pop(string title, string description, string imageUri);
    }
}