using System.Globalization;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioAuditoria
{
    private const string FormatoIso8601 = "yyyy-MM-ddTHH:mm:ss";
    private const string UsuarioSistema = "Sistema";

    private readonly RepositorioAuditoriaSql _registros;
    private readonly RepositorioUsuarioSql _usuarios;

    public ServicioAuditoria(RepositorioAuditoriaSql registros, RepositorioUsuarioSql usuarios)
    {
        _registros = registros;
        _usuarios = usuarios;
    }

    public List<RegistroAuditoriaDto> ListarRegistros(FiltroAuditoriaDto filtro, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        ValidarQueSeaAdministradorActivo(dniResponsable);
        ValidarRango(filtro);
        return _registros.ObtenerRegistros(CrearFiltro(filtro)).Select(CrearDto).ToList();
    }

    public bool TieneCriterios(FiltroAuditoriaDto filtro)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        return CrearFiltro(filtro).TieneCriterios();
    }

    private void ValidarQueSeaAdministradorActivo(string dniResponsable)
    {
        Usuario? responsable = _usuarios.ObtenerUsuarioPorDni(dniResponsable);

        if (responsable is null || !responsable.PuedeGestionarUsuarios())
        {
            throw new ServicioException("Solo un administrador activo puede consultar el log de auditoria");
        }
    }

    private static void ValidarRango(FiltroAuditoriaDto filtro)
    {
        if (filtro.Desde > filtro.Hasta)
        {
            throw new ServicioException(nameof(FiltroAuditoriaDto.Desde),
                "La fecha desde no puede ser posterior a la fecha hasta");
        }
    }

    private static FiltroAuditoria CrearFiltro(FiltroAuditoriaDto filtro)
    {
        return new FiltroAuditoria
        {
            Desde = filtro.Desde?.ToDateTime(TimeOnly.MinValue),
            Hasta = InicioDelDiaSiguiente(filtro.Hasta),
            DniUsuario = string.IsNullOrWhiteSpace(filtro.DniUsuario) ? null : filtro.DniUsuario.Trim()
        };
    }

    // Las fechas son dias completos: Hasta pasa como el inicio del dia siguiente porque el filtro lo excluye.
    // La ultima fecha posible no tiene dia siguiente: se toma como sin limite.
    private static DateTime? InicioDelDiaSiguiente(DateOnly? hasta)
    {
        if (hasta is null || hasta == DateOnly.MaxValue)
        {
            return null;
        }

        return hasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
    }

    private static RegistroAuditoriaDto CrearDto(RegistroAuditoria registro)
    {
        return new RegistroAuditoriaDto
        {
            FechaHora = registro.FechaHora.ToString(FormatoIso8601, CultureInfo.InvariantCulture),
            Usuario = DescribirResponsable(registro),
            Accion = registro.Accion.ToString(),
            Detalle = registro.Detalle
        };
    }

    private static string DescribirResponsable(RegistroAuditoria registro)
    {
        return registro.EsDelSistema() ? UsuarioSistema : registro.Responsable!.NombreCompletoConDni;
    }
}
