namespace PolyPaint.Services.Database
{
    public interface IDatabaseService
    {
        IChildQuery Ref(string name);
    }
}