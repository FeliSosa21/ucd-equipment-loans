namespace Dominio.Tests;

[TestClass]
public sealed class DevolucionTests
{
    private static readonly DateTime InicioPrestamo = new DateTime(2026, 9, 22, 10, 0, 0);

    [TestMethod]
    public void Constructor_DatosValidos_GuardaLosDatos()
    {
        DateTime fecha = new DateTime(2026, 9, 25, 15, 30, 0);
        Usuario funcionario = CrearUsuario();

        Devolucion devolucion = CrearDevolucion(fechaHoraDevolucion: fecha, condicion: CondicionDevolucion.ConFallas,
            observaciones: "Pantalla rayada", registradaPor: funcionario);

        Assert.AreEqual(fecha, devolucion.FechaHoraDevolucion);
        Assert.AreEqual(CondicionDevolucion.ConFallas, devolucion.Condicion);
        Assert.AreEqual("Pantalla rayada", devolucion.Observaciones);
        Assert.AreEqual(funcionario, devolucion.RegistradaPor);
    }

    [TestMethod]
    public void Constructor_SinObservaciones_LasDejaNulas()
    {
        Devolucion devolucion = CrearDevolucion(observaciones: null);

        Assert.IsNull(devolucion.Observaciones);
    }

    [TestMethod]
    public void Constructor_FechaAnteriorAlInicioDelPrestamo_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() =>
            CrearDevolucion(fechaHoraDevolucion: InicioPrestamo.AddMinutes(-1)));
    }

    [TestMethod]
    public void Constructor_FechaIgualAlInicioDelPrestamo_SeCrea()
    {
        Devolucion devolucion = CrearDevolucion(fechaHoraDevolucion: InicioPrestamo);

        Assert.AreEqual(InicioPrestamo, devolucion.FechaHoraDevolucion);
    }

    [TestMethod]
    [DataRow(301)]
    [DataRow(400)]
    public void Constructor_ObservacionesDeMasDe300Caracteres_LanzaExcepcion(int largo)
    {
        string observaciones = new string('a', largo);

        Assert.ThrowsException<DominioException>(() => CrearDevolucion(observaciones: observaciones));
    }

    [TestMethod]
    public void Constructor_ObservacionesDe300Caracteres_SeCrea()
    {
        string observaciones = new string('a', 300);

        Devolucion devolucion = CrearDevolucion(observaciones: observaciones);

        Assert.AreEqual(observaciones, devolucion.Observaciones);
    }

    [TestMethod]
    public void Constructor_SinUsuarioQueRegistra_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() =>
            new Devolucion(InicioPrestamo.AddDays(1), InicioPrestamo, CondicionDevolucion.SinObservaciones, null,
                null!));
    }

    [TestMethod]
    public void ActualizarUnidad_SinObservaciones_QuedaDisponible()
    {
        UnidadFisica unidad = CrearUnidadPrestada();
        Devolucion devolucion = CrearDevolucion(condicion: CondicionDevolucion.SinObservaciones);

        devolucion.ActualizarUnidad(unidad);

        Assert.AreEqual(EstadoUnidadFisica.Disponible, unidad.Estado);
    }

    [TestMethod]
    [DataRow(CondicionDevolucion.ConFallas)]
    [DataRow(CondicionDevolucion.Daniada)]
    public void ActualizarUnidad_ConFallasODaniada_PasaAMantenimiento(CondicionDevolucion condicion)
    {
        UnidadFisica unidad = CrearUnidadPrestada();
        Devolucion devolucion = CrearDevolucion(condicion: condicion);

        devolucion.ActualizarUnidad(unidad);

        Assert.AreEqual(EstadoUnidadFisica.EnMantenimiento, unidad.Estado);
    }

    private static Devolucion CrearDevolucion(DateTime? fechaHoraDevolucion = null,
        CondicionDevolucion condicion = CondicionDevolucion.SinObservaciones, string? observaciones = null,
        Usuario? registradaPor = null)
    {
        return new Devolucion(fechaHoraDevolucion ?? InicioPrestamo.AddDays(1), InicioPrestamo, condicion,
            observaciones, registradaPor ?? CrearUsuario());
    }

    private static Usuario CrearUsuario()
    {
        return new Usuario(dni: "12345672", nombre: "Felipe", apellido: "Sosa", correo: "felipe@hotmail.com",
            contrasena: "VivaLaOrt2026");
    }

    private static UnidadFisica CrearUnidadPrestada()
    {
        UnidadFisica unidad = new UnidadFisica("EQP-0042", "Deposito A", "PF3A7X21");
        unidad.Prestada();
        return unidad;
    }
}
