using Lucent.Core;
using Quartz;

namespace Lucent.Api;

public static class RunRegistryAccessor
{
    public const string Key = "RunRegistry";

    public static void Put(this IScheduler scheduler, RunRegistry reg) =>
        scheduler.Context.Put(Key, reg);

    public static RunRegistry? Get(this IScheduler scheduler) =>
        scheduler.Context.Get(Key) as RunRegistry;
}
