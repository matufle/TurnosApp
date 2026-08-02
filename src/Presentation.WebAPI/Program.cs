using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;
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
using TurnosApp.Presentation.WebAPI.Workers;



var builder = WebApplication.CreateBuilder(args);

// ── Sentry (reporte de errores) ────────────────────────────────────────────
// Antes que cualquier otra cosa: si algo revienta durante el resto del arranque
// (ej. la validación de Jwt:Key de abajo), Sentry ya está armado para capturarlo.
// Sentry:Dsn vacío en Development si no se configuró — el SDK se queda inactivo
// (no-op) sin romper el arranque, mismo criterio que Turnstile:SecretKey vacío.
builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.Environment = builder.Environment.EnvironmentName;
    options.TracesSampleRate = 0.2;
    options.SendDefaultPii = false;
});

// ── Validación de configuración crítica ────────────────────────────────────
// La clave del appsettings.json versionado es sólo un placeholder de desarrollo.
// Si en producción no se seteó Jwt__Key por variable de entorno, cortamos el
// arranque: correr con esa clave conocida permitiría a cualquiera forjar JWTs.
const string DefaultJwtKeyPlaceholder =
    "una-clave-secreta-larga-de-al-menos-32-caracteres-cambiar-en-produccion";

var jwtKey = builder.Configuration["Jwt:Key"];
if (!builder.Environment.IsDevelopment() &&
    (string.IsNullOrWhiteSpace(jwtKey) || jwtKey == DefaultJwtKeyPlaceholder || jwtKey.Length < 32))
{
    throw new InvalidOperationException(
        "Jwt:Key no está configurada de forma segura para este entorno. " +
        "Configurá la variable de entorno Jwt__Key con un secreto único y aleatorio.");
}

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
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPublicAppService, PublicAppService>();
builder.Services.AddScoped<IClienteAuthAppService, ClienteAuthAppService>();
builder.Services.AddScoped<ICurrentClienteService, CurrentClienteService>();
builder.Services.AddScoped<IHorarioAtencionAppService, HorarioAtencionAppService>();
builder.Services.AddScoped<IDisponibilidadAppService, DisponibilidadAppService>();
builder.Services.AddScoped<IPublicCatalogoAppService, PublicCatalogoAppService>();
builder.Services.AddScoped<IRecursoRepository, RecursoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ── Worker de notificaciones (outbox in-process) ───────────────────────────
builder.Services.AddHostedService<NotificacionDispatcherWorker>();

// ── Worker de liquidaciones (genera comisiones cuando cierra el período por tenant) ────
builder.Services.AddHostedService<LiquidacionGeneratorWorker>();

// ── Health check ────────────────────────────────────────────────────────────
// Endpoint liviano para un monitor externo (ej. UptimeRobot) — TenantMiddleware ya lo
// exceptuaba del header X-Tenant-Id, pero nunca se había mapeado. DbConnectivityHealthCheck
// (sin sumar un paquete NuGet aparte solo para esto) hace un Database.CanConnectAsync barato
// — distingue "el proceso está arriba pero la DB no responde" de una caída real del proceso.
builder.Services.AddHealthChecks()
    .AddCheck<TurnosApp.Presentation.WebAPI.HealthChecks.DbConnectivityHealthCheck>("database");

// ── Manejo global de excepciones ───────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

// Los preview deploys de Vercel (por rama/commit) generan una URL random distinta
// cada vez (https://<proyecto>-<hash>-matuflesolutions.vercel.app). En vez de ir
// agregando cada una a mano en AllowedOrigins, permitimos cualquier subdominio
// bajo nuestro propio scope de Vercel — sigue acotado a deploys nuestros, no abre
// CORS a cualquier sitio de vercel.app.
var vercelPreviewPattern = new System.Text.RegularExpressions.Regex(
    @"^https://[a-z0-9-]+-matuflesolutions\.vercel\.app$",
    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
                  allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase) ||
                  vercelPreviewPattern.IsMatch(origin))
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Forwarded headers ──────────────────────────────────────────────────────
// La app corre detrás del load balancer de Render: sin esto, HttpContext.Connection.
// RemoteIpAddress siempre sería la IP interna del proxy, no la del cliente real —
// inutilizando el rate limiting por IP de más abajo (todo el tráfico caería en el
// mismo balde). KnownNetworks/KnownProxies vacíos porque no conocemos de antemano
// la IP del proxy de Render (mismo trade-off que cualquier deploy en un PaaS).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// ── Rate limiting en endpoints de auth (login/registro, staff y cliente) ──────
// Sin esto, nada impide que un bot spamee cuentas nuevas o intente fuerza bruta
// de contraseñas contra /api/auth y /api/cliente-auth. Partición por IP real
// (post ForwardedHeaders); "Login" más permisivo que "Register" porque un usuario
// legítimo puede tipear mal la contraseña varias veces, mientras que registrar
// cuentas es una acción rara que un humano hace una sola vez.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            title = "Demasiados intentos",
            status = StatusCodes.Status429TooManyRequests,
            detail = "Hiciste demasiados intentos en poco tiempo. Esperá un momento y volvé a intentar."
        }, cancellationToken);
    };

    static string ClaveIp(HttpContext http) =>
        http.Connection.RemoteIpAddress?.ToString() ?? "sin-ip";

    options.AddPolicy("AuthLogin", http => RateLimitPartition.GetSlidingWindowLimiter(
        ClaveIp(http),
        _ => new SlidingWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 4,
            PermitLimit = 10,
            QueueLimit = 0
        }));

    options.AddPolicy("AuthRegister", http => RateLimitPartition.GetFixedWindowLimiter(
        ClaveIp(http),
        _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromHours(1),
            PermitLimit = 5,
            QueueLimit = 0
        }));

    // Catálogo público (PublicController): sin límite previo, a diferencia de los endpoints de
    // auth — el load test de rendimiento mostró que tráfico anónimo concentrado (~200-400
    // conexiones simultáneas) tumba la única instancia. 120/min por IP es generoso para un
    // visitante real navegando el catálogo, pero corta rápido una ráfaga/bot/scraper antes de
    // que se coma toda la capacidad compartida.
    options.AddPolicy("PublicoCatalogo", http => RateLimitPartition.GetSlidingWindowLimiter(
        ClaveIp(http),
        _ => new SlidingWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 4,
            PermitLimit = 120,
            QueueLimit = 0
        }));
});

// ── Compresión de respuestas ───────────────────────────────────────────────
// JSON comprime muy bien (70-80% menos bytes en catálogos/listados grandes) — reduce tiempo
// de transferencia, importante en una instancia con ancho de banda/CPU limitados. Habilitado
// para HTTPS: el riesgo de BREACH aplica a HTML con secretos reflejados junto a input del
// atacante (ej. token CSRF); acá es una API JSON sin ese patrón. Nivel "Fastest" a propósito:
// prioriza no sumarle CPU al pico de tráfico por sobre comprimir al máximo.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);

// ── Output caching del catálogo público ────────────────────────────────────
// Solo para lecturas anónimas que cambian poco (servicios/recursos/datos del tenant por slug) —
// bajo tráfico repetido (ej. muchos visitantes viendo el mismo catálogo) evita ir a la DB en
// cada request. Deliberadamente NO se cachea disponibilidad: refleja turnos ya reservados en
// tiempo real, cachearla arriesgaría mostrar un horario como libre cuando ya se ocupó.
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("CatalogoPublico", policy => policy.Expire(TimeSpan.FromSeconds(30)));
});

// ── Controllers y Swagger ──────────────────────────────────────────────────
// RequiereSuscripcionActivaAttribute como filtro global (no atributo por-controller como
// RequierePermiso): el gating de suscripción aplica a toda la app por igual, con sus propias
// excepciones por ruta (ver el atributo) en vez de tener que taggear ~15 controllers a mano.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<TurnosApp.Presentation.WebAPI.Authorization.RequiereSuscripcionActivaAttribute>();
});
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
// Compresión lo antes posible en el pipeline, para envolver la respuesta de todo lo demás.
app.UseResponseCompression();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Antes que todo lo demás: repuebla RemoteIpAddress desde X-Forwarded-For para
// que CORS/rate limiting/logging vean la IP real del cliente, no la del proxy.
app.UseForwardedHeaders();
app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
// TenantMiddleware antes de routing: corta el pipeline si falta el header.
app.UseMiddleware<TenantMiddleware>();

app.UseAuthorization();
app.UseRateLimiter();
app.UseOutputCache();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();