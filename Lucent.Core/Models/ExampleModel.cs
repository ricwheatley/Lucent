namespace Lucent.Core.Models;

/// <summary>
/// Example domain entity for Lucent.
/// </summary>
public class ExampleModel
{
    /// <summary>Primary key.</summary>
    public required int Id { get; init; }

    /// <summary>Human-readable name.</summary>
    public required string Name { get; init; }

    // parameter-less ctor so object-initialiser syntax works
    public ExampleModel() { }

    // convenience ctor for terse builds in tests
    public ExampleModel(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
