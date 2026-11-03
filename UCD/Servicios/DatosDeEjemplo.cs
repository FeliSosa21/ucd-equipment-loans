using Data.Repositorios;
using Dominio;

namespace Servicios;

// Datos de ejemplo para probar la aplicacion y para la demo: un usuario de cada tipo, tipos con y sin
// aprobacion, unidades en distintos estados, dos prestamos activos para probar las devoluciones y una
// solicitud pendiente para probar las aprobaciones. Cada prestamo deja los movimientos de ServicioPrestamo.Solicitar.
public class DatosDeEjemplo
{
    public const string ContrasenaEjemplo = ServicioAdministracionUsuarios.ContrasenaPredeterminada;
    public const string CorreoDocente = "ana.perez@ucd.edu.uy";
    public const string CorreoEstudiante = "juan.gomez@ucd.edu.uy";
    public const string CorreoFuncionario = "laura.diaz@ucd.edu.uy";
    public const string CorreoSupervisor = "sofia.martinez@ucd.edu.uy";
    public const string TipoConAprobacion = "Camara de fotos";

    private const string Deposito = "Deposito central";
    private const int DiasMaximosDePrestamo = 7;

    private readonly RepositorioUsuario _usuarios;
    private readonly RepositorioPrestamo _prestamos;
    private readonly RepositorioCatalogo _repositorioCatalogo;
    private readonly RepositorioMovimiento _movimientos;
    private readonly TimeProvider _reloj;

    public DatosDeEjemplo(RepositorioUsuario usuarios, RepositorioPrestamo prestamos,
        RepositorioCatalogo repositorioCatalogo, RepositorioMovimiento movimientos, TimeProvider reloj)
    {
        _usuarios = usuarios;
        _prestamos = prestamos;
        _repositorioCatalogo = repositorioCatalogo;
        _movimientos = movimientos;
        _reloj = reloj;
    }

    public void Cargar()
    {
        if (_usuarios.ObtenerUsuarioPorCorreo(CorreoDocente) != null)
        {
            return;
        }

        Catalogo catalogo = ObtenerCatalogo();
        CargarUsuarios();
        CargarCatalogo(catalogo);
        CargarPrestamos(catalogo);
    }

    private void CargarUsuarios()
    {
        CrearUsuario(dni: "12345672", nombre: "Ana", apellido: "Perez", correo: CorreoDocente,
            tipo: TipoUsuario.Docente());
        CrearUsuario(dni: "23456783", nombre: "Juan", apellido: "Gomez", correo: CorreoEstudiante,
            tipo: TipoUsuario.Estudiante());
        CrearUsuario(dni: "34567894", nombre: "Laura", apellido: "Diaz", correo: CorreoFuncionario,
            tipo: TipoUsuario.FuncionarioCE());
        CrearUsuario(dni: "61234569", nombre: "Sofia", apellido: "Martinez", correo: CorreoSupervisor,
            tipo: TipoUsuario.Supervisor());
    }

    private static void CargarCatalogo(Catalogo catalogo)
    {
        TipoEquipamiento notebook = CrearTipo("Notebook", "Notebook para uso en clase", Categoria.Computacion,
            "NTB-0001", "NTB-0002", "NTB-0003");
        TipoEquipamiento proyector = CrearTipo("Proyector", "Proyector portatil", Categoria.Audiovisual,
            "PRY-0001");
        TipoEquipamiento kitArduino = CrearTipo("Kit Arduino", "Kit de robotica para laboratorio",
            Categoria.Laboratorio, "KIT-0001", "KIT-0002");
        TipoEquipamiento cableHdmi = CrearTipo("Cable HDMI", "Cable HDMI de 3 metros", Categoria.Accesorios,
            "HDM-0001", "HDM-0002", "HDM-0003");
        TipoEquipamiento osciloscopio = CrearTipo("Osciloscopio", "Osciloscopio digital de dos canales",
            Categoria.Electronica, "OSC-0001");
        TipoEquipamiento camara = CrearTipo(TipoConAprobacion, "Camara reflex con lente 18-55 mm",
            Categoria.Audiovisual, requiereAprobacion: true, "CAM-0001", "CAM-0002");

        EnviarAMantenimiento(kitArduino.Unidades.Last());
        osciloscopio.Discontinuar();

        catalogo.AgregarTipo(notebook);
        catalogo.AgregarTipo(proyector);
        catalogo.AgregarTipo(kitArduino);
        catalogo.AgregarTipo(cableHdmi);
        catalogo.AgregarTipo(osciloscopio);
        catalogo.AgregarTipo(camara);
    }

    private void CargarPrestamos(Catalogo catalogo)
    {
        Usuario docente = _usuarios.ObtenerUsuarioPorCorreo(CorreoDocente)!;
        CrearPrestamoActivo(docente, catalogo.BuscarTipoPorNombre("Notebook")!, cantidad: 2, haceDias: 2);
        CrearPrestamoActivo(_usuarios.ObtenerUsuarioPorCorreo(CorreoEstudiante)!,
            catalogo.BuscarTipoPorNombre("Proyector")!, cantidad: 1, haceDias: 1);
        CrearSolicitudPendiente(docente, catalogo.BuscarTipoPorNombre(TipoConAprobacion)!, cantidad: 1,
            haceDias: 0);
    }

    private Catalogo ObtenerCatalogo()
    {
        Catalogo? catalogo = _repositorioCatalogo.Obtener();
        if (catalogo is null)
        {
            throw new InvalidOperationException("La carga inicial debe crear el catalogo antes de cargar los datos de ejemplo");
        }

        return catalogo;
    }

    private void CrearUsuario(string dni, string nombre, string apellido, string correo, TipoUsuario tipo)
    {
        Usuario usuario = new Usuario(dni: dni, nombre: nombre, apellido: apellido, correo: correo,
            contrasena: ContrasenaEjemplo);
        usuario.AsignarTipo(tipo);
        _usuarios.AgregarUsuario(usuario);
    }

    private static TipoEquipamiento CrearTipo(string nombre, string descripcion, Categoria categoria,
        params string[] idsUnidades)
    {
        return CrearTipo(nombre, descripcion, categoria, requiereAprobacion: false, idsUnidades);
    }

    private static TipoEquipamiento CrearTipo(string nombre, string descripcion, Categoria categoria,
        bool requiereAprobacion, params string[] idsUnidades)
    {
        TipoEquipamiento tipo = new TipoEquipamiento(nombre: nombre, descripcion: descripcion,
            duracionMaxPrestamo: DiasMaximosDePrestamo, requiereAprobacion: requiereAprobacion, categoria: categoria);
        foreach (string id in idsUnidades)
        {
            tipo.AgregarUnidad(new UnidadFisica(idInstitucional: id, ubiFisica: Deposito,
                numSerie: NumeroDeSerieDeEjemplo(id, categoria)));
        }

        return tipo;
    }

    private static string? NumeroDeSerieDeEjemplo(string idInstitucional, Categoria categoria)
    {
        if (categoria == Categoria.Accesorios)
        {
            return null;
        }

        return "SN" + idInstitucional.Replace("-", "");
    }

    // El dominio solo deja pasar a mantenimiento una unidad prestada (al devolverla con fallas).
    private static void EnviarAMantenimiento(UnidadFisica unidad)
    {
        unidad.AMantenimiento();
    }

    private void CrearPrestamoActivo(Usuario solicitante, TipoEquipamiento tipo, int cantidad, int haceDias)
    {
        Prestamo prestamo = CrearPrestamo(solicitante, tipo, cantidad, haceDias);
        prestamo.Activar(tipo.UnidadesDisponibles().Take(cantidad).ToList());
        GuardarConSusMovimientos(prestamo);
    }

    // De un tipo con aprobacion: queda Solicitado, sin unidades, hasta que lo evalue un supervisor.
    private void CrearSolicitudPendiente(Usuario solicitante, TipoEquipamiento tipo, int cantidad, int haceDias)
    {
        GuardarConSusMovimientos(CrearPrestamo(solicitante, tipo, cantidad, haceDias));
    }

    private Prestamo CrearPrestamo(Usuario solicitante, TipoEquipamiento tipo, int cantidad, int haceDias)
    {
        return new Prestamo(solicitante: solicitante, tipoEquipamiento: tipo, cantidadSolicitada: cantidad,
            fechaHoraPrestamo: _reloj.GetLocalNow().DateTime.AddDays(-haceDias));
    }

    // Los mismos movimientos que deja ServicioPrestamo.Solicitar: la solicitud y, si quedo activo, un prestamo
    // por unidad asignada.
    private void GuardarConSusMovimientos(Prestamo prestamo)
    {
        _prestamos.AgregarPrestamo(prestamo);
        _movimientos.AgregarMovimiento(Movimiento.DeSolicitud(prestamo,
            RegistroDeAccion.DeUsuario(prestamo.Solicitante, prestamo.FechaHoraPrestamo)));

        RegistroDeAccion delSistema = RegistroDeAccion.DelSistema(prestamo.FechaHoraPrestamo);
        foreach (UnidadFisica unidad in prestamo.UnidadesAsignadas)
        {
            _movimientos.AgregarMovimiento(Movimiento.DePrestamo(prestamo, delSistema, unidad));
        }
    }
}
