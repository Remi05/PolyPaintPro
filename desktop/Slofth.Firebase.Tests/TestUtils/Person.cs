using Newtonsoft.Json;

namespace PolyPaint.Tests
{
    class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Motto { get; set; }

        public Person(string name, int age, string motto)
        {
            Name = name;
            Age = age;
            Motto = motto;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
