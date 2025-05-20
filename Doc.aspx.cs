using System;
using System.Collections.Generic;
using System.Configuration; // Necesario para ConfigurationManager
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gestor_Desempeno
{
    public partial class Doc : System.Web.UI.Page
    {
        // Instancia del servicio web como miembro de la clase
        // Asegúrate de que la referencia al servicio (apivs2020) esté correctamente agregada a tu proyecto.
        public apivs2020.wsapi vs2020 = new apivs2020.wsapi();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Obtener los detalles de configuración para el servicio web
            string wsUsuario = ConfigurationManager.AppSettings["Usuario"];
            string wsClave = ConfigurationManager.AppSettings["Clave"];
            string wsArchivador = ConfigurationManager.AppSettings["Archivador"];
            string wsGaveta = ConfigurationManager.AppSettings["Gaveta"];

            // Intentar obtener el valor del parámetro 'num' de la QueryString (URL)
            string numValueFromUrl = Request.QueryString["num"];

            // Localiza el panel de mensajes y el literal del archivo .aspx
            // Asumiendo que tienes un Panel con ID "messagePanel" y un Literal con ID "litNumValue" en tu Doc.aspx
            var messagePanel = this.FindControl("messagePanel") as System.Web.UI.HtmlControls.HtmlGenericControl; // Si es un <div runat="server">
            var litMessage = this.FindControl("litNumValue") as Literal; // El Literal para el mensaje

            if (string.IsNullOrEmpty(numValueFromUrl))
            {
                ShowMessage("Error: Parámetro 'num' (ID de documento) no encontrado en la URL.", false, litMessage, messagePanel);
                return;
            }

            try
            {
                // Llamar al servicio web para obtener la URL del visor del documento
                string urlVisor = vs2020.Visor_Documento(wsUsuario, wsClave, wsArchivador, wsGaveta, "", numValueFromUrl);
                //string scripts = $"window.open('{urlVisor}', 'nuevaVentana', 'width=800,height=600,resizable=yes');";
                //ClientScript.RegisterStartupScript(this.GetType(), "abrirVentana", scripts, true);

                // Depuración: Registrar la URL obtenida
                //Debug.WriteLine($"Doc.aspx - URL Visor obtenida para num={numValueFromUrl}: {urlVisor}");
                Response.Redirect(urlVisor, true);
                if (!string.IsNullOrEmpty(urlVisor))
                {
                    // Preparar el script para abrir la URL en una nueva ventana Y LUEGO NAVEGAR HACIA ATRÁS con un pequeño retraso
                    // Escapar comillas simples en la URL por si acaso.
                    string safeUrlVisor = urlVisor.Replace("'", "\\'");

                    // *** IMPORTANTE: Revisa el bloqueador de pop-ups de tu navegador si la ventana no se abre. ***

                    // El script ahora abre la nueva ventana y luego, tras un breve retraso, redirige la ventana actual a Desempeno.aspx
                    string script = $"try {{ window.open('{safeUrlVisor}',  'nuevaVentana', 'width=800,height=600,resizable=yes'); }} catch(e) {{ console.error('Error al intentar abrir la ventana emergente:', e); alert('No se pudo abrir la ventana del documento. Por favor, revisa si tu navegador está bloqueando ventanas emergentes.'); }} setTimeout(function() {{ window.location.href='Desempeno.aspx'; }}, 150);"; // Retraso ligeramente aumentado a 150ms

                    // Registrar el script para que se ejecute en el cliente
                    ClientScript.RegisterStartupScript(this.GetType(), "AbrirVisorYRegresar", script, true);

                    // Mostrar un mensaje indicando que se abre la ventana y se regresará
                    ShowMessage("Abriendo el documento en una nueva ventana... Serás redirigido de vuelta a la página anterior. (Si no ves la nueva ventana, revisa el bloqueador de pop-ups).", true, litMessage, messagePanel);
                }
                else
                {
                    ShowMessage("Error: No se pudo obtener la URL del visor para el documento (URL vacía retornada). Verifica el ID del documento o la configuración del servicio.", false, litMessage, messagePanel);
                }
            }
            catch (System.Web.Services.Protocols.SoapException soapEx)
            {
                // Manejo específico para errores del servicio web (SOAP)
                ShowMessage($"Error al comunicar con el servicio de documentos (SOAP): {Server.HtmlEncode(soapEx.Message)}", false, litMessage, messagePanel);
                // Considera loggear soapEx.Detail o soapEx.ToString() para más información en el servidor.
            }
            catch (Exception ex)
            {
                // Manejo de otros errores generales
                ShowMessage($"Error general al procesar la solicitud: {Server.HtmlEncode(ex.Message)}", false, litMessage, messagePanel);
                // Considera loggear ex.ToString() para detalles completos del error en el servidor.
            }
        }

        /// <summary>
        /// Muestra un mensaje en el control Literal y ajusta el estilo del panel de mensajes.
        /// </summary>
        /// <param name="message">El mensaje a mostrar.</param>
        /// <param name="isSuccess">True si es un mensaje de éxito, false si es de error.</param>
        /// <param name="literalControl">El control Literal donde se mostrará el mensaje.</param>
        /// <param name="panelControl">El panel que contiene el Literal, para cambiar su clase CSS.</param>
        private void ShowMessage(string message, bool isSuccess, Literal literalControl, System.Web.UI.HtmlControls.HtmlGenericControl panelControl)
        {
            if (literalControl != null)
            {
                literalControl.Text = message;
            }
            else // Fallback si el Literal no se encuentra por alguna razón
            {
                // Usar Response.Write puede interferir con el script si se ejecuta antes.
                // Es mejor asegurarse de que los controles existan en el ASPX.
                System.Diagnostics.Debug.WriteLine($"Advertencia: Control Literal no encontrado para mostrar mensaje: {message}");
            }

            if (panelControl != null)
            {
                if (isSuccess)
                {
                    panelControl.Attributes["class"] = "alert alert-success"; // O 'alert-info'
                }
                else
                {
                    panelControl.Attributes["class"] = "alert alert-danger";
                }
                panelControl.Visible = true; // Asegurarse de que el panel sea visible
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Advertencia: Control de Panel de Mensajes no encontrado.");
            }
        }
    }
}