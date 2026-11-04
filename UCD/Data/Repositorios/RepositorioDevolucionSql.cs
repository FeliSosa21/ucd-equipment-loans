using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositorios;

public class RepositorioDevolucionSql
{
    private const string CampoRegistro = "_registro";
    private const string CampoUnidades = "_unidades";

    private readonly SqlContext _contexto;

    public RepositorioDevolucionSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public void AgregarDevolucion(Devolucion devolucion)
    {
        _contexto.Devoluciones.Add(devolucion);
        _contexto.SaveChanges();
    }

    public IReadOnlyList<Devolucion> ObtenerDevoluciones(FiltroPorCatalogoYFechas filtro)
    {
        return _contexto.Devoluciones
            .Include($"{CampoRegistro}.{nameof(RegistroDeAccion.Responsable)}")
            .Include($"{CampoUnidades}.{nameof(UnidadDevuelta.Unidad)}")
            .OrderBy(devolucion => EF.Property<RegistroDeAccion>(devolucion, CampoRegistro).FechaHora)
            .ThenBy(devolucion => devolucion.Id)
            .ToList();
    }
}
