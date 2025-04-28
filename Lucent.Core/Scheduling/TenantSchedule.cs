namespace Lucent.Core.Scheduling;

/// <summary>
/// Persisted schedule row for one tenant.
/// </summary>
public sealed record TenantSchedule(
    Guid TenantId,          // e.g. 9b0d-…
    string? TenantName, // e.g. "My Company"
    TimeOnly RunTime,           // daily HH:mm for the cron job
    DateOnly StartDate,         // load window
    DateOnly EndDate,
    HashSet<string> EnabledEndpoints   // which endpoints we actually fetch
);
