using Data;
using Data.Repositorios;
using Dominio;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioMovimientoTests
{
    private static readonly DateTime FechaFija = new(2026, 10, 5, 10, 0, 0);

    [TestMethod]
    public void ObtenerTodos_DosMovimientosAgregados_LosDevuelveEnElOrdenEnQueSeAgregaron()
    {
        Memoria memoria = new Memoria();
        RepositorioMovimiento repositorio = new RepositorioMovimiento(memoria);
        Movimiento primero = CrearMovimiento(idInstitucional: "EQP-0001");
        Movimiento segundo = CrearMovimiento(idInstitucional: "EQP-0002");
        repositorio.AgregarMovimiento(primero);
        repositorio.AgregarMovimiento(segundo);

        IReadOnlyList<Movimiento> resultado = repositorio.ObtenerTodos();

        CollectionAssert.AreEqual(new List<Movimiento> { primero, segundo }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerTodos_SinMovimientos_DevuelveListaVacia()
    {
        Memoria memoria = new Memoria();
        RepositorioMovimiento repositorio = new RepositorioMovimiento(memoria);

        IReadOnlyList<Movimiento> resultado = repositorio.ObtenerTodos();

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ObtenerTodos_OtroRepositorioSobreLaMismaMemoria_LosEncuentra()
    {
        Memoria memoria = new Memoria();
        RepositorioMovimiento repositorioQueGuarda = new RepositorioMovimiento(memoria);
        RepositorioMovimiento repositorioQueBusca = new RepositorioMovimiento(memoria);
        Movimiento movimiento = CrearMovimiento(idInstitucional: "EQP-0001");
        repositorioQueGuarda.AgregarMovimiento(movimiento);

        IReadOnlyList<Movimiento> resultado = repositorioQueBusca.ObtenerTodos();

        CollectionAssert.AreEqual(new List<Movimiento> { movimiento }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerTodos_ListaDevuelta_EsDeSoloLectura()
    {
        Memoria memoria = new Memoria();
        RepositorioMovimiento repositorio = new RepositorioMovimiento(memoria);

        IReadOnlyList<Movimiento> resultado = repositorio.ObtenerTodos();

        Assert.IsTrue(((ICollection<Movimiento>)resultado).IsReadOnly);
    }

    [TestMethod]
    public void ObtenerTodos_AgregarDespuesDeObtener_NoCambiaLaListaYaDevuelta()
    {
        Memoria memoria = new Memoria();
        RepositorioMovimiento repositorio = new RepositorioMovimiento(memoria);
        IReadOnlyList<Movimiento> resultado = repositorio.ObtenerTodos();
        Movimiento movimiento = CrearMovimiento(idInstitucional: "EQP-0001");

        repositorio.AgregarMovimiento(movimiento);

        Assert.AreEqual(0, resultado.Count);
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

        usuario.AsignarTipo(TipoUsuario.Docente);

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

        usuario.AsignarTipo(TipoUsuario.Supervisor);

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

    private static Movimiento CrearMovimiento(string idInstitucional)
    {
        Usuario evaluador = CrearEvaluador();
        UnidadFisica unidad = CrearUnidad(idInstitucional: idInstitucional);
        TipoEquipamiento tipo = new TipoEquipamiento(nombre: "Proyector Epson", descripcion: "Proyector para aulas",
            duracionMaxPrestamo: 5, requiereAprobacion: true, categoria: Categoria.Audiovisual);
        tipo.AgregarUnidad(unidad);
        Prestamo prestamo = new Prestamo(CrearSolicitante(), tipo, cantidadSolicitada: 1, fechaHoraPrestamo: FechaFija);
        prestamo.Aprobar(evaluador, new List<UnidadFisica> { unidad });

        return Movimiento.DePrestamo(prestamo, RegistroDeAccion.DeUsuario(evaluador, FechaFija), unidad);
    }
}
