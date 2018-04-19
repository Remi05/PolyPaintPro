using Slofth.Firebase.Database;

namespace PolyPaint.Services.Database
{
    public class FirebaseFilterQuery : FirebaseQuery, IFilterQuery
    {
        private FilterQuery Query { get; }

        public FirebaseFilterQuery(FilterQuery query) : base(query)
        {
            Query = query;
        }

        public IFilterQuery EndAt(string value)
        {
            return new FirebaseFilterQuery(Query.EndAt(value));
        }

        public IFilterQuery EndAt(int value)
        {
            return new FirebaseFilterQuery(Query.EndAt(value));
        }

        public IFilterQuery EqualTo(string value)
        {
            return new FirebaseFilterQuery(Query.EqualTo(value));
        }

        public IFilterQuery LimitToFirst(int value)
        {
            return new FirebaseFilterQuery(Query.LimitToFirst(value));
        }

        public IFilterQuery LimitToLast(int value)
        {
            return new FirebaseFilterQuery(Query.LimitToLast(value));
        }

        public IFilterQuery StartAt(string value)
        {
            return new FirebaseFilterQuery(Query.StartAt(value));
        }

        public IFilterQuery StartAt(int value)
        {
            return new FirebaseFilterQuery(Query.StartAt(value));
        }
    }
}