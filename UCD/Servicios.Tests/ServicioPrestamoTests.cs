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