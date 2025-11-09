using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Users.API.Endpoints;
using Users.Core;
using Users.Infrastructure;
using Users.Infrastructure.Data;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();


builder.Services.AddUsersInfrastructure(builder.Configuration);


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    await db.Database.MigrateAsync();
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o.Title = "Users API");
}


app.MapUserEndpoints();


app.Run();