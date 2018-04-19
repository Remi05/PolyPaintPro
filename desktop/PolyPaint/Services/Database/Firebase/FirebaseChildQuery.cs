using Slofth.Firebase.Database;

namespace PolyPaint.Services.Database
{
    public class FirebaseChildQuery : FirebaseQuery, IChildQuery
    {
        private ChildQuery Query { get; }

        public FirebaseChildQuery(ChildQuery query) : base(query)
        {
            Query = query;
        }

        public IFilterQuery OrderBy(string property)
        {
            return new FirebaseFilterQuery(Query.OrderBy(property));
        }

        public IFilterQuery OrderByKey()
        {
            return new FirebaseFilterQuery(Query.OrderByKey());
        }

        public IFilterQuery OrderByPriority()
        {
            return new FirebaseFilterQuery(Query.OrderByPriority());
        }

        public IFilterQuery OrderByValue()
        {
            return new FirebaseFilterQuery(Query.OrderByValue());
        }
    }
}