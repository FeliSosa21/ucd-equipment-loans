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

    [TestMethod]
    public void Constructor_DatosValidos_ConservaSolicitanteTipoCantidadYFecha()
    {
        var solicitante = CrearSolicitante();
        var tipoEquipamiento = CrearTipoEquipamiento();
        const int otraCantidadSolicitada = 1;
        
        var prestamo = CrearPrestamo(solicitante, tipoEquipamiento, otraCantidadSolicitada);
        
        Assert.AreSame(solicitante, prestamo.Solicitante);
        Assert.AreSame(tipoEquipamiento, prestamo.TipoEquipamiento);
        Assert.AreEqual(otraCantidadSolicitada, prestamo.CantidadSolicitada);
        Assert.AreEqual(FechaFija, prestamo.FechaHoraPrestamo);
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

    private static Prestamo CrearPrestamo(Usuario? solicitante = null, TipoEquipamiento? tipoEquipamiento = null, int cantidadSolicitada = CantidadSolicitadaPorDefecto)
    {
        var prestamo = new Prestamo(solicitante ?? CrearSolicitante(), tipoEquipamiento ?? CrearTipoEquipamiento(), cantidadSolicitada, FechaFija);
        return prestamo;
    }
}