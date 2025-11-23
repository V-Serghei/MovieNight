using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using People.API.Endpoints;
using People.Infrastructure;
using People.Infrastructure.Data;
using Scalar.AspNetCore;


        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();

        // JSON: enum CreditRole как строки ("Actor", "Director"...)
        builder.Services.ConfigureHttpJsonOptions(o =>
        {
            o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        // Infra (DbContext + репозиторий)
        builder.Services.AddPeopleInfrastructure(builder.Configuration);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PeopleDbContext>();
            await db.Database.MigrateAsync();
        }

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(o => o.Title = "Tokens API");
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapPeopleEndpoints();

        app.Run();
