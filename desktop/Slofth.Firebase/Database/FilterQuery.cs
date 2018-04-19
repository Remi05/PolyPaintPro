using Slofth.Firebase.Utils;
using System;
using System.Threading.Tasks;

namespace Slofth.Firebase.Database
{
    public class FilterQuery : Query
    {
        internal FilterQuery(UrlBuilder urlBuilder, string name, Func<Task<string>> idTokenFactory)
             : base(urlBuilder, name, idTokenFactory)
        { }

        public FilterQuery LimitToFirst(int value)
        {
            return new FilterQuery(UrlBuilder.AddParam(Params.LimitToFirst, value), Key, IdTokenFactory);
        }

        public FilterQuery LimitToLast(int value)
        {
            return new FilterQuery(UrlBuilder.AddParam(Params.LimitToLast, value), Key, IdTokenFactory);
        }

        public FilterQuery StartAt(string value)
        {
            return new FilterQuery(UrlBuilder.AddParam(Params.StartAt, Quote(value)), Key, IdTokenFactory);
        }

        public FilterQuery StartAt(int value)
        {
            return new FilterQuery(UrlBuilder.AddParam(Params.StartAt, value), Key, IdTokenFactory);
        }

        public FilterQuery EndAt(string value)
        {
            return new FilterQuery(UrlBuilder.AddParam(Params.EndAt, Quote(value)), Key, IdTokenFactory);
        }

        public FilterQuery EndAt(int value)
        {
            return new FilterQuery(UrlBuilder.AddParam(Params.EndAt, value), Key, IdTokenFactory);
        }

        public FilterQuery EqualTo(string value)
        {
            return new FilterQuery(UrlBuilder.AddParam(Params.EqualTo, Quote(value)), Key, IdTokenFactory);
        }

        class Params
        {
            public static readonly string LimitToFirst = "limitToFirst";
            public static readonly string LimitToLast = "limitToLast";
            public static readonly string StartAt = "startAt";
            public static readonly string EndAt = "endAt";
            public static readonly string EqualTo = "equalTo";
        }
    }
}
