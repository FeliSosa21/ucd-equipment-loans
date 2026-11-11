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
    private static readonly DateTime FechaDePrestamo = new DateTime(2026, 11, 9, 10, 0, 0);

    private readonly DbContextOptions<SqlContext> _opciones;
    private readonly List<SqlContext> _contextosQueFallan = new List<SqlContext>();

    public SqlContext Contexto { get; }
    public RepositorioUsuarioSql Usuarios { get; }
    public RepositorioCatalogoSql Catalogos { get; }
    public RepositorioPrestamoSql Prestamos { get; }
    public RepositorioDevolucionSql Devoluciones { get; }

    public ContextoDePrueba()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using SqlContext preparacion = new SqlContext(_opciones);
        TiposDePrueba.GuardarEn(preparacion);
        Contexto = new SqlContext(_opciones);
        Usuarios = new RepositorioUsuarioSql(Contexto);
        Catalogos = new RepositorioCatalogoSql(Contexto);
        Prestamos = new RepositorioPrestamoSql(Contexto);
        Devoluciones = new RepositorioDevolucionSql(Contexto);
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

    public void GuardarPrestamoSolicitado(int idCatalogo, string nombreTipo, string dniSolicitante)
    {
        using SqlContext preparacion = NuevoContexto();
        Prestamo prestamo = CrearPrestamo(preparacion, idCatalogo, nombreTipo, dniSolicitante);
        new RepositorioPrestamoSql(preparacion).AgregarPrestamo(prestamo);
    }

    public void GuardarPrestamoActivo(int idCatalogo, string nombreTipo, string dniSolicitante)
    {
        using SqlContext preparacion = NuevoContexto();
        Prestamo prestamo = CrearPrestamo(preparacion, idCatalogo, nombreTipo, dniSolicitante);
        prestamo.Activar(prestamo.TipoEquipamiento.Unidades.Take(1).ToList());
        new RepositorioPrestamoSql(preparacion).AgregarPrestamo(prestamo);
    }

    // Activa el prestamo con la primera unidad disponible del tipo, asi se pueden guardar varios del mismo tipo.
    public void GuardarPrestamoActivoEn(RegistroDePrueba registro)
    {
        using SqlContext preparacion = NuevoContexto();
        PrestamoActivoDePrueba(preparacion, registro);
    }

    public void GuardarDevolucion(RegistroDePrueba registro)
    {
        using SqlContext preparacion = NuevoContexto();
        Prestamo prestamo = PrestamoActivoDePrueba(preparacion, registro);
        Usuario quienRegistra = new RepositorioUsuarioSql(preparacion).ObtenerUsuarioPorDni(registro.DniQuienRegistra)!;
        UnidadDevuelta unidad = new UnidadDevuelta(prestamo.UnidadesAsignadas[0], registro.Condicion, null);
        Devolucion devolucion = new Devolucion(prestamo,
            RegistroDeAccion.DeUsuario(quienRegistra, registro.FechaDevolucion), new List<UnidadDevuelta> { unidad });
        devolucion.Aplicar();
        new RepositorioDevolucionSql(preparacion).AgregarDevolucion(devolucion);
    }

    public void GuardarUsuario(string dni, TipoUsuario tipoPredefinido)
    {
        using SqlContext preparacion = NuevoContexto();
        new RepositorioUsuarioSql(preparacion).AgregarUsuario(CrearUsuario(preparacion, dni, tipoPredefinido));
    }

    public void GuardarUsuarioDesactivado(string dni, TipoUsuario tipoPredefinido)
    {
        using SqlContext preparacion = NuevoContexto();
        Usuario usuario = CrearUsuario(preparacion, dni, tipoPredefinido);
        usuario.Desactivar();
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

    private static Prestamo PrestamoActivoDePrueba(SqlContext preparacion, RegistroDePrueba registro)
    {
        TipoEquipamiento tipo = new RepositorioCatalogoSql(preparacion)
            .ObtenerTipoPorNombre(registro.IdCatalogo, registro.NombreTipo)!;
        Usuario solicitante = new RepositorioUsuarioSql(preparacion).ObtenerUsuarioPorDni(registro.DniSolicitante)!;
        Prestamo prestamo = new Prestamo(solicitante, tipo, 1, registro.FechaPrestamo);
        List<UnidadFisica> unidades = tipo.Unidades.Where(unidad => unidad.EstaDisponible()).Take(1).ToList();
        if (registro.DniEvaluador.Length == 0)
        {
            prestamo.Activar(unidades);
        }
        else
        {
            prestamo.Aprobar(new RepositorioUsuarioSql(preparacion).ObtenerUsuarioPorDni(registro.DniEvaluador)!,
                unidades);
        }

        new RepositorioPrestamoSql(preparacion).AgregarPrestamo(prestamo);
        return prestamo;
    }

    private static Prestamo CrearPrestamo(SqlContext preparacion, int idCatalogo, string nombreTipo,
        string dniSolicitante)
    {
        TipoEquipamiento tipo = new RepositorioCatalogoSql(preparacion).ObtenerTipoPorNombre(idCatalogo, nombreTipo)!;
        Usuario solicitante = new RepositorioUsuarioSql(preparacion).ObtenerUsuarioPorDni(dniSolicitante)!;
        return new Prestamo(solicitante, tipo, 1, FechaDePrestamo);
    }

    private static Usuario CrearUsuario(SqlContext preparacion, string dni, TipoUsuario tipoPredefinido)
    {
        Usuario usuario = new Usuario(dni, "Nombre", "Apellido", dni + "@ucd.edu.uy", ContrasenaDePrueba);
        usuario.AsignarTipo(new RepositorioTipoUsuarioSql(preparacion).ObtenerTipoPorNombre(tipoPredefinido.Nombre)!);
        return usuario;
    }

}
