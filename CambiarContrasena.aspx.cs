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

        // En CambiarContrasena.aspx.cs
        protected void btnCambiarClave_Click(object sender, EventArgs e)
        {
            if (Session["UsuarioID"] == null)
            {
                Response.Redirect("~/Login.aspx?mensaje=SesionExpirada");
                return;
            }

            if (Page.IsValid)
            {
                string idUsuario = Session["UsuarioID"].ToString();
                string nuevaClave = txtNuevaClave.Text;
                string contrasenaActual = txtContrasenaActual.Text; // Asumiendo que tienes este TextBox en tu .aspx

                UsuarioDAL dal = new UsuarioDAL();
                UsuarioInfo infoUsuario = dal.ObtenerInfoUsuario(idUsuario);

                bool actualizado = false;

                if (infoUsuario != null && !infoUsuario.EsExterno) // Usuario de Active Directory
                {
                    try
                    {
                        // --- INICIO CORRECCIÓN ERROR 1 ---
                        if (string.IsNullOrWhiteSpace(contrasenaActual)) // Quitamos los () de contrasenaActual
                        {
                            MostrarMensaje("Debe ingresar su contraseña actual.", false);
                            return;
                        }
                        // --- FIN CORRECCIÓN ERROR 1 ---

                        actualizado = dal.CambiarContrasenaActiveDirectory(idUsuario, contrasenaActual, nuevaClave);

                        if (actualizado)
                        {
                            // --- VERIFICAR QUE MÉTODO EXISTA (Error 2) ---
                            // Esta línea asume que MarcarUsuarioParaCambioContrasenaEnDB está en UsuarioDAL.cs
                            dal.MarcarUsuarioParaCambioContrasenaEnDB(idUsuario, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje("Error al cambiar contraseña: " + ex.Message, false);
                        return;
                    }
                }
                else if (infoUsuario != null && infoUsuario.EsExterno)
                {
                    // ... (tu lógica para usuarios externos, idealmente con HASHING) ...
                    // IMPORTANTE: Implementar HASHING aquí antes de guardar
                    // string hashNuevaClave = GenerarHash(nuevaClave); // Método para hashear
                    // actualizado = dal.ActualizarContrasenaYDesactivarForzado(idUsuario, hashNuevaClave); // Usar el método que guarda hasheado
                    MostrarMensaje("ADVERTENCIA DE SEGURIDAD: Las contraseñas de usuarios externos se guardan en texto plano. Esto debe corregirse.", false);
                    actualizado = dal.ActualizarContrasenaYDesactivarForzado(idUsuario, nuevaClave); // ¡NO SEGURO! ¡SOLO PARA PRUEBAS HASTA QUE IMPLEMENTES HASHING!
                    if (actualizado)
                    {
                        // Para usuarios externos, ActualizarContrasenaYDesactivarForzado ya actualiza el flag.
                    }
                }
                else
                {
                    MostrarMensaje("No se pudo encontrar la información del usuario.", false);
                    return;
                }

                if (actualizado)
                {
                    MostrarMensaje("Su contraseña ha sido actualizada correctamente. Ahora puede continuar.", true);
                    txtNuevaClave.Enabled = false;
                    txtConfirmarClave.Enabled = false;
                    // txtContrasenaActual.Enabled = false; // Si lo agregaste
                    btnCambiarClave.Visible = false;
                    hlVolverLoginLink.Text = "Ir a la página principal";
                    hlVolverLoginLink.NavigateUrl = "~/Default.aspx"; // O la página que corresponda
                    hlVolverLoginLink.Visible = true;
                }
                else
                {
                    // Si no se actualizó y no fue por una excepción ya manejada (ej. error genérico para externos)
                    if (infoUsuario != null && infoUsuario.EsExterno)
                    {
                        MostrarMensaje("Error al actualizar la contraseña. Por favor, intente de nuevo.", false);
                    }
                    // Para usuarios de AD, el error específico ya se mostró en el catch.
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