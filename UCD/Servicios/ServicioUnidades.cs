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
        ArgumentNullException.ThrowIfNull(datos);
        ValidarResponsable(dniResponsable);
        TipoEquipamiento tipo = ObtenerTipo(datos.NombreTipo);
        string idInstitucional = Normalizar(datos.IdInstitucional);
        string? numeroSerie = NormalizarOpcional(datos.NumeroSerie);
        ValidarIdDisponible(datos.IdInstitucional);
        ValidarSerieDisponible(datos.NumeroSerie, null);
        try
        {
            UnidadFisica unidad = new UnidadFisica(idInstitucional, Normalizar(datos.UbicacionFisica), numeroSerie);
            tipo.AgregarUnidad(unidad);
        }
        catch (DominioException ex)
        {
            throw new ServicioException(ex.Message, ex);
        }
    }

    public void Modificar(DatosUnidadDto datos, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ValidarResponsable(dniResponsable);
        UnidadFisica unidad = ObtenerUnidad(datos.IdInstitucional);
        string? numeroSerie = NormalizarOpcional(datos.NumeroSerie);
        ValidarSerieDisponible(numeroSerie, unidad);
        try
        {
            unidad.ModificarInfo(Normalizar(datos.UbicacionFisica), numeroSerie,
                NormalizarOpcional(datos.Observaciones));
        }
        catch (DominioException ex)
        {
            throw new ServicioException(ex.Message, ex);
        }
    }

    public void EnviarAMantenimiento(string idInstitucional, string dniResponsable)
    {
        ValidarResponsable(dniResponsable);
        UnidadFisica unidad = ObtenerUnidad(idInstitucional);

        try
        {
            unidad.AMantenimiento();
        }
        catch (DominioException ex)
        {
            throw new ServicioException(ex.Message, ex);
        }
    }

    private static string Normalizar(string? texto)
    {
        if (texto is null)
        {
            return string.Empty;
        }

        return texto.Trim();
    }

    // Los campos opcionales en blanco se guardan como null: "sin numero de serie" no es lo mismo que "   ".
    private static string? NormalizarOpcional(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return null;
        }

        return texto.Trim();
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

    private UnidadFisica? BuscarUnidad(string idInstitucional)
    {
        foreach (TipoEquipamiento tipo in ObtenerCatalogo().Tipos)
        {
            foreach (UnidadFisica unidad in tipo.Unidades)
            {
                if (unidad.IdInstitucional == idInstitucional)
                {
                    return unidad;
                }
            }
        }

        return null;
    }

    private UnidadFisica ObtenerUnidad(string idInstitucional)
    {
        UnidadFisica? unidad = BuscarUnidad(Normalizar(idInstitucional));
        if (unidad is null)
        {
            throw new ServicioException("La unidad no existe");
        }

        return unidad;
    }

    private void ValidarIdDisponible(string idInstitucional)
    {
        if (BuscarUnidad(idInstitucional) is not null)
        {
            throw new ServicioException("Ya existe una unidad con ese id institucional");
        }
    }

    private void ValidarSerieDisponible(string? numeroSerie, UnidadFisica? unidadEditada)
    {
        if (string.IsNullOrWhiteSpace(numeroSerie))
        {
            return;
        }

        foreach (TipoEquipamiento tipo in ObtenerCatalogo().Tipos)
        {
            foreach (UnidadFisica unidad in tipo.Unidades)
            {
                if (unidad != unidadEditada && string.Equals(unidad.NumSerie, numeroSerie, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ServicioException("Ya existe una unidad con ese numero de serie");
                }
            }
        }
    }

    private void ValidarResponsable(string dniResponsable)
    {
        Usuario? responsable = _repositorioUsuario.ObtenerUsuarioPorDni(dniResponsable);
        if (responsable is null || responsable.Estado == EstadoUsuario.Desactivado ||
            !responsable.Tipo.TienePermiso(Permiso.GestionarUnidades))
        {
            throw new ServicioException("El usuario no tiene permiso para gestionar unidades");
        }
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