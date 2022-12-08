using Api.Services.Interfaces;

using Api;
using Api.Configuration;

using Domain;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Telegram.Bot;
using Api.Services;
using System;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

builder.Services.Configure<AppConfiguration>(builder.Configuration);
builder.Services.Configure<BotConfiguration>(builder.Configuration);
builder.Services.AddSingleton<ICloudStorage, GoogleCloudStorage>();

builder.Services.AddDomainServices((sp, options) =>
{
    IOptionsMonitor<AppConfiguration> configuration = sp.GetRequiredService<IOptionsMonitor<AppConfiguration>>();
    _ = options.UseNpgsql(Environment.GetEnvironmentVariable("SIRIUS_DOGS_DB_CONNECTION_STRING", EnvironmentVariableTarget.Machine));
});

builder.Services.AddHttpClient("tgclient")
    .AddTypedClient<ITelegramBotClient>((client, sp) =>
    {
        IOptionsMonitor<BotConfiguration> configuration = sp.GetRequiredService<IOptionsMonitor<BotConfiguration>>();
        return new TelegramBotClient(Environment.GetEnvironmentVariable("SIRIUS_DOGS_TG_BOT_TOKEN", EnvironmentVariableTarget.Machine), client);
    });

builder.Services.AddTransient<ITelegramService, TelegramService>();
builder.Services.AddHostedService<InitService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    _ = app.UseHsts();
}

app.UseAuthentication();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();