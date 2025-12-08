using System.Text;
using CampoLibre.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using CampoLibre.Api.Domain.Entities;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.Local.json",
    optional: true,
    reloadOnChange: true);


// ---------- DB ----------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ---------- Identity / Password Hasher ----------
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();


// ---------- Controllers ----------
builder.Services.AddControllers();

// ---------- JWT ----------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CampoLibre.API", Version = "v1" });

    // Configuración para botón "Authorize" con JWT
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Colocá: **Bearer {tu_token}**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


var app = builder.Build();

// ---------- Pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
