using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioDevolucionTests
{
    private const string DniFuncionario = "12345672";
    private const string DniSolicitante = "45678905";
    private static readonly DateTime InicioPrestamo = new DateTime(2026, 9, 22, 10, 0, 0);

    [TestMethod]
    public void CondicionesPosibles_DevuelveLasTresCondiciones()
    {
        ServicioDevolucion servicio = CrearServicio(out _);

        List<string> condiciones = servicio.CondicionesPosibles();

        CollectionAssert.AreEqual(new List<string> { "Sin observaciones", "Con fallas", "Dañada" }, condiciones);
    }

    [TestMethod]
    public void ListarPrestamosActivos_UnoActivo_DevuelveSusDatos()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);

        List<PrestamoActivoDto> activos = servicio.ListarPrestamosActivos();

        Assert.AreEqual(1, activos.Count);
        Assert.AreEqual(prestamo.Id, activos[0].Id);
        Assert.AreEqual("Ana Perez", activos[0].Solicitante);
        Assert.AreEqual("Proyector Epson", activos[0].TipoEquipamiento);
        CollectionAssert.AreEqual(new List<string> { "EQP-0001", "EQP-0002" }, activos[0].Unidades);
    }

    [TestMethod]
    public void RegistrarDevolucion_DatosValidos_ElPrestamoDejaDeEstarActivo()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);

        servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario);

        Assert.AreEqual(EstadoPrestamo.Devuelto, prestamo.Estado);
        Assert.AreEqual(0, servicio.ListarPrestamosActivos().Count);
    }

    [TestMethod]
    public void RegistrarDevolucion_CondicionPorUnidad_ActualizaCadaUnidad()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id, condicionSegunda: "Dañada");

        servicio.RegistrarDevolucion(datos, DniFuncionario);

        Assert.AreEqual(EstadoUnidadFisica.Disponible, prestamo.UnidadesAsignadas[0].Estado);
        Assert.AreEqual(EstadoUnidadFisica.EnMantenimiento, prestamo.UnidadesAsignadas[1].Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_DatosValidos_LaGuarda()
    {
        Memoria memoria = new Memoria();
        ServicioDevolucion servicio = CrearServicio(memoria, out Prestamo prestamo);

        servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario);

        Devolucion guardada = new RepositorioDevolucion(memoria).ObtenerTodas().Single();
        Assert.AreSame(prestamo, guardada.Prestamo);
        Assert.AreEqual(DniFuncionario, guardada.RegistradaPor.Dni);
    }

    [TestMethod]
    public void RegistrarDevolucion_DatosValidos_RegistraUnMovimientoPorUnidad()
    {
        Memoria memoria = new Memoria();
        ServicioDevolucion servicio = CrearServicio(memoria, out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id);

        servicio.RegistrarDevolucion(datos, DniFuncionario);

        List<Movimiento> movimientos = new RepositorioMovimiento(memoria).ObtenerTodos().ToList();
        Assert.AreEqual(2, movimientos.Count);
        CollectionAssert.AreEquivalent(prestamo.UnidadesAsignadas.ToList(),
            movimientos.Select(movimiento => movimiento.Unidad).ToList());
    }

    [TestMethod]
    public void RegistrarDevolucion_DatosValidos_ElMovimientoTieneTipoFechaYSolicitante()
    {
        Memoria memoria = new Memoria();
        ServicioDevolucion servicio = CrearServicio(memoria, out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id);

        servicio.RegistrarDevolucion(datos, DniFuncionario);

        Movimiento movimiento = new RepositorioMovimiento(memoria).ObtenerTodos()[0];
        Assert.AreEqual(TipoMovimiento.Devolucion, movimiento.Tipo);
        Assert.AreEqual(datos.FechaHoraDevolucion, movimiento.FechaHora);
        Assert.AreSame(prestamo.Solicitante, movimiento.Solicitante);
    }

    [TestMethod]
    public void RegistrarDevolucion_DevolucionFallida_NoRegistraMovimientos()
    {
        Memoria memoria = new Memoria();
        ServicioDevolucion servicio = CrearServicio(memoria, out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id);
        datos.Unidades.RemoveAt(1);

        Assert.ThrowsException<ServicioException>(() => servicio.RegistrarDevolucion(datos, DniFuncionario));
        Assert.AreEqual(0, new RepositorioMovimiento(memoria).ObtenerTodos().Count);
    }

    [TestMethod]
    public void RegistrarDevolucion_DatosNulos_LanzaArgumentNullException()
    {
        ServicioDevolucion servicio = CrearServicio(out _);

        Assert.ThrowsException<ArgumentNullException>(() => servicio.RegistrarDevolucion(null!, DniFuncionario));
    }

    [TestMethod]
    public void RegistrarDevolucion_PrestamoInexistente_LanzaExcepcion()
    {
        ServicioDevolucion servicio = CrearServicio(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(CrearDatos(Guid.NewGuid()), DniFuncionario));
    }

    [TestMethod]
    public void RegistrarDevolucion_ResponsableSinPermiso_LanzaExcepcion()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniSolicitante));
        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_ResponsableInexistente_LanzaExcepcion()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), "30000004"));
        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_UnidadQueNoEsDelPrestamo_LanzaExcepcion()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id);
        datos.Unidades[1].IdInstitucional = "EQP-0099";

        Assert.ThrowsException<ServicioException>(() => servicio.RegistrarDevolucion(datos, DniFuncionario));
        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_CondicionInexistente_LanzaExcepcion()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id, condicionSegunda: "Perdida");

        Assert.ThrowsException<ServicioException>(() => servicio.RegistrarDevolucion(datos, DniFuncionario));
    }

    [TestMethod]
    public void RegistrarDevolucion_FaltaUnaUnidad_LanzaExcepcionYNoCambiaNada()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id);
        datos.Unidades.RemoveAt(1);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(datos, DniFuncionario));
        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DominioException));
        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
        Assert.AreEqual(EstadoUnidadFisica.Prestada, prestamo.UnidadesAsignadas[0].Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_FechaAnteriorAlPrestamo_LanzaExcepcion()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id);
        datos.FechaHoraDevolucion = InicioPrestamo.AddDays(-1);

        Assert.ThrowsException<ServicioException>(() => servicio.RegistrarDevolucion(datos, DniFuncionario));
    }

    [TestMethod]
    public void RegistrarDevolucion_ResponsableDesactivado_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioDevolucion servicio = CrearServicio(memoria, out Prestamo prestamo);
        new RepositorioUsuario(memoria).ObtenerUsuarioPorDni(DniFuncionario)!.Desactivar();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario));
        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_ObservacionesDemasiadoLargas_LanzaExcepcionConLaDelDominio()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id, condicionSegunda: "Con fallas");
        datos.Unidades[1].Observaciones = new string('a', 301);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(datos, DniFuncionario));
        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DominioException));
        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_PrestamoYaDevuelto_LanzaExcepcionYNoLaGuardaDosVeces()
    {
        Memoria memoria = new Memoria();
        ServicioDevolucion servicio = CrearServicio(memoria, out Prestamo prestamo);
        servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario));
        Assert.AreEqual(1, new RepositorioDevolucion(memoria).ObtenerTodas().Count);
    }

    private static DevolucionDto CrearDatos(Guid idPrestamo, string condicionSegunda = "Sin observaciones")
    {
        return new DevolucionDto
        {
            IdPrestamo = idPrestamo,
            FechaHoraDevolucion = InicioPrestamo.AddDays(2),
            Unidades = new List<UnidadDevueltaDto>
            {
                new UnidadDevueltaDto { IdInstitucional = "EQP-0001", Condicion = "Sin observaciones" },
                new UnidadDevueltaDto { IdInstitucional = "EQP-0002", Condicion = condicionSegunda }
            }
        };
    }

    private static ServicioDevolucion CrearServicio(out Prestamo prestamo)
    {
        return CrearServicio(new Memoria(), out prestamo);
    }

    private static ServicioDevolucion CrearServicio(Memoria memoria, out Prestamo prestamo)
    {
        RepositorioUsuario usuarios = new RepositorioUsuario(memoria);
        RepositorioPrestamo prestamos = new RepositorioPrestamo(memoria);

        Usuario funcionario = new Usuario(dni: DniFuncionario, nombre: "Felipe", apellido: "Sosa",
            correo: "felipe@hotmail.com", contrasena: "VivaLaOrt2026");
        funcionario.AsignarTipo(TipoUsuario.FuncionarioCE);
        Usuario solicitante = new Usuario(dni: DniSolicitante, nombre: "Ana", apellido: "Perez",
            correo: "ana@hotmail.com", contrasena: "VivaLaOrt2026");
        solicitante.AsignarTipo(TipoUsuario.Docente);
        usuarios.AgregarUsuario(funcionario);
        usuarios.AgregarUsuario(solicitante);

        TipoEquipamiento tipo = new TipoEquipamiento(nombre: "Proyector Epson", descripcion: "Proyector para aulas",
            duracionMaxPrestamo: 5, requiereAprobacion: false, categoria: Categoria.Audiovisual);
        prestamo = new Prestamo(solicitante, tipo, cantidadSolicitada: 2, fechaHoraPrestamo: InicioPrestamo);
        prestamo.Activar(new List<UnidadFisica>
        {
            new UnidadFisica(idInstitucional: "EQP-0001", ubiFisica: "Deposito A", numSerie: null),
            new UnidadFisica(idInstitucional: "EQP-0002", ubiFisica: "Deposito A", numSerie: null)
        });
        prestamos.AgregarPrestamo(prestamo);

        return new ServicioDevolucion(prestamos, usuarios, new RepositorioDevolucion(memoria),
            new RepositorioMovimiento(memoria));
    }
}
