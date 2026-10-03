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
    public void ObtenerPrestamoPorId_IdInexistente_DevuelveNulo()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorio = new RepositorioPrestamo(memoria);
        Prestamo prestamo = CrearPrestamo();
        repositorio.AgregarPrestamo(prestamo);

        Prestamo? resultado = repositorio.ObtenerPrestamoPorId(id: Guid.NewGuid());
        
        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void ObtenerPrestamosActivos_UnoActivoYUnoSolicitado_DevuelveSoloElActivo()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorio = new RepositorioPrestamo(memoria);
        Prestamo solicitado = CrearPrestamo();
        (Prestamo activo, _) = CrearPrestamoActivo();
        repositorio.AgregarPrestamo(solicitado);
        repositorio.AgregarPrestamo(activo);

        IReadOnlyList<Prestamo> resultado = repositorio.ObtenerPrestamosActivos();
        
        CollectionAssert.AreEquivalent(new List<Prestamo> { activo }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerPrestamosActivos_PrestamoDevueltoDespuesDeAgregarlo_NoLoIncluye()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorio = new RepositorioPrestamo(memoria);
        (Prestamo prestamo, List<UnidadFisica> unidades) = CrearPrestamoActivo();
        repositorio.AgregarPrestamo(prestamo);
        foreach (UnidadFisica unidad in unidades)
        {
            unidad.Disponible();
        }
        prestamo.MarcarDevuelto();

        IReadOnlyList<Prestamo> resultado = repositorio.ObtenerPrestamosActivos();

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ObtenerPrestamoPorId_OtroRepositorioSobreLaMismaMemoria_LoEncuentra()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorioQueGuarda = new RepositorioPrestamo(memoria);
        RepositorioPrestamo repositorioQueBusca = new RepositorioPrestamo(memoria);
        Prestamo prestamo = CrearPrestamo();
        repositorioQueGuarda.AgregarPrestamo(prestamo);

        Prestamo? resultado = repositorioQueBusca.ObtenerPrestamoPorId(id: prestamo.Id);

        Assert.AreSame(prestamo, resultado);
    }

    private static Usuario CrearSolicitante()
    {
        Usuario usuario = new Usuario(
            dni: "12345672",
            nombre: "Renato",
            apellido: "Alves",
            correo: "renato.alves@ejemplo.com",
            contrasena: "VivalaOrt2026");

        usuario.AsignarTipo(TipoUsuario.Docente);

        return usuario;
    }

    private static TipoEquipamiento CrearTipoEquipamiento()
    {
        TipoEquipamiento tipo = new TipoEquipamiento(
            nombre: "Proyector Epson PowerLite",
            descripcion: "Proyector para aulas",
            duracionMaxPrestamo: 5,
            requiereAprobacion: false,
            categoria: Categoria.Audiovisual);

        return tipo;
    }

    private static Prestamo CrearPrestamo(
        Usuario? solicitante = null,
        TipoEquipamiento? tipoEquipamiento = null,
        int cantidadSolicitada = CantidadSolicitadaPorDefecto)
    {
        Prestamo prestamo = new Prestamo(
            solicitante: solicitante ?? CrearSolicitante(),
            tipoEquipamiento: tipoEquipamiento ?? CrearTipoEquipamiento(),
            cantidadSolicitada: cantidadSolicitada,
            fechaHoraPrestamo: FechaFija);

        return prestamo;
    }

    private static List<UnidadFisica> CrearUnidadesFisicas(
        int cantidad,
        int primerNumero = 1)
    {
        List<UnidadFisica> unidades = new List<UnidadFisica>();

        for (int i = 0; i < cantidad; i++)
        {
            int numero = primerNumero + i;

            unidades.Add(new UnidadFisica(
                idInstitucional: $"EQP-{numero:D4}",
                ubiFisica: "Laboratorio Prueba",
                numSerie: null));
        }

        return unidades;
    }

    private static (Prestamo prestamo, List<UnidadFisica> unidades) CrearPrestamoActivo()
    {
        Prestamo prestamo = CrearPrestamo();
        List<UnidadFisica> unidades = CrearUnidadesFisicas(cantidad: CantidadSolicitadaPorDefecto);

        prestamo.Activar(unidades);

        return (prestamo, unidades);
    }
}