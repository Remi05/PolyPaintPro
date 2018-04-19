namespace PolyPaint.Tests.Util
{
    public enum Specie { Camel, Cat, Dog, Llama, Sloth, Squirrel, Wombat }

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class Animal
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public Specie Specie { get; set; }
        public string Name { get; set; }

        public Animal(Specie specie, string name)
        {
            Specie = specie;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            var animal = obj as Animal;
            if (animal == null) return false;

            return Specie == animal.Specie && Name == animal.Name;
        }
    }
}
