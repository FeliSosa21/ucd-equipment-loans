using DTOs;
using Microsoft.AspNetCore.Components.Forms;
using Servicios;
using UI.Sesion;

namespace UI.Components.Pages;

// Req. 2.1: el administrador modifica la descripcion, la imagen y los topes de los seis tipos de usuario.
// Las reglas las controla ServicioTiposUsuario; la pagina solo muestra y arma la imagen en base64.
public partial class TiposUsuario
{
    private const long BytesMaximosImagen = 500 * 1024;

    private List<TipoUsuarioDto> _tipos = new List<TipoUsuarioDto>();
    private TipoUsuarioDto? _editando;
    private string? _error;
    private string? _exito;

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.GestionarUsuarios))
        {
            return;
        }

        Recargar();
    }

    private void Recargar()
    {
        try
        {
            _tipos = ServicioTipos.ListarTipos(DniUsuario);
        }
        catch (ServicioException ex)
        {
            _tipos = new List<TipoUsuarioDto>();
            _error = ex.Message;
        }
    }

    private void AbrirEdicion(TipoUsuarioDto tipo)
    {
        LimpiarMensajes();
        _editando = new TipoUsuarioDto
        {
            Nombre = tipo.Nombre,
            Descripcion = tipo.Descripcion,
            Imagen = tipo.Imagen,
            MaxUnidadesPorCatalogo = tipo.MaxUnidadesPorCatalogo,
            MaxPrestamosConcurrentes = tipo.MaxPrestamosConcurrentes
        };
    }

    private void CancelarEdicion()
    {
        _editando = null;
        LimpiarMensajes();
    }

    // El archivo se lee entero y se guarda como data URI; el formato y el peso los vuelve a validar el dominio.
    private async Task CargarImagen(InputFileChangeEventArgs e)
    {
        if (_editando is null)
        {
            return;
        }

        LimpiarMensajes();

        try
        {
            using MemoryStream memoria = new MemoryStream();
            await e.File.OpenReadStream(BytesMaximosImagen).CopyToAsync(memoria);
            _editando.Imagen = $"data:{e.File.ContentType};base64,{Convert.ToBase64String(memoria.ToArray())}";
        }
        catch (IOException)
        {
            _error = "La imagen no puede superar los 500 KB";
        }
    }

    private void Guardar()
    {
        // Un doble clic manda dos envios: el segundo llega con el formulario ya cerrado.
        if (_editando is null)
        {
            return;
        }

        try
        {
            ServicioTipos.ModificarTipo(_editando, DniUsuario);
            _exito = $"Se guardaron los cambios de {_editando.Nombre}";
            _error = null;
            _editando = null;
            Recargar();
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
            _exito = null;
        }
    }

    private static bool TieneTopes(TipoUsuarioDto tipo)
    {
        return tipo.MaxUnidadesPorCatalogo.HasValue && tipo.MaxPrestamosConcurrentes.HasValue;
    }

    private static string TextoDeTope(int? tope)
    {
        return tope.HasValue ? tope.Value.ToString() : "Sin límite";
    }

    // Al abrir o cerrar un formulario se borran los mensajes de la accion anterior.
    private void LimpiarMensajes()
    {
        _error = null;
        _exito = null;
    }
}
