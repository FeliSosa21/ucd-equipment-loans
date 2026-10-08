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
            resultado.Add(CrearDetalle(tipo));
        }

        return resultado;
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