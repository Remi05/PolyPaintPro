namespace PolyPaint.Services.Database
{
    public interface IChildQuery : IQuery
    {
        IFilterQuery OrderBy(string property);
        IFilterQuery OrderByKey();
        IFilterQuery OrderByPriority();
        IFilterQuery OrderByValue();
    }
}