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
builder.Services.AddScoped<RepositorioCatalogo>();
builder.Services.AddScoped<ServicioUsuario>();
builder.Services.AddScoped<ServicioSesion>();
builder.Services.AddScoped<CargaInicial>();
builder.Services.AddScoped<DatosDeEjemplo>();
builder.Services.AddScoped<ServicioAdministracionUsuarios>();
builder.Services.AddScoped<ServicioDevolucion>();
builder.Services.AddScoped<ServicioPrestamo>();
builder.Services.AddScoped<ServicioCatalogo>();
builder.Services.AddScoped<ServicioHistorico>();
builder.Services.AddScoped<ServicioInventario>();
builder.Services.AddScoped<ServicioTiposEquipamiento>();
builder.Services.AddScoped<ServicioUnidades>();

// Reloj del sistema: los servicios piden la hora a TimeProvider (en los tests se usa uno fijo).
builder.Services.AddSingleton(TimeProvider.System);

// Quien esta logueado: uno por conexion del navegador.
builder.Services.AddScoped<SesionActual>();

WebApplication app = builder.Build();

// Precarga el usuario root, el catalogo unico y los datos de ejemplo al arrancar (la Memoria empieza vacia).
// El catalogo va antes que los datos de ejemplo: ahi se agregan los tipos de equipamiento.
using (IServiceScope scope = app.Services.CreateScope())
{
    CargaInicial cargaInicial = scope.ServiceProvider.GetRequiredService<CargaInicial>();
    cargaInicial.CargarRoot();
    cargaInicial.CargarCatalogo();
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
