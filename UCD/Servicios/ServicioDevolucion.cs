using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Registro de devoluciones: lo hace un funcionario o supervisor sobre un prestamo activo.
// La fecha y hora de la devolucion es la del momento en que se registra (la da el servidor, no la pantalla).
public class ServicioDevolucion
{
    public const int LargoMaximoObservaciones = UnidadDevuelta.LargoMaximoObservaciones;

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
    private readonly RepositorioMovimiento _movimientos;
    private readonly TimeProvider _reloj;

    public ServicioDevolucion(RepositorioPrestamo prestamos, RepositorioUsuario usuarios,
        RepositorioDevolucion devoluciones, RepositorioMovimiento movimientos, TimeProvider reloj)
    {
        _prestamos = prestamos;
        _usuarios = usuarios;
        _devoluciones = devoluciones;
        _movimientos = movimientos;
        _reloj = reloj;
    }

    public List<string> CondicionesPosibles()
    {
        return Condiciones.Keys.ToList();
    }

    public List<PrestamoActivoDto> ListarPrestamosActivos(string dniResponsable)
    {
        ObtenerResponsable(dniResponsable);
        return _prestamos.ObtenerPrestamosActivos().Select(CrearPrestamoActivoDto).ToList();
    }

    public void RegistrarDevolucion(DevolucionDto datos, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ArgumentNullException.ThrowIfNull(datos.Unidades);
        Usuario responsable = ObtenerResponsable(dniResponsable);
        Prestamo prestamo = ObtenerPrestamo(datos.IdPrestamo);

        TraduccionDeErrores.Ejecutar(() =>
        {
            List<UnidadDevuelta> unidades = datos.Unidades
                .Select(unidad => CrearUnidadDevuelta(unidad, prestamo))
                .ToList();
            DateTime ahora = _reloj.GetLocalNow().DateTime;
            Devolucion devolucion = new Devolucion(prestamo, new RegistroDeAccion(responsable, ahora), unidades);
            devolucion.Aplicar();
            _devoluciones.AgregarDevolucion(devolucion);
            RegistrarMovimientos(devolucion);
        });
    }

    // Registro historico (req. 10): un movimiento por unidad, con la fecha efectiva de devolucion.
    private void RegistrarMovimientos(Devolucion devolucion)
    {
        foreach (UnidadDevuelta unidadDevuelta in devolucion.Unidades)
        {
            _movimientos.AgregarMovimiento(new Movimiento(devolucion.FechaHoraDevolucion,
                TipoMovimiento.Devolucion, devolucion.Prestamo.Solicitante, devolucion.RegistradaPor,
                unidadDevuelta.Unidad));
        }
    }

    private Usuario ObtenerResponsable(string dniResponsable)
    {
        Usuario? responsable = _usuarios.ObtenerUsuarioPorDni(dniResponsable);

        if (responsable is null || responsable.Estado == EstadoUsuario.Desactivado ||
            !responsable.TienePermiso(Permiso.RegistrarDevolucion))
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

        if (datos.Condicion is null || !Condiciones.TryGetValue(datos.Condicion, out CondicionDevolucion condicion))
        {
            throw new ServicioException("La condicion de devolucion no existe");
        }

        return new UnidadDevuelta(unidad, condicion, datos.Observaciones);
    }

    private static PrestamoActivoDto CrearPrestamoActivoDto(Prestamo prestamo)
    {
        return new PrestamoActivoDto
        {
            Id = prestamo.Id,
            Solicitante = prestamo.Solicitante.NombreCompleto,
            TipoEquipamiento = prestamo.TipoEquipamiento.Nombre,
            FechaHoraPrestamo = prestamo.FechaHoraPrestamo,
            Unidades = prestamo.UnidadesAsignadas.Select(unidad => unidad.IdInstitucional).ToList()
        };
    }
}
