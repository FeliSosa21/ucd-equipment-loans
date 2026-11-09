using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Gestion de tipos de equipamiento (req. 4).
// Registrar, modificar, discontinuar y reactivar requieren el permiso GestionarCatalogo (Supervisor y Administrador).
public class ServicioTiposEquipamiento
{
    private const string MensajeSinPermiso = "El usuario no tiene permiso para gestionar el catalogo";
    private const string MensajeNoSePudoGuardar =
        "No se pudieron guardar los cambios; verifique que no exista otro tipo con ese nombre";
    private const string MensajeSinPermisoParaListar = "El usuario no tiene permiso para ver los tipos de equipamiento";

    private readonly RepositorioCatalogoSql _repositorioCatalogo;
    private readonly RepositorioUsuarioSql _repositorioUsuario;

    public ServicioTiposEquipamiento(RepositorioCatalogoSql repositorioCatalogo, RepositorioUsuarioSql repositorioUsuario)
    {
        _repositorioCatalogo = repositorioCatalogo;
        _repositorioUsuario = repositorioUsuario;
    }

    public List<TipoEquipamientoDetalleDto> Listar(int idCatalogo, FiltroTiposDto filtro, string dniUsuario)
    {
        ValidarQuePuedaListar(dniUsuario);
        List<TipoEquipamientoDetalleDto> resultado = new List<TipoEquipamientoDetalleDto>();

        foreach (TipoEquipamiento tipo in
                 OperacionesComunes.ObtenerCatalogoActivo(_repositorioCatalogo, idCatalogo).Tipos)
        {
            if (CumpleFiltro(tipo, filtro.Nombre, filtro.Categoria))
            {
                resultado.Add(CrearDetalle(tipo));
            }
        }

        return resultado;
    }

    public void Registrar(int idCatalogo, DatosTipoEquipamientoDto datos, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ValidarResponsable(dniResponsable);
        Categoria categoria = ObtenerCategoria(datos.Categoria);
        Catalogo catalogo = OperacionesComunes.ObtenerCatalogoActivo(_repositorioCatalogo, idCatalogo);
        string nombre = OperacionesComunes.Normalizar(datos.Nombre);
        string descripcion = OperacionesComunes.Normalizar(datos.Descripcion);

        TraduccionDeErrores.Ejecutar(() =>
        {
            TipoEquipamiento tipo = new TipoEquipamiento(nombre, descripcion, datos.DuracionMaxPrestamo,
                datos.RequiereAprobacion, categoria);
            catalogo.AgregarTipo(tipo);
        });
        OperacionesComunes.GuardarCambios(_repositorioCatalogo, catalogo, MensajeNoSePudoGuardar);
    }

    public void Modificar(int idCatalogo, ModificacionTipoEquipamientoDto modificacion, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(modificacion);
        DatosTipoEquipamientoDto datos = modificacion.Datos;
        ValidarResponsable(dniResponsable);
        Catalogo catalogo = OperacionesComunes.ObtenerCatalogoActivo(_repositorioCatalogo, idCatalogo);
        TipoEquipamiento tipo = OperacionesComunes.ObtenerTipo(_repositorioCatalogo, idCatalogo,
            modificacion.NombreActual);
        Categoria categoria = ObtenerCategoria(datos.Categoria);
        string nombreNuevo = OperacionesComunes.Normalizar(datos.Nombre);
        ValidarNombreDisponible(catalogo, tipo, nombreNuevo);

        TraduccionDeErrores.Ejecutar(() =>
            tipo.Modificar(nombreNuevo, OperacionesComunes.Normalizar(datos.Descripcion), datos.DuracionMaxPrestamo,
                datos.RequiereAprobacion, categoria));
        _repositorioCatalogo.ActualizarCatalogo(catalogo);
    }

    public void Discontinuar(int idCatalogo, string nombre, string dniResponsable)
    {
        ValidarResponsable(dniResponsable);
        Catalogo catalogo = OperacionesComunes.ObtenerCatalogoActivo(_repositorioCatalogo, idCatalogo);
        TipoEquipamiento tipo = OperacionesComunes.ObtenerTipo(_repositorioCatalogo, idCatalogo, nombre);

        TraduccionDeErrores.Ejecutar(tipo.Discontinuar);
        _repositorioCatalogo.ActualizarCatalogo(catalogo);
    }

    // Foro del curso: un tipo discontinuado puede volver a activarse.
    public void Reactivar(int idCatalogo, string nombre, string dniResponsable)
    {
        ValidarResponsable(dniResponsable);
        Catalogo catalogo = OperacionesComunes.ObtenerCatalogoActivo(_repositorioCatalogo, idCatalogo);
        TipoEquipamiento tipo = OperacionesComunes.ObtenerTipo(_repositorioCatalogo, idCatalogo, nombre);

        TraduccionDeErrores.Ejecutar(tipo.Reactivar);
        _repositorioCatalogo.ActualizarCatalogo(catalogo);
    }

    private static void ValidarNombreDisponible(Catalogo catalogo, TipoEquipamiento tipo, string nombreNuevo)
    {
        TipoEquipamiento? existente = catalogo.BuscarTipoPorNombre(nombreNuevo);
        if (existente is not null && existente != tipo)
        {
            throw new ServicioException("Ya existe un tipo de equipamiento con ese nombre");
        }
    }

    // Listan los tipos quien gestiona el catalogo y quien gestiona unidades (necesita elegir el tipo de cada unidad).
    private void ValidarQuePuedaListar(string dniUsuario)
    {
        Usuario usuario = OperacionesComunes.ObtenerResponsableActivo(_repositorioUsuario, dniUsuario,
            MensajeSinPermisoParaListar);
        if (!usuario.TienePermiso(Permiso.GestionarCatalogo) && !usuario.TienePermiso(Permiso.GestionarUnidades))
        {
            throw new ServicioException(MensajeSinPermisoParaListar);
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