using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gestor_Desempeno
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Podrías agregar lógica aquí si es necesario, como limpiar mensajes de error
            if (!IsPostBack)
            {
                litError.Visible = false;
            }
        }

        protected void btnIngresar_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                string idUsuario = txtUsuario.Text.Trim();
                string clave = txtClave.Text;

                UsuarioDAL dal = new UsuarioDAL();
                UsuarioInfo usuarioInfo = dal.ObtenerInfoUsuario(idUsuario); // Asumo que UsuarioInfo tiene propiedades como NombreCompleto, etc.

                bool autenticadoConExito = false; // Cambiado el nombre de la variable para claridad

                if (usuarioInfo == null)
                {
                    MostrarError("Usuario o contraseña incorrectos.");
                    return;
                }

                if (!usuarioInfo.EstaActivo)
                {
                    MostrarError("La cuenta de usuario está inactiva.");
                    return;
                }

                if (usuarioInfo.EsExterno)
                {
                    // ADVERTENCIA: Comparación insegura de texto plano. DEBES CAMBIAR ESTO a HASHING.
                    if (usuarioInfo.ContrasenaAlmacenada != null && usuarioInfo.ContrasenaAlmacenada == clave)
                    {
                        autenticadoConExito = true;
                    }
                    else
                    {
                        MostrarError("Usuario o contraseña incorrectos.");
                        return;
                    }
                }
                else // Usuario INTERNO
                {
                    autenticadoConExito = dal.ValidarConActiveDirectory(idUsuario, clave);
                    if (!autenticadoConExito)
                    {
                        MostrarError("Usuario o contraseña incorrectos. Verifique sus credenciales de red.");
                        return;
                    }
                }

                // --- SI LA AUTENTICACIÓN FUE EXITOSA ---
                if (autenticadoConExito)
                {
                    // 1. Configurar la sesión como ya lo haces (o como necesites)
                    Session["UsuarioID"] = usuarioInfo.IdUsuario; // O la propiedad correcta de usuarioInfo que contiene el ID
                    Session["NombreCompletoUsuario"] = usuarioInfo.NombreCompleto; // Asumiendo que tienes esta propiedad
                    Session["EsExterno"] = usuarioInfo.EsExterno;
                    // ... cualquier otra variable de sesión que necesites ...


                    // 2. Establecer la cookie de Forms Authentication
                    // El primer parámetro es el nombre que se mostrará en asp:LoginName.
                    // Puedes usar idUsuario o un nombre más descriptivo si lo tienes (ej: usuarioInfo.NombreCompleto)
                    string nombreParaLoginName = usuarioInfo.NombreCompleto; // O idUsuario si prefieres
                    if (string.IsNullOrEmpty(nombreParaLoginName))
                    {
                        nombreParaLoginName = idUsuario; // Fallback si NombreCompleto está vacío
                    }

                    bool esPersistente = false; // O true si tienes un checkbox "Recordarme"
                    FormsAuthentication.SetAuthCookie(nombreParaLoginName, esPersistente);


                    // 3. Actualizar el estado de la sesión en la BD (si es necesario)
                    //    Si tienes un campo como "ESTA_SESION" o "ULTIMO_LOGIN"
                    //dal.ActualizarEstadoSesionUsuario(usuarioInfo.IdUsuario, true); // Ejemplo

                    
                    // 4. Redirigir
                    if (usuarioInfo.NecesitaCambiarContrasena)
                    {
                        // Guardar el ID de usuario en sesión temporal para el cambio de contraseña
                        Session["UsuarioCambioContrasenaID"] = usuarioInfo.IdUsuario;
                        Response.Redirect("~/CambiarContrasena.aspx", false); // false para evitar ThreadAbortException
                        Context.ApplicationInstance.CompleteRequest(); // Necesario después de Response.Redirect con false
                    }
                    else
                    {
                        // Redirige a la página de inicio o a la URL solicitada originalmente
                        // Response.Redirect(FormsAuthentication.GetRedirectUrl(nombreParaLoginName, esPersistente));
                        // O una redirección directa:
                        Response.Redirect("~/Default.aspx", false); // false para evitar ThreadAbortException
                        Context.ApplicationInstance.CompleteRequest(); // Necesario después de Response.Redirect con false
                    }
                }
                // No necesitas un 'else' aquí porque los casos de fallo ya hacen 'return'
            }
        }


        private void MostrarError(string mensaje)
        {
            // Suponiendo que tienes un Label o un Literal para mostrar errores
            // lblErrorLogin.Text = mensaje;
            // lblErrorLogin.Visible = true;
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", $"alert('{mensaje.Replace("'", "\\'")}');", true);
        }


    }
}