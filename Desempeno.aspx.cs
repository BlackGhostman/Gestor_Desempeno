using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services; // Needed for WebMethod
using System.Globalization; // Needed for CalendarWeekRule
using System.Web.Script.Serialization; // Needed for JSON response
using System.Configuration;
using System.Data;

namespace Gestor_Desempeno
{
    public class DocumentoRespuestaInfo
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Url { get; set; }
        public string TipoExtension { get; set; }
        public string TamanoMB { get; set; }
        public string FechaArchivado { get; set; }
    }

    public partial class Desempeno : System.Web.UI.Page
    {
        // DAL Instances are now instance members
        private MetaIndividualDAL metaIndDAL = new MetaIndividualDAL();
        private RespuestaDAL respuestaDAL = new RespuestaDAL();
        private DetalleEstadoDAL detalleEstadoDAL = new DetalleEstadoDAL();
        private UsuarioDAL usuarioDAL_Instance = new UsuarioDAL();


        // Constants for Estado IDs
        private const int ID_ESTADO_RESPONDIDO = 11;
        private const int ID_ESTADO_ACTIVO_SEMANAL = 9;
        private const int ID_CLASE_META_IND = 4;
        private static int? ID_ESTADO_INACTIVO_META_IND = null;

        // Instancia del servicio web
        public apivs2020.wsapi vs2020 = new apivs2020.wsapi();

        protected void Page_Load(object sender, EventArgs e)
        {
            // --- Security Checks ---

            if (Session["UsuarioID"] == null) { Response.Redirect("~/Login.aspx?mensaje=SesionExpirada"); return; }

            bool necesitaCambiar = false;
            string idUsuarioActual = Session["UsuarioID"].ToString();
            UsuarioInfo infoActual = usuarioDAL_Instance.ObtenerInfoUsuario(idUsuarioActual);
            if (infoActual != null && infoActual.NecesitaCambiarContrasena) { necesitaCambiar = true; }
            if (necesitaCambiar) { Response.Redirect("~/CambiarContrasena.aspx"); return; }
            
            // --- End Security Checks ---

            // Asegurar que el formulario tenga el enctype correcto para subida de archivos
            if (this.Page.Form != null)
            {
                this.Page.Form.Enctype = "multipart/form-data";
            }

            if (ID_ESTADO_INACTIVO_META_IND == null)
            {
                ID_ESTADO_INACTIVO_META_IND = detalleEstadoDAL.ObtenerIdEstadoPorDescripcion("Inactivo", ID_CLASE_META_IND);
                if (ID_ESTADO_INACTIVO_META_IND == null)
                {
                    Console.WriteLine($"CRITICAL ERROR: Could not find 'Inactivo' state ID for Clase {ID_CLASE_META_IND}. Logical delete will fail.");
                    if (!IsPostBack) MostrarMensaje("Error de configuración: No se encontró el estado inactivo para metas.", false);
                }
            }

            if (!IsPostBack || ScriptManager.GetCurrent(this).IsInAsyncPostBack)
            {
                if (!string.IsNullOrEmpty(idUsuarioActual))
                {
                    LoadMetasUsuario(idUsuarioActual);
                }
                else if (!IsPostBack)
                {
                    Response.Redirect("~/Login.aspx?mensaje=SesionExpirada");
                    return;
                }
            }
        }

        private void LoadMetasUsuario(string usuario)
        {
            try
            {
                List<MetaIndividualInfo> todasLasMetas = metaIndDAL.ObtenerMetasIndividualesPorUsuario(usuario);

                List<MetaIndividualInfo> metasFinalizablesInicial = todasLasMetas
                                                                    .Where(m => m.EsFinalizable == true)
                                                                    .ToList();
                List<MetaIndividualInfo> metasSemanales = todasLasMetas
                                                            .Where(m => m.EsFinalizable != true)
                                                            .ToList();

                List<MetaIndividualInfo> metasFinalizablesFiltradas = new List<MetaIndividualInfo>();
                if (metasFinalizablesInicial.Any())
                {
                    List<int> idsFinalizables = metasFinalizablesInicial.Select(m => m.IdMetaIndividual).ToList();
                    HashSet<int> idsConRespuestaFinalizada = respuestaDAL.ObtenerIdsMetasConEstadoRespuesta(idsFinalizables, ID_ESTADO_RESPONDIDO);
                    metasFinalizablesFiltradas = metasFinalizablesInicial
                                                    .Where(m => !idsConRespuestaFinalizada.Contains(m.IdMetaIndividual))
                                                    .ToList();
                }

                rptMetasFinalizables.DataSource = metasFinalizablesFiltradas;
                rptMetasFinalizables.DataBind();
                pnlEmptyMetasFinalizables.Visible = (metasFinalizablesFiltradas.Count == 0);

                BindMetasSemanales(metasSemanales);
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error cargando metas: {ex.Message}", false);
                Console.WriteLine($"Error en LoadMetasUsuario: {ex.ToString()}");
                pnlEmptyMetasFinalizables.Visible = false; // Asegurar que los paneles de vacío se oculten en caso de error general
                pnlEmptySemana1.Visible = false; pnlEmptySemana2.Visible = false; pnlEmptySemana3.Visible = false; pnlEmptySemana4.Visible = false; pnlEmptySemana5.Visible = false;
            }
        }

        private void BindMetasSemanales(List<MetaIndividualInfo> metasSemanales)
        {
            bool hayMetasSemanales = (metasSemanales != null && metasSemanales.Count > 0);
            BindRepeaterSemana(rptSemana1, pnlEmptySemana1, hayMetasSemanales ? metasSemanales : null);
            BindRepeaterSemana(rptSemana2, pnlEmptySemana2, hayMetasSemanales ? metasSemanales : null);
            BindRepeaterSemana(rptSemana3, pnlEmptySemana3, hayMetasSemanales ? metasSemanales : null);
            BindRepeaterSemana(rptSemana4, pnlEmptySemana4, hayMetasSemanales ? metasSemanales : null);
            BindRepeaterSemana(rptSemana5, pnlEmptySemana5, hayMetasSemanales ? metasSemanales : null);
        }

        private void BindRepeaterSemana(Repeater rpt, Panel pnlEmpty, List<MetaIndividualInfo> data)
        {
            bool hasData = (data != null && data.Count > 0);
            rpt.DataSource = data;
            rpt.DataBind();
            if (pnlEmpty != null) pnlEmpty.Visible = !hasData;
        }

        private int GetWeekOfMonth(DateTime date,int Semana) // No usado actualmente, pero podría ser útil
        {
            DateTime firstOfMonth = new DateTime(date.Year, date.Month, Semana);
            int firstDayOfWeek = ((int)firstOfMonth.DayOfWeek + 6) % 7;
            int weekNum = (date.Day + firstDayOfWeek - 1) / 7 + 1;
            return Math.Min(weekNum, 5);
        }

        private void MostrarMensaje(string texto, bool esExito)
        {
            if (litFeedback != null)
            {
                litFeedback.Text = $"<div class='alert alert-{(esExito ? "success" : "danger")} alert-dismissible fade show' role='alert'>" +
                                   $"{Server.HtmlEncode(texto)}" +
                                   "<button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>" +
                                   "</div>";
                litFeedback.Visible = true;
            }
            else
            {
                Console.WriteLine($"Error MostrarMensaje: Literal 'litFeedback' not found. Message: {texto}");
            }
        }



        [WebMethod(EnableSession = true)]
        public static object GetRespuestaDetalles(int metaId, bool esFinalizable, string codigoSemanaDeLaPestana)
        {
            string currentUser = HttpContext.Current.Session["UsuarioID"]?.ToString();
            if (string.IsNullOrEmpty(currentUser))
            {
                return new { success = false, message = "Error: Sesión de usuario no encontrada." };
            }

            Desempeno paginaActual = HttpContext.Current.Handler as Desempeno;
            if (paginaActual == null)
            {
                Console.WriteLine("Error en GetRespuestaDetalles: No se pudo obtener la instancia de la página actual (HttpContext.Current.Handler as Desempeno).");
                return new { success = false, message = "Error de contexto de página para cargar documentos." };
            }

            MetaIndividualDAL localMetaIndDAL = paginaActual.metaIndDAL;
            RespuestaDAL localRespuestaDAL = paginaActual.respuestaDAL;

            try
            {
                MetaIndividualInfo metaInfo = localMetaIndDAL.ObtenerMetaIndividualPorId(metaId);
                string descripcionMeta = metaInfo?.Descripcion ?? "Descripción de meta no encontrada.";
                RespuestaInfo respuestaExistente = null;

                if (esFinalizable)
                {
                    respuestaExistente = localRespuestaDAL.ObtenerRespuestaPorMetaId(metaId);
                }
                else if (!string.IsNullOrEmpty(codigoSemanaDeLaPestana))
                {
                    respuestaExistente = localRespuestaDAL.ObtenerRespuestaSemanal(metaId, codigoSemanaDeLaPestana);
                }
                string observacionGuardada = respuestaExistente?.Descripcion ?? "";
                List<DocumentoRespuestaInfo> documentos = new List<DocumentoRespuestaInfo>();
                string datosDocs = "";
                if (respuestaExistente != null && respuestaExistente.IdRespuesta > 0)
                {
                    try
                    {
                        string wsUsuario = ConfigurationManager.AppSettings["Usuario"];
                        string wsClave = ConfigurationManager.AppSettings["Clave"];
                        string wsArchivador = ConfigurationManager.AppSettings["Archivador"];
                        string wsGaveta = ConfigurationManager.AppSettings["Gaveta"];
                        string wsLlave = ConfigurationManager.AppSettings["Llave"];

                        string idCarpetaRespuesta = "";
                        

                        idCarpetaRespuesta = paginaActual.vs2020.Devuelve_Ids_Gaveta_Carpeta(wsUsuario, wsClave, wsArchivador, wsGaveta, wsLlave, respuestaExistente.IdRespuesta.ToString()).Tables[0].Rows[1]["Valor"].ToString();
                        if(idCarpetaRespuesta == "")
                        {
                            idCarpetaRespuesta = paginaActual.vs2020.Insertar_Carpeta(wsUsuario, wsClave, wsArchivador, wsGaveta, wsLlave, respuestaExistente.IdRespuesta.ToString());
                        }

                        DataSet dsDocumentos = paginaActual.vs2020.Lista_De_Documentos(idCarpetaRespuesta);

                        if (dsDocumentos != null && dsDocumentos.Tables.Count > 0 && dsDocumentos.Tables[0].Rows.Count > 0)
                        {
                            DataTable dtDocs = dsDocumentos.Tables[0];
                            foreach (DataRow row in dtDocs.Rows)
                            {

                                string ideObj = row.Table.Columns.Contains("ide_obj") ? row["ide_obj"].ToString() : null;
                                string desObj = row.Table.Columns.Contains("des_obj") ? row["des_obj"].ToString() : null;
                                string extTip = row.Table.Columns.Contains("ext_tip") ? row["ext_tip"].ToString() : null;
                                string mb = row.Table.Columns.Contains("MB") ? row["MB"].ToString() : null;
                                string archivado = row.Table.Columns.Contains("Archivado") ? row["Archivado"].ToString() : null;

                                string fechaFormateada = archivado;
                                if (DateTime.TryParse(archivado, out DateTime fechaArch))
                                {
                                    fechaFormateada = fechaArch.ToString("dd/MM/yyyy HH:mm");
                                }


                                if (!string.IsNullOrEmpty(ideObj) && !string.IsNullOrEmpty(desObj))
                                {

                                    datosDocs += $"Doc.aspx?num={HttpUtility.UrlEncode(ideObj)}" + "," + desObj + "|";
                                }


                            }
                        }

                    }
                    catch (Exception exListaDocs)
                    {
                        Console.WriteLine($"Error obteniendo lista de documentos para RespuestaID {respuestaExistente.IdRespuesta}: {exListaDocs.ToString()}");
                    }
                }
                datosDocs = datosDocs.Trim('|');

                return new { success = true, data = new { DescripcionMeta = descripcionMeta, ObservacionGuardada = observacionGuardada, DatosDocs = datosDocs } };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en WebMethod GetRespuestaDetalles (MetaID: {metaId}, CodigoSemana: {codigoSemanaDeLaPestana}): {ex.ToString()}");
                return new { success = false, message = "Error al obtener detalles." };
            }
        }

        // Evento Click del botón de guardar en el Modal (Postback Completo)
        protected void btModalGuardar_Click(object sender, EventArgs e)
        {
            string currentUser = Session["UsuarioID"]?.ToString();
            if (string.IsNullOrEmpty(currentUser))
            {
                MostrarMensaje("Error: Sesión de usuario no encontrada.", false);
                // Considerar recargar o redirigir
                return;
            }

            try
            {
                int metaId = Convert.ToInt32(hfSelectedMetaId.Value);
                bool esFinalizable = Convert.ToBoolean(hfSelectedIsFinalizable.Value);
                string observacion = modalObservacion.Value;
                string nombreArchivoParaBd = fileUploadControl.HasFile ? fileUploadControl.FileName : null;

                int respuestaId = respuestaDAL.GuardarRespuesta(metaId, observacion,
                                                                esFinalizable ? ID_ESTADO_RESPONDIDO : ID_ESTADO_ACTIVO_SEMANAL,
                                                                esFinalizable, nombreArchivoParaBd,hfActiveWeekNumber.Value);

                if (respuestaId > 0)
                {
                    bool archivoGuardadoExitosamente = true; // Asumir éxito si no hay archivo
                    if (fileUploadControl.HasFile)
                    {
                        archivoGuardadoExitosamente = GuardarDocumentoServicioWeb(respuestaId);
                    }

                    if (archivoGuardadoExitosamente)
                    {
                        MostrarMensaje("Respuesta guardada correctamente.", true);
                        hdGuardado.Value = "true"; // Para que JS pueda mostrar mensaje si es necesario
                    }
                    else
                    {
                        MostrarMensaje("Respuesta guardada, pero hubo un error al guardar el archivo adjunto.", false);
                    }

                    // Refrescar datos en la página
                    LoadMetasUsuario(currentUser);
                    // ScriptManager.RegisterStartupScript(this, GetType(), "closeModal", "if(window.respuestaModal) { window.respuestaModal.hide(); }", true); // Opcional si el modal no se cierra solo
                }
                else
                {
                    MostrarMensaje("No se pudo guardar la respuesta (DAL).", false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en btModalGuardar_Click (MetaID: {hfSelectedMetaId.Value}): {ex.ToString()}");
                MostrarMensaje("Ocurrió un error en el servidor al guardar.", false);
            }
            // El UpdatePanel se refrescará. Si quieres un refresh completo, usa Response.Redirect.
        }

        // Método para guardar el documento usando el servicio web
        // Renombrado para claridad, ya que el anterior se llamaba GuardarDocumento
        public bool GuardarDocumentoServicioWeb(int idRespuesta)
        {
            if (!fileUploadControl.HasFile)
            {
                return true;
            }

            bool bl = false;
            try
            {
                string nomDoct = fileUploadControl.FileName;
                byte[] fileBytes = fileUploadControl.FileBytes;

                string wsUsuario = ConfigurationManager.AppSettings["Usuario"];
                string wsClave = ConfigurationManager.AppSettings["Clave"];
                string wsArchivador = ConfigurationManager.AppSettings["Archivador"];
                string wsGaveta = ConfigurationManager.AppSettings["Gaveta"];
                string wsLlave = ConfigurationManager.AppSettings["Llave"]; // Llave de la carpeta principal
                string idCarpetaRespuesta = idRespuesta.ToString(); // Nombre de la subcarpeta específica

                string resultadoArchivar = "";
           
                string carpetaDestinoParaArchivar; // Esta será la ID de la carpeta donde realmente se archiva.

                if (vs2020.Existe_Carpeta(wsArchivador, wsGaveta, wsLlave, idRespuesta.ToString()))
                {

                    carpetaDestinoParaArchivar = vs2020.Devuelve_Ids_Gaveta_Carpeta(wsUsuario, wsClave, wsArchivador, wsGaveta, wsLlave, idRespuesta.ToString()).Tables[0].Rows[1]["Valor"].ToString();
                }
                else
                {

                    carpetaDestinoParaArchivar = vs2020.Insertar_Carpeta(wsUsuario, wsClave, wsArchivador, wsGaveta, wsLlave, idCarpetaRespuesta);
                }

                // Ahora archivamos usando la carpetaDestinoParaArchivar (que es el ID de la carpeta específica de la respuesta)
                resultadoArchivar = vs2020.ArchivarDocumentoByte(wsUsuario, wsClave, wsArchivador, wsGaveta, carpetaDestinoParaArchivar, nomDoct, fileBytes);

                if (resultadoArchivar == "") // O una mejor verificación del éxito
                {
                    bl = true;
                }
                else
                {
                    Console.WriteLine($"Error de ArchivarDocumentoByte: {resultadoArchivar}");
                    bl = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GuardarDocumentoServicioWeb (RespuestaID: {idRespuesta}): {ex.ToString()}");
                return false;
            }
            return bl;
        }


        [WebMethod(EnableSession = true)]
        public static object GuardarRespuesta(int metaId, string observacion, string nombreArchivo, bool esFinalizable,int numSemana)
        {
            string currentUser = HttpContext.Current.Session["UsuarioID"]?.ToString();
            if (string.IsNullOrEmpty(currentUser))
            {
                return new { success = false, message = "Error: Sesión de usuario no encontrada.", redirectTo = "" };
            }

            Desempeno paginaActual = HttpContext.Current.Handler as Desempeno;
            if (paginaActual == null)
            {
                RespuestaDAL localRespuestaDAL = new RespuestaDAL();
                System.Diagnostics.Debug.WriteLine("WebMethod GuardarRespuesta: Instanciando RespuestaDAL localmente.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("WebMethod GuardarRespuesta: Usando instancia de página para DAL.");
            }

            RespuestaDAL dalParaUsar = paginaActual?.respuestaDAL ?? new RespuestaDAL();

            string codigoSemanaParaGuardar = null;
            if (!esFinalizable)
            {
                if (numSemana > 0 && numSemana <= 5) // Usando el nuevo nombre del parámetro: numSemana
                {
                    // Construir el CodigoSemana usando numSemana y el mes/año actual
                    string monthStr = DateTime.Now.ToString("MM");
                    string yearStr = DateTime.Now.Year.ToString();
                    codigoSemanaParaGuardar = $"{numSemana}{monthStr}{yearStr}"; // Usando numSemana
                    System.Diagnostics.Debug.WriteLine($"WebMethod GuardarRespuesta: Usando numSemana '{numSemana}' para generar CodigoSemana '{codigoSemanaParaGuardar}'.");
                }
                else
                {
                    // Fallback si numSemana no es válido, usa la lógica de la fecha actual
                    codigoSemanaParaGuardar = RespuestaDALExtensions.GetCodigoSemana_WMMYYYY_Static(DateTime.Now,numSemana);
                    System.Diagnostics.Debug.WriteLine($"WebMethod GuardarRespuesta: numSemana ('{numSemana}') no válido, usando CodigoSemana por defecto '{codigoSemanaParaGuardar}'.");
                }
            }

            try
            {
                int estadoDestino = esFinalizable ? ID_ESTADO_RESPONDIDO : ID_ESTADO_ACTIVO_SEMANAL;

                // Llamada al método original de la DAL.
                // RECUERDA: Si quieres forzar el uso de 'codigoSemanaParaGuardar', 
                // debes modificar el método en RespuestaDAL para aceptar este parámetro.
                int respuestaId = dalParaUsar.GuardarRespuesta(metaId, observacion, estadoDestino, esFinalizable, nombreArchivo /*, codigoSemanaParaGuardar */);

                if (respuestaId > 0)
                {
                    return new { success = true, message = "Respuesta guardada (vía WebMethod).", codigoSemana = codigoSemanaParaGuardar, redirectTo = "Desempeno.aspx" };
                }
                else
                {
                    return new { success = false, message = "No se pudo guardar la respuesta (DAL).", redirectTo = "" };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en WebMethod GuardarRespuesta (MetaID: {metaId}, numSemana: {numSemana}): {ex.ToString()}"); // Usando numSemana en el log
                return new { success = false, message = "Ocurrió un error en el servidor al guardar.", redirectTo = "" };
            }
        }




    } // Fin clase Desempeno


    // Clase de extensión (si no existe ya en tu proyecto)
    public static class RespuestaDALExtensions
    {
        private static int GetWeekOfMonth_Static(DateTime date,int Semana)
        {
            DateTime firstOfMonth = new DateTime(date.Year, date.Month, Semana);
            // DayOfWeek.Sunday = 0, Monday = 1, ..., Saturday = 6
            // Queremos que Lunes sea el primer día de la semana (0) y Domingo el último (6) para el cálculo.
            // O, si la semana empieza en Domingo, ajustar. CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek
            int firstDayOfWeek = ((int)firstOfMonth.DayOfWeek + 6) % 7; // Lunes = 0, Martes = 1 ... Domingo = 6
            int weekNum = (date.Day + firstDayOfWeek - 1) / 7 + 1;
            return Math.Min(weekNum, 5); // Limitar a 5 semanas máximo
        }
        public static string GetCodigoSemana_WMMYYYY_Static(DateTime fecha, int Semana)
        {
            // No depende de CultureInfo para el número de semana del mes, usa la lógica GetWeekOfMonth_Static
            int weekOfMonth = GetWeekOfMonth_Static(fecha, Semana);
            string monthStr = fecha.ToString("MM"); // Formato MM (01, 02, ..., 12)
            return $"{weekOfMonth}{monthStr}{fecha.Year}";
        }
    }
}