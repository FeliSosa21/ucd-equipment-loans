using Data.Repositorios;
using DTOs;

namespace Servicios;

// Gestion de unidades fisicas (req. 4): registrar, modificar, cambiar de estado y dar de baja.
// Las operaciones que modifican requieren el permiso GestionarUnidades (Funcionario, Supervisor y Administrador).
public class ServicioUnidades
{
    private readonly RepositorioCatalogo _repositorioCatalogo;
    private readonly RepositorioUsuario _repositorioUsuario;

    public ServicioUnidades(RepositorioCatalogo repositorioCatalogo, RepositorioUsuario repositorioUsuario)
    {
        _repositorioCatalogo = repositorioCatalogo;
        _repositorioUsuario = repositorioUsuario;
    }

    public List<UnidadDetalleDto> Listar(string? nombreTipo, string? estado)
    {
        return new List<UnidadDetalleDto>();
    }
}