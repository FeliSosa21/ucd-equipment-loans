namespace Dominio.Tests;

[TestClass]
public sealed class PrestamoTests
{
    private static readonly DateTime FechaFija = new(2026, 10, 5, 10, 0, 0);
    private const int CantidadSolicitadaPorDefecto = 2;

    [TestMethod]
    public void Constructor_DatosValidos_NaceSolicitado()
    {
        var prestamo = CrearPrestamo();
        
        Assert.AreEqual(EstadoPrestamo.Solicitado, prestamo.Estado);
    }
    
    private static Usuario CrearSolicitante()
    {
        var usuario = new Usuario("12345672", "Renato", "Alves", "renato.alves@ejemplo.com", "VivalaOrt2026");
        usuario.AsignarTipo(TipoUsuario.Docente);
        return usuario;
    }

    private static TipoEquipamiento CrearTipoEquipamiento()
    {
        var tipo = new TipoEquipamiento("Proyector Epson PowerLite",
            "Proyector para aulas",
            5,
            false,
            Categoria.Audiovisual);
        return tipo;
    }

    private static Prestamo CrearPrestamo(int cantidadSolicitada = CantidadSolicitadaPorDefecto)
    {
        var prestamo = new Prestamo(CrearSolicitante(), CrearTipoEquipamiento(), cantidadSolicitada, FechaFija);
        return prestamo;
    }
}