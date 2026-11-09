using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Gestion de unidades fisicas (req. 4): registrar, modificar, cambiar de estado y dar de baja.
// Las operaciones que modifican requieren el permiso GestionarUnidades (Funcionario, Supervisor y Administrador).
public class ServicioUnidades
{
    private const string MensajeSinPermiso = "El usuario no tiene permiso para gestionar unidades";

    private readonly RepositorioCatalogoSql _repositorioCatalogo;
    private readonly RepositorioUsuarioSql _repositorioUsuario;

    public ServicioUnidades(RepositorioCatalogoSql repositorioCatalogo, RepositorioUsuarioSql repositorioUsuario)
    {
        _repositorioCatalogo = repositorioCatalogo;
        _repositorioUsuario = repositorioUsuario;
    }

    public List<UnidadDetalleDto> Listar(int idCatalogo, FiltroUnidadesDto filtro, string dniUsuario)
    {
        ValidarResponsable(dniUsuario);
        List<UnidadDetalleDto> resultado = new List<UnidadDetalleDto>();
        Catalogo catalogo = OperacionesComunes.ObtenerCatalogoActivo(_repositorioCatalogo, idCatalogo);

        foreach ((TipoEquipamiento tipo, UnidadFisica unidad) in UnidadesConSuTipo(catalogo))
        {
            if (CoincideTipo(tipo, filtro.NombreTipo) && CoincideEstado(unidad, filtro.Estado))
            {
                resultado.Add(CrearDetalle(tipo, unidad));
            }
        }

        return resultado;
    }

    public void Registrar(int idCatalogo, DatosUnidadDto datos, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ValidarResponsable(dniResponsable);
        Catalogo catalogo = OperacionesComunes.ObtenerCatalogoActivo(_repositorioCatalogo, idCatalogo);
        TipoEquipamiento tipo = OperacionesComunes.ObtenerTipo(_repositorioCatalogo, idCatalogo, datos.NombreTipo);
        string idInstitucional = OperacionesComunes.Normalizar(datos.IdInstitucional);
        string? numeroSerie = NormalizarOpcional(datos.NumeroSerie);
        ValidarIdDisponible(idInstitucional);
        ValidarSerieDisponible(catalogo, numeroSerie, null);
        string ubicacionFisica = OperacionesComunes.Normalizar(datos.UbicacionFisica);

        TraduccionDeErrores.Ejecutar(() =>
            tipo.AgregarUnidad(new UnidadFisica(idInstitucional, ubicacionFisica, numeroSerie)));
        _repositorioCatalogo.ActualizarCatalogo(catalogo);
    }

    public void Modificar(int idCatalogo, DatosUnidadDto datos, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ValidarResponsable(dniResponsable);
        Catalogo catalogo = OperacionesComunes.ObtenerCatalogoActivo(_repositorioCatalogo, idCatalogo);
        UnidadFisica unidad = ObtenerUnidad(catalogo, datos.IdInstitucional);
        string? numeroSerie = NormalizarOpcional(datos.NumeroSerie);
        ValidarSerieDisponible(catalogo, numeroSerie, unidad);

        TraduccionDeErrores.Ejecutar(() =>
            unidad.ModificarInfo(OperacionesComunes.Normalizar(datos.UbicacionFisica), numeroSerie,
                NormalizarOpcional(datos.Observaciones)));
    }

    public void EnviarAMantenimiento(int idCatalogo, string idInstitucional, string dniResponsable)
    {
        ValidarResponsable(dniResponsable);
        UnidadFisica unidad = ObtenerUnidad(ObtenerCatalogoActivo(idCatalogo), idInstitucional);
        // El dominio permite pasar de prestada a mantenimiento, pero eso solo ocurre al registrar la devolucion.
        if (!unidad.EstaDisponible())
        {
            throw new ServicioException(
                "Solo una unidad disponible puede enviarse a mantenimiento; las prestadas cambian con la devolucion");
        }

        TraduccionDeErrores.Ejecutar(unidad.AMantenimiento);
    }

    public void VolverADisponible(int idCatalogo, string idInstitucional, string dniResponsable)
    {
        ValidarResponsable(dniResponsable);
        UnidadFisica unidad = ObtenerUnidad(ObtenerCatalogoActivo(idCatalogo), idInstitucional);
        if (unidad.Estado != EstadoUnidadFisica.EnMantenimiento)
        {
            throw new ServicioException(
                "Solo una unidad en mantenimiento puede volver a disponible; las prestadas vuelven con la devolucion");
        }

        TraduccionDeErrores.Ejecutar(unidad.Disponible);
    }

    public void DarDeBaja(int idCatalogo, string idInstitucional, string dniResponsable)
    {
        ValidarResponsable(dniResponsable);
        UnidadFisica unidad = ObtenerUnidad(ObtenerCatalogoActivo(idCatalogo), idInstitucional);

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

    private Catalogo ObtenerCatalogoActivo(int idCatalogo)
    {
        return OperacionesComunes.ObtenerCatalogoActivo(_repositorioCatalogo, idCatalogo);
    }

    // Cada unidad junto con el tipo al que pertenece, recorriendo todo el catalogo.
    private static List<(TipoEquipamiento Tipo, UnidadFisica Unidad)> UnidadesConSuTipo(Catalogo catalogo)
    {
        List<(TipoEquipamiento Tipo, UnidadFisica Unidad)> resultado = new List<(TipoEquipamiento, UnidadFisica)>();
        foreach (TipoEquipamiento tipo in catalogo.Tipos)
        {
            foreach (UnidadFisica unidad in tipo.Unidades)
            {
                resultado.Add((tipo, unidad));
            }
        }

        return resultado;
    }

    private static UnidadFisica? BuscarUnidad(Catalogo catalogo, string idInstitucional)
    {
        foreach ((TipoEquipamiento _, UnidadFisica unidad) in UnidadesConSuTipo(catalogo))
        {
            if (unidad.IdInstitucional == idInstitucional)
            {
                return unidad;
            }
        }

        return null;
    }

    private static UnidadFisica ObtenerUnidad(Catalogo catalogo, string idInstitucional)
    {
        UnidadFisica? unidad = BuscarUnidad(catalogo, OperacionesComunes.Normalizar(idInstitucional));
        if (unidad is null)
        {
            throw new ServicioException("La unidad no existe");
        }

        return unidad;
    }

    // El ID institucional es unico en todo el sistema, no solo dentro del catalogo.
    private void ValidarIdDisponible(string idInstitucional)
    {
        if (_repositorioCatalogo.ExisteUnidadConIdInstitucional(idInstitucional))
        {
            throw new ServicioException("Ya existe una unidad con ese id institucional");
        }
    }

    private static void ValidarSerieDisponible(Catalogo catalogo, string? numeroSerie, UnidadFisica? unidadEditada)
    {
        if (string.IsNullOrWhiteSpace(numeroSerie))
        {
            return;
        }

        foreach ((TipoEquipamiento _, UnidadFisica unidad) in UnidadesConSuTipo(catalogo))
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
