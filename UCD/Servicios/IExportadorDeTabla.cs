using DTOs;

namespace Servicios;

public interface IExportadorDeTabla
{
    string Formato { get; }
    string Extension { get; }
    string TipoContenido { get; }

    byte[] Exportar(TablaExportableDto tabla);
}
