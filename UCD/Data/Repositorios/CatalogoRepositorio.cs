using Dominio;

namespace Data.Repositorios;

public class CatalogoRepositorio
{
    private Catalogo? _catalogo;
    
    public CatalogoRepositorio(BaseDeDatosEnMemoria baseDeDatosEnMemoria)
    {
        
    }

    public Catalogo? Obtener()
    {
        return _catalogo;
    }

    public void Guardar(Catalogo catalogo)
    {
        _catalogo = catalogo;
    }
}