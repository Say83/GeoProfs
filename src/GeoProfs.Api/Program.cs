using GeoProfs.Application.Common.Interfaces;
using GeoProfs.Infrastructure.Persistence.Context;
using GeoProfs.Infrastructure.Persistence.Repositories;
using GeoProfs.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Service Registratie (Dependency Injection) ---

// Voeg de DbContext toe en configureer de PostgreSQL-verbinding.
builder.Services.AddDbContext<GeoProfsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// BELANGRIJKE FIX: Registreer de DbContext ook voor de IGeoProfsDbContext interface.
// Dit is waarschijnlijk de oorzaak van de buildfout.
builder.Services.AddScoped<IGeoProfsDbContext>(provider => provider.GetRequiredService<GeoProfsDbContext>());

// Registreer de repositories.
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();

// Registreer de services.
builder.Services.AddTransient<IEmailService, EmailService>();

// Voeg MediatR toe en laat het de handlers scannen in de Application assembly.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("GeoProfs.Application")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// FIX: Voeg Swagger-configuratie toe voor het gebruik van JWT-tokens in de UI.
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GeoProfs API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
