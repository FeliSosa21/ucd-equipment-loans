namespace Dominio;

public class UnidadFisica
{
    private const int LargoIdInstitucional = 8;
    private const int PosicionGuion = 3;

    public string IdInstitucional { get; private set; }
    public string UbiFisica { get; private set; }
    public string? NumSerie { get; private set; }
    public string? Observaciones { get; private set; }
    public EstadoUnidadFisica Estado { get; private set; }

    public UnidadFisica(string idInstitucional, string ubiFisica, string? numSerie)
    {
        ValidarIdInstitucional(idInstitucional);
        IdInstitucional = idInstitucional;

        ValidarUbiFisica(ubiFisica);
        UbiFisica = ubiFisica;

        ValidarLargoNumSerie(numSerie);
        NumSerie = numSerie;

        Estado = EstadoUnidadFisica.Disponible;
    }

    public void ModificarInfo(string ubiFisica, string observaciones)
    {
        ValidarUbiFisica(ubiFisica);
        UbiFisica = ubiFisica;
        Observaciones = observaciones;
    }

    public bool EstaDisponible()
    {
        return Estado == EstadoUnidadFisica.Disponible;
    }

    public void Prestada()
    {
        if (!EstaDisponible())
        {
            throw new DominioException("La unidad debe estar disponible para prestarse");
        }

        Estado = EstadoUnidadFisica.Prestada;
    }

    public void Disponible()
    {
        if (Estado != EstadoUnidadFisica.Prestada && Estado != EstadoUnidadFisica.EnMantenimiento)
        {
            throw new DominioException("La unidad debe estar prestada o en mantenimiento para pasar a disponible");
        }

        Estado = EstadoUnidadFisica.Disponible;
    }

    public void AMantenimiento()
    {
        if (Estado != EstadoUnidadFisica.Prestada)
        {
            throw new DominioException("La unidad debe estar prestada para pasar a mantenimiento");
        }

        Estado = EstadoUnidadFisica.EnMantenimiento;
    }

    public void DarDeBaja()
    {
        if (Estado == EstadoUnidadFisica.Prestada)
        {
            throw new DominioException("La unidad prestada no puede darse de baja directamente");
        }

        Estado = EstadoUnidadFisica.DadaDeBaja;
    }

    private void ValidarUbiFisica(string ubiFisica)
    {
        if (string.IsNullOrWhiteSpace(ubiFisica))
        {
            throw new DominioException("La ubicacion fisica no puede estar vacia");
        }

        if (ubiFisica.Length < 3)
        {
            throw new DominioException("La ubicacion fisica no puede tener menos de 3 caracteres");
        }

        if (ubiFisica.Length > 80)
        {
            throw new DominioException("La ubicacion fisica no puede tener mas de 80 caracteres");
        }
    }

    private void ValidarIdInstitucional(string idInstitucional)
    {
        if (string.IsNullOrWhiteSpace(idInstitucional) || !TieneFormatoValido(idInstitucional))
        {
            throw new DominioException("El id institucional debe tener el formato AAA-0000");
        }
    }

    private static bool TieneFormatoValido(string idInstitucional)
    {
        if (idInstitucional.Length != LargoIdInstitucional || idInstitucional[PosicionGuion] != '-')
        {
            return false;
        }

        string letras = idInstitucional[..PosicionGuion];
        string numeros = idInstitucional[(PosicionGuion + 1)..];

        return letras.All(char.IsUpper) && numeros.All(char.IsDigit);
    }

    private void ValidarLargoNumSerie(string numSerie)
    {
        if (numSerie != null && numSerie.Length > 50)
        {
            throw new DominioException("El numero de serie no puede tener mas de 50 caracteres");
        }
    }
}

