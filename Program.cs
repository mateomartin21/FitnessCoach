using Scalar.AspNetCore;
using Scalar.AspNetCore;
var builder = WebApplication.CreateBuilder(args);

// MVC (vistas web)
builder.Services.AddControllersWithViews();

// API controllers
builder.Services.AddControllers();

// OpenAPI integrado de .NET 9/10 (sin Swashbuckle)
builder.Services.AddOpenApi();

// Repositorio de usuarios (Singleton: un único estado compartido en memoria)
builder.Services.AddSingleton<FitnessCoach.Repositories.IRepositorioUsuario,
                              FitnessCoach.Repositories.RepositorioUsuarioMemoria>();

// Servicio de cálculo calórico
builder.Services.AddScoped<FitnessCoach.Services.ICalculadorCalorico,
                           FitnessCoach.Services.CalculadorCaloricoService>();

// Generador de rutinas
builder.Services.AddScoped<FitnessCoach.Services.IGeneradorRutinas,
                           FitnessCoach.Services.GeneradorRutinasService>();

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
