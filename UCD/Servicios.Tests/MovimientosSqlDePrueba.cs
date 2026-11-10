using Data;
using Data.Repositorios;
using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Servicios.Tests;

// Base InMemory para los tests del historico, con los repositorios SQL sobre el mismo contexto, como en la app.
// Es independiente de ContextoDePrueba para no chocar con los cambios de D19; cuando se integren, conviene unificarlos.
internal sealed class MovimientosSqlDePrueba : IDisposable
{
    private const string ContrasenaDePrueba = "VivaLaOrt2026";
    private static readonly DateTime FechaDePrestamo = new DateTime(2026, 9, 22, 10, 30, 0);

    private readonly DbContextOptions<SqlContext> _opciones;

    public SqlContext Contexto { get; }

    public MovimientosSqlDePrueba()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using SqlContext preparacion = new SqlContext(_opciones);
        preparacion.Database.EnsureCreated();
        GuardarTiposDeUsuarioSiFaltan(preparacion);
        Contexto = new SqlContext(_opciones);
    }

    public int GuardarCatalogo(string nombre, params TipoEquipamiento[] tipos)
    {
        Catalogo catalogo = new Catalogo(nombre);
        foreach (TipoEquipamiento tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        using SqlContext preparacion = new SqlContext(_opciones);
        new RepositorioCatalogoSql(preparacion).AgregarCatalogo(catalogo);
        return catalogo.Id;
    }

    public void GuardarUsuario(string dni, TipoUsuario tipoPredefinido)
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        Usuario usuario = CrearUsuario(preparacion, new DatosDeUsuario(dni, "Nombre", "Apellido"), tipoPredefinido);
        new RepositorioUsuarioSql(preparacion).AgregarUsuario(usuario);
    }

    // El prestamo es de Ana Perez (Docente) con un tipo del catalogo guardado; el evaluador es Felipe Sosa (Supervisor).
    public void GuardarMovimiento(int idCatalogo, string nombreTipo, Func<Prestamo, Usuario, Movimiento> crear)
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        TipoEquipamiento tipo = new RepositorioCatalogoSql(preparacion).ObtenerTipoPorNombre(idCatalogo, nombreTipo)!;
        Usuario solicitante = CrearUsuario(preparacion, new DatosDeUsuario("12345672", "Ana", "Perez"),
            TipoUsuario.Docente());
        Usuario evaluador = CrearUsuario(preparacion, new DatosDeUsuario("67890127", "Felipe", "Sosa"),
            TipoUsuario.Supervisor());
        Prestamo prestamo = new Prestamo(solicitante, tipo, cantidadSolicitada: 1, fechaHoraPrestamo: FechaDePrestamo);
        new RepositorioMovimientoSql(preparacion).AgregarMovimiento(crear(prestamo, evaluador));
    }

    public void Dispose()
    {
        Contexto.Dispose();
    }

    private static Usuario CrearUsuario(SqlContext preparacion, DatosDeUsuario datos, TipoUsuario tipoPredefinido)
    {
        Usuario usuario = new Usuario(datos.Dni, datos.Nombre, datos.Apellido, datos.Dni + "@ucd.edu.uy",
            ContrasenaDePrueba);
        usuario.AsignarTipo(new RepositorioTipoUsuarioSql(preparacion).ObtenerTipoPorNombre(tipoPredefinido.Nombre)!);
        return usuario;
    }

    private static void GuardarTiposDeUsuarioSiFaltan(SqlContext preparacion)
    {
        RepositorioTipoUsuarioSql tipos = new RepositorioTipoUsuarioSql(preparacion);
        if (tipos.ObtenerTodos().Count > 0)
        {
            return;
        }

        foreach (TipoUsuario tipo in TipoUsuario.Todos())
        {
            tipos.AgregarTipo(tipo);
        }
    }

    private sealed record DatosDeUsuario(string Dni, string Nombre, string Apellido);
}
