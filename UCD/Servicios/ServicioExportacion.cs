using Data.Repositorios;
using DTOs;

namespace Servicios;

public class ServicioExportacion
{
    public ServicioExportacion(RepositorioPrestamoSql prestamos, RepositorioDevolucionSql devoluciones,
        RepositorioCatalogoSql catalogos, RepositorioUsuarioSql usuarios, IEnumerable<IExportadorDeTabla> exportadores)
    {
    }

    public CantidadesAExportarDto ContarRegistros(RangoDeExportacionDto rango, string dniResponsable)
    {
        throw new NotImplementedException();
    }
}
