using System.Diagnostics.CodeAnalysis;

namespace Dominio;

public class TipoEquipamiento
{
    private const int LargoMinimoNombre = 3;
    public const int LargoMaximoNombre = 80;
    public const int LargoMaximoDescripcion = 300;

    public string Nombre { get; private set; }
    public string Descripcion { get; private set; }
    public int DuracionMaxPrestamo { get; private set; }
    public bool RequiereAprobacion { get; private set; }
    public Categoria Categoria { get; private set; }
    public EstadoTipoEquipamiento Estado { get; private set; } = EstadoTipoEquipamiento.Activo;
    public bool EstaActivo => Estado == EstadoTipoEquipamiento.Activo;
    public int Id { get; private set; }

    private readonly List<UnidadFisica> _unidades = new();

    public IReadOnlyCollection<UnidadFisica> Unidades => _unidades.AsReadOnly();

    private TipoEquipamiento()
    {
        Nombre = null!;
        Descripcion = null!;
    }

    public TipoEquipamiento(string nombre, string descripcion, int duracionMaxPrestamo, bool requiereAprobacion,
        Categoria categoria)
    {
        Modificar(nombre, descripcion, duracionMaxPrestamo, requiereAprobacion, categoria);
    }

    [MemberNotNull(nameof(Nombre), nameof(Descripcion))]
    public void Modificar(string nombre, string descripcion, int duracionMaxPrestamo, bool requiereAprobacion,
        Categoria categoria)
    {
        ValidarNombre(nombre);
        ValidarDescripcion(descripcion);
        ValidarDuracion(duracionMaxPrestamo);

        Nombre = nombre;
        Descripcion = descripcion;
        DuracionMaxPrestamo = duracionMaxPrestamo;
        RequiereAprobacion = requiereAprobacion;
        Categoria = categoria;
    }

    public void Discontinuar()
    {
        if (Estado == EstadoTipoEquipamiento.Discontinuado)
        {
            throw new DominioException("El tipo de equipamiento ya esta discontinuado");
        }

        Estado = EstadoTipoEquipamiento.Discontinuado;
    }

    public void Reactivar()
    {
        if (EstaActivo)
        {
            throw new DominioException("El tipo de equipamiento ya esta activo");
        }

        Estado = EstadoTipoEquipamiento.Activo;
    }

    public void AgregarUnidad(UnidadFisica unidad)
    {
        ValidarQueSePuedaAgregar(unidad);

        _unidades.Add(unidad);
    }

    public int CantidadDisponibles()
    {
        return UnidadesDisponibles().Count;
    }

    public IReadOnlyCollection<UnidadFisica> UnidadesDisponibles()
    {
        var disponibles = new List<UnidadFisica>();
        foreach (var unidad in _unidades)
        {
            if (unidad.EstaDisponible())
            {
                disponibles.Add(unidad);
            }
        }

        return disponibles;
    }

    private static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DominioException("El nombre del tipo de equipamiento no puede ser vacio");
        }

        if (nombre.Length < LargoMinimoNombre)
        {
            throw new DominioException(
                $"El nombre del tipo de equipamiento debe tener al menos {LargoMinimoNombre} caracteres");
        }

        if (nombre.Length > LargoMaximoNombre)
        {
            throw new DominioException(
                $"El nombre del tipo de equipamiento no puede superar los {LargoMaximoNombre} caracteres");
        }
    }

    private static void ValidarDescripcion(string descripcion)
    {
        if (descripcion is null)
        {
            throw new DominioException("La descripcion del tipo de equipamiento no puede ser nula");
        }

        if (descripcion.Length > LargoMaximoDescripcion)
        {
            throw new DominioException(
                $"La descripcion del tipo de equipamiento no puede superar los {LargoMaximoDescripcion} caracteres");
        }
    }

    private static void ValidarDuracion(int duracionMaxPrestamo)
    {
        if (duracionMaxPrestamo <= 0)
        {
            throw new DominioException("La duracion maxima de prestamo debe ser mayor que cero");
        }
    }

    private void ValidarQueSePuedaAgregar(UnidadFisica unidad)
    {
        if (unidad is null)
        {
            throw new DominioException("La unidad no puede ser nula");
        }

        if (!EstaActivo)
        {
            throw new DominioException("No se pueden agregar unidades a un tipo discontinuado");
        }

        foreach (var existente in _unidades)
        {
            if (existente.IdInstitucional == unidad.IdInstitucional)
            {
                throw new DominioException("Ya existe una unidad con ese id institucional en el tipo");
            }
        }
    }
}