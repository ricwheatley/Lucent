using Microsoft.Data.SqlClient;

namespace Lucent.Loader;
public interface ILucentLoader
{
    // Define a method to load data for a specific tenant and report.
    Task LoadDataAsync(Guid tenantId, string report, DateTime asAt, SqlConnection conn, CancellationToken ct);
}
