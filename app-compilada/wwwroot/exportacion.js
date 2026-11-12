// Guarda en el navegador un archivo que el servidor entrega como DotNetStreamReference (ver Exportacion.razor.cs).
// El navegador no deja escribir un archivo desde JavaScript: se arma una URL temporal con el contenido y se
// "hace clic" en un enlace con el atributo download, que es lo que abre el guardado.
window.descargarArchivo = async (nombre, tipoContenido, flujo) => {
    const bytes = await flujo.arrayBuffer();
    const url = URL.createObjectURL(new Blob([bytes], { type: tipoContenido }));
    const enlace = document.createElement("a");
    enlace.href = url;
    enlace.download = nombre;
    document.body.appendChild(enlace);
    enlace.click();
    enlace.remove();
    URL.revokeObjectURL(url);
};
