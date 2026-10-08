using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Gestion de unidades fisicas (req. 4): registrar, modificar, cambiar de estado y dar de baja.
// Las operaciones que modifican requieren el permiso GestionarUnidades (Funcionario, Supervisor y Administrador).
public class ServicioUnidades
{
    private const string MensajeSinPermiso = "El usuario no tiene permiso para gestionar unidades";

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

        foreach ((TipoEquipamiento tipo, UnidadFisica unidad) in UnidadesConSuTipo())
        {
            if (CoincideTipo(tipo, nombreTipo) && CoincideEstado(unidad, estado))
            {
                resultado.Add(CrearDetalle(tipo, unidad));
            }
        }

        return resultado;
    }

    public void Registrar(DatosUnidadDto datos, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ValidarResponsable(dniResponsable);
        TipoEquipamiento tipo = ObtenerTipo(datos.NombreTipo);
        string idInstitucional = OperacionesComunes.Normalizar(datos.IdInstitucional);
        string? numeroSerie = NormalizarOpcional(datos.NumeroSerie);
        ValidarIdDisponible(idInstitucional);
        ValidarSerieDisponible(numeroSerie, null);
        string ubicacionFisica = OperacionesComunes.Normalizar(datos.UbicacionFisica);

        TraduccionDeErrores.Ejecutar(() =>
            tipo.AgregarUnidad(new UnidadFisica(idInstitucional, ubicacionFisica, numeroSerie)));
    }

    public void Modificar(DatosUnidadDto datos, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ValidarResponsable(dniResponsable);
        UnidadFisica unidad = ObtenerUnidad(datos.IdInstitucional);
        string? numeroSerie = NormalizarOpcional(datos.NumeroSerie);
        ValidarSerieDisponible(numeroSerie, unidad);

        TraduccionDeErrores.Ejecutar(() =>
            unidad.ModificarInfo(OperacionesComunes.Normalizar(datos.UbicacionFisica), numeroSerie,
                NormalizarOpcional(datos.Observaciones)));
    }

    public void EnviarAMantenimiento(string idInstitucional, string dniResponsable)
    {
        ValidarResponsable(dniResponsable);
        UnidadFisica unidad = ObtenerUnidad(idInstitucional);
        // El dominio permite pasar de prestada a mantenimiento, pero eso solo ocurre al registrar la devolucion.
        if (!unidad.EstaDisponible())
        {
            throw new ServicioException(
                "Solo una unidad disponible puede enviarse a mantenimiento; las prestadas cambian con la devolucion");
        }

        TraduccionDeErrores.Ejecutar(unidad.AMantenimiento);
    }

    public void VolverADisponible(string idInstitucional, string dniResponsable)
    {
        ValidarResponsable(dniResponsable);
        UnidadFisica unidad = ObtenerUnidad(idInstitucional);
        if (unidad.Estado != EstadoUnidadFisica.EnMantenimiento)
        {
            throw new ServicioException(
                "Solo una unidad en mantenimiento puede volver a disponible; las prestadas vuelven con la devolucion");
        }

        TraduccionDeErrores.Ejecutar(unidad.Disponible);
    }

    public void DarDeBaja(string idInstitucional, string dniResponsable)
    {
        ValidarResponsable(dniResponsable);
        UnidadFisica unidad = ObtenerUnidad(idInstitucional);

        TraduccionDeErrores.Ejecutar(unidad.DarDeBaja);
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

    private TipoEquipamiento ObtenerTipo(string nombreTipo)
    {
        TipoEquipamiento? tipo = OperacionesComunes.ObtenerCatalogo(_repositorioCatalogo)
            .BuscarTipoPorNombre(nombreTipo);
        if (tipo is null)
        {
            throw new ServicioException("El tipo de equipamiento no existe");
        }

        return tipo;
    }

    // Cada unidad junto con el tipo al que pertenece, recorriendo todo el catalogo.
    private List<(TipoEquipamiento Tipo, UnidadFisica Unidad)> UnidadesConSuTipo()
    {
        List<(TipoEquipamiento Tipo, UnidadFisica Unidad)> resultado = new List<(TipoEquipamiento, UnidadFisica)>();
        foreach (TipoEquipamiento tipo in OperacionesComunes.ObtenerCatalogo(_repositorioCatalogo).Tipos)
        {
            foreach (UnidadFisica unidad in tipo.Unidades)
            {
                resultado.Add((tipo, unidad));
            }
        }

        return resultado;
    }

    private UnidadFisica? BuscarUnidad(string idInstitucional)
    {
        foreach ((TipoEquipamiento _, UnidadFisica unidad) in UnidadesConSuTipo())
        {
            if (unidad.IdInstitucional == idInstitucional)
            {
                return unidad;
            }
        }

        return null;
    }

    private UnidadFisica ObtenerUnidad(string idInstitucional)
    {
        UnidadFisica? unidad = BuscarUnidad(OperacionesComunes.Normalizar(idInstitucional));
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

        foreach ((TipoEquipamiento _, UnidadFisica unidad) in UnidadesConSuTipo())
        {
            if (unidad != unidadEditada &&
                string.Equals(unidad.NumSerie, numeroSerie, StringComparison.OrdinalIgnoreCase))
            {
                throw new ServicioException("Ya existe una unidad con ese numero de serie");
            }
        }
    }

    private void ValidarResponsable(string dniResponsable)
    {
        OperacionesComunes.ValidarResponsable(_repositorioUsuario, dniResponsable, Permiso.GestionarUnidades,
            MensajeSinPermiso);
    }

    private static bool CoincideTipo(TipoEquipamiento tipo, string? nombreTipo)
    {
        if (string.IsNullOrEmpty(nombreTipo))
        {
            return true;
        }

        return string.Equals(tipo.Nombre, nombreTipo, StringComparison.OrdinalIgnoreCase);
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