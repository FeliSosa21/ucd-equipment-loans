using Data;
using Data.Repositorios;
using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Servicios.Tests;

// Base InMemory con nombre al azar y los repositorios SQL sobre EL MISMO contexto, como en la app (Scoped).
// Para preparar datos o leer lo guardado se usa un contexto nuevo sobre la misma base.
internal sealed class ContextoDePrueba : IDisposable
{
    private const string ContrasenaDePrueba = "VivaLaOrt2026";

    private readonly DbContextOptions<SqlContext> _opciones;
    private readonly List<SqlContext> _contextosQueFallan = new List<SqlContext>();

    public SqlContext Contexto { get; }
    public RepositorioUsuarioSql Usuarios { get; }
    public RepositorioCatalogoSql Catalogos { get; }

    public ContextoDePrueba()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using SqlContext preparacion = new SqlContext(_opciones);
        preparacion.Database.EnsureCreated();
        GuardarTiposDeUsuarioSiFaltan(preparacion);
        Contexto = new SqlContext(_opciones);
        Usuarios = new RepositorioUsuarioSql(Contexto);
        Catalogos = new RepositorioCatalogoSql(Contexto);
    }

    public SqlContext NuevoContexto()
    {
        return new SqlContext(_opciones);
    }

    public SqlContext NuevoContextoQueFallaAlGuardar()
    {
        SqlContext contexto = new ContextoQueFallaAlGuardar(_opciones);
        _contextosQueFallan.Add(contexto);
        return contexto;
    }

    public int GuardarCatalogo(string nombre, params TipoEquipamiento[] tipos)
    {
        Catalogo catalogo = new Catalogo(nombre);
        foreach (TipoEquipamiento tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        return GuardarCatalogo(catalogo);
    }

    public void DarDeBajaCatalogo(int idCatalogo)
    {
        using SqlContext preparacion = NuevoContexto();
        RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(preparacion);
        Catalogo catalogo = repositorio.ObtenerCatalogoActivoPorId(idCatalogo)!;
        catalogo.DarDeBaja();
        repositorio.ActualizarCatalogo(catalogo);
    }

    public int GuardarCatalogo(Catalogo catalogo)
    {
        using SqlContext preparacion = NuevoContexto();
        new RepositorioCatalogoSql(preparacion).AgregarCatalogo(catalogo);
        return catalogo.Id;
    }

    public void GuardarUsuario(string dni, string correo, TipoUsuario tipoPredefinido, bool desactivado = false)
    {
        using SqlContext preparacion = NuevoContexto();
        Usuario usuario = new Usuario(dni, "Nombre", "Apellido", correo, ContrasenaDePrueba);
        usuario.AsignarTipo(new RepositorioTipoUsuarioSql(preparacion).ObtenerTipoPorNombre(tipoPredefinido.Nombre)!);
        if (desactivado)
        {
            usuario.Desactivar();
        }

        new RepositorioUsuarioSql(preparacion).AgregarUsuario(usuario);
    }

    public void Dispose()
    {
        Contexto.Dispose();
        foreach (SqlContext contexto in _contextosQueFallan)
        {
            contexto.Dispose();
        }
    }

    // Con HasData los seis tipos ya vienen sembrados; si el mapeo deja de sembrarlos, se cargan aca.
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
}
