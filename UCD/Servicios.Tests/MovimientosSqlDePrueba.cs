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
    private const string DniSolicitante = "12345672";
    private const string DniEvaluador = "67890127";
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
        GuardarSolicitanteYEvaluador(preparacion);
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

    public void DarDeBajaCatalogo(int idCatalogo)
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(preparacion);
        Catalogo catalogo = repositorio.ObtenerCatalogoActivoPorId(idCatalogo)
            ?? throw new InvalidOperationException($"No existe un catalogo activo con id {idCatalogo}");
        catalogo.DarDeBaja();
        repositorio.ActualizarCatalogo(catalogo);
    }

    public void GuardarUsuario(string dni, TipoUsuario tipoPredefinido)
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        Usuario usuario = CrearUsuario(preparacion, new DatosDeUsuario(dni, "Nombre", "Apellido"), tipoPredefinido);
        new RepositorioUsuarioSql(preparacion).AgregarUsuario(usuario);
    }

    public void GuardarUsuarioDesactivado(string dni, TipoUsuario tipoPredefinido)
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        Usuario usuario = CrearUsuario(preparacion, new DatosDeUsuario(dni, "Nombre", "Apellido"), tipoPredefinido);
        usuario.Desactivar();
        new RepositorioUsuarioSql(preparacion).AgregarUsuario(usuario);
    }

    // El prestamo es de Ana Perez (Docente) con un tipo del catalogo guardado.
    // El evaluador que recibe crear es Felipe Sosa (Supervisor).
    public void GuardarMovimiento(int idCatalogo, string nombreTipo, Func<Prestamo, Usuario, Movimiento> crear)
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        TipoEquipamiento tipo = new RepositorioCatalogoSql(preparacion).ObtenerTipoPorNombre(idCatalogo, nombreTipo)
            ?? throw new InvalidOperationException($"No existe el tipo {nombreTipo} en el catalogo {idCatalogo}");
        RepositorioUsuarioSql usuarios = new RepositorioUsuarioSql(preparacion);
        Usuario solicitante = ObtenerUsuarioGuardado(usuarios, DniSolicitante);
        Usuario evaluador = ObtenerUsuarioGuardado(usuarios, DniEvaluador);
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
        TipoUsuario tipo = new RepositorioTipoUsuarioSql(preparacion).ObtenerTipoPorNombre(tipoPredefinido.Nombre)
            ?? throw new InvalidOperationException($"No existe el tipo de usuario {tipoPredefinido.Nombre}");
        usuario.AsignarTipo(tipo);
        return usuario;
    }

    private static Usuario ObtenerUsuarioGuardado(RepositorioUsuarioSql usuarios, string dni)
    {
        return usuarios.ObtenerUsuarioPorDni(dni)
            ?? throw new InvalidOperationException($"No existe el usuario con DNI {dni}");
    }

    private static void GuardarSolicitanteYEvaluador(SqlContext preparacion)
    {
        RepositorioUsuarioSql usuarios = new RepositorioUsuarioSql(preparacion);
        usuarios.AgregarUsuario(CrearUsuario(preparacion, new DatosDeUsuario(DniSolicitante, "Ana", "Perez"),
            TipoUsuario.Docente()));
        usuarios.AgregarUsuario(CrearUsuario(preparacion, new DatosDeUsuario(DniEvaluador, "Felipe", "Sosa"),
            TipoUsuario.Supervisor()));
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
