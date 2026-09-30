using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Dominio;

public class TipoEquipamiento
{
    private const int LargoMinimoNombre = 3;
    private const int LargoMaximoNombre = 80;
    private const int LargoMaximoDescripcion = 300;

    public string Nombre { get; private set; }
    public string Descripcion { get; private set; }
    public int DuracionMaxPrestamo { get; private set; }
    public bool RequiereAprobacion { get; private set; }
    public Categoria Categoria { get; private set; }
    public EstadoTipoEquipamiento Estado { get; private set; } = EstadoTipoEquipamiento.Activo;

    public List<UnidadFisica> Unidades { get; private set; } = new();

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

    public bool EstaActivo => Estado == EstadoTipoEquipamiento.Activo;

    private static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DominioException("El nombre del tipo de equipamiento no puede ser vacío");
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
        if (descripcion.Length > LargoMaximoDescripcion)
        {
            throw new DominioException(
                $"La descripción del tipo de equipamiento no puede superar los {LargoMaximoDescripcion} caracteres");
        }
    }

    private static void ValidarDuracion(int duracionMaxPrestamo)
    {
        if (duracionMaxPrestamo <= 0)
        {
            throw new DominioException("La duración máxima de préstamo debe ser mayor que cero");
        }
    }

    public void AgregarUnidad(UnidadFisica unidad)
    {
        ValidadQueSePuedeAgregar(unidad);
        foreach (var unidadFisica in Unidades)
        {
            if (unidadFisica.IdInstitucional == unidad.IdInstitucional)
            {
                throw new DominioException("No se pueden agregar unidades ya existentes");
            }
        }
        Unidades.Add(unidad);
    }

    private void ValidadQueSePuedeAgregar(UnidadFisica unidad)
    {
        if (unidad is null)
        {
            throw new DominioException("La unidad no puede ser nula");
        }

        if (!EstaActivo)
        {
            throw new DominioException("No se pueden agregar unidades a un tipo discontinuo");
        }
    }
}