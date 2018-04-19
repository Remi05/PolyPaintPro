using Newtonsoft.Json;

namespace PolyPaint.Tests.Services.Database.Firebase
{
    internal class Person
    {
        public Person(string name, int age, string motto)
        {
            Name = name;
            Age = age;
            Motto = motto;
        }

        public string Name { get; set; }
        public int Age { get; set; }
        public string Motto { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}