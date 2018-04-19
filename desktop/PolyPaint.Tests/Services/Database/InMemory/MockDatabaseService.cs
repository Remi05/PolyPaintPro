using Newtonsoft.Json.Linq;
using PolyPaint.Services.Database;
using System.Collections.Generic;

namespace PolyPaint.Tests.Services.Database
{
    class MockDatabaseService : IDatabaseService
    {
        private JObject Database { get; set; } = new JObject();
        private Dictionary<string, MockChildQuery> Refs { get; set; } = new Dictionary<string, MockChildQuery>();

        public IChildQuery Ref(string name)
        {
            if (Database[name] == null) { Database[name] = new JObject(); }
            if (!Refs.ContainsKey(name)) { Refs[name] = new MockChildQuery(name, Database); }

            return Refs[name];
        }

        public override string ToString()
        {
            return Database.ToString();
        }
    }
}
