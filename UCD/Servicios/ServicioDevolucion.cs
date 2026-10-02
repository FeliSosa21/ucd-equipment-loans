using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Registro de devoluciones: lo hace un funcionario o supervisor sobre un prestamo activo.
public class ServicioDevolucion
{
    private static readonly Dictionary<string, CondicionDevolucion> Condiciones =
        new Dictionary<string, CondicionDevolucion>
        {
            { "Sin observaciones", CondicionDevolucion.SinObservaciones },
            { "Con fallas", CondicionDevolucion.ConFallas },
            { "Dañada", CondicionDevolucion.Daniada }
        };

    private readonly RepositorioPrestamo _prestamos;
    private readonly RepositorioUsuario _usuarios;
    private readonly RepositorioDevolucion _devoluciones;

    public ServicioDevolucion(RepositorioPrestamo prestamos, RepositorioUsuario usuarios,
        RepositorioDevolucion devoluciones)
    {
        _prestamos = prestamos;
        _usuarios = usuarios;
        _devoluciones = devoluciones;
    }

    public List<string> CondicionesPosibles()
    {
        return Condiciones.Keys.ToList();
    }

    public List<PrestamoActivoDto> ListarPrestamosActivos()
    {
        return _prestamos.ObtenerPrestamosActivos().Select(CrearPrestamoActivoDto).ToList();
    }

    public void RegistrarDevolucion(DevolucionDto datos, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(datos);
        Usuario responsable = ObtenerResponsable(dniResponsable);
        Prestamo prestamo = ObtenerPrestamo(datos.IdPrestamo);
        List<UnidadDevuelta> unidades = datos.Unidades.Select(unidad => CrearUnidadDevuelta(unidad, prestamo)).ToList();

        try
        {
            Devolucion devolucion = new Devolucion(prestamo, responsable, datos.FechaHoraDevolucion, unidades);
            devolucion.Aplicar();
            _devoluciones.AgregarDevolucion(devolucion);
        }
        catch (DominioException ex)
        {
            throw new ServicioException(ex.Message, ex);
        }
    }

    private Usuario ObtenerResponsable(string dniResponsable)
    {
        Usuario? responsable = _usuarios.ObtenerUsuarioPorDni(dniResponsable);

        if (responsable is null || responsable.Estado == EstadoUsuario.Desactivado ||
            !responsable.Tipo.TienePermiso(Permiso.RegistrarDevolucion))
        {
            throw new ServicioException("El usuario no tiene permiso para registrar devoluciones");
        }

        return responsable;
    }

    private Prestamo ObtenerPrestamo(Guid idPrestamo)
    {
        Prestamo? prestamo = _prestamos.ObtenerPrestamoPorId(idPrestamo);

        if (prestamo is null)
        {
            throw new ServicioException("El prestamo no existe");
        }

        return prestamo;
    }

    private static UnidadDevuelta CrearUnidadDevuelta(UnidadDevueltaDto datos, Prestamo prestamo)
    {
        UnidadFisica? unidad = prestamo.UnidadesAsignadas
            .FirstOrDefault(asignada => asignada.IdInstitucional == datos.IdInstitucional);

        if (unidad is null)
        {
            throw new ServicioException($"La unidad {datos.IdInstitucional} no pertenece al prestamo");
        }

        if (!Condiciones.TryGetValue(datos.Condicion, out CondicionDevolucion condicion))
        {
            throw new ServicioException("La condicion de devolucion no existe");
        }

        try
        {
            return new UnidadDevuelta(unidad, condicion, datos.Observaciones);
        }
        catch (DominioException ex)
        {
            throw new ServicioException(ex.Message, ex);
        }
    }

    private static PrestamoActivoDto CrearPrestamoActivoDto(Prestamo prestamo)
    {
        return new PrestamoActivoDto
        {
            Id = prestamo.Id,
            Solicitante = $"{prestamo.Solicitante.Nombre} {prestamo.Solicitante.Apellido}",
            TipoEquipamiento = prestamo.TipoEquipamiento.Nombre,
            FechaHoraPrestamo = prestamo.FechaHoraPrestamo,
            Unidades = prestamo.UnidadesAsignadas.Select(unidad => unidad.IdInstitucional).ToList()
        };
    }
}
