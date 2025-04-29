// Lucent.Client / XeroApiClient.cs
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
       Endpoint mapping
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
        // reports
        ["Report"] = "Reports"
    };

    /* ------------------------------------------------------------------ */
    private readonly ILucentAuth _auth;
    private readonly ILogger<XeroApiClient> _log;
    private readonly RestClient _client;

    public XeroApiClient(
        ILucentAuth auth,
        ILogger<XeroApiClient> log,
        IHttpClientFactory factory)
    {
        _auth = auth;
        _log = log;

        var http = factory.CreateClient("LucentHttp");     // Polly retry policy attached
        http.BaseAddress = new Uri("https://api.xero.com/");

        _client = new RestClient(http);
    }

    /* ------------------------------------------------------------------
       Discover first ORGANISATION tenant
    ------------------------------------------------------------------- */
    public async Task<Guid> DiscoverFirstTenantIdAsync(CancellationToken ct)
    {
        var token = await _auth.GetAccessTokenAsync(ct);

        var resp = await _client.ExecuteGetAsync(
            new RestRequest("connections")
                .AddHeader("Authorization", $"Bearer {token}")
                .AddHeader("Accept", "application/json"), ct);

        if (!resp.IsSuccessful)
            throw new InvalidOperationException($"Connections call failed {(int)resp.StatusCode}: {resp.Content}");

        var org = JsonNode.Parse(resp.Content!)!
                          .AsArray()
                          .FirstOrDefault(n => n!["tenantType"]!.GetValue<string>() == "ORGANISATION")
                  ?? throw new InvalidOperationException("No ORGANISATION tenant found.");

        var id = Guid.Parse(org["id"]!.GetValue<string>());
        _log.LogInformation("Discovered tenant id {Tenant}", id);
        return id;
    }

    /* ------------------------------------------------------------------
       Fetch paged endpoint
    ------------------------------------------------------------------- */
    public async IAsyncEnumerable<(int pageNo, string json)> FetchAllPagesAsync(
        string endpoint, DateTime? modifiedSinceUtc, Guid tenantId,
        [EnumeratorCancellation] CancellationToken ct)
    {
        if (!_endpointPath.TryGetValue(endpoint, out var path))
        {
            _log.LogWarning("Endpoint {Endpoint} not mapped — skipping.", endpoint);
            yield break;
        }

        var token = await _auth.GetAccessTokenAsync(ct);
        int page = 1;
        int totalPages = 1;

        while (page <= totalPages)
        {
            var req = new RestRequest(path)
                         .AddHeader("Authorization", $"Bearer {token}")
                         .AddHeader("Xero-tenant-id", tenantId.ToString())
                         .AddHeader("Accept", "application/json")
                         .AddQueryParameter("page", page.ToString());

            if (modifiedSinceUtc is { } msu)
                req.AddHeader("If-Modified-Since", msu.ToUniversalTime().ToString("R"));

            var resp = await _client.ExecuteAsync(req, ct);

            /* 429 throttling ------------------------------------------------ */
            if ((int)resp.StatusCode == 429)
            {
                var seconds = 60;
                if (resp.Headers?.FirstOrDefault(h => h.Name == "Retry-After")?.Value is string s &&
                    int.TryParse(s, out var parsed))
                    seconds = parsed;

                await Task.Delay(TimeSpan.FromSeconds(seconds), ct);
                continue;
            }

            if (resp.StatusCode == HttpStatusCode.Unauthorized &&
                resp.Content?.Contains("AuthorizationUnsuccessful") == true)
            {
                _log.LogWarning("{Endpoint}: 401 AuthorizationUnsuccessful — skipping.", endpoint);
                yield break;
            }

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                _log.LogWarning("{Endpoint}: 404 Not Found — skipping.", endpoint);
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
