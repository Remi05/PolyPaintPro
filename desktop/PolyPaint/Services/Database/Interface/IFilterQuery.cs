namespace PolyPaint.Services.Database
{
    public interface IFilterQuery : IQuery
    {
        IFilterQuery EndAt(string value);
        IFilterQuery EndAt(int value);
        IFilterQuery EqualTo(string value);
        IFilterQuery LimitToFirst(int value);
        IFilterQuery LimitToLast(int value);
        IFilterQuery StartAt(string value);
        IFilterQuery StartAt(int value);
    }
}