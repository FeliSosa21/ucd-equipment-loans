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
        IdInstitucional= idInstitucional;
        UbiFisica = ubiFisica;
        NumSerie = numSerie;
    }
    
    
}