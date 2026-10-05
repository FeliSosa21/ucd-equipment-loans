using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public sealed class ServicioPrestamoTests
{
    private static readonly DateTime FechaFija = new DateTime(2026, 10, 5, 10, 0, 0);

    private const string DniDocente = "12345672";
    private const string DniEstudiante = "45678905";
    private const string DniFuncionario = "23456783";
    private const string DniDocenteDesactivado = "34567894";
    private const string DniInexistente = "56789016";

    private const string NombreTipoSinAprobacion = "Proyector Epson PowerLite";
    private const string NombreTipoSinUnidades = "Parlante JBL Flip";
    private const string NombreTipoConAprobacion = "Camara Canon EOS R50";
    private const string NombreTipoDiscontinuado = "Retroproyector 3M";
    private const string NombreTipoInexistente = "Microscopio Zeiss Primo";

    private const int CantidadSolicitadaPorDefecto = 2;

    [TestMethod]
    public void Solicitar_DatosNulos_LanzaArgumentNullException()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);

        Assert.ThrowsException<ArgumentNullException>(
            () => servicio.Solicitar(null!, DniDocente));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniDocente);
    }

    [TestMethod]
    public void Solicitar_SolicitanteInexistente_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniInexistente));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniInexistente);
    }

    [TestMethod]
    public void Solicitar_SolicitanteDesactivado_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniDocenteDesactivado));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniDocenteDesactivado);
    }

    [TestMethod]
    public void Solicitar_SolicitanteSinPermiso_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Solicitar(datos, DniFuncionario));
        Assert.AreEqual("El solicitante no tiene permiso para solicitar prestamos", excepcion.Message);

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniFuncionario);
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionConUnidades_QuedaActivo()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        PrestamoDto resultado = servicio.Solicitar(datos, DniDocente);

        Assert.AreEqual("Activo", resultado.Estado);
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionConUnidades_PrestaLaCantidadSolicitada()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        servicio.Solicitar(datos, DniDocente);

        TipoEquipamiento tipo = new RepositorioCatalogo(memoria).ObtenerTipoPorNombre(NombreTipoSinAprobacion)!;

        Assert.AreEqual(CantidadSolicitadaPorDefecto, tipo.Unidades.Count(unidad => unidad.Estado == EstadoUnidadFisica.Prestada));
        Assert.AreEqual(1, tipo.CantidadDisponibles());
    }

    [TestMethod]
    public void Solicitar_DatosValidos_LoGuarda()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        servicio.Solicitar(datos, DniDocente);

        IReadOnlyList<Prestamo> guardados = new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(DniDocente);

        Assert.AreEqual(1, guardados.Count);
        Assert.AreEqual(EstadoPrestamo.Activo, guardados[0].Estado);
    }

    [TestMethod]
    public void Solicitar_DatosValidos_UsaElMomentoDelReloj()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        servicio.Solicitar(datos, DniDocente);

        Prestamo guardado =
            new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(DniDocente).Single();

        Assert.AreEqual(FechaFija, guardado.FechaHoraPrestamo);
    }

    [TestMethod] public void Solicitar_TipoSinAprobacionConUnidades_RegistraUnMovimientoDePrestamoPorUnidad()
    {

        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        servicio.Solicitar(datos, DniDocente);

        Prestamo guardado =
            new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(DniDocente).Single();
        IReadOnlyList<Movimiento> movimientos = new RepositorioMovimiento(memoria).ObtenerTodos();

        Assert.AreEqual(CantidadSolicitadaPorDefecto, movimientos.Count);
        foreach (Movimiento movimiento in movimientos)
        {
            Assert.AreEqual(TipoMovimiento.Prestamo, movimiento.Tipo);
            Assert.AreEqual(FechaFija, movimiento.FechaHora);
            Assert.AreSame(guardado.Solicitante, movimiento.Solicitante);
            Assert.IsNull(movimiento.Evaluador);
        }
        CollectionAssert.AreEquivalent(
            guardado.UnidadesAsignadas.ToList(),
            movimientos.Select(movimiento => movimiento.Unidad).ToList());
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionSinUnidadesSuficientes_QuedaRechazadoConMotivo()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoSinUnidades);

        PrestamoDto resultado = servicio.Solicitar(datos, DniDocente);

        Assert.AreEqual("Rechazado", resultado.Estado);
        Assert.IsFalse(string.IsNullOrWhiteSpace(resultado.MotivoRechazo),
            "La solicitud rechazada deberia tener un motivo");
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionSinUnidadesSuficientes_LoGuardaSinPrestarNiRegistrarMovimientos()
    {

        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(cantidadSolicitada: 4);

        servicio.Solicitar(datos, DniDocente);

        IReadOnlyList<Prestamo> guardados =
            new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(DniDocente);

        Assert.AreEqual(1, guardados.Count);
        Assert.AreEqual(EstadoPrestamo.Rechazado, guardados[0].Estado);

        Assert.AreEqual(0, guardados[0].UnidadesAsignadas.Count);
        TipoEquipamiento tipo = new RepositorioCatalogo(memoria).ObtenerTipoPorNombre(NombreTipoSinAprobacion)!;
        Assert.AreEqual(3, tipo.CantidadDisponibles());

        Assert.AreEqual(0, new RepositorioMovimiento(memoria).ObtenerTodos().Count);
    }

    [TestMethod]
    public void Solicitar_TipoConAprobacion_QuedaSolicitadoSinUnidades()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion);

        PrestamoDto resultado = servicio.Solicitar(datos, DniDocente);

        Assert.AreEqual("Solicitado", resultado.Estado);

        TipoEquipamiento tipo = new RepositorioCatalogo(memoria).ObtenerTipoPorNombre(NombreTipoConAprobacion)!;
        Assert.AreEqual(3, tipo.CantidadDisponibles());
    }

    [TestMethod]
    public void Solicitar_TipoConAprobacion_LoGuardaSinRegistrarMovimientos()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion);

        servicio.Solicitar(datos, DniDocente);

        IReadOnlyList<Prestamo> guardados = new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(DniDocente);
        Assert.AreEqual(1, guardados.Count);
        Assert.AreEqual(EstadoPrestamo.Solicitado, guardados[0].Estado);

        Assert.AreEqual(0, new RepositorioMovimiento(memoria).ObtenerTodos().Count);
    }

    [TestMethod]
    public void Solicitar_TipoInexistente_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoInexistente);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniDocente));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniDocente);
    }

    [TestMethod]
    public void Solicitar_TipoSinNombre_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: null!);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniDocente));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniDocente);
    }

    [TestMethod]
    public void Solicitar_TipoDiscontinuado_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoDiscontinuado);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniDocente));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniDocente);
    }

    [TestMethod]
    public void Solicitar_SuperaUnidadesPorPrestamo_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(cantidadSolicitada: 3);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniEstudiante));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniEstudiante);
    }

    [TestMethod]
    public void Solicitar_SuperaPrestamosConcurrentes_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        servicio.Solicitar(CrearSolicitud(cantidadSolicitada: 1), DniEstudiante);
        servicio.Solicitar(CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion, cantidadSolicitada: 1),
            DniEstudiante);
        SolicitudPrestamoDto tercera = CrearSolicitud(cantidadSolicitada: 1);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(tercera, DniEstudiante));

        Assert.AreEqual(2, new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(DniEstudiante).Count);
        Assert.AreEqual(1, new RepositorioMovimiento(memoria).ObtenerTodos().Count);
        TipoEquipamiento tipo = new RepositorioCatalogo(memoria).ObtenerTipoPorNombre(NombreTipoSinAprobacion)!;
        Assert.AreEqual(2, tipo.CantidadDisponibles());
    }

    [TestMethod]
    public void Solicitar_SolicitudesRechazadasPrevias_NoCuentanParaElLimite()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        servicio.Solicitar(CrearSolicitud(tipoEquipamiento: NombreTipoSinUnidades), DniEstudiante);
        servicio.Solicitar(CrearSolicitud(tipoEquipamiento: NombreTipoSinUnidades), DniEstudiante);

        PrestamoDto resultado = servicio.Solicitar(CrearSolicitud(), DniEstudiante);

        Assert.AreEqual("Activo", resultado.Estado);
    }

    private static ServicioPrestamo CrearServicio(Memoria memoria)
    {
        CargarCatalogo(memoria);
        CargarUsuarios(memoria);

        return new ServicioPrestamo(
            prestamos: new RepositorioPrestamo(memoria),
            usuarios: new RepositorioUsuario(memoria),
            catalogo: new RepositorioCatalogo(memoria),
            movimientos: new RepositorioMovimiento(memoria),
            reloj: new RelojFijo(FechaFija));
    }

    private static SolicitudPrestamoDto CrearSolicitud(
        string tipoEquipamiento = NombreTipoSinAprobacion,
        int cantidadSolicitada = CantidadSolicitadaPorDefecto)
    {
        return new SolicitudPrestamoDto
        {
            TipoEquipamiento = tipoEquipamiento,
            CantidadSolicitada = cantidadSolicitada
        };
    }

    private static void CargarCatalogo(Memoria memoria)
    {
        Catalogo catalogo = new Catalogo("Catalogo general");

        catalogo.AgregarTipo(CrearTipoEquipamiento(
            nombre: NombreTipoSinAprobacion,
            requiereAprobacion: false,
            cantidadUnidades: 3));
        catalogo.AgregarTipo(CrearTipoEquipamiento(
            nombre: NombreTipoSinUnidades,
            requiereAprobacion: false,
            cantidadUnidades: 0));
        catalogo.AgregarTipo(CrearTipoEquipamiento(
            nombre: NombreTipoConAprobacion,
            requiereAprobacion: true,
            cantidadUnidades: 3,
            primerNumero: 101));

        TipoEquipamiento tipoEquipamientoDiscontinuado = CrearTipoEquipamiento(
            nombre: NombreTipoDiscontinuado,
            requiereAprobacion: false,
            cantidadUnidades: 3,
            primerNumero: 201);

        tipoEquipamientoDiscontinuado.Discontinuar();
        catalogo.AgregarTipo(tipoEquipamientoDiscontinuado);

        new RepositorioCatalogo(memoria).Guardar(catalogo);
    }

    private static TipoEquipamiento CrearTipoEquipamiento(string nombre, bool requiereAprobacion, int cantidadUnidades,
        int primerNumero = 1)
    {
        TipoEquipamiento tipoEquipamiento = new TipoEquipamiento(
            nombre: nombre,
            descripcion: "Equipo audiovisual para pruebas de solicitud",
            duracionMaxPrestamo: 5,
            requiereAprobacion: requiereAprobacion,
            categoria: Categoria.Audiovisual);

        for (int i = 0; i < cantidadUnidades; i++)
        {
            int numero = primerNumero + i;

            tipoEquipamiento.AgregarUnidad(new UnidadFisica(
                idInstitucional: $"EQP-{numero:D4}",
                ubiFisica: "Deposito A",
                numSerie: null));
        }

        return tipoEquipamiento;
    }

    private static void CargarUsuarios(Memoria memoria)
    {
        RepositorioUsuario usuarios = new RepositorioUsuario(memoria);

        usuarios.AgregarUsuario(CrearUsuario(dni: DniDocente, tipoUsuario: TipoUsuario.Docente));
        usuarios.AgregarUsuario(CrearUsuario(dni: DniEstudiante, tipoUsuario: TipoUsuario.Estudiante));
        usuarios.AgregarUsuario(CrearUsuario(dni: DniFuncionario, tipoUsuario: TipoUsuario.FuncionarioCE));

        Usuario desactivado = CrearUsuario(dni: DniDocenteDesactivado, tipoUsuario: TipoUsuario.Docente);
        desactivado.Desactivar();
        usuarios.AgregarUsuario(desactivado);
    }

    private static Usuario CrearUsuario(string dni, TipoUsuario tipoUsuario)
    {
        Usuario usuario = new Usuario(
            dni: dni,
            nombre: "Renato",
            apellido: "Alves",
            correo: $"usuario{dni}@ejemplo.com",
            contrasena: "Vivalaort2026");

        usuario.AsignarTipo(tipoUsuario);

        return usuario;
    }

    private static void VerificarQueNoSeGuardoNiSePrestoNada(Memoria memoria, string dni)
    {
        Assert.AreEqual(0, new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(dni: dni).Count,
            "No deberia haberse guardado ningun prestamo");
        Assert.AreEqual(0, new RepositorioMovimiento(memoria).ObtenerTodos().Count,
            "No deberia haberse registrado ningun movimiento");

        Catalogo catalogo = new RepositorioCatalogo(memoria).Obtener()!;
        Assert.IsTrue(catalogo.Tipos.SelectMany(tipo => tipo.Unidades).All(unidad => unidad.EstaDisponible()),
            "Ninguna unidad deberia estar prestada");
    }
}