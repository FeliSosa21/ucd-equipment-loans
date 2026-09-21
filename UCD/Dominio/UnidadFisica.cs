namespace Dominio;

public class UnidadFisica
{
    public string IdInstitucional { get; private set; }
    public string UbiFisica { get; private  set; }
    public string NumSerie { get; private set; }


    public UnidadFisica(string idInstitucional, string ubiFisica, string numSerie)
    {
        if (string.IsNullOrWhiteSpace(ubiFisica))
        {
            throw new DominioException("La ubicacion fisica no puede estar vacia");
        }

        if (ubiFisica.Length < 3)
        {
            throw new DominioException("La ubicacion fisica no puede tener menos de 3 caracteres"); 
        }

        if (ubiFisica.Length>80)
        {
            throw new DominioException("La ubicacion fisica no puede tener mas de 80 caracteres"); 
        }
        IdInstitucional= idInstitucional;
        UbiFisica = ubiFisica;
        NumSerie = numSerie;
    }
    
    
}