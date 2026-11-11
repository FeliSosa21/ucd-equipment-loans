using Data;
using Data.Repositorios;
using Microsoft.EntityFrameworkCore;
using Servicios;
using UI.Components;
using UI.Sesion;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Contexto de Entity Framework: se conecta a la base de Docker con la connection string de appsettings.json.
// AddDbContext lo registra como Scoped: un contexto por conexion del navegador.
builder.Services.AddDbContext<SqlContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorios y servicios: uno por conexion del navegador, todos sobre el mismo SqlContext.
builder.Services.AddScoped<RepositorioUsuarioSql>();
builder.Services.AddScoped<RepositorioTipoUsuarioSql>();
builder.Services.AddScoped<RepositorioPrestamoSql>();
builder.Services.AddScoped<RepositorioDevolucionSql>();
builder.Services.AddScoped<RepositorioMovimientoSql>();
builder.Services.AddScoped<RepositorioCatalogoSql>();
builder.Services.AddScoped<RepositorioAuditoriaSql>();
builder.Services.AddScoped<RepositorioNotificacionSql>();
builder.Services.AddScoped<RegistradorDeAuditoria>();
builder.Services.AddScoped<ServicioUsuario>();
builder.Services.AddScoped<ServicioSesion>();
builder.Services.AddScoped<CargaInicialSql>();
builder.Services.AddScoped<ServicioAdministracionUsuarios>();
builder.Services.AddScoped<ServicioTiposUsuario>();
builder.Services.AddScoped<ServicioDevolucion>();
builder.Services.AddScoped<ServicioPrestamo>();
builder.Services.AddScoped<ServicioCatalogo>();
builder.Services.AddScoped<ServicioGestionCatalogos>();
builder.Services.AddScoped<ServicioHistorico>();
builder.Services.AddScoped<ServicioInventario>();
builder.Services.AddScoped<ServicioTiposEquipamiento>();
builder.Services.AddScoped<ServicioUnidades>();
builder.Services.AddScoped<ServicioAuditoria>();
builder.Services.AddScoped<ServicioNotificacion>();
builder.Services.AddScoped<IExportadorDeTabla, ExportadorCsv>();
builder.Services.AddScoped<IExportadorDeTabla, ExportadorXlsx>();
builder.Services.AddScoped<ServicioExportacion>();

// Reloj del sistema: los servicios piden la hora a TimeProvider (en los tests se usa uno fijo).
builder.Services.AddSingleton(TimeProvider.System);

// Quien esta logueado: uno por conexion del navegador.
builder.Services.AddScoped<SesionActual>();

WebApplication app = builder.Build();

// Al arrancar se aplican las migraciones y se crea el root si no existe.
// Los tipos de usuario los carga el script de instalacion (D19): sin el tipo Administrador el arranque se corta.
using (IServiceScope scope = app.Services.CreateScope())
{
    SqlContext contexto = scope.ServiceProvider.GetRequiredService<SqlContext>();
    contexto.Database.Migrate();
    scope.ServiceProvider.GetRequiredService<CargaInicialSql>().CargarRoot();
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
