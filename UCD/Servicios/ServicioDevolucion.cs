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

    private readonly RepositorioPrestamoSql _prestamos;
    private readonly RepositorioUsuarioSql _usuarios;
    private readonly RepositorioDevolucionSql _devoluciones;
    private readonly RepositorioMovimientoSql _movimientos;
    private readonly TimeProvider _reloj;
    private readonly RegistradorDeAuditoria _auditoria;

    public ServicioDevolucion(RepositorioPrestamoSql prestamos, RepositorioUsuarioSql usuarios,
        RepositorioDevolucionSql devoluciones, RepositorioMovimientoSql movimientos, TimeProvider reloj,
        RegistradorDeAuditoria auditoria)
    {
        _prestamos = prestamos;
        _usuarios = usuarios;
        _devoluciones = devoluciones;
        _movimientos = movimientos;
        _reloj = reloj;
        _auditoria = auditoria;
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

    public DevolucionRegistradaDto RegistrarDevolucion(DevolucionDto datos, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ArgumentNullException.ThrowIfNull(datos.Unidades);
        Usuario responsable = ObtenerResponsable(dniResponsable);
        Prestamo prestamo = ObtenerPrestamo(datos.IdPrestamo);

        Devolucion devolucion = TraduccionDeErrores.Ejecutar(() =>
        {
            List<UnidadDevuelta> unidades = datos.Unidades
                .Select(unidad => CrearUnidadDevuelta(unidad, prestamo))
                .ToList();
            DateTime ahora = _reloj.GetLocalNow().DateTime;
            Func<RegistroDeAccion> crearRegistro = () => RegistroDeAccion.DeUsuario(responsable, ahora);
            Devolucion nueva = new Devolucion(prestamo, crearRegistro(), unidades);
            nueva.Aplicar();
            _devoluciones.AgregarDevolucion(nueva);
            RegistrarMovimientos(nueva, crearRegistro);
            RegistrarEnAuditoria(nueva, crearRegistro());
            return nueva;
        });

        return CrearDevolucionRegistradaDto(devolucion);
    }

    private void RegistrarEnAuditoria(Devolucion devolucion, RegistroDeAccion registro)
    {
        _auditoria.Registrar(registro, AccionAuditada.DevolucionDePrestamo,
            $"Devolución del préstamo de {devolucion.Prestamo.Solicitante.NombreCompletoConDni}: " +
            DescribirCantidad(devolucion.Unidades.Count));
    }

    private static string DescribirCantidad(int cantidadUnidades)
    {
        return cantidadUnidades == 1 ? "1 unidad" : $"{cantidadUnidades} unidades";
    }

    // Registro historico (req. 10): un movimiento por unidad, con la fecha efectiva de devolucion.
    // Un registro nuevo por movimiento: con EF, RegistroDeAccion es owned y una misma instancia
    // no puede pertenecer a dos entidades (D18, regla 4).
    private void RegistrarMovimientos(Devolucion devolucion, Func<RegistroDeAccion> crearRegistro)
    {
        foreach (UnidadDevuelta unidadDevuelta in devolucion.Unidades)
        {
            _movimientos.AgregarMovimiento(Movimiento.DeDevolucion(devolucion.Prestamo, crearRegistro(),
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

    private static DevolucionRegistradaDto CrearDevolucionRegistradaDto(Devolucion devolucion)
    {
        return new DevolucionRegistradaDto
        {
            RegistradaPor = devolucion.RegistradaPor.NombreCompleto,
            Unidades = devolucion.Unidades.Select(CrearUnidadDevueltaDto).ToList()
        };
    }

    private static UnidadDevueltaDto CrearUnidadDevueltaDto(UnidadDevuelta unidadDevuelta)
    {
        return new UnidadDevueltaDto
        {
            IdInstitucional = unidadDevuelta.Unidad.IdInstitucional,
            Condicion = Condiciones.First(par => par.Value == unidadDevuelta.Condicion).Key,
            Observaciones = unidadDevuelta.Observaciones
        };
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
