using FitnessCoach.Infrastructure.Data;
using FitnessCoach.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using FitnessCoach.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Memory;
using System.Net;


var builder = WebApplication.CreateBuilder(args);

// MVC (vistas web)
builder.Services.AddControllersWithViews();

// API controllers
builder.Services.AddControllers();

// OpenAPI integrado de .NET 9/10 (sin Swashbuckle)
builder.Services.AddOpenApi();

// El chat del Lobo Coach postea JSON por fetch(), no un formulario: sin esto,
// [ValidateAntiForgeryToken] solo buscaria el token en los campos del form y nunca lo encontraria.
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

// Detras de un proxy, la IP real del cliente llega en X-Forwarded-*: sin esto el rate
// limiter contaria todo el trafico bajo la IP del balanceador (D-24). Solo se confia en
// los proxies declarados por configuracion, no en cabeceras de cualquier cliente.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Por defecto trae localhost: se limpia para confiar solo en lo declarado.
    options.KnownProxies.Clear();
    options.KnownIPNetworks.Clear();

    foreach (var ip in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? Array.Empty<string>())
        if (IPAddress.TryParse(ip, out var direccion))
            options.KnownProxies.Add(direccion);

    foreach (var red in builder.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? Array.Empty<string>())
    {
        var partes = red.Split('/');
        if (partes.Length == 2 && IPAddress.TryParse(partes[0], out var prefijo) && int.TryParse(partes[1], out var longitud))
            options.KnownIPNetworks.Add(new System.Net.IPNetwork(prefijo, longitud));
    }
});

// Limite de intentos por IP en las pantallas de sesion.
// Complementa al bloqueo de cuenta de Identity, que cuenta fallos POR CUENTA y por eso
// no frena el "password spraying": una sola contrasena probada contra miles de correos
// distintos nunca acumula 5 fallos en ninguno. Esto se cuenta por origen, no por cuenta.
// NOTA (D-24): el estado vive en la memoria del proceso. Con una sola instancia alcanza;
// con varias haria falta un almacen compartido (Redis), fuera del stack del proyecto.
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("login", contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: contexto.Connection.RemoteIpAddress?.ToString() ?? "sin-ip",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,               // 10 envios por minuto desde la misma IP
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0                  // sin cola: lo que sobra se rechaza, no espera
            }));

    options.OnRejected = async (contexto, token) =>
    {
        if (contexto.Lease.TryGetMetadata(MetadataName.RetryAfter, out var esperar))
            contexto.HttpContext.Response.Headers.RetryAfter = ((int)esperar.TotalSeconds).ToString();

        if (contexto.HttpContext.Request.Path.StartsWithSegments("/api"))
        {
            contexto.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await contexto.HttpContext.Response.WriteAsJsonAsync(
                new { mensaje = "Demasiadas peticiones. Espera un minuto y vuelve a intentar." }, token);
            return;
        }

        // En el navegador conviene devolver la pantalla de login con una explicacion,
        // no un 429 crudo que el usuario no sabria interpretar.
        contexto.HttpContext.Response.Redirect("/Account/Login?demasiadosIntentos=true");
    };
});

// Persistencia real: el puerto IRepositorioUsuario ahora apunta al adaptador SQL.
// Scoped = una instancia por peticion HTTP, que es lo que EF Core espera para su DbContext.
builder.Services.AddScoped<FitnessCoach.Domain.Ports.IRepositorioUsuario,
                           FitnessCoach.Infrastructure.Repositories.RepositorioUsuarioSql>();

// Los dos catalogos son datos de referencia, asi que el puerto no apunta al adaptador
// SQL sino a un decorador de cache que lo envuelve. La cache es singleton; los
// adaptadores siguen scoped porque usan el DbContext.
builder.Services.AddMemoryCache();

builder.Services.AddScoped<FitnessCoach.Infrastructure.Repositories.RepositorioEjerciciosSql>();
builder.Services.AddScoped<FitnessCoach.Domain.Ports.IRepositorioEjercicios>(proveedor =>
    new FitnessCoach.Infrastructure.Repositories.RepositorioEjerciciosEnCache(
        proveedor.GetRequiredService<FitnessCoach.Infrastructure.Repositories.RepositorioEjerciciosSql>(),
        proveedor.GetRequiredService<IMemoryCache>()));

builder.Services.AddScoped<FitnessCoach.Infrastructure.Repositories.RepositorioAlimentosSql>();
builder.Services.AddScoped<FitnessCoach.Domain.Ports.IRepositorioAlimentos>(proveedor =>
    new FitnessCoach.Infrastructure.Repositories.RepositorioAlimentosEnCache(
        proveedor.GetRequiredService<FitnessCoach.Infrastructure.Repositories.RepositorioAlimentosSql>(),
        proveedor.GetRequiredService<IMemoryCache>()));

// Servicio de cálculo calórico
builder.Services.AddScoped<FitnessCoach.Application.Services.ICalculadorCalorico,
                           FitnessCoach.Application.Services.CalculadorCaloricoService>();
builder.Services.AddScoped<FitnessCoach.Application.Services.IServicioPerfilUsuario,
                           FitnessCoach.Application.Services.ServicioPerfilUsuario>();
builder.Services.AddScoped<FitnessCoach.Application.Services.IServicioProgreso,
                           FitnessCoach.Application.Services.ServicioProgreso>();
builder.Services.AddScoped<FitnessCoach.Application.Services.IServicioEntrenamientos,
                           FitnessCoach.Application.Services.ServicioEntrenamientos>();
builder.Services.AddScoped<FitnessCoach.Application.Services.IServicioRecords,
                           FitnessCoach.Application.Services.ServicioRecords>();

// Generador de rutinas
builder.Services.AddScoped<FitnessCoach.Application.Services.IGeneradorRutinas,
                           FitnessCoach.Application.Services.GeneradorRutinasService>();

// Cambiar un ejercicio de la rutina por otro del mismo grupo muscular
builder.Services.AddScoped<FitnessCoach.Application.Services.IServicioSustitucionEjercicios,
                           FitnessCoach.Application.Services.ServicioSustitucionEjercicios>();


builder.Services.AddScoped<FitnessCoach.Application.Services.IGeneradorAlimentacion, FitnessCoach.Application.Services.GeneradorAlimentacionService>();

// Diario de adherencia
builder.Services.AddScoped<FitnessCoach.Application.Services.IServicioDiario,
                           FitnessCoach.Application.Services.ServicioDiario>();

// Gamificación: nivel, logros y misiones derivados de los hechos del usuario
builder.Services.AddScoped<FitnessCoach.Application.Services.IServicioGamificacion,
                           FitnessCoach.Application.Services.ServicioGamificacion>();

// Cliente HTTP compartido por los proveedores de IA (timeout acotado para que un
// proveedor colgado no cuelgue toda la consulta: se corta y la cadena sigue).
builder.Services.AddHttpClient("ia", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});

// Factory que arma los proveedores de IA reales según la configuración (Gemini y,
// si hay clave, Groq/OpenRouter). Agregar un proveedor es tocar solo la fábrica.
builder.Services.AddScoped<FitnessCoach.Domain.Ports.IFabricaProveedoresIA,
                           FitnessCoach.Infrastructure.Adapters.FabricaProveedoresIA>();

// El respaldo offline: última garantía, sin red. Vive en Application (es testeable).
builder.Services.AddScoped<FitnessCoach.Application.Coaching.CoachOfflineService>();

// Arma el contexto rico (plan, rutina, diario, récords, catálogo) para cada consulta.
builder.Services.AddScoped<FitnessCoach.Application.Coaching.IArmadorContextoCoach,
                           FitnessCoach.Application.Coaching.ArmadorContextoCoach>();

// El coach que consume el controlador: la cadena de la fábrica + el offline al final.
// Prueba los proveedores en orden y garantiza siempre una respuesta del Lobo.
builder.Services.AddScoped<FitnessCoach.Application.Coaching.ICoachIA>(sp =>
{
    var fabrica = sp.GetRequiredService<FitnessCoach.Domain.Ports.IFabricaProveedoresIA>();
    var offline = sp.GetRequiredService<FitnessCoach.Application.Coaching.CoachOfflineService>();
    var log = sp.GetRequiredService<ILogger<FitnessCoach.Application.Coaching.CoachResiliente>>();

    var proveedores = fabrica.CrearProveedores()
        .Append<FitnessCoach.Domain.Ports.IProveedorIA>(offline)
        .ToList();

    return new FitnessCoach.Application.Coaching.CoachResiliente(proveedores, log);
});

// El valor de appsettings.json apunta a LocalDB, que solo existe en Windows de escritorio.
// Fuera de desarrollo hay que pasar la cadena real por entorno
// (ConnectionStrings__DefaultConnection) o por el proveedor de configuracion del host.
// Se falla aca con un mensaje claro en vez de arrastrar el error hasta la primera consulta.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "Falta la cadena de conexion. Define ConnectionStrings__DefaultConnection en el entorno.");

if (!builder.Environment.IsDevelopment() && connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException(
        $"La cadena de conexion apunta a LocalDB y el entorno es '{builder.Environment.EnvironmentName}'. " +
        "LocalDB no existe en un servidor: define ConnectionStrings__DefaultConnection.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Las claves que firman las cookies de sesion van a la base, no al disco del proceso:
// en un contenedor el disco es efimero y cada despliegue desloguearia a todo el mundo.
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName("FitnessCoach");

// Sonda para el balanceador o el proveedor de hosting: comprueba que la base responda,
// que es la unica dependencia sin la que la app no sirve para nada.
builder.Services.AddHealthChecks()
    .AddCheck<SondaBaseDeDatos>("base-de-datos");

// ASP.NET Identity sobre el mismo DbContext.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;

    // Freno a la fuerza bruta: 5 intentos fallidos y la cuenta queda bloqueada 15 minutos.
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Rutas de la cookie de autenticacion (usaremos vistas propias en /Account).
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";

    // Las rutas /api son para clientes, no para navegadores: responden 401/403
    // en vez de redirigir a la pantalla de login.
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

var app = builder.Build();

using (var alcance = app.Services.CreateScope())
{
    var contexto = alcance.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var registro = alcance.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("Arranque");

    // Contra una base recien creada no hay tablas y la siembra reventaria. EF toma un
    // lock, asi que dos instancias arrancando a la vez no se pisan.
    if (app.Configuration.GetValue("Despliegue:MigrarAlArrancar", true))
    {
        registro.LogInformation("Aplicando migraciones pendientes...");
        await contexto.Database.MigrateAsync();
    }

    // Cada sembrador solo hace algo si su tabla esta vacia.
    await SembradorCatalogoEjercicios.SembrarAsync(contexto, registro);
    await SembradorCatalogoAlimentos.SembrarAsync(contexto, registro);
}

// Pipeline HTTP

// Antes que nada, para que todo lo que sigue vea al cliente real (D-24).
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// OpenAPI — genera el JSON en /openapi/v1.json
app.MapOpenApi();
app.MapScalarApiReference();
// Detras de un proxy que ya termina el TLS (lo normal en un PaaS), la app recibe la
// peticion por HTTP: si redirige a HTTPS, el proxy la vuelve a mandar por HTTP y se
// hace un bucle infinito. Ahi hay que apagarlo con Despliegue:RedirigirAHttps=false
// y dejar que el proxy imponga HTTPS.
if (app.Configuration.GetValue("Despliegue:RedirigirAHttps", true))
    app.UseHttpsRedirection();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

// Rutas MVC (vistas)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Rutas API
app.MapControllers();

app.MapHealthChecks("/health").AllowAnonymous();

app.Run();
