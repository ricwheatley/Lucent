using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lucent.Core;

public interface ITenantScheduleStore
{
    Task<IReadOnlyList<TenantSchedule>> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(IReadOnlyList<TenantSchedule> schedules,
                   CancellationToken ct = default);
}
