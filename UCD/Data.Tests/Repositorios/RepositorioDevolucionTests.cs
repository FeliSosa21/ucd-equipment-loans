using Data;
using Data.Repositorios;
using Dominio;

namespace Data.Tests.Repositorios;

[TestClass]
public class RepositorioDevolucionTests
{
    private static readonly DateTime InicioPrestamo = new DateTime(2026, 9, 22, 10, 0, 0);

    [TestMethod]
    public void ObtenerTodas_MemoriaNueva_DevuelveVacia()
    {
        RepositorioDevolucion repositorio = new RepositorioDevolucion(new Memoria());

        IReadOnlyList<Devolucion> devoluciones = repositorio.ObtenerTodas();

        Assert.AreEqual(0, devoluciones.Count);
    }

    [TestMethod]
    public void AgregarDevolucion_Valida_LaGuarda()
    {
        RepositorioDevolucion repositorio = new RepositorioDevolucion(new Memoria());
        Devolucion devolucion = CrearDevolucion();

        repositorio.AgregarDevolucion(devolucion);

        CollectionAssert.AreEqual(new List<Devolucion> { devolucion }, repositorio.ObtenerTodas().ToList());
    }

    [TestMethod]
    public void ObtenerTodas_OtroRepositorioSobreLaMismaMemoria_LaEncuentra()
    {
        Memoria memoria = new Memoria();
        Devolucion devolucion = CrearDevolucion();
        new RepositorioDevolucion(memoria).AgregarDevolucion(devolucion);

        IReadOnlyList<Devolucion> devoluciones = new RepositorioDevolucion(memoria).ObtenerTodas();

        Assert.AreSame(devolucion, devoluciones[0]);
    }

    [TestMethod]
    public void ObtenerDevoluciones_SinFiltro_LasDevuelveDeLaMasViejaALaMasNueva()
    {
        RepositorioDevolucion repositorio = new RepositorioDevolucion(new Memoria());
        Devolucion tardia = CrearDevolucion(diasDespuesDelPrestamo: 3);
        Devolucion temprana = CrearDevolucion(diasDespuesDelPrestamo: 1);
        repositorio.AgregarDevolucion(tardia);
        repositorio.AgregarDevolucion(temprana);

        IReadOnlyList<Devolucion> devoluciones = repositorio.ObtenerDevoluciones(new FiltroPorCatalogoYFechas());

        CollectionAssert.AreEqual(new List<Devolucion> { temprana, tardia }, devoluciones.ToList());
    }

    [TestMethod]
    public void ObtenerDevoluciones_ConFechas_IncluyeDesdeYExcluyeHasta()
    {
        RepositorioDevolucion repositorio = new RepositorioDevolucion(new Memoria());
        repositorio.AgregarDevolucion(CrearDevolucion(diasDespuesDelPrestamo: 1));
        Devolucion enDesde = CrearDevolucion(diasDespuesDelPrestamo: 2);
        repositorio.AgregarDevolucion(enDesde);
        repositorio.AgregarDevolucion(CrearDevolucion(diasDespuesDelPrestamo: 3));
        FiltroPorCatalogoYFechas filtro = new FiltroPorCatalogoYFechas
        {
            Desde = InicioPrestamo.AddDays(2),
            Hasta = InicioPrestamo.AddDays(3)
        };

        IReadOnlyList<Devolucion> devoluciones = repositorio.ObtenerDevoluciones(filtro);

        CollectionAssert.AreEqual(new List<Devolucion> { enDesde }, devoluciones.ToList());
    }

    private static Devolucion CrearDevolucion(int diasDespuesDelPrestamo = 1)
    {
        Usuario solicitante = new Usuario(dni: "45678905", nombre: "Ana", apellido: "Perez",
            correo: "ana@hotmail.com", contrasena: "VivaLaOrt2026");
        TipoEquipamiento tipo = new TipoEquipamiento(nombre: "Proyector Epson", descripcion: "Proyector para aulas",
            duracionMaxPrestamo: 5, requiereAprobacion: false, categoria: Categoria.Audiovisual);
        Prestamo prestamo = new Prestamo(solicitante, tipo, cantidadSolicitada: 1, fechaHoraPrestamo: InicioPrestamo);
        UnidadFisica unidad = new UnidadFisica(idInstitucional: "EQP-0001", ubiFisica: "Deposito A", numSerie: null);
        tipo.AgregarUnidad(unidad);
        prestamo.Activar(new List<UnidadFisica> { unidad });
        Usuario funcionario = new Usuario(dni: "12345672", nombre: "Felipe", apellido: "Sosa",
            correo: "felipe@hotmail.com", contrasena: "VivaLaOrt2026");

        return new Devolucion(prestamo, RegistroDeAccion.DeUsuario(funcionario, InicioPrestamo.AddDays(diasDespuesDelPrestamo)),
            new List<UnidadDevuelta> { new UnidadDevuelta(unidad, CondicionDevolucion.SinObservaciones, null) });
    }
}
