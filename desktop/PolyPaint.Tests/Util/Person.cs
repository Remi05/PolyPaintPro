namespace PolyPaint.Tests.Util
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    internal class Person
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Animal Animal { get; set; }

        public Person(string name, int age, Animal animal)
        {
            Name = name;
            Age = age;
            Animal = animal;
        }

        public override bool Equals(object obj)
        {
            var person = obj as Person;
            if (person == null) return false;

            return Age == person.Age && Name == person.Name && Animal.Equals(person.Animal);
        }
    }
}
