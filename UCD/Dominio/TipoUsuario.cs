namespace Dominio;

public class TipoUsuario
{
    private readonly PresentacionTipoUsuario presentacion;
    private readonly TopesDePrestamo? topes;
    private readonly HashSet<Permiso> permisos;

    public string Nombre => presentacion.Nombre;
    public string Descripcion => presentacion.Descripcion;
    public string Imagen => presentacion.Imagen;
    public int? MaxUnidadesPorCatalogo => topes?.MaxUnidadesPorCatalogo;
    public int? MaxPrestamosConcurrentes => topes?.MaxPrestamosConcurrentes;

    public static TipoUsuario SinPermisos { get; } = new TipoUsuario(
        "Sin permisos",
        topes: new TopesDePrestamo(maxUnidadesPorCatalogo: 0, maxPrestamosConcurrentes: 0),
        new HashSet<Permiso>());

    public static TipoUsuario Estudiante { get; } = new TipoUsuario(
        "Estudiante",
        topes: new TopesDePrestamo(maxUnidadesPorCatalogo: 2, maxPrestamosConcurrentes: 2),
        new HashSet<Permiso>
        {
            Permiso.ConsultarCatalogo,
            Permiso.ConsultarDisponibilidad,
            Permiso.SolicitarPrestamo,
            Permiso.ConsultarMisSolicitudes
        });

    public static TipoUsuario Docente { get; } = new TipoUsuario(
        "Docente",
        topes: new TopesDePrestamo(maxUnidadesPorCatalogo: 40, maxPrestamosConcurrentes: 3),
        new HashSet<Permiso>
        {
            Permiso.ConsultarCatalogo,
            Permiso.ConsultarDisponibilidad,
            Permiso.SolicitarPrestamo,
            Permiso.ConsultarMisSolicitudes,
            Permiso.ConsultarHistorico
        });

    public static TipoUsuario FuncionarioCE { get; } = new TipoUsuario(
        "Funcionario del centro de equipamiento",
        topes: new TopesDePrestamo(maxUnidadesPorCatalogo: 0, maxPrestamosConcurrentes: 0),
        new HashSet<Permiso>
        {
            Permiso.ConsultarCatalogo,
            Permiso.GestionarUnidades,
            Permiso.RealizarPrestamo,
            Permiso.RegistrarDevolucion
        });

    public static TipoUsuario Supervisor { get; } = new TipoUsuario(
        "Supervisor",
        topes: new TopesDePrestamo(maxUnidadesPorCatalogo: 0, maxPrestamosConcurrentes: 0),
        new HashSet<Permiso>
        {
            Permiso.ConsultarCatalogo,
            Permiso.ConsultarHistorico,
            Permiso.GestionarCatalogo,
            Permiso.GestionarUnidades,
            Permiso.EvaluarSolicitud,
            Permiso.RegistrarDevolucion,
            Permiso.ConsultarPrestamos
        });

    public static TipoUsuario Administrador { get; } = new TipoUsuario(
        "Administrador",
        topes: null,
        new HashSet<Permiso>(Enum.GetValues<Permiso>()));

    public static IReadOnlyList<TipoUsuario> Todos()
    {
        return new[]
        {
            SinPermisos,
            Estudiante,
            Docente,
            FuncionarioCE,
            Supervisor,
            Administrador
        };
    }

    public TipoUsuario(PresentacionTipoUsuario presentacion, TopesDePrestamo? topes, IEnumerable<Permiso> permisos)
    {
        if (presentacion is null)
        {
            throw new DominioException("La presentacion del tipo de usuario no puede ser nula");
        }

        this.presentacion = presentacion;
        this.topes = topes;
        this.permisos = new HashSet<Permiso>(permisos);
    }

    private TipoUsuario(string nombre, TopesDePrestamo? topes, HashSet<Permiso> permisos)
    {
        presentacion = PresentacionTipoUsuario.SoloNombre(nombre);
        this.topes = topes;
        this.permisos = permisos;
    }

    public void ModificarDescripcion(string descripcion)
    {
        presentacion.ModificarDescripcion(descripcion);
    }

    public void ModificarImagen(string imagen)
    {
        presentacion.ModificarImagen(imagen);
    }

    public void ModificarTopes(int maxUnidadesPorCatalogo, int maxPrestamosConcurrentes)
    {
        if (topes is null)
        {
            throw new DominioException("Un tipo de usuario sin limites no admite configurar topes");
        }

        topes.Modificar(maxUnidadesPorCatalogo, maxPrestamosConcurrentes);
    }

    public bool TienePermiso(Permiso permiso)
    {
        return permisos.Contains(permiso);
    }

    public bool NoTienePermisos()
    {
        return permisos.Count == 0;
    }

    public bool PuedeAgregarPrestamo(int prestamosVigentes)
    {
        return topes is null || topes.PuedeAgregarPrestamo(prestamosVigentes);
    }

    public bool PermiteUnidades(int cantidadSolicitada)
    {
        return topes is null || topes.PermiteUnidades(cantidadSolicitada);
    }
}
