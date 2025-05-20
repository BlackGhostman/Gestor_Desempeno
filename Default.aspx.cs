using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gestor_Desempeno
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("~/Desempeno.aspx");
            // --- LA VERIFICACIÓN DE SESIÓN Y CAMBIO DE CONTRASEÑA AHORA ESTÁ EN Site.Master.cs ---
            // --- Y TAMBIÉN DEBERÍA ESTAR AQUÍ COMO DOBLE VERIFICACIÓN ---

            // 1. Verificar si hay sesión (Doble chequeo)
            if (Session["UsuarioID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return; // Salir
            }

            // 2. Verificar si necesita cambiar contraseña (Doble chequeo)
            bool necesitaCambiar = false;
            string idUsuario = Session["UsuarioID"].ToString();
            UsuarioDAL dal = new UsuarioDAL(); // Necesitas una instancia de tu DAL
            UsuarioInfo infoActual = dal.ObtenerInfoUsuario(idUsuario); // Obtener info actual
            if (infoActual != null && infoActual.NecesitaCambiarContrasena)
            {
                necesitaCambiar = true;
            }

            if (necesitaCambiar)
            {
                Response.Redirect("~/CambiarContrasena.aspx");
                return; // Salir
            }

            // Si llegó aquí, está logueado y no necesita cambiar contraseña
            if (!IsPostBack)
            {
                // Cargar datos de la página
                // El control LoginName en la Master Page mostrará el nombre automáticamente.
                // Puedes mostrar un mensaje de bienvenida adicional si quieres:
                lblBienvenida.Text = $"¡Bienvenido de nuevo, {idUsuario}!"; // Usar el ID de sesión
            }
        }

        // Si quitaste el botón btnLogout de Default.aspx, puedes eliminar este método:
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }
}