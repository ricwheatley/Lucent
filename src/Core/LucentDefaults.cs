namespace Lucent.Core;

public static class LucentDefaults
{
    public const int TokenEarlyExpirySeconds = 300;

    public const int HttpRetryCount = 3;
    public static readonly TimeSpan HttpBaseDelay = TimeSpan.FromSeconds(2);

    public const int SqlRetryCount = 5;
    public static readonly TimeSpan SqlBaseDelay = TimeSpan.FromMilliseconds(250);
}
