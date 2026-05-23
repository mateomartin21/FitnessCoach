var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Registrar nuestro repositorio en memoria como un Singleton
builder.Services.AddSingleton<FitnessCoach.Repositories.IRepositorioUsuario, FitnessCoach.Repositories.RepositorioUsuarioMemoria>();
// Registrar el servicio de cálculo (Scoped significa que se crea una instancia por cada petición HTTP)
builder.Services.AddScoped<FitnessCoach.Services.ICalculadorCalorico, FitnessCoach.Services.CalculadorCaloricoService>();
//Generador de rutinas
builder.Services.AddScoped<FitnessCoach.Services.IGeneradorRutinas, FitnessCoach.Services.GeneradorRutinasService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
