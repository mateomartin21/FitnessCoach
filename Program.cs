using FitnessCoach.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// MVC (vistas web)
builder.Services.AddControllersWithViews();

// API controllers
builder.Services.AddControllers();

// OpenAPI integrado de .NET 9/10 (sin Swashbuckle)
builder.Services.AddOpenApi();

builder.Services.AddSingleton<FitnessCoach.Domain.Ports.IRepositorioUsuario,
                              FitnessCoach.Infrastructure.Repositoriess.RepositorioUsuarioMemoria>();

// Servicio de cálculo calórico
builder.Services.AddScoped<FitnessCoach.Application.Services.ICalculadorCalorico,
                           FitnessCoach.Application.Services.CalculadorCaloricoService>();

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
