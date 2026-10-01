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

    [TestMethod]
    public void Constructor_DosPrestamos_TienenIdDistinto()
    {
        var primerPrestamo = CrearPrestamo();
        var segundoPrestamo = CrearPrestamo();

        Assert.AreNotEqual(primerPrestamo.Id, segundoPrestamo.Id);
    }

    [TestMethod]
    public void Constructor_DatosValidos_NaceSinUnidadesAsignadas()
    {
        var prestamo = CrearPrestamo();

        Assert.IsNotNull(prestamo.UnidadesAsignadas);
        Assert.AreEqual(0, prestamo.UnidadesAsignadas.Count);
    }

    [TestMethod]
    public void Activar_CantidadSolicitadaDeUnidadesDisponibles_QuedaActivo()
    {
        var prestamo = CrearPrestamo();
        var unidades = CrearUnidadesFisicas(CantidadSolicitadaPorDefecto);

        prestamo.Activar(unidades);

        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
    }

    [TestMethod]
    public void Activar_CantidadSolicitadaDeUnidadesDisponibles_AsignaEsasUnidades()
    {
        var prestamo = CrearPrestamo();
        var unidades = CrearUnidadesFisicas(CantidadSolicitadaPorDefecto);

        prestamo.Activar(unidades);

        CollectionAssert.AreEquivalent(unidades, prestamo.UnidadesAsignadas.ToList());
    }

    [TestMethod]
    public void Activar_CantidadSolicitadaDeUnidadesDisponibles_UnidadesQuedanPrestadas()
    {
        var prestamo = CrearPrestamo();
        var unidades = CrearUnidadesFisicas(CantidadSolicitadaPorDefecto);

        prestamo.Activar(unidades);

        foreach (var unidad in unidades)
        {
            Assert.AreEqual(EstadoUnidadFisica.Prestada, unidad.Estado,
                $"La unidad {unidad.IdInstitucional} deberia estar prestada");
        }
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(3)]
    public void Activar_CantidadDistintaALaSolicitada_LanzaExcepcion(int cantidadEntregada)
    {
        var prestamo = CrearPrestamo();
        var unidades = CrearUnidadesFisicas(cantidadEntregada);

        Assert.ThrowsException<DominioException>(() => prestamo.Activar(unidades));
        Assert.AreEqual(EstadoPrestamo.Solicitado, prestamo.Estado);
    }

    [TestMethod]
    public void Activar_AlgunaUnidadNoDisponible_LanzaExcepcionYNoPrestaNinguna()
    {
        var prestamo = CrearPrestamo();
        var unidades = CrearUnidadesFisicas(CantidadSolicitadaPorDefecto);
        var disponible = unidades[0];
        var prestada = unidades[1];
        prestada.Prestada();

        Assert.ThrowsException<DominioException>(() => prestamo.Activar(unidades));
        Assert.AreEqual(EstadoUnidadFisica.Disponible, disponible.Estado);
        Assert.AreEqual(EstadoPrestamo.Solicitado, prestamo.Estado);
        Assert.AreEqual(0, prestamo.UnidadesAsignadas.Count);
    }

    [TestMethod]
    public void Activar_PrestamoYaActivo_LanzaExcepcion()
    {
        var prestamo = CrearPrestamo();
        var primerasUnidades = CrearUnidadesFisicas(CantidadSolicitadaPorDefecto);
        prestamo.Activar(primerasUnidades);
        var nuevasUnidades = CrearUnidadesFisicas(CantidadSolicitadaPorDefecto);
        
        Assert.ThrowsException<DominioException>(() => prestamo.Activar(nuevasUnidades));
        foreach (var unidad in nuevasUnidades)
        {
            Assert.AreEqual(EstadoUnidadFisica.Disponible, unidad.Estado,
                $"La unidad {unidad.IdInstitucional} deberia seguir disponible");
        }
        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
        CollectionAssert.AreEquivalent(primerasUnidades, prestamo.UnidadesAsignadas.ToList());
    }

    [TestMethod]
    public void MarcarDevuelto_PrestamoActivoSinUnidadesPrestadas_QuedaDevuelto()
    {
        var prestamo = CrearPrestamo();
        var unidades = CrearUnidadesFisicas(CantidadSolicitadaPorDefecto);
        prestamo.Activar(unidades);
        unidades[0].Disponible();
        unidades[1].AMantenimiento();
        
        prestamo.MarcarDevuelto();
        
        Assert.AreEqual(EstadoPrestamo.Devuelto, prestamo.Estado);
    }

    [TestMethod]
    public void MarcarDevuelto_PrestamoSolicitado_LanzaExcepcion()
    {
        var prestamo = CrearPrestamo();
        
        Assert.ThrowsException<DominioException>(() => prestamo.MarcarDevuelto());
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

    private static Prestamo CrearPrestamo(Usuario? solicitante = null, TipoEquipamiento? tipoEquipamiento = null, int cantidadSolicitada = CantidadSolicitadaPorDefecto)
    {
        var prestamo = new Prestamo(solicitante ?? CrearSolicitante(), tipoEquipamiento ?? CrearTipoEquipamiento(), cantidadSolicitada, FechaFija);
        return prestamo;
    }

    private static List<UnidadFisica> CrearUnidadesFisicas(
        int cantidad,
        int primerNumero = 1)
    {
        var unidades = new List<UnidadFisica>();
        for (int i = 0; i < cantidad; i++)
        {
            var numero = primerNumero + i;
            unidades.Add(new UnidadFisica(
                $"EQP-{numero:D4}",
                "Laboratorio Prueba",
                null
            ));
        }
        return unidades;
    }

    
}