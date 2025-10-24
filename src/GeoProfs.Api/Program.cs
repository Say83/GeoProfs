using GeoProfs.Application.Common.Interfaces;
using GeoProfs.Infrastructure.Persistence.Context;
using GeoProfs.Infrastructure.Persistence.Repositories;
using GeoProfs.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Service Registratie (Dependency Injection) ---

// Voeg de DbContext toe en configureer de PostgreSQL-verbinding.
// De connection string wordt uit appsettings.json gehaald.
builder.Services.AddDbContext<GeoProfsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registreer de repositories.
// Wanneer een class een ILeaveRequestRepository vraagt, krijgt het een LeaveRequestRepository.
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();

// Registreer de services.
builder.Services.AddTransient<IEmailService, EmailService>();

// Voeg MediatR toe en laat het de handlers scannen in de Application assembly.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("GeoProfs.Application")));

// Voeg controllers en Swagger/OpenAPI toe.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "GeoProfs API", Version = "v1" });
});

// --- 2. Authenticatie Configuratie (JWT) ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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
    });


var app = builder.Build();

// --- 3. Middleware Pipeline ---

// Gebruik Swagger in development voor API-testen.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Belangrijk: Authenticatie en Autorisatie moeten in de juiste volgorde staan.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
