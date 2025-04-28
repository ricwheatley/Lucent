using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Lucent.Core.Scheduling;

public sealed class JsonTenantScheduleStore : ITenantScheduleStore
{
    private string _path;
    private readonly IConfiguration _cfg;
    private readonly JsonSerializerOptions _opts =
        new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public JsonTenantScheduleStore(string path, IConfiguration cfg)
    {
        _path = path;
        _cfg = cfg;          // keep a reference for the fallback
    }

    /* ------------------------------------------------------------------
       1.  Read – returns an *always-non-null* list
    ------------------------------------------------------------------*/
    public async Task<IReadOnlyList<TenantSchedule>> LoadAsync(
                                        CancellationToken ct = default)
    {
        /* 1A – primary file (inside Lucent.Api/config/) */
        if (File.Exists(_path))
        {
            await using var fs = File.OpenRead(_path);
            return (await JsonSerializer.DeserializeAsync<List<TenantSchedule>>(fs, _opts, ct))
                   ?? new List<TenantSchedule>();
        }

        /* 1B – fallback to solution-level config folder */
        var sharedCfgPath = ConfigPathHelper.GetSharedConfigPath();
        var altPath = Path.Combine(
                Path.GetDirectoryName(sharedCfgPath)!,
                "tenant-schedule.json");

        if (File.Exists(altPath))
        {
            _path = altPath;                     // remember for SaveAsync
            await using var fs = File.OpenRead(altPath);
            return (await JsonSerializer.DeserializeAsync<List<TenantSchedule>>(fs, _opts, ct))
                   ?? new List<TenantSchedule>();
        }

        /* 1C – final fallback: build rows from Schedule:Tenants */
        var defaultTenants = _cfg.GetSection("Schedule:Tenants").Get<string[]>()
                            ?? Array.Empty<string>();

        return defaultTenants.Select(id => new TenantSchedule(
                        TenantId: Guid.Parse(id),
                        TenantName: null,          // not used in this context
                        RunTime: new(1, 0),          // 01:00
                        StartDate: DateOnly.FromDateTime(DateTime.UtcNow.Date),
                        EndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1).Date),
                        EnabledEndpoints: new HashSet<string>()))
                    .ToList();
    }

    /* ------------------------------------------------------------------
       2.  Write – always writes to whatever _path currently points at
    ------------------------------------------------------------------*/
    public async Task SaveAsync(IReadOnlyList<TenantSchedule> model,
                                CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        await using var fs = File.Create(_path);
        await JsonSerializer.SerializeAsync(fs, model, _opts, ct);
    }
}
