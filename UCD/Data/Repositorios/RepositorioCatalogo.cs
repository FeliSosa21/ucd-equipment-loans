using Dominio;

namespace Data.Repositorios;

public class RepositorioCatalogo
{

    private readonly Memoria _baseDeDatos;
    public RepositorioCatalogo(Memoria baseDeDatos)
    {
        _baseDeDatos = baseDeDatos;
    }

    public Catalogo? Obtener()
    {
        return _baseDeDatos.Catalogo;
    }

    public void Guardar(Catalogo catalogo)
    {
        ValidarQueSePuedaGuardar(catalogo);

        _baseDeDatos.Catalogo = catalogo;
    }

    private void ValidarQueSePuedaGuardar(Catalogo catalogo)
    {
        if (catalogo is null)
        {
            throw new DominioException("El catálogo no puede ser nulo");
        }

        if (_baseDeDatos.Catalogo is not null)
        {
            throw new DominioException("Ya existe un catálogo, el sistema admite uno solo");
        }
    }
}