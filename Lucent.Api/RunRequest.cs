using System;

namespace Lucent.Api;

public sealed record RunRequest(
    DateTime StartDate,
    DateTime EndDate,
    Guid TenantId,
    bool OdsTables
);
