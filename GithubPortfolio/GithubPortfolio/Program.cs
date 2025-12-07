// GithubPortfolio\Program.cs
using GithubService;
using GithubService.CacheServices;
using Microsoft.Extensions.Options;
using Octokit;

var builder = WebApplication.CreateBuilder(args);

// הוספת שירותים ל-Service Container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.Configure<GitHubOptions>(
    builder.Configuration.GetSection("GithubToken"));

builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.Decorate<IGitHubService, GitHubCacheService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();