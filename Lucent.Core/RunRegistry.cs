using System;
using System.Collections.Concurrent;

namespace Lucent.Core;

public sealed class RunRegistry
{
    private readonly ConcurrentDictionary<string, RunStatus> _runs = new();

    public string Register() => Guid.NewGuid().ToString("N")[..8];
    public void Set(string id, RunStatus s) => _runs[id] = s;
    public bool TryGet(string id, out RunStatus s) => _runs.TryGetValue(id, out s);
}
