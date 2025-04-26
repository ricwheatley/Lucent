using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Lucent.Auth;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Lucent.Client;

public interface ILucentClient
{
    IAsyncEnumerable<(int pageNo, string json)> FetchAllPagesAsync(
        string endpoint, DateTime? modifiedSinceUtc, Guid tenantId, CancellationToken ct);

    Task<Guid> DiscoverFirstTenantIdAsync(CancellationToken ct);
}

public sealed class XeroApiClient : ILucentClient
{
    /* ------------------------------------------------------------------
       Mapping: singular table name  →  actual API path
       (Endpoints that are already singular map to themselves.)
    ------------------------------------------------------------------- */
    private static readonly Dictionary<string, string> _endpointPath = new(StringComparer.OrdinalIgnoreCase)
    {
        // core
        ["Account"] = "Accounts",
        ["Contact"] = "Contacts",
        ["ContactGroup"] = "ContactGroups",
        ["Currency"] = "Currencies",
        ["BrandingTheme"] = "BrandingThemes",
        ["TrackingCategory"] = "TrackingCategories",
        ["TrackingOption"] = "TrackingOptions",
        ["TaxRate"] = "TaxRates",
        ["User"] = "Users",
        // transactions
        ["Invoice"] = "Invoices",
        ["CreditNote"] = "CreditNotes",
        ["Payment"] = "Payments",
        ["BatchPayment"] = "BatchPayments",
        ["BankTransaction"] = "BankTransactions",
        ["BankTransfer"] = "BankTransfers",
        ["Prepayment"] = "Prepayments",
        ["Overpayment"] = "Overpayments",
        ["ExpenseClaim"] = "ExpenseClaims",
        ["Receipt"] = "Receipts",
        ["PurchaseOrder"] = "PurchaseOrders",
        ["Quote"] = "Quotes",
        ["RepeatingInvoice"] = "RepeatingInvoices",
        ["ManualJournal"] = "ManualJournals",
        ["Journal"] = "Journals",
        ["LinkedTransaction"] = "LinkedTransactions",
        // low-volume or settings
        ["Item"] = "Items",
        ["Employee"] = "Employees",
        ["Organisation"] = "Organisation",
        ["Budget"] = "Budgets",
        // reports (the Accounting Reports API)
        ["Report"] = "Reports"
    };

    /* ------------------------------------------------------------------ */
    private readonly ILucentAuth _auth;
    private readonly ILogger<XeroApiClient> _log;
    private readonly RestClient _client = new("https://api.xero.com/api.xro/2.0/");

    public XeroApiClient(ILucentAuth auth, ILogger<XeroApiClient> log)
    {
        _auth = auth;
        _log = log;
    }

    /*  Discover first ORGANISATION tenant  */
    public async Task<Guid> DiscoverFirstTenantIdAsync(CancellationToken ct)
    {
        string token = await _auth.GetAccessTokenAsync(ct);

        var resp = await new RestClient()
            .ExecuteGetAsync(new RestRequest("https://api.xero.com/connections")
                .AddHeader("Authorization", $"Bearer {token}")
                .AddHeader("Accept", "application/json"), ct);

        if (!resp.IsSuccessful)
            throw new InvalidOperationException($"Connections call failed {(int)resp.StatusCode}: {resp.Content}");

        var org = JsonNode.Parse(resp.Content!)!
                          .AsArray()
                          .FirstOrDefault(n => n!["tenantType"]!.GetValue<string>() == "ORGANISATION")
                  ?? throw new InvalidOperationException("No ORGANISATION tenant found.");

        Guid id = Guid.Parse(org["id"]!.GetValue<string>());
        _log.LogInformation("Discovered tenant id {Tenant}", id);
        return id;
    }

    /*  Fetch paged endpoint  */
    public async IAsyncEnumerable<(int pageNo, string json)> FetchAllPagesAsync(
        string endpoint, DateTime? modifiedSinceUtc, Guid tenantId,
        [EnumeratorCancellation] CancellationToken ct)
    {
        if (!_endpointPath.TryGetValue(endpoint, out var path))
        {
            _log.LogWarning("Endpoint {Endpoint} not mapped – skipping.", endpoint);
            yield break;
        }

        string token = await _auth.GetAccessTokenAsync(ct);
        int page = 1, totalPages = 1;

        while (page <= totalPages)
        {
            var req = new RestRequest(path)
                         .AddHeader("Authorization", $"Bearer {token}")
                         .AddHeader("Xero-tenant-id", tenantId.ToString())
                         .AddHeader("Accept", "application/json");

            // only endpoints that support paging need the parameter
            req.AddQueryParameter("page", page.ToString());

            if (modifiedSinceUtc is { } msu)
                req.AddHeader("If-Modified-Since", msu.ToUniversalTime().ToString("R"));

            var resp = await _client.ExecuteAsync(req, ct);

            /* 429 throttling ------------------------------------------------ */
            if ((int)resp.StatusCode == 429)
            {
                int seconds = 60;  // default value

                // Ensure resp.Headers is not null before accessing it
                if (resp.Headers != null)
                {
                    var retryHeader = resp.Headers.FirstOrDefault(h => h.Name == "Retry-After");
                    if (retryHeader?.Value != null && int.TryParse(retryHeader.Value.ToString(), out var parsedSeconds))
                    {
                        seconds = parsedSeconds;
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(seconds), ct);
                continue;
            }

            if (resp.StatusCode == HttpStatusCode.Unauthorized &&
                resp.Content?.Contains("AuthorizationUnsuccessful") == true)
            {
                _log.LogWarning("{Endpoint}: 401 AuthorizationUnsuccessful – skipping.", endpoint);
                yield break;   // continue with the next endpoint
            }

            /* 404 – endpoint not available for this org / plan ------------- */
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _log.LogWarning("{Endpoint}: 404 Not Found – skipping.", endpoint);
                yield break;
            }

            if (!resp.IsSuccessful)
                throw new HttpRequestException(
                    $"Xero call ({endpoint} page {page}) failed: {(int)resp.StatusCode} {resp.Content}");

            yield return (page, resp.Content!);

            totalPages = JsonNode.Parse(resp.Content!)?["pagination"]?["pageCount"]?.GetValue<int>() ?? 1;
            page++;
        }
    }
}
