using Dominio;

namespace Data.Repositorios;

public class CatalogoRepositorio
{
    private readonly BaseDeDatosEnMemoria _catalogo;
    
    public CatalogoRepositorio(BaseDeDatosEnMemoria baseDeDatosEnMemoria)
    {
        _catalogo = baseDeDatosEnMemoria;
    }

    public Catalogo? Obtener()
    {
        return _catalogo.Catalogo;
    }

    public void Guardar(Catalogo catalogo)
    {
        _catalogo.Catalogo = catalogo;
    }
}