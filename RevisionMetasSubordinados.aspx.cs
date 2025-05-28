using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Data;
using System.Configuration;

namespace Gestor_Desempeno
{
    public partial class RevisionMetasSubordinados : System.Web.UI.Page
    {
        private const int ID_ESTADO_PENDIENTE_REVISION_JEFE = 13; // Metas que el jefe debe revisar
        private const int ID_ESTADO_DEVUELTA_A_ACTIVO = 7;      // Nuevo estado si el jefe devuelve
        private const int ID_ESTADO_GESTION_COMPLETADA = 12;    // Nuevo estado si el jefe completa/aprueba

        private MetaIndividualDAL metaIndDAL = new MetaIndividualDAL();
        private RespuestaDAL respuestaDAL = new RespuestaDAL(); // Para el historial de respuestas
        private LogRevisionMetaDAL logRevisionDAL = new LogRevisionMetaDAL(); // Nueva DAL para la tabla Log_Revision_Meta
        public wsRRHH.WS_RecursosHumanos apiRH = new wsRRHH.WS_RecursosHumanos();

        public apivs2020.wsapi vs2020Service = new apivs2020.wsapi();


        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UsuarioID"] == null) { Response.Redirect("~/Login.aspx?mensaje=SesionExpirada"); return; }
            if (!IsPostBack)
            {
                CargarMetasParaRevision();
            }
        }


        // En RevisionMetasSubordinados.aspx.cs

        private void CargarMetasParaRevision()
        {
            string idUsuarioJefe = Session["UsuarioID"].ToString();
            List<string> listaSubordinados = ObtenerListaSubordinados(idUsuarioJefe);

            if (listaSubordinados.Any())
            {
                List<MetaIndividualInfo> metasPendientes = metaIndDAL.ObtenerMetasIndividualesPorUsuariosYEstado(
                    listaSubordinados,
                    ID_ESTADO_PENDIENTE_REVISION_JEFE
                );

                var viewModelList = new List<MetaRevisionViewModel>(); // O la lista que estés usando
                foreach (var meta in metasPendientes)
                {
                    var vm = new MetaRevisionViewModel(meta);
                    vm.RespondidaFueraDeTiempo = DeterminarSiRespondioTarde(meta.IdMetaIndividual, meta.FechaFinal);
                    viewModelList.Add(vm);
                }

                rptMetasRespondidasParaRevision.DataSource = viewModelList;
                rptMetasRespondidasParaRevision.DataBind();

                // Lógica para mostrar el mensaje si no hay datos
                if (viewModelList.Any())
                {
                    rptMetasRespondidasParaRevision.Visible = true;
                    pnlNoHayMetasParaRevision.Visible = false;
                }
                else
                {
                    rptMetasRespondidasParaRevision.Visible = false;
                    pnlNoHayMetasParaRevision.Visible = true; // Mostrar el panel con el mensaje
                }
            }
            else
            {
                rptMetasRespondidasParaRevision.DataSource = null; // Limpiar por si acaso
                rptMetasRespondidasParaRevision.DataBind();
                rptMetasRespondidasParaRevision.Visible = false;
                pnlNoHayMetasParaRevision.Visible = true; // Mostrar el panel
                                                          // Podrías cambiar el texto del panel aquí si es por "no hay subordinados"
                                                          // ((Label)pnlNoHayMetasParaRevision.FindControl("id_de_un_label_dentro_del_panel")).Text = "No tiene subordinados...";
            }
        }


        private List<string> ObtenerListaSubordinados(string idUsuarioJefe)
        {
            List<string> subordinados = new List<string>();
            try
            {
                System.Data.DataTable dtColaboradores = apiRH.ObtenerColaboradoresUsuarioJefe(idUsuarioJefe);
                // ¡VERIFICA EL NOMBRE DE ESTA COLUMNA!
                string nombreColumnaUsuario = "Id_Colaborador"; // O como se llame en tu DataTable
                if (dtColaboradores != null)
                {
                    foreach (DataRow row in dtColaboradores.Rows)
                    {
                        subordinados.Add(row[nombreColumnaUsuario].ToString());
                    }
                }
            }
            catch (Exception ex) { /* Manejar error, log */ }
            return subordinados;
        }

        private bool DeterminarSiRespondioTarde(int idMetaIndividual, DateTime? fechaFinalMeta)
        {
            if (!fechaFinalMeta.HasValue) return false; // No hay fecha final, no puede estar tarde

            // Necesitas una forma de obtener la fecha de la ÚLTIMA respuesta del subordinado
            // que llevó la meta al estado 13.
            // Esto podría ser la fecha de la respuesta en la tabla Respuesta que tiene Id_Detalle_Estado = 11 (Respondido para clase Respuesta)
            // y Codigo_Semana IS NULL o "0000000" (si era vencida).
            DateTime? fechaUltimaRespuesta = respuestaDAL.ObtenerFechaUltimaRespuestaSubordinado(idMetaIndividual); // Necesitarás este método en RespuestaDAL

            return fechaUltimaRespuesta.HasValue && fechaUltimaRespuesta.Value.Date > fechaFinalMeta.Value.Date;
        }


        [System.Web.Services.WebMethod(EnableSession = true)]
        public static object GetHistorialRespuestasMeta(int metaId)
        {
            try
            {
                RespuestaDAL localRespuestaDAL = new RespuestaDAL();
                List<RespuestaInfo> historial = localRespuestaDAL.ObtenerHistorialRespuestas(metaId); // Asume que este método ya existe y funciona

                if (historial != null && historial.Any())
                {
                    // Instanciar el servicio para documentos una sola vez
                    apivs2020.wsapi servicioVs2020 = new apivs2020.wsapi();
                    string wsUsuario = ConfigurationManager.AppSettings["Usuario"];
                    string wsClave = ConfigurationManager.AppSettings["Clave"];
                    string wsArchivador = ConfigurationManager.AppSettings["Archivador"];
                    string wsGaveta = ConfigurationManager.AppSettings["Gaveta"];
                    string wsLlave = ConfigurationManager.AppSettings["Llave"];

                    // Iterar sobre cada respuesta en el historial para obtener sus documentos
                    foreach (RespuestaInfo respuestaItem in historial)
                    {
                        if (respuestaItem.IdRespuesta > 0) // Solo si hay un IdRespuesta válido
                        {
                            string datosDocsParaEsteItem = "";
                            string idCarpetaRespuestaWs = "";
                            string nombreCarpetaWs = respuestaItem.IdRespuesta.ToString();

                            // 1. Obtener ID de la carpeta (o crearla)
                            DataSet dsCarpetaInfo = null;
                            try
                            {
                                dsCarpetaInfo = servicioVs2020.Devuelve_Ids_Gaveta_Carpeta(wsUsuario, wsClave, wsArchivador, wsGaveta, wsLlave, nombreCarpetaWs);
                            }
                            catch (Exception exServicio)
                            {
                                Console.WriteLine($"Docs Historial: Excepción Devuelve_Ids_Gaveta_Carpeta para carpeta '{nombreCarpetaWs}' (RespuestaID: {respuestaItem.IdRespuesta}): {exServicio.Message}");
                            }

                            if (dsCarpetaInfo != null && dsCarpetaInfo.Tables.Count > 0 && dsCarpetaInfo.Tables[0].Rows.Count > 0)
                            {
                                if (dsCarpetaInfo.Tables[0].Rows.Count > 1 && dsCarpetaInfo.Tables[0].Columns.Contains("Valor"))
                                    idCarpetaRespuestaWs = dsCarpetaInfo.Tables[0].Rows[1]["Valor"]?.ToString();
                                else if (dsCarpetaInfo.Tables[0].Columns.Contains("Valor"))
                                    idCarpetaRespuestaWs = dsCarpetaInfo.Tables[0].Rows[0]["Valor"]?.ToString();
                            }

                            if (string.IsNullOrEmpty(idCarpetaRespuestaWs))
                            {
                                try
                                {
                                    idCarpetaRespuestaWs = servicioVs2020.Insertar_Carpeta(wsUsuario, wsClave, wsArchivador, wsGaveta, wsLlave, nombreCarpetaWs);
                                    if (string.IsNullOrEmpty(idCarpetaRespuestaWs))
                                        Console.WriteLine($"Docs Historial: Falló creación de carpeta '{nombreCarpetaWs}' (RespuestaID: {respuestaItem.IdRespuesta}).");
                                }
                                catch (Exception exCrear)
                                {
                                    Console.WriteLine($"Docs Historial: Excepción Insertar_Carpeta '{nombreCarpetaWs}' (RespuestaID: {respuestaItem.IdRespuesta}): {exCrear.Message}");
                                }
                            }

                            // 2. Listar documentos de la carpeta
                            if (!string.IsNullOrEmpty(idCarpetaRespuestaWs))
                            {
                                DataSet dsDocumentos = null;
                                try
                                {
                                    dsDocumentos = servicioVs2020.Lista_De_Documentos(idCarpetaRespuestaWs);
                                }
                                catch (Exception exListar)
                                {
                                    Console.WriteLine($"Docs Historial: Excepción Lista_De_Documentos carpetaID '{idCarpetaRespuestaWs}' (RespuestaID: {respuestaItem.IdRespuesta}): {exListar.Message}");
                                }

                                if (dsDocumentos != null && dsDocumentos.Tables.Count > 0 && dsDocumentos.Tables[0].Rows.Count > 0)
                                {
                                    DataTable dtDocs = dsDocumentos.Tables[0];
                                    foreach (DataRow row in dtDocs.Rows)
                                    {
                                        string ideObj = row.Table.Columns.Contains("ide_obj") ? row["ide_obj"]?.ToString() : null;
                                        string desObj = row.Table.Columns.Contains("des_obj") ? row["des_obj"]?.ToString() : null;
                                        if (!string.IsNullOrEmpty(ideObj) && !string.IsNullOrEmpty(desObj))
                                        {
                                            datosDocsParaEsteItem += $"Doc.aspx?num={HttpUtility.UrlEncode(ideObj)},{HttpUtility.HtmlEncode(desObj)}|";
                                        }
                                    }
                                    // Asignar la cadena de documentos al item del historial correspondiente
                                    respuestaItem.DatosDocs = datosDocsParaEsteItem.TrimEnd('|');
                                }
                            }
                        }
                    }
                }
                // Devolver la lista de historial, donde cada RespuestaInfo ahora puede tener su propiedad DatosDocs poblada.
                return new { success = true, data = historial };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error CRÍTICO en WebMethod GetHistorialRespuestasMeta (MetaID: {metaId}): {ex.ToString()}");
                return new { success = false, message = "Error crítico al obtener historial y documentos." };
            }
        }

        // En RevisionMetasSubordinados.aspx.cs

        protected void btnDevolverMeta_Click(object sender, EventArgs e)
        {
            int metaId = 0;
            bool parseIdOk = int.TryParse(hfSelectedMetaIdRevision.Value, out metaId);
            string comentarioJefe = txtComentarioJefeModal.Value; // Textarea del modal

            if (!parseIdOk || metaId == 0)
            {
                MostrarMensajeGeneral("Error: No se pudo identificar la meta seleccionada.", false);
                // Podrías querer mantener el modal abierto aquí si es un error recuperable
                return;
            }

            if (string.IsNullOrWhiteSpace(comentarioJefe))
            {
                // Mostrar error: el comentario es obligatorio para devolver
                // Este mensaje podría ir a un Literal DENTRO del UpdatePanelModal
                // Ejemplo: litModalError.Text = "<div class='alert alert-danger'>El comentario es obligatorio.</div>";
                // Y luego asegurar que el modal se muestre de nuevo si es necesario.
                // Por ahora, asumimos que la validación OnClientClick ya previno esto o se muestra el error y el modal permanece.
                // Si el postback ocurre, y esta validación server-side falla, necesitas mantener el modal:
                ScriptManager.RegisterStartupScript(UpdatePanelModal, UpdatePanelModal.GetType(), "ShowModalOnError", "if(revisionModal) { revisionModal.show(); }", true);
                litMensajeGeneral.Text = "<div class='alert alert-warning'>El comentario es obligatorio para devolver la meta.</div>"; // O un literal dentro del modal
                return;
            }

            bool actualizacionOk = metaIndDAL.ActualizarEstadoMetaIndividual(metaId, ID_ESTADO_DEVUELTA_A_ACTIVO);
            if (actualizacionOk)
            {
                logRevisionDAL.GuardarLogRevision(metaId, Session["UsuarioID"].ToString(), comentarioJefe, ID_ESTADO_PENDIENTE_REVISION_JEFE, ID_ESTADO_DEVUELTA_A_ACTIVO, "Devuelta");
                MostrarMensajeGeneral("Meta devuelta correctamente al colaborador.", true); // Usa tu método para mostrar mensajes
                CargarMetasParaRevision(); // Esto recarga la lista, que podría quedar vacía

                // Script para cerrar el modal y limpiar el backdrop
                string script = "if (typeof revisionModal !== 'undefined' && revisionModal !== null) { revisionModal.hide(); } " +
                                "$('body').removeClass('modal-open'); " +
                                "$('.modal-backdrop').remove();";
                ScriptManager.RegisterStartupScript(UpdatePanelModal, UpdatePanelModal.GetType(), "CloseRevisionModalScript", script, true);
            }
            else
            {
                MostrarMensajeGeneral("Error: No se pudo devolver la meta.", false);
                // Mantener el modal abierto para que el usuario vea el error o intente de nuevo
                ScriptManager.RegisterStartupScript(UpdatePanelModal, UpdatePanelModal.GetType(), "ShowModalOnErrorDB", "if(revisionModal) { revisionModal.show(); }", true);
            }
        }

        protected void btnCompletarGestion_Click(object sender, EventArgs e)
        {
            int metaId = 0;
            bool parseIdOk = int.TryParse(hfSelectedMetaIdRevision.Value, out metaId);
            string comentarioJefe = txtComentarioJefeModal.Value; // Opcional

            if (!parseIdOk || metaId == 0)
            {
                MostrarMensajeGeneral("Error: No se pudo identificar la meta seleccionada.", false);
                return;
            }

            bool actualizacionOk = metaIndDAL.ActualizarEstadoMetaIndividual(metaId, ID_ESTADO_GESTION_COMPLETADA);
            if (actualizacionOk)
            {
                logRevisionDAL.GuardarLogRevision(metaId, Session["UsuarioID"].ToString(), comentarioJefe, ID_ESTADO_PENDIENTE_REVISION_JEFE, ID_ESTADO_GESTION_COMPLETADA, "Completada");
                MostrarMensajeGeneral("Gestión de la meta completada exitosamente.", true);
                CargarMetasParaRevision();

                // Script para cerrar el modal y limpiar el backdrop
                string script = "if (typeof revisionModal !== 'undefined' && revisionModal !== null) { revisionModal.hide(); } " +
                                "$('body').removeClass('modal-open'); " +
                                "$('.modal-backdrop').remove();";
                ScriptManager.RegisterStartupScript(UpdatePanelModal, UpdatePanelModal.GetType(), "CloseRevisionModalScript", script, true);
            }
            else
            {
                MostrarMensajeGeneral("Error: No se pudo completar la gestión de la meta.", false);
                ScriptManager.RegisterStartupScript(UpdatePanelModal, UpdatePanelModal.GetType(), "ShowModalOnErrorDB", "if(revisionModal) { revisionModal.show(); }", true);
            }
        }

        // Método para mostrar mensajes (asumiendo que tienes litMensajeGeneral en tu UpdatePanelPagina)
        private void MostrarMensajeGeneral(string mensaje, bool esExito)
        {
            string cssClass = esExito ? "alert-success" : "alert-danger";
            litMensajeGeneral.Text = $"<div class='alert {cssClass} alert-dismissible fade show' role='alert'>{Server.HtmlEncode(mensaje)}<button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button></div>";
        }




    }


    public class MetaRevisionViewModel
    {
        public MetaIndividualInfo Meta { get; set; }
        public bool RespondidaFueraDeTiempo { get; set; }
        // Otros campos que necesites específicamente para la vista de revisión

        public MetaRevisionViewModel(MetaIndividualInfo meta)
        {
            this.Meta = meta;
            // this.RespondidaFueraDeTiempo se calculará en CargarMetasParaRevision
        }
    }
}