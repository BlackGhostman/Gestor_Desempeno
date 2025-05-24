using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Globalization;
using System.Web.Script.Serialization;
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
        // DAL Instances
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

        private const string CSS_META_VENCIDA = "meta-vencida";
        private const string CSS_META_HOY = "meta-hoy";
        private const string CSS_META_A_TIEMPO = "meta-a-tiempo";
        private const string CSS_META_SEMANAL_ORIGINAL = "meta-semanal-original";

        private const string BADGE_STYLE_VENCIDA = "background-color: #DC3545; color: white;";
        private const string BADGE_STYLE_HOY = "background-color: #FFC107; color: #000;";
        private const string BADGE_STYLE_A_TIEMPO = "background-color: #198754; color: white;";
        private const string BADGE_STYLE_SEMANAL_ORIGINAL = "background-color: #0EA5E9; color: white;";

        private const int ID_ESTADO_ACTIVO_META_INDIVIDUAL = 7; // Ya existe, "Activo" para Meta_Individual
        private const int ID_ESTADO_RESPONDIDO_META_INDIVIDUAL = 13; // ID del nuevo estado "Respondido" para Meta_Individual


        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UsuarioID"] == null) { Response.Redirect("~/Login.aspx?mensaje=SesionExpirada"); return; }

            bool necesitaCambiar = false;
            string idUsuarioActual = Session["UsuarioID"].ToString();
            UsuarioInfo infoActual = usuarioDAL_Instance.ObtenerInfoUsuario(idUsuarioActual);
            if (infoActual != null && infoActual.NecesitaCambiarContrasena) { necesitaCambiar = true; }
            if (necesitaCambiar) { Response.Redirect("~/CambiarContrasena.aspx"); return; }

            if (this.Page.Form != null)
            {
                this.Page.Form.Enctype = "multipart/form-data";
            }

            if (ID_ESTADO_INACTIVO_META_IND == null)
            {
                ID_ESTADO_INACTIVO_META_IND = detalleEstadoDAL.ObtenerIdEstadoPorDescripcion("Inactivo", ID_CLASE_META_IND);
                if (ID_ESTADO_INACTIVO_META_IND == null)
                {
                    Console.WriteLine($"CRITICAL ERROR: Could not find 'Inactivo' state ID for Clase {ID_CLASE_META_IND}.");
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
                // 1. Obtener TODAS las metas asignadas al usuario.
                //    Tu método 'ObtenerMetasIndividualesPorUsuario' ya trae el 'IdDetalleEstado' de Meta_Individual.
                List<MetaIndividualInfo> todasLasMetasDelUsuario = metaIndDAL.ObtenerMetasIndividualesPorUsuario(usuario);

                // 2. Filtrar estas metas para procesar solo aquellas cuyo 'IdDetalleEstado' en 'Meta_Individual' sea 7 ("Activo").
                List<MetaIndividualInfo> metasActivasParaProcesar = todasLasMetasDelUsuario
                    .Where(m => m.IdDetalleEstado == ID_ESTADO_ACTIVO_META_INDIVIDUAL)
                    .ToList();

                // Inicializar listas para los ViewModels de cada sección/repeater
                var metasParaVencidas = new List<MetaIndividualInfoViewModel>();
                var metasParaSemana1 = new List<MetaIndividualInfoViewModel>();
                var metasParaSemana2 = new List<MetaIndividualInfoViewModel>();
                var metasParaSemana3 = new List<MetaIndividualInfoViewModel>();
                var metasParaSemana4 = new List<MetaIndividualInfoViewModel>();
                var metasParaSemana5 = new List<MetaIndividualInfoViewModel>();

                DateTime hoy = DateTime.Today;
                DateTime ahora = DateTime.Now;

                // 3. Procesar la lista de 'metasActivasParaProcesar'
                foreach (var meta in metasActivasParaProcesar)
                {
                    // Tu 'MetaIndividualInfoViewModel' ya está definido en MetaIndividualDAL.cs
                    var viewModel = new MetaIndividualInfoViewModel(meta);

                    if (meta.EsFinalizable == true)
                    {
                        if (!meta.FechaFinal.HasValue)
                        {
                            viewModel.EstadoColorCss = CSS_META_A_TIEMPO;
                            viewModel.MensajeTiempo = "Fecha final no definida";
                            viewModel.BadgeStyle = BADGE_STYLE_A_TIEMPO;
                            int semanaActual = GetWeekOfMonth(hoy);
                            AsignarViewModelASemana(viewModel, semanaActual, metasParaSemana1, metasParaSemana2, metasParaSemana3, metasParaSemana4, metasParaSemana5);
                            continue;
                        }

                        DateTime fechaFinalMetaDate = meta.FechaFinal.Value.Date;

                        if (fechaFinalMetaDate < hoy) // Vencida
                        {
                            viewModel.EstadoColorCss = CSS_META_VENCIDA;
                            viewModel.BadgeStyle = BADGE_STYLE_VENCIDA;
                            TimeSpan tsVencida = hoy - fechaFinalMetaDate;
                            viewModel.MensajeTiempo = tsVencida.Days == 1 ? "Venció ayer" : $"Venció hace {tsVencida.Days} días";
                            metasParaVencidas.Add(viewModel);
                        }
                        else if (fechaFinalMetaDate == hoy) // Vence Hoy
                        {
                            viewModel.EstadoColorCss = CSS_META_HOY;
                            viewModel.BadgeStyle = BADGE_STYLE_HOY;
                            if (meta.FechaFinal.Value > ahora)
                            {
                                TimeSpan restanteHoy = meta.FechaFinal.Value - ahora;
                                viewModel.MensajeTiempo = $"Vence hoy (en {Math.Floor(restanteHoy.TotalHours)}h {restanteHoy.Minutes}m)";
                            }
                            else
                            {
                                viewModel.MensajeTiempo = "Vence hoy";
                            }
                            int semanaDeVencimientoHoy = GetWeekOfMonth(fechaFinalMetaDate);
                            AsignarViewModelASemana(viewModel, semanaDeVencimientoHoy, metasParaSemana1, metasParaSemana2, metasParaSemana3, metasParaSemana4, metasParaSemana5);
                        }
                        else // A Tiempo (vence en el futuro)
                        {
                            if (fechaFinalMetaDate.Year == hoy.Year && fechaFinalMetaDate.Month == hoy.Month)
                            {
                                viewModel.EstadoColorCss = CSS_META_A_TIEMPO;
                                viewModel.BadgeStyle = BADGE_STYLE_A_TIEMPO;
                                TimeSpan tsATiempo = fechaFinalMetaDate - hoy;
                                viewModel.MensajeTiempo = tsATiempo.Days == 1 ? "Vence mañana" : $"Vence en {tsATiempo.Days} días";
                                int semanaDeVencimiento = GetWeekOfMonth(fechaFinalMetaDate);
                                AsignarViewModelASemana(viewModel, semanaDeVencimiento, metasParaSemana1, metasParaSemana2, metasParaSemana3, metasParaSemana4, metasParaSemana5);
                            }
                            // Metas a tiempo de meses futuros no se añaden a las pestañas de este mes.
                        }
                    }
                    else // Metas NO Finalizables (semanales puras)
                    {
                        // Estas metas, si están activas (IdDetalleEstado = 7), se muestran.
                        // Su estilo y mensaje se manejan aquí. El ItemTemplate usa 'DisplayTextLista2'.
                        viewModel.EstadoColorCss = CSS_META_SEMANAL_ORIGINAL;
                        viewModel.BadgeStyle = BADGE_STYLE_SEMANAL_ORIGINAL;
                        viewModel.MensajeTiempo = !string.IsNullOrWhiteSpace(meta.NombreTipoObjetivo) ? meta.NombreTipoObjetivo : "Reporte Semanal";

                        metasParaSemana1.Add(viewModel);
                        metasParaSemana2.Add(viewModel);
                        metasParaSemana3.Add(viewModel);
                        metasParaSemana4.Add(viewModel);
                        metasParaSemana5.Add(viewModel);
                    }
                }

                // 4. Hacer DataBind a los Repeaters y manejar paneles vacíos
                rptMetasVencidas.DataSource = metasParaVencidas.OrderBy(m => m.Meta.FechaFinal ?? DateTime.MaxValue).ThenBy(m => m.Meta.Descripcion);
                rptMetasVencidas.DataBind();
                pnlEmptyMetasVencidas.Visible = !metasParaVencidas.Any();

                BindRepeaterSemanaConViewModel(rptSemana1, pnlEmptySemana1, metasParaSemana1);
                BindRepeaterSemanaConViewModel(rptSemana2, pnlEmptySemana2, metasParaSemana2);
                BindRepeaterSemanaConViewModel(rptSemana3, pnlEmptySemana3, metasParaSemana3);
                BindRepeaterSemanaConViewModel(rptSemana4, pnlEmptySemana4, metasParaSemana4);
                BindRepeaterSemanaConViewModel(rptSemana5, pnlEmptySemana5, metasParaSemana5);
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error cargando metas: {ex.Message}", false);
                Console.WriteLine($"Error en LoadMetasUsuario: {ex.ToString()}");
                pnlEmptyMetasVencidas.Visible = true;
                pnlEmptySemana1.Visible = true; pnlEmptySemana2.Visible = true;
                pnlEmptySemana3.Visible = true; pnlEmptySemana4.Visible = true;
                pnlEmptySemana5.Visible = true;
            }
        }

        // Asegúrate de que estos métodos auxiliares (GetWeekOfMonth, AsignarViewModelASemana, BindRepeaterSemanaConViewModel)
        // estén presentes en tu clase Desempeno.aspx.cs y funcionen como esperas.
        // Por ejemplo, AsignarViewModelASemana:
        private void AsignarViewModelASemana(MetaIndividualInfoViewModel viewModel, int numeroSemanaDelMes,
            List<MetaIndividualInfoViewModel> s1, List<MetaIndividualInfoViewModel> s2,
            List<MetaIndividualInfoViewModel> s3, List<MetaIndividualInfoViewModel> s4, List<MetaIndividualInfoViewModel> s5)
        {
            switch (numeroSemanaDelMes)
            {
                case 1: if (!s1.Any(vm => vm.Meta.IdMetaIndividual == viewModel.Meta.IdMetaIndividual)) s1.Add(viewModel); break;
                case 2: if (!s2.Any(vm => vm.Meta.IdMetaIndividual == viewModel.Meta.IdMetaIndividual)) s2.Add(viewModel); break;
                case 3: if (!s3.Any(vm => vm.Meta.IdMetaIndividual == viewModel.Meta.IdMetaIndividual)) s3.Add(viewModel); break;
                case 4: if (!s4.Any(vm => vm.Meta.IdMetaIndividual == viewModel.Meta.IdMetaIndividual)) s4.Add(viewModel); break;
                case 5: if (!s5.Any(vm => vm.Meta.IdMetaIndividual == viewModel.Meta.IdMetaIndividual)) s5.Add(viewModel); break;
                default:
                    Console.WriteLine($"Warning (AsignarViewModelASemana): Número de semana ({numeroSemanaDelMes}) inválido para meta ID {viewModel.Meta.IdMetaIndividual}.");
                    break;
            }
        }

        // Y BindRepeaterSemanaConViewModel (parece que ya lo tienes)
        // private void BindRepeaterSemanaConViewModel(Repeater rpt, Panel pnlEmpty, List<MetaIndividualInfoViewModel> data) { ... }

        // Y GetWeekOfMonth (parece que ya lo tienes)
        // public int GetWeekOfMonth(DateTime date) { ... }



        private void BindRepeaterSemanaConViewModel(Repeater rpt, Panel pnlEmpty, List<MetaIndividualInfoViewModel> data)
        {
            var dataToShow = data.Distinct(new MetaIndividualViewModelComparer()).ToList();
            bool hasData = (dataToShow != null && dataToShow.Any());

            rpt.DataSource = dataToShow.OrderBy(vm => vm.Meta.EsFinalizable == true && vm.Meta.FechaFinal.HasValue ? vm.Meta.FechaFinal.Value : DateTime.MaxValue)
                                     .ThenBy(vm => vm.Meta.Descripcion);
            rpt.DataBind();
            if (pnlEmpty != null) pnlEmpty.Visible = !hasData;
        }

        public class MetaIndividualViewModelComparer : IEqualityComparer<MetaIndividualInfoViewModel>
        {
            public bool Equals(MetaIndividualInfoViewModel x, MetaIndividualInfoViewModel y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return x.Meta.IdMetaIndividual == y.Meta.IdMetaIndividual;
            }

            public int GetHashCode(MetaIndividualInfoViewModel obj)
            {
                return obj.Meta.IdMetaIndividual.GetHashCode();
            }
        }

        public int GetWeekOfMonth(DateTime date)
        {
            DateTime firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            DayOfWeek firstDayOfWeekCulture = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            int dayOfWeekOfFirstDayOfMonth = (int)firstDayOfMonth.DayOfWeek;
            int firstDayOffset = (dayOfWeekOfFirstDayOfMonth - (int)firstDayOfWeekCulture + 7) % 7;
            int weekNumber = (date.Day + firstDayOffset - 1) / 7 + 1;
            return Math.Min(weekNumber, 5);
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
                Console.WriteLine("Error en GetRespuestaDetalles: No se pudo obtener la instancia de la página actual.");
                return new { success = false, message = "Error de contexto de página para cargar documentos." };
            }

            MetaIndividualDAL localMetaIndDAL = paginaActual.metaIndDAL;
            RespuestaDAL localRespuestaDAL = paginaActual.respuestaDAL;

            try
            {
                MetaIndividualInfo metaInfo = localMetaIndDAL.ObtenerMetaIndividualPorId(metaId);
                string descripcionMeta = metaInfo?.Descripcion ?? "Descripción de meta no encontrada.";
                RespuestaInfo respuestaExistente = null;

                // Si es finalizable, y se pasa un codigoSemanaDeLaPestana (incluyendo "0000000" para vencidas),
                // se busca una respuesta semanal (avance).
                // Si es finalizable y NO se pasa codigoSemanaDeLaPestana (o es null/empty),
                // se busca la respuesta "final" (aquella sin código de semana).
                if (esFinalizable)
                {
                    if (!string.IsNullOrEmpty(codigoSemanaDeLaPestana))
                    {
                        respuestaExistente = localRespuestaDAL.ObtenerRespuestaSemanal(metaId, codigoSemanaDeLaPestana);
                        Console.WriteLine($"GetRespuestaDetalles (Finalizable, con CodigoSemana='{codigoSemanaDeLaPestana}'): RespuestaExistente ID = {(respuestaExistente?.IdRespuesta.ToString() ?? "NULL")}");
                    }
                    else
                    {
                        // Esto es para cargar el estado de una meta que ya fue FINALIZADA (estado RESPONDIDO)
                        // y que no tiene un CodigoSemana asociado a su respuesta final.
                        respuestaExistente = localRespuestaDAL.ObtenerRespuestaPorMetaId(metaId);
                        Console.WriteLine($"GetRespuestaDetalles (Finalizable, SIN CodigoSemana): RespuestaExistente ID = {(respuestaExistente?.IdRespuesta.ToString() ?? "NULL")}");
                    }
                }
                else if (!string.IsNullOrEmpty(codigoSemanaDeLaPestana)) // Meta semanal no finalizable
                {
                    respuestaExistente = localRespuestaDAL.ObtenerRespuestaSemanal(metaId, codigoSemanaDeLaPestana);
                    Console.WriteLine($"GetRespuestaDetalles (No Finalizable, con CodigoSemana='{codigoSemanaDeLaPestana}'): RespuestaExistente ID = {(respuestaExistente?.IdRespuesta.ToString() ?? "NULL")}");
                }

                string observacionGuardada = respuestaExistente?.Descripcion ?? "";
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
                        string idCarpetaRespuestaWs = "";
                        string nombreCarpetaWs = respuestaExistente.IdRespuesta.ToString();

                        DataSet dsCarpetaInfo = null;
                        try
                        {
                            dsCarpetaInfo = paginaActual.vs2020.Devuelve_Ids_Gaveta_Carpeta(wsUsuario, wsClave, wsArchivador, wsGaveta, wsLlave, nombreCarpetaWs);
                        }
                        catch (Exception exServicio)
                        {
                            Console.WriteLine($"GetRespuestaDetalles: Excepción llamando a Devuelve_Ids_Gaveta_Carpeta para '{nombreCarpetaWs}': {exServicio.Message}");
                        }

                        if (dsCarpetaInfo != null && dsCarpetaInfo.Tables.Count > 0 && dsCarpetaInfo.Tables[0].Rows.Count > 0)
                        {
                            if (dsCarpetaInfo.Tables[0].Rows.Count > 1 && dsCarpetaInfo.Tables[0].Columns.Contains("Valor"))
                            {
                                idCarpetaRespuestaWs = dsCarpetaInfo.Tables[0].Rows[1]["Valor"]?.ToString();
                            }
                            else if (dsCarpetaInfo.Tables[0].Columns.Contains("Valor"))
                            {
                                idCarpetaRespuestaWs = dsCarpetaInfo.Tables[0].Rows[0]["Valor"]?.ToString();
                            }
                            else
                            {
                                Console.WriteLine($"GetRespuestaDetalles: Devuelve_Ids_Gaveta_Carpeta para '{nombreCarpetaWs}' no devolvió la estructura esperada. Filas: {dsCarpetaInfo.Tables[0].Rows.Count}, Columnas: {string.Join(",", dsCarpetaInfo.Tables[0].Columns.Cast<DataColumn>().Select(c => c.ColumnName))}");
                            }
                            if (!string.IsNullOrEmpty(idCarpetaRespuestaWs)) Console.WriteLine($"GetRespuestaDetalles: ID de carpeta existente encontrado para '{nombreCarpetaWs}': {idCarpetaRespuestaWs}");

                        }
                        else
                        {
                            Console.WriteLine($"GetRespuestaDetalles: Devuelve_Ids_Gaveta_Carpeta para '{nombreCarpetaWs}' no encontró la carpeta o devolvió un DataSet/DataTable vacío o nulo.");
                        }

                        if (string.IsNullOrEmpty(idCarpetaRespuestaWs))
                        {
                            Console.WriteLine($"GetRespuestaDetalles: Carpeta '{nombreCarpetaWs}' no encontrada por nombre. Intentando crearla.");
                            try
                            {
                                idCarpetaRespuestaWs = paginaActual.vs2020.Insertar_Carpeta(wsUsuario, wsClave, wsArchivador, wsGaveta, wsLlave, nombreCarpetaWs);
                                if (string.IsNullOrEmpty(idCarpetaRespuestaWs))
                                {
                                    Console.WriteLine($"GetRespuestaDetalles: Falló la creación de la carpeta '{nombreCarpetaWs}' (Insertar_Carpeta devolvió vacío/nulo). No se pueden listar documentos.");
                                }
                                else
                                {
                                    Console.WriteLine($"GetRespuestaDetalles: Carpeta '{nombreCarpetaWs}' creada con ID: {idCarpetaRespuestaWs}.");
                                }
                            }
                            catch (Exception exCrearCarpeta)
                            {
                                Console.WriteLine($"GetRespuestaDetalles: Excepción llamando a Insertar_Carpeta para '{nombreCarpetaWs}': {exCrearCarpeta.Message}");
                            }
                        }

                        if (!string.IsNullOrEmpty(idCarpetaRespuestaWs))
                        {
                            DataSet dsDocumentos = null;
                            try
                            {
                                dsDocumentos = paginaActual.vs2020.Lista_De_Documentos(idCarpetaRespuestaWs);
                            }
                            catch (Exception exListarDocs)
                            {
                                Console.WriteLine($"GetRespuestaDetalles: Excepción llamando a Lista_De_Documentos para carpeta ID '{idCarpetaRespuestaWs}': {exListarDocs.Message}");
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
                                        datosDocs += $"Doc.aspx?num={HttpUtility.UrlEncode(ideObj)},{HttpUtility.HtmlEncode(desObj)}|";
                                    }
                                }
                                datosDocs = datosDocs.TrimEnd('|');
                                Console.WriteLine($"GetRespuestaDetalles: Documentos encontrados para carpeta ID '{idCarpetaRespuestaWs}': {datosDocs}");
                            }
                            else
                            {
                                Console.WriteLine($"GetRespuestaDetalles: Lista_De_Documentos para carpeta ID '{idCarpetaRespuestaWs}' no devolvió documentos o la tabla estaba vacía.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"GetRespuestaDetalles: No se pudo obtener/crear un ID de carpeta para RespuestaID {respuestaExistente.IdRespuesta}. No se listarán documentos.");
                        }
                    }
                    catch (Exception exBloqueDocumentosGeneral)
                    {
                        Console.WriteLine($"GetRespuestaDetalles: Error general en el bloque de obtención de documentos para RespuestaID {respuestaExistente.IdRespuesta}: {exBloqueDocumentosGeneral.ToString()}");
                    }
                }
                return new { success = true, data = new { DescripcionMeta = descripcionMeta, ObservacionGuardada = observacionGuardada, DatosDocs = datosDocs } };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error CRÍTICO en WebMethod GetRespuestaDetalles (MetaID: {metaId}, CodigoSemana: {codigoSemanaDeLaPestana}): {ex.ToString()}");
                return new { success = false, message = "Error crítico al obtener detalles de la respuesta." };
            }
        }

        protected void btModalGuardarAvance_Click(object sender, EventArgs e)
        {
            string currentUser = Session["UsuarioID"]?.ToString();
            if (string.IsNullOrEmpty(currentUser)) { MostrarMensaje("Error: Sesión de usuario no encontrada.", false); return; }

            try
            {
                int metaId = Convert.ToInt32(hfSelectedMetaId.Value);
                string observacion = modalObservacion.Value;
                string nombreArchivoParaBd = fileUploadControl.HasFile ? fileUploadControl.FileName : null;
                string pestanaActivaValor = hfActiveWeekNumber.Value; // "overdue", "1", "2", ...
                string codigoSemanaParaGuardar = null;

                if (pestanaActivaValor == "overdue")
                {
                    codigoSemanaParaGuardar = "0000000"; // Código especial para avances de metas vencidas
                    Console.WriteLine($"GuardarAvance: Meta Vencida. CodigoSemana a usar: {codigoSemanaParaGuardar}");
                }
                else if (int.TryParse(pestanaActivaValor, out int numSemana) && numSemana >= 1 && numSemana <= 5)
                {
                    codigoSemanaParaGuardar = $"{numSemana}{DateTime.Now:MM}{DateTime.Now.Year}";
                    Console.WriteLine($"GuardarAvance: Meta en Semana {numSemana}. CodigoSemana a usar: {codigoSemanaParaGuardar}");
                }
                else
                {
                    // Fallback si hfActiveWeekNumber tiene un valor inesperado (no es "overdue" ni un número 1-5)
                    // Esto podría indicar un error en la lógica de JS o un estado no previsto.
                    // Por seguridad, podríamos no guardar o usar un código de semana basado en la fecha actual.
                    // Optaremos por usar la semana actual del mes como fallback, pero registrando una advertencia.
                    int semanaActualCalculada = GetWeekOfMonth(DateTime.Now);
                    codigoSemanaParaGuardar = $"{semanaActualCalculada}{DateTime.Now:MM}{DateTime.Now.Year}";
                    Console.WriteLine($"Warning: GuardarAvance con valor de pestaña activa inesperado ('{pestanaActivaValor}'). Usando semana actual calculada: {codigoSemanaParaGuardar}");
                    MostrarMensaje("Advertencia: No se pudo determinar la pestaña activa con claridad, avance asociado a la semana actual.", false); // false para que sea 'danger'
                }

                int respuestaId = respuestaDAL.GuardarRespuesta(metaId, observacion,
                                                ID_ESTADO_ACTIVO_SEMANAL,
                                                true,
                                                nombreArchivoParaBd,
                                                codigoSemanaParaGuardar);

                if (respuestaId > 0)
                {
                    bool archivoOk = true;
                    if (fileUploadControl.HasFile) archivoOk = GuardarDocumentoServicioWeb(respuestaId);
                    MostrarMensaje(archivoOk ? "Avance guardado correctamente." : "Avance guardado, pero hubo un error al guardar el archivo adjunto.", archivoOk);
                    hdGuardado.Value = "true";
                }
                else
                {
                    MostrarMensaje("No se pudo guardar el avance (DAL).", false);
                }
                LoadMetasUsuario(currentUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en btModalGuardarAvance_Click (MetaID: {hfSelectedMetaId.Value}): {ex.ToString()}");
                MostrarMensaje("Ocurrió un error en el servidor al guardar el avance.", false);
            }
        }

        

        // En Desempeno.aspx.cs

        protected void btModalFinalizarMeta_Click(object sender, EventArgs e)
        {
            string currentUser = Session["UsuarioID"]?.ToString();
            if (string.IsNullOrEmpty(currentUser)) { MostrarMensaje("Error: Sesión de usuario no encontrada.", false); return; }

            try
            {
                int metaId = Convert.ToInt32(hfSelectedMetaId.Value);
                string observacion = modalObservacion.Value; // Textarea del modal
                string nombreArchivoParaBd = fileUploadControl.HasFile ? fileUploadControl.FileName : null;

                // hfActiveWeekNumber.Value contendrá "overdue" si la meta se abrió desde la sección de vencidas,
                // o el número de semana ("1", "2", etc.) si se abrió desde una pestaña semanal.
                string pestanaActivaOContexto = hfActiveWeekNumber.Value;

                string codigoSemanaParaGuardarFinalizacion = "0000000"; // Valor por defecto para finalización estándar

                // Determinar el Codigo_Semana para la Respuesta al finalizar:
                // Si la meta que se está finalizando proviene de la sección de "Vencidas",
                // su Codigo_Semana en la tabla Respuesta debe ser "0000000".
                if (pestanaActivaOContexto == "overdue")
                {
                    codigoSemanaParaGuardarFinalizacion = "0000000";
                    Console.WriteLine($"FinalizarMeta (Contexto: Vencida): MetaID {metaId}. CodigoSemana para Respuesta: {codigoSemanaParaGuardarFinalizacion}");
                }
                else
                {
                    // Para metas finalizadas que no estaban en "Vencidas" (ej. finalizadas desde una pestaña semanal
                    // antes de vencer o justo el día que vencen), su registro de Respuesta final no lleva un Codigo_Semana específico.
                    Console.WriteLine($"FinalizarMeta (Contexto: No Vencida): MetaID {metaId}. CodigoSemana para Respuesta: null");
                }

                // Llamar a GuardarRespuesta.
                // ID_ESTADO_RESPONDIDO (e.g., 11) es para la tabla Respuesta.
                // ID_ESTADO_RESPONDIDO_META_INDIVIDUAL (e.g., 13 o 12) es para la tabla Meta_Individual.
                int respuestaId = respuestaDAL.GuardarRespuesta(
                    metaId,
                    observacion,
                    ID_ESTADO_RESPONDIDO, // Estado de la RESPUESTA (ej: 11)
                    true,                 // Es una meta finalizable (asumiendo que este botón es para ellas)
                    nombreArchivoParaBd,
                    codigoSemanaParaGuardarFinalizacion // Será "0000000" para vencidas, o null para otras finalizaciones
                );

                if (respuestaId > 0)
                {
                    bool archivoOk = true;
                    if (fileUploadControl.HasFile)
                    {
                        archivoOk = GuardarDocumentoServicioWeb(respuestaId); // Asegúrate que este método use 'respuestaId' para la carpeta.
                    }

                    if (archivoOk)
                    {
                        // Actualizar el estado de la Meta_Individual a "Respondido" (o "Completado")
                        bool estadoMetaActualizado = metaIndDAL.ActualizarEstadoMetaIndividual(metaId, ID_ESTADO_RESPONDIDO_META_INDIVIDUAL);

                        if (estadoMetaActualizado)
                        {
                            MostrarMensaje("Meta finalizada y su estado actualizado correctamente.", true);
                        }
                        else
                        {
                            MostrarMensaje("Meta finalizada (Respuesta guardada), pero hubo un error al actualizar el estado de la meta. Contacte a soporte.", false);
                        }
                    }
                    else
                    {
                        // La respuesta se guardó, pero el archivo no. El estado de la meta individual no se actualizó para prevenir inconsistencias.
                        MostrarMensaje("Meta finalizada (Respuesta guardada), pero hubo un error al guardar el archivo adjunto. El estado de la meta no fue actualizado.", false);
                    }
                    hdGuardado.Value = "true"; // Para que el JavaScript cierre el modal
                }
                else
                {
                    MostrarMensaje("No se pudo finalizar la meta (error al guardar en tabla Respuesta).", false);
                }

                LoadMetasUsuario(currentUser); // Recargar la lista de metas
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en btModalFinalizarMeta_Click (MetaID: {hfSelectedMetaId.Value}): {ex.ToString()}");
                MostrarMensaje("Ocurrió un error en el servidor al finalizar la meta.", false);
            }
        }

        protected void btModalGuardarSemanal_Click(object sender, EventArgs e)
        {
            string currentUser = Session["UsuarioID"]?.ToString();
            if (string.IsNullOrEmpty(currentUser)) { MostrarMensaje("Error: Sesión de usuario no encontrada.", false); return; }

            try
            {
                int metaId = Convert.ToInt32(hfSelectedMetaId.Value);
                string observacion = modalObservacion.Value;
                string nombreArchivoParaBd = fileUploadControl.HasFile ? fileUploadControl.FileName : null;
                string numeroSemanaPestanaActiva = hfActiveWeekNumber.Value;
                string codigoSemanaParaGuardar = null;

                if (int.TryParse(numeroSemanaPestanaActiva, out int numSemana) && numSemana >= 1 && numSemana <= 5)
                {
                    codigoSemanaParaGuardar = $"{numSemana}{DateTime.Now:MM}{DateTime.Now.Year}";
                }
                else
                {
                    MostrarMensaje("Error: No se pudo determinar el código de semana para la meta semanal (valor de pestaña inválido: '" + numeroSemanaPestanaActiva + "').", false);
                    return;
                }

                int respuestaId = respuestaDAL.GuardarRespuesta(metaId, observacion,
                                                ID_ESTADO_ACTIVO_SEMANAL,
                                                false,
                                                nombreArchivoParaBd,
                                                codigoSemanaParaGuardar);

                if (respuestaId > 0)
                {
                    bool archivoOk = true;
                    if (fileUploadControl.HasFile) archivoOk = GuardarDocumentoServicioWeb(respuestaId);
                    MostrarMensaje(archivoOk ? "Respuesta semanal guardada correctamente." : "Respuesta guardada, pero hubo un error con el archivo.", archivoOk);
                    hdGuardado.Value = "true";
                }
                else
                {
                    MostrarMensaje("No se pudo guardar la respuesta semanal (DAL).", false);
                }
                LoadMetasUsuario(currentUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en btModalGuardarSemanal_Click (MetaID: {hfSelectedMetaId.Value}): {ex.ToString()}");
                MostrarMensaje("Ocurrió un error en el servidor al guardar la respuesta semanal.", false);
            }
        }

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
                string wsLlave = ConfigurationManager.AppSettings["Llave"];
                string nombreCarpetaWs = idRespuesta.ToString();
                string idRealDeLaCarpeta = "";

                DataSet dsCarpetaInfo = vs2020.Devuelve_Ids_Gaveta_Carpeta(wsUsuario, wsClave, wsArchivador, wsGaveta, wsLlave, nombreCarpetaWs);

                if (dsCarpetaInfo != null && dsCarpetaInfo.Tables.Count > 0 && dsCarpetaInfo.Tables[0].Rows.Count > 0)
                {
                    if (dsCarpetaInfo.Tables[0].Rows.Count > 1 && dsCarpetaInfo.Tables[0].Columns.Contains("Valor"))
                    { // Original assumption
                        idRealDeLaCarpeta = dsCarpetaInfo.Tables[0].Rows[1]["Valor"]?.ToString();
                    }
                    else if (dsCarpetaInfo.Tables[0].Columns.Contains("Valor"))
                    { // Check first row if only one
                        idRealDeLaCarpeta = dsCarpetaInfo.Tables[0].Rows[0]["Valor"]?.ToString();
                    }
                }

                if (string.IsNullOrEmpty(idRealDeLaCarpeta))
                {
                    idRealDeLaCarpeta = vs2020.Insertar_Carpeta(wsUsuario, wsClave, wsArchivador, wsGaveta, wsLlave, nombreCarpetaWs);
                }

                if (string.IsNullOrEmpty(idRealDeLaCarpeta))
                {
                    Console.WriteLine($"Error: No se pudo obtener ni crear el ID de la carpeta de destino para RespuestaID {idRespuesta}. Nombre: {nombreCarpetaWs}");
                    return false;
                }
                string resultadoArchivar = vs2020.ArchivarDocumentoByte(wsUsuario, wsClave, wsArchivador, wsGaveta, idRealDeLaCarpeta, nomDoct, fileBytes);
                if (string.IsNullOrEmpty(resultadoArchivar) || resultadoArchivar.ToUpper().Contains("EXITO"))
                {
                    bl = true;
                }
                else
                {
                    Console.WriteLine($"Error de ArchivarDocumentoByte para RespuestaID {idRespuesta} en CarpetaID {idRealDeLaCarpeta}: {resultadoArchivar}");
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
    }

    public static class RespuestaDALExtensions
    {
        private static int GetWeekOfMonth_Static(DateTime date, int diaReferenciaParaCalculoSemana = 1)
        {
            DateTime firstOfMonth = new DateTime(date.Year, date.Month, diaReferenciaParaCalculoSemana);
            int firstDayOfWeek = ((int)firstOfMonth.DayOfWeek + 6) % 7;
            int weekNum = (date.Day + firstDayOfWeek - 1) / 7 + 1;
            return Math.Min(weekNum, 5);
        }
        public static string GetCodigoSemana_WMMYYYY_Static(DateTime fecha, int numeroSemanaDelMes)
        {
            string monthStr = fecha.ToString("MM");
            return $"{numeroSemanaDelMes}{monthStr}{fecha.Year}";
        }
    }
}
