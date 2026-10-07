namespace UI.Sesion;

// Nombres de los permisos tal como llegan en UsuarioSesionDto.Permisos (la UI no conoce el enum del dominio).
public static class Permisos
{
    public const string ConsultarCatalogo = "ConsultarCatalogo";
    public const string ConsultarMisSolicitudes = "ConsultarMisSolicitudes";
    public const string SolicitarPrestamo = "SolicitarPrestamo";
    public const string GestionarCatalogo = "GestionarCatalogo";
    public const string GestionarUnidades = "GestionarUnidades";
    public const string EvaluarSolicitud = "EvaluarSolicitud";
    public const string RealizarPrestamo = "RealizarPrestamo";
    public const string RegistrarDevolucion = "RegistrarDevolucion";
    public const string ConsultarHistorico = "ConsultarHistorico";
    public const string GestionarUsuarios = "GestionarUsuarios";
}
