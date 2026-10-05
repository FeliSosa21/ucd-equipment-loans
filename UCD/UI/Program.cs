using Data;
using Data.Repositorios;
using Servicios;
using UI.Components;
using UI.Sesion;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Una sola Memoria para toda la aplicacion: guarda los datos mientras corre el sistema.
builder.Services.AddSingleton<Memoria>();

// Repositorios y servicios: uno por conexion; no guardan estado propio.
builder.Services.AddScoped<RepositorioUsuario>();
builder.Services.AddScoped<RepositorioPrestamo>();
builder.Services.AddScoped<RepositorioDevolucion>();
builder.Services.AddScoped<RepositorioMovimiento>();
builder.Services.AddScoped<ServicioUsuario>();
builder.Services.AddScoped<ServicioSesion>();
builder.Services.AddScoped<CargaInicial>();
builder.Services.AddScoped<DatosDeEjemplo>();
builder.Services.AddScoped<ServicioAdministracionUsuarios>();
builder.Services.AddScoped<ServicioDevolucion>();

// Quien esta logueado: uno por conexion del navegador.
builder.Services.AddScoped<SesionActual>();

WebApplication app = builder.Build();

// Precarga el usuario root y los datos de ejemplo al arrancar (la Memoria empieza vacia).
using (IServiceScope scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<CargaInicial>().CargarRoot();
    scope.ServiceProvider.GetRequiredService<DatosDeEjemplo>().Cargar();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
