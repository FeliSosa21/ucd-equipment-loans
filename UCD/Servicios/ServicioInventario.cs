using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Consulta de inventario (req. 9): cantidad de unidades en cada estado, por tipo y por categoria.
// La disponibilidad sale del estado de las unidades fisicas (letra).
// Quien consulta necesita el permiso GestionarUnidades (Funcionario, Supervisor y Administrador).
public class ServicioInventario
{
    private const string MensajeSinPermiso = "El usuario no tiene permiso para consultar el inventario";

    private readonly RepositorioCatalogoSql _repositorioCatalogo;
    private readonly RepositorioUsuarioSql _repositorioUsuario;

    public ServicioInventario(RepositorioCatalogoSql repositorioCatalogo, RepositorioUsuarioSql repositorioUsuario)
    {
        _repositorioCatalogo = repositorioCatalogo;
        _repositorioUsuario = repositorioUsuario;
    }

    public List<CantidadesPorEstadoDto> ContarPorTipo(int idCatalogo, FiltroInventarioDto filtro, string dniUsuario)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        ValidarResponsable(dniUsuario);
        List<CantidadesPorEstadoDto> filas = new List<CantidadesPorEstadoDto>();
        foreach (TipoEquipamiento tipo in ObtenerTipos(idCatalogo))
        {
            if (CumpleFiltro(tipo, filtro))
            {
                filas.Add(CrearFila(tipo.Nombre, tipo.Categoria, tipo.Unidades));
            }
        }

        return filas;
    }

    public List<CantidadesPorEstadoDto> ContarPorCategoria(int idCatalogo, string dniUsuario)
    {
        IReadOnlyCollection<TipoEquipamiento> tipos = ObtenerTipos(idCatalogo);
        List<CantidadesPorEstadoDto> filas = new List<CantidadesPorEstadoDto>();
        foreach (Categoria categoria in Enum.GetValues<Categoria>())
        {
            filas.Add(CrearFila(categoria.ToString(), categoria, UnidadesDeCategoria(tipos, categoria)));
        }

        return filas;
    }

    private void ValidarResponsable(string dniUsuario)
    {
        OperacionesComunes.ValidarResponsable(_repositorioUsuario, dniUsuario, Permiso.GestionarUnidades,
            MensajeSinPermiso);
    }

    private IReadOnlyCollection<TipoEquipamiento> ObtenerTipos(int idCatalogo)
    {
        return OperacionesComunes.ObtenerCatalogoActivo(_repositorioCatalogo, idCatalogo).Tipos;
    }

    private static bool CumpleFiltro(TipoEquipamiento tipo, FiltroInventarioDto filtro)
    {
        if (filtro.Categoria is not null && tipo.Categoria.ToString() != filtro.Categoria)
        {
            return false;
        }

        if (filtro.Estado is not null && !TieneUnidadesEnEstado(tipo, filtro.Estado))
        {
            return false;
        }

        return true;
    }

    private static bool TieneUnidadesEnEstado(TipoEquipamiento tipo, string estado)
    {
        foreach (UnidadFisica unidad in tipo.Unidades)
        {
            if (unidad.Estado.ToString() == estado)
            {
                return true;
            }
        }

        return false;
    }

    private static List<UnidadFisica> UnidadesDeCategoria(IReadOnlyCollection<TipoEquipamiento> tipos,
        Categoria categoria)
    {
        List<UnidadFisica> unidades = new List<UnidadFisica>();
        foreach (TipoEquipamiento tipo in tipos)
        {
            if (tipo.Categoria == categoria)
            {
                unidades.AddRange(tipo.Unidades);
            }
        }

        return unidades;
    }

    private static CantidadesPorEstadoDto CrearFila(string agrupador, Categoria categoria,
        IReadOnlyCollection<UnidadFisica> unidades)
    {
        return new CantidadesPorEstadoDto
        {
            Agrupador = agrupador,
            Categoria = categoria.ToString(),
            Disponibles = ContarEnEstado(unidades, EstadoUnidadFisica.Disponible),
            Prestadas = ContarEnEstado(unidades, EstadoUnidadFisica.Prestada),
            EnMantenimiento = ContarEnEstado(unidades, EstadoUnidadFisica.EnMantenimiento),
            DadasDeBaja = ContarEnEstado(unidades, EstadoUnidadFisica.DadaDeBaja),
            Total = unidades.Count
        };
    }

    private static int ContarEnEstado(IEnumerable<UnidadFisica> unidades, EstadoUnidadFisica estado)
    {
        int cantidad = 0;
        foreach (UnidadFisica unidad in unidades)
        {
            if (unidad.Estado == estado)
            {
                cantidad++;
            }
        }

        return cantidad;
    }
}