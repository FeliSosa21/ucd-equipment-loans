namespace Dominio;

public class TipoUsuario
{
    private PresentacionTipoUsuario presentacion;
    private TopesDePrestamo? topes;
    private HashSet<Permiso> permisos;

    public int Id { get; private set; }

    public string Nombre => presentacion.Nombre;
    public string Descripcion => presentacion.Descripcion;
    public string Imagen => presentacion.Imagen;
    public int? MaxUnidadesPorCatalogo => topes?.MaxUnidadesPorCatalogo;
    public int? MaxPrestamosConcurrentes => topes?.MaxPrestamosConcurrentes;

    public static TipoUsuario SinPermisos() => new TipoUsuario(
        "Sin permisos",
        topes: new TopesDePrestamo(maxUnidadesPorCatalogo: 0, maxPrestamosConcurrentes: 0),
        new HashSet<Permiso>());

    public static TipoUsuario Estudiante() => new TipoUsuario(
        "Estudiante",
        topes: new TopesDePrestamo(maxUnidadesPorCatalogo: 2, maxPrestamosConcurrentes: 2),
        new HashSet<Permiso>
        {
            Permiso.ConsultarCatalogo,
            Permiso.ConsultarDisponibilidad,
            Permiso.SolicitarPrestamo,
            Permiso.ConsultarMisSolicitudes
        });

    public static TipoUsuario Docente() => new TipoUsuario(
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

    public static TipoUsuario FuncionarioCE() => new TipoUsuario(
        "Funcionario del centro de equipamiento",
        topes: new TopesDePrestamo(maxUnidadesPorCatalogo: 0, maxPrestamosConcurrentes: 0),
        new HashSet<Permiso>
        {
            Permiso.ConsultarCatalogo,
            Permiso.GestionarUnidades,
            Permiso.RealizarPrestamo,
            Permiso.RegistrarDevolucion
        });

    public static TipoUsuario Supervisor() => new TipoUsuario(
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

    public static TipoUsuario Administrador() => new TipoUsuario(
        "Administrador",
        topes: null,
        new HashSet<Permiso>(Enum.GetValues<Permiso>()));

    public static IReadOnlyList<TipoUsuario> Todos()
    {
        return new[]
        {
            SinPermisos(),
            Estudiante(),
            Docente(),
            FuncionarioCE(),
            Supervisor(),
            Administrador()
        };
    }

    private TipoUsuario()
    {
        presentacion = null!;
        permisos = new HashSet<Permiso>();
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

    public void Modificar(string descripcion, string imagen, TopesDePrestamo? topesNuevos)
    {
        PresentacionTipoUsuario presentacionNueva = new PresentacionTipoUsuario(Nombre, descripcion, imagen);

        if (topesNuevos is not null)
        {
            ModificarTopes(topesNuevos.MaxUnidadesPorCatalogo, topesNuevos.MaxPrestamosConcurrentes);
        }

        presentacion = presentacionNueva;
    }

    public void ModificarPresentacion(string descripcion, string imagen)
    {
        presentacion = new PresentacionTipoUsuario(Nombre, descripcion, imagen);
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
