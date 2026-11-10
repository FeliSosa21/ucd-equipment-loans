using Data;
using Data.Repositorios;
using Dominio;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioMovimientoTests
{
    private static readonly DateTime FechaFija = new(2026, 10, 5, 10, 0, 0);

    [TestMethod]
    public void ObtenerMovimientos_FiltroSinCriterios_DevuelveListaVacia()
    {
        RepositorioMovimiento repositorio = new RepositorioMovimiento(new Memoria());
        repositorio.AgregarMovimiento(CrearMovimiento(idInstitucional: "EQP-0001"));

        IReadOnlyList<Movimiento> resultado = repositorio.ObtenerMovimientos(new FiltroPorCatalogoYFechas());

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ObtenerMovimientos_FiltroPorFechas_DelMasRecienteAlMasViejoYEnEmpateElUltimoRegistrado()
    {
        RepositorioMovimiento repositorio = new RepositorioMovimiento(new Memoria());
        Movimiento masViejo = CrearMovimiento(idInstitucional: "EQP-0001", FechaFija);
        Movimiento masReciente = CrearMovimiento(idInstitucional: "EQP-0002", FechaFija.AddDays(2));
        Movimiento intermedio = CrearMovimiento(idInstitucional: "EQP-0003", FechaFija.AddDays(1));
        Movimiento empatadoConElMasReciente = CrearMovimiento(idInstitucional: "EQP-0004", FechaFija.AddDays(2));
        repositorio.AgregarMovimiento(masViejo);
        repositorio.AgregarMovimiento(masReciente);
        repositorio.AgregarMovimiento(intermedio);
        repositorio.AgregarMovimiento(empatadoConElMasReciente);
        FiltroPorCatalogoYFechas filtro = new FiltroPorCatalogoYFechas { Desde = FechaFija, Hasta = FechaFija.AddDays(3) };

        IReadOnlyList<Movimiento> resultado = repositorio.ObtenerMovimientos(filtro);

        CollectionAssert.AreEqual(
            new List<Movimiento> { empatadoConElMasReciente, masReciente, intermedio, masViejo },
            resultado.ToList());
    }

    [TestMethod]
    public void ObtenerMovimientos_FiltroPorFechas_IncluyeDesdeYExcluyeHasta()
    {
        RepositorioMovimiento repositorio = new RepositorioMovimiento(new Memoria());
        Movimiento antesDeDesde = CrearMovimiento(idInstitucional: "EQP-0001", FechaFija.AddMinutes(-1));
        Movimiento enDesde = CrearMovimiento(idInstitucional: "EQP-0002", FechaFija);
        Movimiento enHasta = CrearMovimiento(idInstitucional: "EQP-0003", FechaFija.AddDays(1));
        repositorio.AgregarMovimiento(antesDeDesde);
        repositorio.AgregarMovimiento(enDesde);
        repositorio.AgregarMovimiento(enHasta);
        FiltroPorCatalogoYFechas filtro = new FiltroPorCatalogoYFechas { Desde = FechaFija, Hasta = FechaFija.AddDays(1) };

        IReadOnlyList<Movimiento> resultado = repositorio.ObtenerMovimientos(filtro);

        CollectionAssert.AreEqual(new List<Movimiento> { enDesde }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerMovimientos_FiltroPorCatalogo_DevuelveSoloLosDeTiposDeEseCatalogo()
    {
        Memoria memoria = new Memoria();
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        Catalogo catalogo = new Catalogo("Catalogo de la facultad");
        catalogo.AgregarTipo(proyector);
        new RepositorioCatalogo(memoria).Guardar(catalogo);
        RepositorioMovimiento repositorio = new RepositorioMovimiento(memoria);
        Movimiento delCatalogo = CrearMovimiento(idInstitucional: "EQP-0001", FechaFija, proyector);
        repositorio.AgregarMovimiento(delCatalogo);
        repositorio.AgregarMovimiento(CrearMovimiento(idInstitucional: "EQP-0002", FechaFija, CrearTipo("Camara Sony")));

        IReadOnlyList<Movimiento> resultado =
            repositorio.ObtenerMovimientos(new FiltroPorCatalogoYFechas { IdCatalogo = catalogo.Id });

        CollectionAssert.AreEqual(new List<Movimiento> { delCatalogo }, resultado.ToList());
    }

    [TestMethod]
    public void AgregarMovimiento_MovimientoNulo_LanzaArgumentNullException()
    {
        Memoria memoria = new Memoria();
        RepositorioMovimiento repositorio = new RepositorioMovimiento(memoria);

        Assert.ThrowsException<ArgumentNullException>(() => repositorio.AgregarMovimiento(null!));
    }

    private static Usuario CrearSolicitante()
    {
        Usuario usuario = new Usuario(
            dni: "12345672",
            nombre: "Renato",
            apellido: "Alves",
            correo: "renato.alves@ejemplo.com",
            contrasena: "VivalaOrt2026");

        usuario.AsignarTipo(TipoUsuario.Docente());

        return usuario;
    }

    private static Usuario CrearEvaluador()
    {
        Usuario usuario = new Usuario(
            dni: "45678905",
            nombre: "Ana",
            apellido: "Perez",
            correo: "ana.perez@ejemplo.com",
            contrasena: "VivalaOrt2026");

        usuario.AsignarTipo(TipoUsuario.Supervisor());

        return usuario;
    }

    private static UnidadFisica CrearUnidad(string idInstitucional)
    {
        UnidadFisica unidad = new UnidadFisica(
            idInstitucional: idInstitucional,
            ubiFisica: "Laboratorio Prueba",
            numSerie: null);

        return unidad;
    }

    private static TipoEquipamiento CrearTipo(string nombre)
    {
        return new TipoEquipamiento(nombre: nombre, descripcion: "Equipo para aulas", duracionMaxPrestamo: 5,
            requiereAprobacion: true, categoria: Categoria.Audiovisual);
    }

    private static Movimiento CrearMovimiento(string idInstitucional)
    {
        return CrearMovimiento(idInstitucional, FechaFija);
    }

    private static Movimiento CrearMovimiento(string idInstitucional, DateTime fechaHora)
    {
        return CrearMovimiento(idInstitucional, fechaHora, CrearTipo("Proyector Epson"));
    }

    private static Movimiento CrearMovimiento(string idInstitucional, DateTime fechaHora, TipoEquipamiento tipo)
    {
        Usuario evaluador = CrearEvaluador();
        UnidadFisica unidad = CrearUnidad(idInstitucional: idInstitucional);
        tipo.AgregarUnidad(unidad);
        Prestamo prestamo = new Prestamo(CrearSolicitante(), tipo, cantidadSolicitada: 1, fechaHoraPrestamo: FechaFija);
        prestamo.Aprobar(evaluador, new List<UnidadFisica> { unidad });

        return Movimiento.DePrestamo(prestamo, RegistroDeAccion.DeUsuario(evaluador, fechaHora), unidad);
    }
}
