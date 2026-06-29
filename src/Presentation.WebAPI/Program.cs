using TurnosApp.Core.Application.Extensions;
using TurnosApp.Core.Application.Interfaces;
using TurnosApp.Infra.Data.Extensions;
using TurnosApp.Presentation.WebAPI.ExceptionHandlers;
using TurnosApp.Presentation.WebAPI.Middleware;
using TurnosApp.Presentation.WebAPI.Providers;

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
    options.SwaggerDoc("v1", new()
    {
        Title = "TurnosApp API",
        Version = "v1",
        Description = "Sistema de gestión de turnos Multi-tenant"
    });

    // Permite enviar el header X-Tenant-Id desde Swagger UI para pruebas.
    options.AddSecurityDefinition("TenantHeader", new()
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = HttpContextTenantProvider.HeaderName,
        Description = "ID del tenant para filtrar los datos. Ej: 1"
    });

    options.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "TenantHeader"
                }
            },
            []
        }
    });
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