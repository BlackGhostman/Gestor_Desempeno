using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gestor_Desempeno
{
    public partial class CambiarContrasena : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Verificar si el usuario está logueado. Si no, redirigir a Login.
            if (Session["UsuarioID"] == null)
            {
                Response.Redirect("~/Login.aspx?mensaje=SesionExpirada");
            }
            if (!IsPostBack)
            {
                litMensajeCambio.Visible = false;
                hlVolverLoginLink.Visible = false;
            }
        }

        protected void btnCambiarClave_Click(object sender, EventArgs e)
        {
            // Doble verificación de sesión por si acaso
            if (Session["UsuarioID"] == null)
            {
                Response.Redirect("~/Login.aspx?mensaje=SesionExpirada");
                return;
            }

            if (Page.IsValid) // Verifica los validadores del grupo "CambioPass"
            {
                string idUsuario = Session["UsuarioID"].ToString();
                string nuevaClave = txtNuevaClave.Text; // ¡Texto plano!

                // Opcional: Validar complejidad de la contraseña aquí si es necesario
                // if (nuevaClave.Length < 8 || ...) { MostrarMensaje("La contraseña no cumple los requisitos...", false); return; }

                UsuarioDAL dal = new UsuarioDAL();
                // ADVERTENCIA: Guarda la contraseña nueva en texto plano y desactiva el flag DebeCambiarContrasena
                bool actualizado = dal.ActualizarContrasenaYDesactivarForzado(idUsuario, nuevaClave);

                if (actualizado)
                {
                    MostrarMensaje("Su contraseña ha sido actualizada correctamente. Ahora puede continuar.", true);
                    // Ocultar campos y botón, mostrar enlace a Default
                    txtNuevaClave.Enabled = false;
                    txtConfirmarClave.Enabled = false;
                    btnCambiarClave.Visible = false;
                    // Podrías redirigir a Default.aspx o mostrar un enlace
                    // Response.Redirect("~/Default.aspx");
                    hlVolverLoginLink.Text = "Ir a la página principal";
                    hlVolverLoginLink.NavigateUrl = "~/Default.aspx";
                    hlVolverLoginLink.Visible = true;
                }
                else
                {
                    MostrarMensaje("Error al actualizar la contraseña. Por favor, intente de nuevo.", false);
                    hlVolverLoginLink.Text = "Volver a Inicio de Sesión";
                    hlVolverLoginLink.NavigateUrl = "~/Login.aspx";
                    hlVolverLoginLink.Visible = true;
                }
            }
        }

        private void MostrarMensaje(string texto, bool esExito)
        {
            litMensajeCambio.Text = $"<div class='alert alert-{(esExito ? "success" : "danger")} mt-3'>{texto}</div>";
            litMensajeCambio.Visible = true;
        }
    }
}