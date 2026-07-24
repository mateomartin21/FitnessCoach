using FitnessCoach.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using FitnessCoach.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;


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

// Limite de intentos por IP en las pantallas de sesion.
// Complementa al bloqueo de cuenta de Identity, que cuenta fallos POR CUENTA y por eso
// no frena el "password spraying": una sola contrasena probada contra miles de correos
// distintos nunca acumula 5 fallos en ninguno. Esto se cuenta por origen, no por cuenta.
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

// Servicio de cálculo calórico
builder.Services.AddScoped<FitnessCoach.Application.Services.ICalculadorCalorico,
                           FitnessCoach.Application.Services.CalculadorCaloricoService>();
builder.Services.AddScoped<FitnessCoach.Application.Services.IServicioPerfilUsuario,
                           FitnessCoach.Application.Services.ServicioPerfilUsuario>();

// Generador de rutinas
builder.Services.AddScoped<FitnessCoach.Application.Services.IGeneradorRutinas,
                           FitnessCoach.Application.Services.GeneradorRutinasService>();


builder.Services.AddScoped<FitnessCoach.Application.Services.IGeneradorAlimentacion, FitnessCoach.Application.Services.GeneradorAlimentacionService>();

builder.Services.AddHttpClient<FitnessCoach.Infrastructure.Adapters.GeminiCoachService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

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

// Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// OpenAPI — genera el JSON en /openapi/v1.json
app.MapOpenApi();
app.MapScalarApiReference();
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

app.Run();
