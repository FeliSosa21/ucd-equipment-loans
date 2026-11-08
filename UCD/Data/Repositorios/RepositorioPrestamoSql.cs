using Dominio;
using Microsoft.EntityFrameworkCore;
using static Data.Configuraciones.NombresDeMapeo;

namespace Data.Repositorios;

public class RepositorioPrestamoSql
{
    private readonly SqlContext _contexto;

    public RepositorioPrestamoSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    private IQueryable<Prestamo> PrestamosConSolicitanteTipoYAsignadas =>
        _contexto.Prestamos
            .Include(prestamo => prestamo.Solicitante)
            .Include(prestamo => prestamo.TipoEquipamiento)
            .Include(prestamo => prestamo.UnidadesAsignadas);

    public void AgregarPrestamo(Prestamo prestamo)
    {
        _contexto.Prestamos.Add(prestamo);
        _contexto.SaveChanges();
    }

    public Prestamo? ObtenerPrestamoPorId(Guid id)
    {
        return _contexto.Prestamos
            .Include(prestamo => prestamo.Solicitante)
            .Include(prestamo => prestamo.Evaluador)
            .Include(prestamo => prestamo.TipoEquipamiento)
            .ThenInclude(tipo => tipo.Unidades)
            .Include(prestamo => prestamo.UnidadesAsignadas)
            .FirstOrDefault(prestamo => prestamo.Id == id);
    }

    public void ActualizarPrestamo(Prestamo prestamo)
    {
        _contexto.SaveChanges();
    }

    public IReadOnlyList<Prestamo> ObtenerPrestamos(FiltroPorCatalogoYFechas filtro, EstadoPrestamo? estado = null)
    {
        return PrestamosConSolicitanteTipoYAsignadas
            .Include(prestamo => prestamo.Evaluador)
            .Where(prestamo => filtro.Desde == null || prestamo.FechaHoraPrestamo >= filtro.Desde)
            .Where(prestamo => filtro.Hasta == null || prestamo.FechaHoraPrestamo < filtro.Hasta)
            .Where(prestamo => filtro.IdCatalogo == null
                || EF.Property<int>(prestamo.TipoEquipamiento, ColumnaCatalogo) == filtro.IdCatalogo)
            .Where(prestamo => estado == null || prestamo.Estado == estado)
            .OrderBy(prestamo => prestamo.FechaHoraPrestamo)
            .ToList();
    }

    public IReadOnlyList<Prestamo> ObtenerPrestamosActivos()
    {
        return PrestamosConSolicitanteTipoYAsignadas
            .Where(prestamo => prestamo.Estado == EstadoPrestamo.Activo)
            .OrderBy(prestamo => prestamo.FechaHoraPrestamo)
            .ToList();
    }

    public IReadOnlyList<Prestamo> ObtenerPrestamosPendientes()
    {
        return PrestamosConSolicitanteTipoYAsignadas
            .Where(prestamo => prestamo.Estado == EstadoPrestamo.Solicitado)
            .OrderBy(prestamo => prestamo.FechaHoraPrestamo)
            .ToList();
    }

    public IReadOnlyList<Prestamo> ObtenerPrestamosDeSolicitante(string dni)
    {
        return PrestamosConSolicitanteTipoYAsignadas
            .Where(prestamo => prestamo.Solicitante.Dni == dni)
            .ToList();
    }

    public bool ExistenPrestamosVigentesDelCatalogo(int idCatalogo)
    {
        return _contexto.Prestamos
            .Where(prestamo => EF.Property<int>(prestamo.TipoEquipamiento, ColumnaCatalogo) == idCatalogo)
            .Where(prestamo => prestamo.Estado == EstadoPrestamo.Solicitado
                || prestamo.Estado == EstadoPrestamo.Activo)
            .Any();
    }

    public int ContarPrestamosVigentesDeSolicitante(string dni, Guid? idExcluido = null)
    {
        return _contexto.Prestamos
            .Where(prestamo => prestamo.Solicitante.Dni == dni)
            .Where(prestamo => prestamo.Estado == EstadoPrestamo.Solicitado
                               || prestamo.Estado == EstadoPrestamo.Activo)
            .Where(prestamo => prestamo.Id != idExcluido)
            .Count();
    }

    public int ContarUnidadesActivasDeSolicitanteEnCatalogoDelTipo(string dni, int idTipoEquipamiento)
    {
        int idCatalogo = _contexto.Set<TipoEquipamiento>()
            .Where(tipo => tipo.Id == idTipoEquipamiento)
            .Select(tipo => EF.Property<int>(tipo, ColumnaCatalogo))
            .Single();

        return _contexto.Prestamos
            .Where(prestamo => prestamo.Solicitante.Dni == dni)
            .Where(prestamo => EF.Property<int>(prestamo.TipoEquipamiento, ColumnaCatalogo) == idCatalogo)
            .SelectMany(prestamo => prestamo.UnidadesAsignadas)
            .Count();
    }
}