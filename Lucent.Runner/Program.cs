var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddCors(o => o.AddPolicy("api", p =>
    p.WithOrigins("http://localhost:5214")    // Lucent.Api dev URL
     .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("api");
app.MapRazorPages();
app.Run();
