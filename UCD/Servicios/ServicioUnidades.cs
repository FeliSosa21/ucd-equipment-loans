using System.Runtime.CompilerServices;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Gestion de unidades fisicas (req. 4): registrar, modificar, cambiar de estado y dar de baja.
// Las operaciones que modifican requieren el permiso GestionarUnidades (Funcionario, Supervisor y Administrador).
public class ServicioUnidades
{
    private readonly RepositorioCatalogo _repositorioCatalogo;
    private readonly RepositorioUsuario _repositorioUsuario;

    public ServicioUnidades(RepositorioCatalogo repositorioCatalogo, RepositorioUsuario repositorioUsuario)
    {
        _repositorioCatalogo = repositorioCatalogo;
        _repositorioUsuario = repositorioUsuario;
    }

    public List<UnidadDetalleDto> Listar(string? nombreTipo, string? estado)
    {
        List<UnidadDetalleDto> resultado = new List<UnidadDetalleDto>();

        foreach (TipoEquipamiento tipo in ObtenerCatalogo().Tipos)
        {
            if (CoincideTipo(tipo, nombreTipo))
            {
                foreach (UnidadFisica unidad in tipo.Unidades)
                {
                    if (CoincideEstado(unidad, estado))
                    {
                        resultado.Add(CrearDetalle(tipo, unidad));
                    }
                }
            }
        }

        return resultado;
    }

    public void Registrar(DatosUnidadDto datos, string dniResponsable)
    {
        TipoEquipamiento tipo = ObtenerTipo(datos.NombreTipo);
        UnidadFisica unidad = new UnidadFisica(datos.IdInstitucional, datos.UbicacionFisica, datos.NumeroSerie);
        tipo.AgregarUnidad(unidad);
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

    private TipoEquipamiento ObtenerTipo(string nombreTipo)
    {
        TipoEquipamiento? tipo = ObtenerCatalogo().BuscarTipoPorNombre(nombreTipo);
        if (tipo is null)
        {
            throw new ServicioException("El tipo de equipamiento no existe");
        }

        return tipo;
    }

    private static bool CoincideTipo(TipoEquipamiento tipo, string? nombreTipo)
    {
        if (string.IsNullOrEmpty(nombreTipo))
        {
            return true;
        }

        return tipo.Nombre == nombreTipo;
    }

    private static bool CoincideEstado(UnidadFisica unidad, string? estado)
    {
        if (string.IsNullOrEmpty(estado))
        {
            return true;
        }

        return unidad.Estado.ToString() == estado;
    }

    private static UnidadDetalleDto CrearDetalle(TipoEquipamiento tipo, UnidadFisica unidad)
    {
        return new UnidadDetalleDto
        {
            IdInstitucional = unidad.IdInstitucional,
            Tipo = tipo.Nombre,
            UbicacionFisica = unidad.UbiFisica,
            NumeroSerie = unidad.NumSerie,
            Observaciones = unidad.Observaciones,
            Estado = unidad.Estado.ToString()
        };
    }
}