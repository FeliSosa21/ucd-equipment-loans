namespace Dominio;

public enum AccionAuditada
{
    RegistroDeUsuario = 1,
    SolicitudDePrestamo,
    ActivacionDePrestamo,
    RechazoDePrestamo,
    DevolucionDePrestamo,
    IntentoDeAprobacion,
    AprobacionDePrestamo,
    AltaDeUsuario,
    CambioDeTipoDeUsuario,
    DesactivacionDeUsuario,
    ReactivacionDeUsuario
}
