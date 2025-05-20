// Gestor_Desempeno/GestionFotosUsuario.aspx.cs
using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;

namespace Gestor_Desempeno
{
    public partial class GestionFotosUsuario : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        string fotosPath = "~/Fotos/"; // Ruta base donde se guardan las fotos
        string defaultFoto = "avatar_defecto.png"; // Nombre de la foto por defecto

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarUsuarios();
                pnlEditarFotoSeccion.Visible = false; // Asegurar que la sección de edición esté oculta al cargar
            }
        }

        // Carga los usuarios en el GridView, aplicando el filtro si existe.
        private void CargarUsuarios()
        {
            try
            {
                string filtroNombre = txtFiltroNombre.Text.Trim();
                DataTable dtUsuarios = SegUsuarioDAL.ObtenerTodosLosUsuarios(connectionString, filtroNombre);
                gvUsuarios.DataSource = dtUsuarios;
                gvUsuarios.DataBind();
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al cargar la lista de usuarios: " + ex.Message, false);
            }
        }

        // Evento Click del botón Filtrar
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            gvUsuarios.PageIndex = 0;
            CargarUsuarios();
            pnlEditarFotoSeccion.Visible = false; // Ocultar sección al filtrar
        }

        // Evento Click del botón Limpiar Filtro
        protected void btnLimpiarFiltro_Click(object sender, EventArgs e)
        {
            txtFiltroNombre.Text = string.Empty;
            gvUsuarios.PageIndex = 0;
            CargarUsuarios();
            pnlEditarFotoSeccion.Visible = false; // Ocultar sección al limpiar filtro
        }

        // Método auxiliar para obtener la URL completa de la imagen de perfil.
        protected string GetProfileImageUrl(object linkFotoData)
        {
            string linkFoto = null;
            if (linkFotoData != DBNull.Value)
            {
                linkFoto = Convert.ToString(linkFotoData);
            }

            if (!string.IsNullOrEmpty(linkFoto))
            {
                // Limpiar el inicio de la ruta si viene con ~ o /
                string cleanedLinkFoto = linkFoto.TrimStart('~', '/');
                string pathToCheck;

                // Comprobar si linkFoto ya incluye la carpeta base "Fotos/"
                if (cleanedLinkFoto.StartsWith(fotosPath.TrimStart('~', '/'), StringComparison.OrdinalIgnoreCase))
                {
                    // Ya la incluye, ej: "Fotos/usuario.png"
                    pathToCheck = "~/" + cleanedLinkFoto;
                }
                else
                {
                    // No la incluye, es solo el nombre del archivo, ej: "usuario.png"
                    pathToCheck = Path.Combine(fotosPath, cleanedLinkFoto);
                }

                if (File.Exists(Server.MapPath(pathToCheck)))
                {
                    return ResolveUrl(pathToCheck); // Resuelve la URL para el cliente
                }
            }
            // Si no hay foto específica, o el archivo no existe, o linkFoto es nulo/vacío, devolver la foto por defecto.
            return ResolveUrl(Path.Combine(fotosPath, defaultFoto));
        }

        // Evento RowDataBound para el GridView, para establecer la imagen de perfil.
        protected void gvUsuarios_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Image imgFotoGrid = (Image)e.Row.FindControl("imgFotoGrid");
                if (imgFotoGrid != null)
                {
                    object linkFotoValue = DataBinder.Eval(e.Row.DataItem, "LinkFoto");
                    imgFotoGrid.ImageUrl = GetProfileImageUrl(linkFotoValue);
                    // Fallback en caso de que la imagen no cargue en el cliente
                    imgFotoGrid.Attributes["onerror"] = string.Format("this.onerror=null;this.src='{0}';", ResolveUrl(Path.Combine(fotosPath, defaultFoto)));
                }
            }
        }

        // Maneja los comandos del GridView, principalmente "EditarFoto".
        protected void gvUsuarios_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditarFoto")
            {
                string idUsuario = e.CommandArgument.ToString();
                hfIdUsuarioModal.Value = idUsuario; // Usamos el mismo HiddenField

                try
                {
                    DataRow drUsuario = SegUsuarioDAL.ObtenerUsuarioPorId(idUsuario, connectionString);
                    if (drUsuario != null)
                    {
                        lblIdUsuarioModal.Text = drUsuario["ID_USUARIO"].ToString(); // Label para ID
                        lblNombreUsuarioModal.Text = $"{drUsuario["NOMBRE"]} {drUsuario["PRIMER_APELLIDO"]} {drUsuario["SEGUNDO_APELLIDO"]}".Trim(); // Label para Nombre

                        string imageUrl = GetProfileImageUrl(drUsuario["LinkFoto"]);
                        // Añadir un parámetro aleatorio para evitar problemas de caché del navegador
                        imageUrl += (imageUrl.Contains("?") ? "&" : "?") + "t=" + DateTime.Now.Ticks;
                        imgFotoActualModal.ImageUrl = imageUrl; // Imagen actual en la sección

                        imgNuevaFotoPreviewModal.ImageUrl = string.Empty; // Limpiar previsualización anterior
                        imgNuevaFotoPreviewModal.Visible = false;
                        fuNuevaFotoModal.Attributes.Clear(); // Limpiar FileUpload
                        if (fuNuevaFotoModal.PostedFile != null)
                        {
                            fuNuevaFotoModal.PostedFile.InputStream.Dispose();
                        }


                        pnlEditarFotoSeccion.Visible = true; // Mostrar la sección de edición
                        lblMensaje.Text = ""; // Limpiar mensajes anteriores
                        pnlMensaje.Visible = false;
                    }
                    else
                    {
                        MostrarMensaje("No se encontró el usuario seleccionado.", false);
                        pnlEditarFotoSeccion.Visible = false;
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al obtener datos del usuario: " + ex.Message, false);
                    pnlEditarFotoSeccion.Visible = false;
                }
            }
        }

        // Maneja el cambio de página en el GridView.
        protected void gvUsuarios_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUsuarios.PageIndex = e.NewPageIndex;
            CargarUsuarios();
            pnlEditarFotoSeccion.Visible = false; // Ocultar sección al cambiar de página
        }

        // Evento click del botón "Guardar Foto" en la sección de edición.
        // Renombrado de btnGuardarFotoModal_Click a btnGuardarFotoSeccion_Click
        protected void btnGuardarFotoSeccion_Click(object sender, EventArgs e)
        {
            if (fuNuevaFotoModal.HasFile)
            {
                try
                {
                    string idUsuario = hfIdUsuarioModal.Value;
                    if (string.IsNullOrEmpty(idUsuario))
                    {
                        MostrarMensaje("ID de usuario no encontrado. No se puede guardar la foto.", false);
                        pnlEditarFotoSeccion.Visible = true; // Mantener sección visible
                        return;
                    }

                    // Validar extensión del archivo
                    string extension = Path.GetExtension(fuNuevaFotoModal.FileName).ToLower();
                    if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
                    {
                        MostrarMensaje("Tipo de archivo no permitido. Solo se aceptan .png, .jpg o .jpeg.", false);
                        pnlEditarFotoSeccion.Visible = true; // Mantener sección visible
                        return;
                    }

                    // Crear directorio si no existe
                    string dirPath = Server.MapPath(fotosPath); // ej: "C:\inetpub\wwwroot\YourApp\Fotos\"
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }

                    // Eliminar foto anterior si no es la por defecto
                    DataRow drUsuarioActual = SegUsuarioDAL.ObtenerUsuarioPorId(idUsuario, connectionString);
                    if (drUsuarioActual != null && drUsuarioActual["LinkFoto"] != DBNull.Value)
                    {
                        string linkFotoAntiguaRelativa = drUsuarioActual["LinkFoto"].ToString();
                        if (!string.IsNullOrEmpty(linkFotoAntiguaRelativa) &&
                            !linkFotoAntiguaRelativa.EndsWith(defaultFoto, StringComparison.OrdinalIgnoreCase)) // No eliminar la foto por defecto
                        {
                            string pathFotoAntiguaAbsoluta;
                            string cleanedLinkFotoAntigua = linkFotoAntiguaRelativa.TrimStart('~', '/');

                            if (cleanedLinkFotoAntigua.StartsWith(fotosPath.TrimStart('~', '/'), StringComparison.OrdinalIgnoreCase))
                            {
                                pathFotoAntiguaAbsoluta = Server.MapPath("~/" + cleanedLinkFotoAntigua);
                            }
                            else
                            {
                                pathFotoAntiguaAbsoluta = Path.Combine(dirPath, Path.GetFileName(cleanedLinkFotoAntigua));
                            }

                            if (File.Exists(pathFotoAntiguaAbsoluta))
                            {
                                File.Delete(pathFotoAntiguaAbsoluta);
                            }
                        }
                    }

                    // Guardar nueva foto
                    string nuevoNombreArchivo = idUsuario + extension; // Nombre de archivo basado en ID de usuario para unicidad
                    string rutaCompletaNuevaFoto = Path.Combine(dirPath, nuevoNombreArchivo);
                    fuNuevaFotoModal.SaveAs(rutaCompletaNuevaFoto);

                    // Actualizar LinkFoto en la base de datos
                    // Guardar la ruta relativa desde la raíz de la aplicación, incluyendo la carpeta "Fotos"
                    string linkFotoParaBD = fotosPath.TrimStart('~').TrimStart('/') + nuevoNombreArchivo; // ej: "Fotos/eaguilar.png"
                    SegUsuarioDAL.ActualizarLinkFoto(idUsuario, linkFotoParaBD, connectionString);

                    MostrarMensaje("Foto de perfil actualizada exitosamente.", true);
                    CargarUsuarios(); // Recargar GridView para mostrar la nueva foto.
                    pnlEditarFotoSeccion.Visible = false; // Ocultar la sección de edición
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al guardar la foto: " + ex.Message, false);
                    pnlEditarFotoSeccion.Visible = true; // Mantener sección visible en caso de error
                }
            }
            else
            {
                MostrarMensaje("Por favor, seleccione un archivo de imagen.", false);
                pnlEditarFotoSeccion.Visible = true; // Mantener sección visible
            }
        }

        // Evento click del botón "Cancelar" en la sección de edición.
        protected void btnCancelarEdicionSeccion_Click(object sender, EventArgs e)
        {
            pnlEditarFotoSeccion.Visible = false; // Ocultar la sección de edición
            lblMensaje.Text = ""; // Limpiar mensajes
            pnlMensaje.Visible = false;
        }

        // Muestra un mensaje al usuario.
        private void MostrarMensaje(string mensaje, bool esExito)
        {
            lblMensaje.Text = Server.HtmlEncode(mensaje);
            pnlMensaje.CssClass = esExito ? "alert alert-success alert-dismissible fade show" : "alert alert-danger alert-dismissible fade show";
            pnlMensaje.Visible = true;
        }

        // La función FindControlRecursive ya no es necesaria ya que no usamos ScriptManager para mostrar/ocultar el modal.
        // Los controles de la sección son directamente accesibles.
    }
}
