using Data;
using Data.Repositorios;
using Dominio;

namespace Data.Tests.Repositorios;

[TestClass]
public class RepositorioMovimientoTests
{
    private static readonly DateTime FechaFija = new(2026, 10, 5, 10, 0, 0);

    [TestMethod]
    public void ObtenerTodos_DosMovimientosAgregados_LosDevuelveEnElOrdenEnQueSeAgregaron()
    {
        Memoria memoria = new Memoria();
        RepositorioMovimiento repositorio = new RepositorioMovimiento(memoria);
        Movimiento primero = CrearMovimiento(tipo: TipoMovimiento.Prestamo, idInstitucional: "EQP-0001");
        Movimiento segundo = CrearMovimiento(tipo: TipoMovimiento.Devolucion, idInstitucional: "EQP-0002");
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
        Movimiento movimiento = CrearMovimiento(tipo: TipoMovimiento.Prestamo, idInstitucional: "EQP-0001");
        repositorioQueGuarda.AgregarMovimiento(movimiento);

        IReadOnlyList<Movimiento> resultado = repositorioQueBusca.ObtenerTodos();

        CollectionAssert.AreEqual(new List<Movimiento> { movimiento }, resultado.ToList());
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

    private static Movimiento CrearMovimiento(TipoMovimiento tipo, string idInstitucional)
    {
        Movimiento movimiento = new Movimiento(
            fechaHora: FechaFija,
            tipo: tipo,
            solicitante: CrearSolicitante(),
            evaluador: CrearEvaluador(),
            unidad: CrearUnidad(idInstitucional: idInstitucional));

        return movimiento;
    }
}
