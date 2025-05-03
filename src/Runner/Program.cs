// Lucent.Runner / Program.cs  – aligned with other hosts
using Microsoft.Extensions.Logging;

const string CorsPolicyName = "api";          // <— single source of truth

var builder = WebApplication.CreateBuilder(args);

/* ─── configuration & logging (same recipe as Loader/API) ────────────── */
builder.Configuration
       .AddJsonFile("config/appsettings.Development.json", optional: true, reloadOnChange: true)
       .AddUserSecrets<Program>(optional: true)
       .AddEnvironmentVariables();

builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Services.AddLogging(c => c.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss ";
}));

/* ─── services ───────────────────────────────────────────────────────── */
builder.Services.AddRazorPages();

/* CORS so the UI can call the dev API --------------------------------- */
var apiOrigin = builder.Configuration["Runner:ApiOrigin"]
             ?? "http://localhost:5214";              // default dev URL
builder.Services.AddCors(o => o.AddPolicy(CorsPolicyName, p =>
    p.WithOrigins(apiOrigin).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

/* ─── middleware & endpoints ─────────────────────────────────────────── */
app.UseStaticFiles();
app.UseRouting();
app.UseCors(CorsPolicyName);
app.MapRazorPages();

app.Run();
