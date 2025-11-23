using Review.Infrastructure;              
using Review.Infrastructure.Data;         
using Microsoft.EntityFrameworkCore;
using Review.API.Endpoints;
//using Review.Infrastructure.Data.Migrations;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddReviewInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o =>
    {
        o.Title = "Review API";
        o.WithTheme(ScalarTheme.Mars);
    }).WithDisplayName("API Docs");
}

app.MapReviewEndpoints();

app.Run();