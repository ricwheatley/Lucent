#nullable enable
using System;

namespace Lucent.Tests.Unit.Core;

/// <summary>
/// Minimal domain entity used in demos and unit-tests.
/// Declared as a <c>record</c> so value-equality Just Works™.
/// </summary>
/// <param name="Id">The unique identifier supplied by the caller.</param>
/// <param name="Name">A human-readable display name (required).</param>
public sealed record ExampleModel(Guid Id, string Name);
