using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Gestion de tipos de equipamiento y del nombre del catalogo (req. 4).
// Registrar, modificar, discontinuar y reactivar requieren el permiso GestionarCatalogo (Supervisor y Administrador).
public class ServicioTiposEquipamiento
{
    private const string MensajeSinPermiso = "El usuario no tiene permiso para gestionar el catalogo";

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

        foreach (TipoEquipamiento tipo in OperacionesComunes.ObtenerCatalogo(_repositorioCatalogo).Tipos)
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
        ArgumentNullException.ThrowIfNull(datos);
        ValidarResponsable(dniResponsable);
        Categoria categoria = ObtenerCategoria(datos.Categoria);
        Catalogo catalogo = OperacionesComunes.ObtenerCatalogo(_repositorioCatalogo);
        string nombre = OperacionesComunes.Normalizar(datos.Nombre);
        string descripcion = OperacionesComunes.Normalizar(datos.Descripcion);

        TraduccionDeErrores.Ejecutar(() =>
        {
            TipoEquipamiento tipo = new TipoEquipamiento(nombre, descripcion, datos.DuracionMaxPrestamo,
                datos.RequiereAprobacion, categoria);
            catalogo.AgregarTipo(tipo);
        });
    }

    public void Modificar(string nombreActual, DatosTipoEquipamientoDto datos, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ValidarResponsable(dniResponsable);
        Catalogo catalogo = OperacionesComunes.ObtenerCatalogo(_repositorioCatalogo);
        TipoEquipamiento tipo = ObtenerTipo(catalogo, nombreActual);
        Categoria categoria = ObtenerCategoria(datos.Categoria);
        string nombreNuevo = OperacionesComunes.Normalizar(datos.Nombre);
        ValidarNombreDisponible(catalogo, tipo, nombreNuevo);

        TraduccionDeErrores.Ejecutar(() =>
            tipo.Modificar(nombreNuevo, OperacionesComunes.Normalizar(datos.Descripcion), datos.DuracionMaxPrestamo,
                datos.RequiereAprobacion, categoria));
    }

    public void Discontinuar(string nombre, string dniResponsable)
    {
        ValidarResponsable(dniResponsable);
        TipoEquipamiento tipo = ObtenerTipo(OperacionesComunes.ObtenerCatalogo(_repositorioCatalogo), nombre);

        TraduccionDeErrores.Ejecutar(tipo.Discontinuar);
    }

    // Foro del curso: un tipo discontinuado puede volver a activarse.
    public void Reactivar(string nombre, string dniResponsable)
    {
        ValidarResponsable(dniResponsable);
        TipoEquipamiento tipo = ObtenerTipo(OperacionesComunes.ObtenerCatalogo(_repositorioCatalogo), nombre);

        TraduccionDeErrores.Ejecutar(tipo.Reactivar);
    }

    public string ObtenerNombreCatalogo()
    {
        return OperacionesComunes.ObtenerCatalogo(_repositorioCatalogo).Nombre;
    }

    public void ModificarNombreCatalogo(string nombre, string dniResponsable)
    {
        ValidarResponsable(dniResponsable);
        Catalogo catalogo = OperacionesComunes.ObtenerCatalogo(_repositorioCatalogo);

        TraduccionDeErrores.Ejecutar(() => catalogo.ModificarNombre(OperacionesComunes.Normalizar(nombre)));
    }

    private static TipoEquipamiento ObtenerTipo(Catalogo catalogo, string nombre)
    {
        TipoEquipamiento? tipo = catalogo.BuscarTipoPorNombre(nombre);
        if (tipo is null)
        {
            throw new ServicioException("El tipo de equipamiento no existe");
        }

        return tipo;
    }

    private static void ValidarNombreDisponible(Catalogo catalogo, TipoEquipamiento tipo, string nombreNuevo)
    {
        TipoEquipamiento? existente = catalogo.BuscarTipoPorNombre(nombreNuevo);
        if (existente is not null && existente != tipo)
        {
            throw new ServicioException("Ya existe un tipo de equipamiento con ese nombre");
        }
    }

    private void ValidarResponsable(string dniResponsable)
    {
        OperacionesComunes.ValidarResponsable(_repositorioUsuario, dniResponsable, Permiso.GestionarCatalogo,
            MensajeSinPermiso);
    }

    // Solo nombres de la enumeracion: Enum.TryParse tambien aceptaria numeros como "1".
    private static Categoria ObtenerCategoria(string categoria)
    {
        foreach (string nombre in Enum.GetNames<Categoria>())
        {
            if (string.Equals(nombre, categoria, StringComparison.OrdinalIgnoreCase))
            {
                return Enum.Parse<Categoria>(nombre);
            }
        }

        throw new ServicioException("La categoria seleccionada no existe");
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