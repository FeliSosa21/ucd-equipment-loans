using Dominio;

namespace Data.Repositorios;

public class CatalogoRepositorio
{
    private readonly BaseDeDatosEnMemoria _BaseDeDatos;
    
    public CatalogoRepositorio(BaseDeDatosEnMemoria baseDeDatosEnMemoria)
    {
        _BaseDeDatos = baseDeDatosEnMemoria;
    }

    public Catalogo? Obtener()
    {
        return _BaseDeDatos.Catalogo;
    }

    public void Guardar(Catalogo catalogo)
    {
        if (catalogo == null)
        {
            throw new DominioException("El catalogo no puede ser nulo");
        }

        if (_BaseDeDatos.Catalogo is not null)
        {
            throw new DominioException("Ya existe un catálogo, el sistemas admite uno solo");
        }
        
        _BaseDeDatos.Catalogo = catalogo;
    }
}