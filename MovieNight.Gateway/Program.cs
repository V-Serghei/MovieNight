using MovieNight.Gateway.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
// Swagger/OpenAPI
builder.Services.AddOpenApi();
//  CORS.
//  Next `/api/gw`, CORS not needed.
// builder.Services.AddCors(options =>
// {
//      options.AddPolicy("Frontend", policy =>
//                policy
//                     .WithOrigins(
//                          "http://localhost:3000", // Next dev
//                          "http://localhost:5173"  // Vite
//                     )
//                     .AllowAnyHeader()
//                     .AllowAnyMethod()
//                     .AllowCredentials() 
//      );
// });
builder.Services.AddHttpClient("auth", c =>
{
     var baseUrl = builder.Configuration["Services:Auth"] ?? "http://localhost:7010";
     c.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();
 
if (app.Environment.IsDevelopment())
{
     app.MapOpenApi();
     app.MapScalarApiReference(options =>
          {
               options.Title = "MovieNight Gateway API";
               options.WithTheme(ScalarTheme.Mars); 
          })
          .WithDisplayName("API Docs");
}

app.UseHttpsRedirection(); 

app.MapHealth();
app.MapDebug();
app.MapApiV1();
app.MapAuthProxy();

app.Run();