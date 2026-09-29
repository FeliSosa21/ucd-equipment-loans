using Data;
using Data.Repositorios;
using Servicios;
using UI.Components;
using UI.Sesion;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Una sola Memoria para toda la aplicacion: guarda los datos mientras corre el sistema.
builder.Services.AddSingleton<Memoria>();

// Repositorios y servicios: uno por conexion; no guardan estado propio.
builder.Services.AddScoped<RepositorioUsuario>();
builder.Services.AddScoped<ServicioUsuario>();
builder.Services.AddScoped<ServicioSesion>();
builder.Services.AddScoped<CargaInicial>();

// Quien esta logueado: uno por conexion del navegador.
builder.Services.AddScoped<SesionActual>();

var app = builder.Build();

// Precarga el usuario root al arrancar (la Memoria empieza vacia).
using (IServiceScope scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<CargaInicial>().CargarRoot();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
