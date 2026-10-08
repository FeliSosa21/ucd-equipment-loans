using Data.Repositorios;
using DTOs;

namespace Servicios;

// Gestion de tipos de equipamiento y del nombre del catalogo (req. 4).
// Registrar, modificar y discontinuar requieren el permiso GestionarCatalogo (Supervisor y Administrador).
public class ServicioTiposEquipamiento
{
    private readonly RepositorioCatalogo _repositorioCatalogo;
    private readonly RepositorioUsuario _repositorioUsuario;

    public ServicioTiposEquipamiento(RepositorioCatalogo repositorioCatalogo, RepositorioUsuario repositorioUsuario)
    {
        _repositorioCatalogo = repositorioCatalogo;
        _repositorioUsuario = repositorioUsuario;
    }

    public List<TipoEquipamientoDetalleDto> Listar(string? nombre, string? categoria)
    {
        return new List<TipoEquipamientoDetalleDto>();
    }
}