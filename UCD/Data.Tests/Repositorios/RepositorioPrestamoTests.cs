using Data;
using Data.Repositorios;
using Dominio;

namespace Data.Tests.Repositorios;

[TestClass]
public class RepositorioPrestamoTests
{
    private static readonly DateTime FechaFija = new(2026, 10, 5, 10, 0, 0);
    private const int CantidadSolicitadaPorDefecto = 2;

    [TestMethod]
    public void ObtenerPrestamoPorId_PrestamoAgregado_DevuelveElMismoPrestamo()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorio = new RepositorioPrestamo(memoria);
        Prestamo prestamo = CrearPrestamo();
        repositorio.AgregarPrestamo(prestamo);
        
        Prestamo? resultado = repositorio.ObtenerPrestamoPorId(id: prestamo.Id);
        
        Assert.AreSame(prestamo, resultado);
    }

    [TestMethod]
    public void ObtenerUsuarioPorDni_DniInexistente_DevuelveNulo()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorio = new RepositorioPrestamo(memoria);
        Prestamo prestamo = CrearPrestamo();
        repositorio.AgregarPrestamo(prestamo);

        Prestamo? resultado = repositorio.ObtenerPrestamoPorId(id: Guid.NewGuid());
        
        Assert.IsNull(resultado);
    }
    
    private static Usuario CrearSolicitante()
    {
        var usuario = new Usuario(
            "12345672",
            "Renato",
            "Alves",
            "renato.alves@ejemplo.com",
            "VivalaOrt2026");

        usuario.AsignarTipo(TipoUsuario.Docente);

        return usuario;
    }

    private static TipoEquipamiento CrearTipoEquipamiento()
    {
        var tipo = new TipoEquipamiento(
            "Proyector Epson PowerLite",
            "Proyector para aulas",
            5,
            false,
            Categoria.Audiovisual);

        return tipo;
    }

    private static Prestamo CrearPrestamo(Usuario? solicitante = null, TipoEquipamiento? tipoEquipamiento = null, int cantidadSolicitada = CantidadSolicitadaPorDefecto)
    {
        var prestamo = new Prestamo(
            solicitante ?? CrearSolicitante(),
            tipoEquipamiento ?? CrearTipoEquipamiento(),
            cantidadSolicitada,
            FechaFija);

        return prestamo;
    }
}