using TurnosApp.Core.Application.Extensions;
using TurnosApp.Core.Application.Interfaces;
using TurnosApp.Infra.Data.Extensions;
using TurnosApp.Presentation.WebAPI.ExceptionHandlers;
using TurnosApp.Presentation.WebAPI.Middleware;
using TurnosApp.Presentation.WebAPI.Providers;
using TurnosApp.Presentation.WebAPI.Filters;
using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);

// ── Servicios de infraestructura ───────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Servicios de aplicación ────────────────────────────────────────────────
builder.Services.AddApplication();

// ── Multi-tenancy ──────────────────────────────────────────────────────────
// IHttpContextAccessor necesario para que HttpContextTenantProvider
// acceda al HttpContext fuera de los controllers.
builder.Services.AddHttpContextAccessor();

// Scoped: una instancia por request, mismo ciclo de vida que DbContext.
builder.Services.AddScoped<ITenantProvider, HttpContextTenantProvider>();

// ── Manejo global de excepciones ───────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

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

    // Registra el filtro — se ejecuta una vez por cada endpoint descubierto.
    // No requiere instanciar IOpenApiSecurityScheme ni tocar propiedades globales.
    options.OperationFilter<TenantHeaderFilter>();
});

// ── Construcción de la app ─────────────────────────────────────────────────
var app = builder.Build();

// ── Pipeline de middlewares ────────────────────────────────────────────────

// ExceptionHandler primero: captura excepciones de todo el pipeline posterior.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// TenantMiddleware antes de routing: corta el pipeline si falta el header.
app.UseMiddleware<TenantMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();