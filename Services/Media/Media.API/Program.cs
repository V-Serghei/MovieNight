using Media.API.Endpoints;
using Media.Core.Repositories;
using Media.Infrastructure;
using Media.Infrastructure.Data;
using Media.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddMediaInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
    await db.Database.MigrateAsync();
    // TODO : Enable seeders as needed
    //await Media.Infrastructure.Seed.MediaFilmPosterSeeder.SeedAsync(db);
    //await Media.Infrastructure.Seed.MediaSampleVideoSeeder.SeedAsync(db);
    //await Media.Infrastructure.Seed.DefaultImageSeeder.SeedAsync(db); 
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o.Title = "Media API");
}


app.MapMediaEndpoints();

app.Run();