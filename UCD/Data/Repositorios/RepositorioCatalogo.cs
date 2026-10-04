using Dominio;

namespace Data.Repositorios;

public class RepositorioCatalogo
{

    private readonly Memoria _memoria;
    public RepositorioCatalogo(Memoria memoria)
    {
        _memoria = memoria;
    }

    public Catalogo? Obtener()
    {
        return _memoria.Catalogo;
    }

    public void Guardar(Catalogo catalogo)
    {
        ValidarQueSePuedaGuardar(catalogo);

        _memoria.Catalogo = catalogo;
    }

    public TipoEquipamiento? ObtenerTipoPorNombre(string nombre)
    {
        var catalogo = Obtener();

        return catalogo?.BuscarTipoPorNombre(nombre);
    }

    private void ValidarQueSePuedaGuardar(Catalogo catalogo)
    {
        if (catalogo is null)
        {
            throw new DominioException("El catálogo no puede ser nulo");
        }

        if (_memoria.Catalogo is not null)
        {
            throw new DominioException("Ya existe un catálogo, el sistema admite uno solo");
        }
    }
}