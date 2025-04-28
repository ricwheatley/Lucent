namespace Lucent.Core.Models
{
    /// <summary>
    /// Example domain model for Lucent.
    /// </summary>
    public class ExampleModel
    {
        // Callers are obliged to set this, so it's never left null.
        public required string Name { get; init; }

        // You can still include an optional constructor for convenience,
        // but the object-initialiser syntax will work too.
        public ExampleModel() { }

        // Optional convenience constructor
        public ExampleModel(string name)
        {
            Name = name;
        }
    }
}
