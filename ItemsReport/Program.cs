using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ItemsReport.Services;
using ItemsReport;
using CommonCode;

var builder = WebApplication.CreateBuilder(args);

Config config = Config.GetConfig("Config.json");

builder.Services.AddControllers();
builder.Services.AddSingleton(config);
builder.Services.AddSingleton<ReportService>();
builder.Services.AddSingleton<ICacheHandler>(new CacheHandler(config));

var app = builder.Build();

app.UseFileServer(new FileServerOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(PPath.GetExeDirectory()),
    RequestPath = "",
    EnableDirectoryBrowsing = false
});

app.MapControllers();

app.Run();
