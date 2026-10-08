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
                    resultado.Add(CrearDetalle(tipo, unidad));
                }
            }
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

    private static bool CoincideTipo(TipoEquipamiento tipo, string? nombreTipo)
    {
        if (string.IsNullOrEmpty(nombreTipo))
        {
            return true;
        }

        return tipo.Nombre == nombreTipo;
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