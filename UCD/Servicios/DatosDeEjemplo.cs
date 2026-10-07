using Data.Repositorios;
using Dominio;

namespace Servicios;

// Datos de ejemplo para probar la aplicacion y para la demo: usuarios, unidades y prestamos activos.
// Los prestamos se crean ya activos porque todavia no hay pantalla para solicitarlos y aprobarlos.
public class DatosDeEjemplo
{
    public const string ContrasenaEjemplo = ServicioAdministracionUsuarios.ContrasenaPredeterminada;
    public const string CorreoDocente = "ana.perez@ucd.edu.uy";
    public const string CorreoEstudiante = "juan.gomez@ucd.edu.uy";
    public const string CorreoFuncionario = "laura.diaz@ucd.edu.uy";

    private const string Deposito = "Deposito central";
    private const int DiasMaximosDePrestamo = 7;

    private readonly RepositorioUsuario _usuarios;
    private readonly RepositorioPrestamo _prestamos;
    private readonly RepositorioCatalogo _repositorioCatalogo;
    private readonly TimeProvider _reloj;

    public DatosDeEjemplo(RepositorioUsuario usuarios, RepositorioPrestamo prestamos,
        RepositorioCatalogo repositorioCatalogo, TimeProvider reloj)
    {
        _usuarios = usuarios;
        _prestamos = prestamos;
        _repositorioCatalogo = repositorioCatalogo;
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
            tipo: TipoUsuario.Docente);
        CrearUsuario(dni: "23456783", nombre: "Juan", apellido: "Gomez", correo: CorreoEstudiante,
            tipo: TipoUsuario.Estudiante);
        CrearUsuario(dni: "34567894", nombre: "Laura", apellido: "Diaz", correo: CorreoFuncionario,
            tipo: TipoUsuario.FuncionarioCE);
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

        EnviarAMantenimiento(kitArduino.Unidades.Last());
        osciloscopio.Discontinuar();

        catalogo.AgregarTipo(notebook);
        catalogo.AgregarTipo(proyector);
        catalogo.AgregarTipo(kitArduino);
        catalogo.AgregarTipo(cableHdmi);
        catalogo.AgregarTipo(osciloscopio);
    }

    private void CargarPrestamos(Catalogo catalogo)
    {
        CrearPrestamoActivo(_usuarios.ObtenerUsuarioPorCorreo(CorreoDocente)!,
            catalogo.BuscarTipoPorNombre("Notebook")!, cantidad: 2, haceDias: 2);
        CrearPrestamoActivo(_usuarios.ObtenerUsuarioPorCorreo(CorreoEstudiante)!,
            catalogo.BuscarTipoPorNombre("Proyector")!, cantidad: 1, haceDias: 1);
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
        TipoEquipamiento tipo = new TipoEquipamiento(nombre: nombre, descripcion: descripcion,
            duracionMaxPrestamo: DiasMaximosDePrestamo, requiereAprobacion: false, categoria: categoria);
        foreach (string id in idsUnidades)
        {
            tipo.AgregarUnidad(new UnidadFisica(idInstitucional: id, ubiFisica: Deposito, numSerie: NumeroDeSerieDeEjemplo(id, categoria)));
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
        unidad.Prestada();
        unidad.AMantenimiento();
    }

    private void CrearPrestamoActivo(Usuario solicitante, TipoEquipamiento tipo, int cantidad, int haceDias)
    {
        Prestamo prestamo = new Prestamo(solicitante: solicitante, tipoEquipamiento: tipo,
            cantidadSolicitada: cantidad, fechaHoraPrestamo: _reloj.GetLocalNow().DateTime.AddDays(-haceDias));
        prestamo.Activar(tipo.UnidadesDisponibles().Take(cantidad).ToList());
        _prestamos.AgregarPrestamo(prestamo);
    }
}
