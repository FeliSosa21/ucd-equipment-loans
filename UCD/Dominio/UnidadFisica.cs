namespace Dominio;

public class UnidadFisica
{
    public string IdInstitucional { get; private set; }
    public string UbiFisica { get; private  set; }
    public string NumSerie { get; private set; }
    public string? Observaciones { get; private set; }
    public EstadoUnidadFisica Estado { get; private set; }


    public UnidadFisica(string idInstitucional, string ubiFisica, string numSerie)
    {
        ValidarIdInstitucional(idInstitucional);
        IdInstitucional= idInstitucional;
        
        ValidarUbiFisica(ubiFisica);
        UbiFisica = ubiFisica;
        
        NumSerie = numSerie;
        
        Estado = EstadoUnidadFisica.Disponible;
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
        bool formatoValido = idInstitucional.Length == 8
                             && char.IsUpper(idInstitucional[0])
                             && char.IsUpper(idInstitucional[1])
                             && char.IsUpper(idInstitucional[2])
                             && idInstitucional[3] == '-'
                             && char.IsDigit(idInstitucional[4])
                             && char.IsDigit(idInstitucional[5])
                             && char.IsDigit(idInstitucional[6])
                             && char.IsDigit(idInstitucional[7]);

        if (!formatoValido)
        {
            throw new DominioException("El id institucional debe tener el formato AAA-0000");
        }
    }

    public void ModificarInfo(string ubiFisica,string observaciones)
    {
        ValidarUbiFisica(ubiFisica);
        UbiFisica = ubiFisica;
        Observaciones = observaciones;
    }
    public bool EstaDisponible()
    {
        return Estado == EstadoUnidadFisica.Disponible;
    }
}