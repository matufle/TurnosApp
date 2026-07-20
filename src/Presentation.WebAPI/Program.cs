using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using TurnosApp.Core.Application.Extensions;
using TurnosApp.Core.Application.Interfaces.Persistence;
using TurnosApp.Core.Application.Interfaces.Services;
using TurnosApp.Core.Application.Services;
using TurnosApp.Infra.Data.Context;
using TurnosApp.Infra.Data.Extensions;
using TurnosApp.Infra.Data.Repositories;
using TurnosApp.Presentation.WebAPI.Filters;
using TurnosApp.Presentation.WebAPI.Middleware;
using TurnosApp.Presentation.WebAPI.Providers;
using TurnosApp.Presentation.WebAPI.Services;


var builder = WebApplication.CreateBuilder(args);

// ── Servicios de infraestructura ───────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
// ── Servicios de aplicación ────────────────────────────────────────────────
builder.Services.AddApplication();

// ── Multi-tenancy ──────────────────────────────────────────────────────────
// IHttpContextAccessor necesario para que HttpContextTenantProvider
// acceda al HttpContext fuera de los controllers.
builder.Services.AddHttpContextAccessor();

// Scoped: una instancia por request, mismo ciclo de vida que DbContext.
builder.Services.AddScoped<ITenantProvider, HttpContextTenantProvider>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthAppService, AuthAppService>();
builder.Services.AddScoped<IRecursoRepository, RecursoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
// ── Manejo global de excepciones ───────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Controllers y Swagger ──────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TurnosApp API",
        Version = "v1",
        Description = "Sistema de gestión de turnos Multi-tenant"
    });

    options.OperationFilter<TenantHeaderFilter>();

    // --- Definición del esquema de seguridad Bearer ---
    const string schemeId = "Bearer";

    options.AddSecurityDefinition(schemeId, new OpenApiSecurityScheme
    {
        Description = "Ingresá el token JWT así: Bearer {tu token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // --- Requerimiento: aplica el esquema a todos los endpoints ---
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(schemeId, document)] = new List<string>()
    });
});

// ── Construcción de la app ─────────────────────────────────────────────────
var app = builder.Build();

// ── Pipeline de middlewares ────────────────────────────────────────────────

// ExceptionHandler primero: captura excepciones de todo el pipeline posterior.
app.UseExceptionHandler();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
// TenantMiddleware antes de routing: corta el pipeline si falta el header.
app.UseMiddleware<TenantMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();