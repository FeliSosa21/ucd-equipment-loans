using Dominio;
using Microsoft.EntityFrameworkCore;
using static Data.Configuraciones.NombresDeMapeo;

namespace Data.Repositorios;

public class RepositorioAuditoriaSql
{
    private readonly SqlContext _contexto;

    public RepositorioAuditoriaSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public void AgregarRegistro(RegistroAuditoria registro)
    {
        ArgumentNullException.ThrowIfNull(registro);
        _contexto.RegistrosAuditoria.Add(registro);
        _contexto.SaveChanges();
    }

    public IReadOnlyList<RegistroAuditoria> ObtenerRegistros(FiltroAuditoria filtro)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        if (!filtro.TieneCriterios())
        {
            return new List<RegistroAuditoria>();
        }

        IQueryable<RegistroAuditoria> registros = FiltrarDesde(RegistrosConResponsable, filtro.Desde);
        registros = FiltrarHasta(registros, filtro.Hasta);
        registros = FiltrarPorResponsable(registros, filtro.DniUsuario);

        return registros
            .OrderByDescending(registro => EF.Property<RegistroDeAccion>(registro, CampoRegistro).FechaHora)
            .ThenByDescending(registro => registro.Id)
            .ToList();
    }

    private IQueryable<RegistroAuditoria> RegistrosConResponsable =>
        _contexto.RegistrosAuditoria.Include($"{CampoRegistro}.{nameof(RegistroDeAccion.Responsable)}");

    private static IQueryable<RegistroAuditoria> FiltrarDesde(IQueryable<RegistroAuditoria> registros, DateTime? desde)
    {
        if (desde is null)
        {
            return registros;
        }

        return registros.Where(registro => EF.Property<RegistroDeAccion>(registro, CampoRegistro).FechaHora >= desde);
    }

    private static IQueryable<RegistroAuditoria> FiltrarHasta(IQueryable<RegistroAuditoria> registros, DateTime? hasta)
    {
        if (hasta is null)
        {
            return registros;
        }

        return registros.Where(registro => EF.Property<RegistroDeAccion>(registro, CampoRegistro).FechaHora < hasta);
    }

    private static IQueryable<RegistroAuditoria> FiltrarPorResponsable(IQueryable<RegistroAuditoria> registros,
        string? dniUsuario)
    {
        if (dniUsuario is null)
        {
            return registros;
        }

        return registros.Where(registro =>
            EF.Property<RegistroDeAccion>(registro, CampoRegistro).Responsable!.Dni == dniUsuario);
    }
}
