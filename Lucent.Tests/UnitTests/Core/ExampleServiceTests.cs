using System;

namespace Lucent.Core.Models;

/// <summary>
/// Core domain entity for examples and demos.
/// </summary>
public class ExampleModel
{
    /// <summary>
    /// Primary key – callers must supply it.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Human-readable name – also required.
    /// </summary>
    public required string Name { get; init; }

    // Convenience ctor for terse object builds in tests
    public ExampleModel(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    // Parameter-less ctor keeps object-initialiser syntax working
    public ExampleModel() { }
}
