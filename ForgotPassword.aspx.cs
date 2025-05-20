using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Gestor_Desempeno.CorreoServiceReference;

namespace Gestor_Desempeno
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                litMensaje.Visible = false;
            }
        }

        protected void btnEnviarCorreo_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                string idUsuario = txtUsuarioRecuperacion.Text.Trim();
                UsuarioDAL dal = new UsuarioDAL();
                UsuarioInfo usuarioInfo = dal.ObtenerInfoUsuario(idUsuario);

                // Validar que el usuario exista, esté activo y tenga correo
                if (usuarioInfo == null || !usuarioInfo.EstaActivo || string.IsNullOrEmpty(usuarioInfo.Correo))
                {
                    // Mostrar mensaje genérico por seguridad
                    MostrarMensaje("Si su usuario existe, está activo y tiene un correo registrado, recibirá instrucciones en breve.", true);
                    return;
                }

                string asunto = "";
                string mensaje = "";

                try
                {
                    WebService1 clienteCorreo = new WebService1();

                    if (!usuarioInfo.EsExterno)
                    {
                        // Usuario INTERNO (AD)
                        asunto = "Solicitud de Recuperación de Contraseña - Municipalidad de Curridabat";
                        mensaje = $"Hola {idUsuario},\n\nRecibimos una solicitud para restablecer la contraseña de su cuenta asociada a la Municipalidad de Curridabat.\n\n" +
                                  "Dado que su cuenta es administrada a través de la red interna, por favor diríjase al departamento de Informática de la Municipalidad para que le asistan con el cambio de su contraseña.\n\n" +
                                  "Si usted no realizó esta solicitud, por favor contacte a Informática.\n\n" +
                                  "Saludos,\nMunicipalidad de Curridabat";

                        // Enviar correo informativo
                        clienteCorreo.enviarCorreoDefault(asunto, mensaje, usuarioInfo.Correo);
                        MostrarMensaje("Se han enviado instrucciones a su correo electrónico registrado.", true);
                    }
                    else
                    {
                        // Usuario EXTERNO (BD)
                        asunto = "Restablecimiento de Contraseña Temporal - Municipalidad de Curridabat";
                        string contrasenaTemporal = dal.GenerarContrasenaTemporal(); // Generar contraseña

                        // Actualizar la BD con la contraseña temporal y el flag
                        bool actualizado = dal.ActualizarContrasenaYTempora(idUsuario, contrasenaTemporal);

                        if (actualizado)
                        {
                            mensaje = $"Hola {idUsuario},\n\nSe ha generado una contraseña temporal para su cuenta en el sistema de la Municipalidad de Curridabat.\n\n" +
                                      $"Su contraseña temporal es: {contrasenaTemporal}\n\n" + // ¡Incluir la contraseña temporal!
                                      "Por favor, inicie sesión con esta contraseña. El sistema le solicitará que establezca una nueva contraseña permanente de inmediato.\n\n" +
                                      "Si usted no solicitó este cambio, por favor contacte al soporte.\n\n" +
                                      "Saludos,\nMunicipalidad de Curridabat";

                            // Enviar correo con la contraseña temporal
                            clienteCorreo.enviarCorreoDefault(asunto, mensaje, usuarioInfo.Correo);
                            MostrarMensaje("Se ha enviado una contraseña temporal a su correo electrónico.", true);
                        }
                        else
                        {
                            MostrarMensaje("Ocurrió un error al intentar restablecer la contraseña. Por favor, intente de nuevo más tarde o contacte a soporte.", false);
                            // Registrar error de actualización de BD
                            Console.WriteLine($"Error al actualizar contraseña temporal para usuario {idUsuario}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error al procesar su solicitud. Por favor, intente de nuevo más tarde.", false);
                    Console.WriteLine($"Error en ForgotPassword.aspx.cs: {ex.ToString()}");
                }
            }
        }

        private void MostrarMensaje(string texto, bool esExito)
        {
            litMensaje.Text = $"<div class='alert alert-{(esExito ? "success" : "danger")} mt-3'>{texto}</div>";
            litMensaje.Visible = true;
        }
    }
}