using Data.Repositorios;
using DTOs;

namespace Servicios;

public class ServicioPrestamo
{
    private readonly RepositorioPrestamo _prestamos;
    private readonly RepositorioUsuario _usuarios;
    private readonly RepositorioCatalogo _catalogo;
    private readonly RepositorioMovimiento _movimientos;
    private readonly TimeProvider _reloj;

    public ServicioPrestamo(RepositorioPrestamo prestamos, RepositorioUsuario usuarios,
        RepositorioCatalogo catalogo, RepositorioMovimiento movimientos, TimeProvider reloj)
    {
        _prestamos = prestamos;
        _usuarios = usuarios;
        _catalogo = catalogo;
        _movimientos = movimientos;
        _reloj = reloj;
    }

    public PrestamoDto Solicitar(SolicitudPrestamoDto datos, string dniSolicitante)
    {
        ArgumentNullException.ThrowIfNull(datos);

        return new PrestamoDto();
    }
}