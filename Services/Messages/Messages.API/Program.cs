using Messages.Infrastructure;              
using Messages.Infrastructure.Data;         
using Microsoft.EntityFrameworkCore;
using Messages.API.Endpoints;
//using Messages.Infrastructure.Data.Migrations;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMessagesInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MessagesDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o =>
    {
        o.Title = "Messages API";
        o.WithTheme(ScalarTheme.Mars);
    }).WithDisplayName("API Docs");
}

app.MapMessagesEndpoints();

app.Run();