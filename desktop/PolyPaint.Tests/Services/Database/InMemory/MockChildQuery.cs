using Newtonsoft.Json.Linq;
using PolyPaint.Services.Database;

namespace PolyPaint.Tests.Services.Database
{
    class MockChildQuery : MockQuery, IChildQuery
    {
        public MockChildQuery(string key, JObject parent)
            : base(key, parent)
        {
        }

        public IFilterQuery OrderBy(string property)
        {
            return null;
        }

        public IFilterQuery OrderByKey()
        {
            return null;
        }

        public IFilterQuery OrderByPriority()
        {
            return null;
        }

        public IFilterQuery OrderByValue()
        {
            return null;
        }
    }
}
