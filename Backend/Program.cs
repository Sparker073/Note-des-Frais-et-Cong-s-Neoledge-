using Microsoft.EntityFrameworkCore;
using MonBackend.Data;
using MonBackend.Repositories;
using MonBackend.Services;
using MonBackend.Services.Interfaces;
using MonBackend.Repositories.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//connexion à la base de données
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDemandeCongeRepository, DemandeCongeRepository>();
builder.Services.AddScoped<IJourFerieRepository, JourFerieRepository>();
builder.Services.AddScoped<ITarifKmRepository, TarifKmRepository>();
builder.Services.AddScoped<IProjetRepository, ProjetRepository>();
builder.Services.AddScoped<INoteDeFraisRepository, NoteDeFraisRepository>();
builder.Services.AddScoped<ILigneNoteFraisRepository, LigneNoteFraisRepository>();

//services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDemandeCongeService, DemandeCongeService>();
builder.Services.AddScoped<IJourFerieService, JourFerieService>();
builder.Services.AddScoped<ITarifKmService, TarifKmService>();
builder.Services.AddScoped<IProjetService, ProjetService>();
builder.Services.AddScoped<INoteDeFraisService, NoteDeFraisService>();
builder.Services.AddScoped<ILigneNoteFraisService, LigneNoteFraisService>();
builder.Services.AddScoped<IAuthService, AuthService>();



//security configuration

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("authToken"))
                {
                    context.Token = context.Request.Cookies["authToken"];
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost5173",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowLocalhost5173");

app.UseHttpsRedirection();
app.MapControllers();
app.UseDeveloperExceptionPage();
//security configuration 
app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
