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
        _BaseDeDatos.Catalogo = catalogo;
    }
}