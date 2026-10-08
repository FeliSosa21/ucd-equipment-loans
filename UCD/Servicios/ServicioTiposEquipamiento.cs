using Data.Repositorios;
using DTOs;
using Dominio;
namespace Servicios;

// Gestion de tipos de equipamiento y del nombre del catalogo (req. 4).
// Registrar, modificar y discontinuar requieren el permiso GestionarCatalogo (Supervisor y Administrador).
public class ServicioTiposEquipamiento
{
    private readonly RepositorioCatalogo _repositorioCatalogo;
    private readonly RepositorioUsuario _repositorioUsuario;

    public ServicioTiposEquipamiento(RepositorioCatalogo repositorioCatalogo, RepositorioUsuario repositorioUsuario)
    {
        _repositorioCatalogo = repositorioCatalogo;
        _repositorioUsuario = repositorioUsuario;
    }

    public List<TipoEquipamientoDetalleDto> Listar(string? nombre, string? categoria)
    {
        List<TipoEquipamientoDetalleDto> resultado = new List<TipoEquipamientoDetalleDto>();

        foreach (TipoEquipamiento tipo in ObtenerCatalogo().Tipos)
        {
            if (CumpleFiltro(tipo, nombre, categoria))
            {
                resultado.Add(CrearDetalle(tipo));
            }
        }

        return resultado;
    }

    public void Registrar(DatosTipoEquipamientoDto datos, string dniResponsable)
    {
        ValidarResponsable(dniResponsable);
        Categoria categoria = Enum.Parse<Categoria>(datos.Categoria);

        try
        {
            TipoEquipamiento tipo = new TipoEquipamiento(datos.Nombre, datos.Descripcion, datos.DuracionMaxPrestamo,
                datos.RequiereAprobacion, categoria);
            ObtenerCatalogo().AgregarTipo(tipo);
        }
        catch (DominioException ex)
        {
            throw new ServicioException(ex.Message, ex);
        }
    }

    private Catalogo ObtenerCatalogo()
    {
        Catalogo? catalogo = _repositorioCatalogo.Obtener();
        if (catalogo is null)
        {
            throw new ServicioException("El catalogo no esta cargado");
        }

        return catalogo;
    }

    private void ValidarResponsable(string dniResponsable)
    {
        Usuario? responsable = _repositorioUsuario.ObtenerUsuarioPorDni(dniResponsable);

        if (responsable is null || responsable.Estado == EstadoUsuario.Desactivado ||
            !responsable.Tipo.TienePermiso(Permiso.GestionarCatalogo))
        {
            throw new ServicioException("El usuario no tiene permiso para gestionar el catalogo");
        }
    }

    private static bool CumpleFiltro(TipoEquipamiento tipo, string? nombre, string? categoria)
    {
        return CoincideNombre(tipo, nombre) && CoincideCategoria(tipo, categoria);
    }

    private static bool CoincideNombre(TipoEquipamiento tipo, string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return true;
        }

        return tipo.Nombre.Contains(nombre.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool CoincideCategoria(TipoEquipamiento tipo, string? categoria)
    {
        if (string.IsNullOrEmpty(categoria))
        {
            return true;
        }

        return tipo.Categoria.ToString() == categoria;
    }

    private static TipoEquipamientoDetalleDto CrearDetalle(TipoEquipamiento tipo)
    {
        return new TipoEquipamientoDetalleDto
        {
            Nombre = tipo.Nombre,
            Descripcion = tipo.Descripcion,
            DuracionMaxPrestamo = tipo.DuracionMaxPrestamo,
            RequiereAprobacion = tipo.RequiereAprobacion,
            Categoria = tipo.Categoria.ToString(),
            Estado = tipo.Estado.ToString()
        };
    }
}