using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services; // Needed for WebMethod
using System.Globalization; // Needed for CalendarWeekRule
using System.Web.Script.Serialization; // Needed for JSON response
using System.Data;

namespace Gestor_Desempeno
{
    public partial class GestionMetasInd : System.Web.UI.Page
    {
        public wsRRHH.WS_RecursosHumanos apiRH = new wsRRHH.WS_RecursosHumanos();
        // DALs
        private MetaIndividualDAL metaIndDAL = new MetaIndividualDAL();
        private MetaDepartamentalDAL metaDepDAL = new MetaDepartamentalDAL();
        private DetalleEstadoDAL detalleEstadoDAL = new DetalleEstadoDAL();
        private TipoObjetivoDAL tipoObjetivoDAL = new TipoObjetivoDAL();
        private AreaEjecutoraDAL areaDAL = new AreaEjecutoraDAL();
        private UsuarioDAL usuarioDAL_Instance = new UsuarioDAL(); // For security checks

        // ID Estado Inactivo para Metas Individuales (Clase 4 - AJUSTAR!)
        private int? ID_ESTADO_INACTIVO_META_IND = null;
        private const int ID_CLASE_META_IND = 4; // Clase para estados de Meta Individual

        protected void Page_Load(object sender, EventArgs e)
        {
            UsuariosXJefe();
            // --- Security Checks ---
            if (Session["UsuarioID"] == null) { Response.Redirect("~/Login.aspx?mensaje=SesionExpirada"); return; }
            bool necesitaCambiar = false;
            string idUsuarioActual = Session["UsuarioID"].ToString();
            UsuarioInfo infoActual = usuarioDAL_Instance.ObtenerInfoUsuario(idUsuarioActual);
            if (infoActual != null && infoActual.NecesitaCambiarContrasena) { necesitaCambiar = true; }
            if (necesitaCambiar) { Response.Redirect("~/CambiarContrasena.aspx"); return; }
            // --- End Security Checks ---

            // Load Inactive State ID once
            if (ID_ESTADO_INACTIVO_META_IND == null)
            {
                ID_ESTADO_INACTIVO_META_IND = detalleEstadoDAL.ObtenerIdEstadoPorDescripcion("Inactivo", ID_CLASE_META_IND);
                if (ID_ESTADO_INACTIVO_META_IND == null)
                {
                    Console.WriteLine($"CRITICAL ERROR: Could not find 'Inactivo' state ID for Clase {ID_CLASE_META_IND}. Logical delete will fail.");
                }
            }

            if (!IsPostBack)
            {
                LoadFilterDropdowns(); // Este ya carga ddlTipoObjetivoFiltro y ddlAreaFiltro
                LoadUsuariosFiltroDropdown(ddlUsuarioFiltro, "-- Seleccione Usuario --"); // <-- AÑADIR ESTA LÍNEA
                LoadModalDropdowns(); // Load dropdowns for the modal
                BindGrid();
            }
            if (IsPostBack && !ScriptManager.GetCurrent(this.Page).IsInAsyncPostBack)
            {
                litMensaje.Visible = false;
                litModalMensaje.Visible = false;
            }
        }

        protected void ddlUsuarioFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            gvMetasInd.PageIndex = 0; // Resetear a la primera página al cambiar el filtro
            BindGrid();
        }

        private void LoadUsuariosFiltroDropdown(DropDownList ddl, string initialText)
        {
            if (ddl == null) return;

            string idUsuarioJefe = Session["UsuarioID"]?.ToString();
            if (string.IsNullOrEmpty(idUsuarioJefe))
            {
                MostrarMensaje("Error: No se pudo identificar al usuario jefe para cargar la lista de colaboradores.", false);
                ddl.Items.Clear();
                ddl.Items.Insert(0, new ListItem(initialText, "0")); // Opción por defecto aunque no se pueda cargar
                return;
            }

            try
            {
                System.Data.DataTable dtColaboradores = apiRH.ObtenerColaboradoresUsuarioJefe(idUsuarioJefe);

                ddl.DataSource = dtColaboradores;
                ddl.DataTextField = "Nombre";         // Como devuelve tu WebMethod
                ddl.DataValueField = "Id_Colaborador"; // Como devuelve tu WebMethod
                ddl.DataBind();

                ddl.Items.Insert(0, new ListItem(initialText, "0")); // "0" para "-- Seleccione Usuario --"
            }
            catch (System.Net.WebException webEx)
            {
                MostrarMensaje($"Error de red al contactar el servicio de RRHH para usuarios: {webEx.Message}", false);
                Console.WriteLine($"Error de WebService en LoadUsuariosFiltroDropdown: {webEx.ToString()}");
                ddl.Items.Clear();
                ddl.Items.Insert(0, new ListItem("-- Error WS Colab. --", "0"));
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error crítico al cargar la lista de colaboradores: {ex.Message}", false);
                Console.WriteLine($"Error en LoadUsuariosFiltroDropdown: {ex.ToString()}");
                ddl.Items.Clear();
                ddl.Items.Insert(0, new ListItem("-- Error Carga Colab. --", "0"));
            }
        }

        // Cargar GridView aplicando filtros
        private void BindGrid()
        {
            try
            {
                int? tipoObjFiltro = GetNullableIntFromDDL(ddlTipoObjetivoFiltro);
                int? areaFiltro = GetNullableIntFromDDL(ddlAreaFiltro);
                int? numMetaFiltro = GetNullableIntFromTextBox(txtNumMetaFiltro);

                // CAMBIO AQUÍ: Leer del DropDownList ddlUsuarioFiltro
                string usuarioFiltroSeleccionado = ddlUsuarioFiltro.SelectedValue;
                // Si "-- Seleccione Usuario --" tiene valor "0", lo convertimos a null para no filtrar
                string usuarioFiltro = (usuarioFiltroSeleccionado == "0" || string.IsNullOrEmpty(usuarioFiltroSeleccionado)) ? null : usuarioFiltroSeleccionado;

                List<MetaIndividualInfo> metasInd = metaIndDAL.ObtenerMetasIndividuales(tipoObjFiltro, numMetaFiltro, areaFiltro, usuarioFiltro, UsuariosXJefe());
                gvMetasInd.DataSource = metasInd;
                gvMetasInd.DataBind();
                litMensaje.Visible = false;
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error crítico al cargar metas individuales: {ex.Message}.", false);
                Console.WriteLine($"Error en BindGrid (MetasInd): {ex.ToString()}");
                gvMetasInd.DataSource = null;
                gvMetasInd.DataBind();
            }
        }

        

        // Cargar DropDowns de Filtros
        private void LoadFilterDropdowns()
        {
            LoadTiposObjetivoDropdown(ddlTipoObjetivoFiltro, "-- Todos los Tipos --");
            LoadAreasDropdown(ddlAreaFiltro, "-- Todas las Áreas --");
        }

        public string UsuariosXJefe()
        {
            
            try
            {
                string cadena = "";
                DataTable dtColaboradores = apiRH.ObtenerColaboradoresUsuarioJefe(Session["UsuarioID"].ToString());
                for (int i = 0; i < dtColaboradores.Rows.Count; i++)
                {
                    if(i == 0)
                    {
                        cadena =  $"'{dtColaboradores.Rows[i][0].ToString()}'";
                    }
                    else
                    {
                        cadena = cadena + $",'{dtColaboradores.Rows[i][0].ToString()}'";
                    }
                   
                }
                return cadena;

            }
            catch (Exception)
            {

                return null;
            }

            
        }



    // Cargar DropDowns del Modal Add/Edit
    private void LoadModalDropdowns()
        {
            LoadMetasDepDropdown(ddlModalMetaDep, "-- Seleccione Meta Dep. --");
            LoadEstadosDropdown(ddlModalEstado, "-- Seleccione Estado --", ID_CLASE_META_IND); // Filter by Class 4
            LoadUsuariosFiltroDropdown(ddlModalUsuario, "-- Seleccione Usuario --");
        }

        // Métodos genéricos para cargar DropDownLists
        private void LoadTiposObjetivoDropdown(DropDownList ddl, string initialText)
        {
            if (ddl == null) return; try { ddl.DataSource = tipoObjetivoDAL.ObtenerTiposObjetivo(true); ddl.DataTextField = "Nombre"; ddl.DataValueField = "Id_Tipo_Objetivo"; ddl.DataBind(); ddl.Items.Insert(0, new ListItem(initialText, "0")); } catch (Exception ex) { MostrarMensaje($"Error cargando tipos: {ex.Message}", false); }
        }
        private void LoadAreasDropdown(DropDownList ddl, string initialText)
        {
            if (ddl == null) return; try { ddl.DataSource = areaDAL.ObtenerAreas(Session["UsuarioID"].ToString()); ddl.DataTextField = "Nombre"; ddl.DataValueField = "Id_Area_Ejecutora"; ddl.DataBind(); ddl.Items.Insert(0, new ListItem(initialText, "0")); } catch (Exception ex) { MostrarMensaje($"Error cargando áreas: {ex.Message}", false); }
        }
        private void LoadMetasDepDropdown(DropDownList ddl, string initialText)
        {
            if (ddl == null) return; try
            {
                ddl.DataSource = metaDepDAL.ObtenerMetasDepartamentales(Session["UsuarioID"].ToString());
                ddl.DataTextField = "DisplayTextForDropdown";
                ddl.DataValueField = "IdMetaDepartamental";
                ddl.DataBind(); ddl.Items.Insert(0, new ListItem(initialText, "0"));
            }
            catch (Exception ex) { MostrarMensaje($"Error cargando metas dep.: {ex.Message}", false); }
        }
        private void LoadEstadosDropdown(DropDownList ddl, string initialText, int? idClaseFiltro = null)
        {
            if (ddl == null) return; try { ddl.DataSource = detalleEstadoDAL.ObtenerDetallesEstado(idClaseFiltro); ddl.DataTextField = "Descripcion"; ddl.DataValueField = "Id_Detalle_Estado"; ddl.DataBind(); ddl.Items.Insert(0, new ListItem(initialText, "0")); } catch (Exception ex) { MostrarMensaje($"Error cargando estados: {ex.Message}", false); }
        }

        // Mostrar mensajes (página principal)
        private void MostrarMensaje(string texto, bool esExito)
        {
            litMensaje.Text = $"<div class='alert alert-{(esExito ? "success" : "danger")} alert-dismissible fade show' role='alert'>{Server.HtmlEncode(texto)}<button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button></div>";
            litMensaje.Visible = true;
        }
        // Mostrar mensajes (modal)
        private void MostrarMensajeModal(string texto, bool esExito)
        {
            litModalMensaje.Text = $"<div class='alert alert-{(esExito ? "success" : "danger")} mb-3'>{Server.HtmlEncode(texto)}</div>";
            litModalMensaje.Visible = true;
        }

        // Eventos de Filtros
        protected void ddlTipoObjetivoFiltro_SelectedIndexChanged(object sender, EventArgs e) { gvMetasInd.PageIndex = 0; BindGrid(); }
        protected void ddlAreaFiltro_SelectedIndexChanged(object sender, EventArgs e) { gvMetasInd.PageIndex = 0; BindGrid(); } // Added handler
        protected void btnFiltrar_Click(object sender, EventArgs e) { gvMetasInd.PageIndex = 0; BindGrid(); }
        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            ddlTipoObjetivoFiltro.SelectedValue = "0"; // O SelectedIndex = 0;
            ddlAreaFiltro.SelectedValue = "0";      // O SelectedIndex = 0;
            ddlUsuarioFiltro.SelectedValue = "0";   // <-- AÑADIR ESTA LÍNEA (O SelectedIndex = 0;)
            txtNumMetaFiltro.Text = "";
                                        // Si eliminaste txtUsuarioFiltro del ASPX, elimina esta línea también.
                                        // Si el TextBox sigue existiendo pero oculto, o si lo renombraste, ajusta.
                                        // Por seguridad, si ya no existe txtUsuarioFiltro, elimina la línea anterior.

            gvMetasInd.PageIndex = 0;
            BindGrid();
        }

        // Paginación
        protected void gvMetasInd_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvMetasInd.PageIndex = e.NewPageIndex; BindGrid(); }

        // Comandos del GridView (Editar/Eliminar)
        protected void gvMetasInd_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int metaIndId = 0;
            if (!string.IsNullOrEmpty(e.CommandArgument?.ToString()) && int.TryParse(e.CommandArgument.ToString(), out metaIndId))
            {
                if (e.CommandName == "EditarMetaInd") { CargarDatosModalParaEditar(metaIndId); }
                else if (e.CommandName == "EliminarMetaInd") { EliminarMetaIndividual(metaIndId); }
            }
            else if (e.CommandName == "EditarMetaInd" || e.CommandName == "EliminarMetaInd")
            {
                MostrarMensaje("Error: No se pudo obtener el ID de la meta individual.", false);
            }
        }

        // Botón "Agregar Nueva Meta Individual"
        protected void btnAbrirModalAgregar_Click(object sender, EventArgs e)
        {
            hfMetaIndId.Value = "0";
            lblModalTitle.Text = "Agregar Nueva Meta Individual";
            LimpiarCamposModal();
            litModalMensaje.Visible = false;
            LoadModalDropdowns();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowMetaIndModalScript", "showModal('metaIndModal');", true);
        }

        // Cargar datos en modal para editar
        private void CargarDatosModalParaEditar(int metaIndId)
        {
            try
            {
                MetaIndividualInfo metaInd = metaIndDAL.ObtenerMetaIndividualPorId(metaIndId);
                if (metaInd != null)
                {
                    hfMetaIndId.Value = metaInd.IdMetaIndividual.ToString();
                    lblModalTitle.Text = "Editar Meta Individual";
                    litModalMensaje.Visible = false;

                    LoadModalDropdowns(); // Esto ya llama a LoadUsuariosFiltroDropdown(ddlModalUsuario,...)

                    // Seleccionar valores para los DropDownLists
                    SetSelectedValue(ddlModalMetaDep, metaInd.IdMetaDepartamental); // SetSelectedValue es para int?
                    SetSelectedValue(ddlModalEstado, metaInd.IdDetalleEstado);   // SetSelectedValue es para int?

                    // CAMBIO: Seleccionar el usuario en ddlModalUsuario
                    // metaInd.Usuario contiene el Id_Colaborador (string)
                    if (!string.IsNullOrEmpty(metaInd.Usuario))
                    {
                        ListItem itemUsuario = ddlModalUsuario.Items.FindByValue(metaInd.Usuario);
                        if (itemUsuario != null)
                        {
                            ddlModalUsuario.ClearSelection();
                            itemUsuario.Selected = true;
                        }
                        else
                        {
                            // El usuario de la meta no está en la lista (ej. inactivo o de otro jefe)
                            // Mostrar advertencia y dejar en "-- Seleccione Usuario --"
                            MostrarMensajeModal($"Advertencia: El usuario '{metaInd.Usuario}' de esta meta no es una opción seleccionable en la lista. Por favor, elija uno.", false);
                            if (ddlModalUsuario.Items.FindByValue("0") != null) { ddlModalUsuario.SelectedValue = "0"; }
                        }
                    }
                    else
                    {
                        // No hay usuario asignado, seleccionar el item por defecto
                        if (ddlModalUsuario.Items.FindByValue("0") != null) { ddlModalUsuario.SelectedValue = "0"; }
                    }

                    txtModalDescMetaInd.Text = metaInd.Descripcion;
                    txtModalAlcance.Text = metaInd.Alcance;
                    txtModalPesoN4.Text = metaInd.PesoPonderadoN4?.ToString() ?? string.Empty;
                    txtModalPesoN5.Text = metaInd.PesoPonderadoN5?.ToString() ?? string.Empty;
                    txtModalFechaIni.Text = metaInd.FechaInicial?.ToString("yyyy-MM-dd") ?? string.Empty;
                    txtModalFechaFin.Text = metaInd.FechaFinal?.ToString("yyyy-MM-dd") ?? string.Empty;
                    chkModalFinalizable.Checked = !(metaInd.EsFinalizable ?? true);
                    // txtModalUsuario.Text = metaInd.Usuario; // YA NO SE USA EL TEXTBOX

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowMetaIndModalScript", "showModal('metaIndModal');", true);
                }
                else { MostrarMensaje("Error: Meta Individual no encontrada.", false); }
            }
            catch (Exception ex) { MostrarMensaje($"Error al cargar datos para editar: {ex.Message}", false); Console.WriteLine($"Error CargarDatosModalParaEditar (MetaInd): {ex}"); }
        }

        // Botón "Guardar" del Modal
        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            Page.Validate("ModalValidation");
            if (!Page.IsValid)
            {
                MostrarMensajeModal("Corrija los errores.", false);
                // ScriptManager.RegisterStartupScript(this, this.GetType(), "ReShowMetaIndModalOnError", "showModal('metaIndModal');", true); // Se maneja en el goto
                goto ShowModalOnError; // Mantener el modal abierto
            }

            try
            {
                int metaIndId = Convert.ToInt32(hfMetaIndId.Value);
                int? idMetaDep = GetNullableIntFromDDL(ddlModalMetaDep);
                // CAMBIO: Leer del DropDownList
                string usuario = ddlModalUsuario.SelectedValue;
                string desc = txtModalDescMetaInd.Text.Trim();
                string alcance = txtModalAlcance.Text.Trim();
                int? pesoN4 = GetNullableIntFromTextBox(txtModalPesoN4);
                int? pesoN5 = GetNullableIntFromTextBox(txtModalPesoN5);
                DateTime? fechaIni = GetNullableDateTimeFromTextBox(txtModalFechaIni);
                DateTime? fechaFin = GetNullableDateTimeFromTextBox(txtModalFechaFin);
                bool? esFinalizable = !chkModalFinalizable.Checked;
                int? idEstado = GetNullableIntFromDDL(ddlModalEstado);

                bool success = false; string actionMessage = "";

                // Validations
                if (!idMetaDep.HasValue) { MostrarMensajeModal("Seleccione Meta Dep.", false); goto ShowModalOnError; }
                if (string.IsNullOrEmpty(usuario) || usuario == "0") { MostrarMensajeModal("Seleccione Usuario Asignado.", false); goto ShowModalOnError; }
                if (!idEstado.HasValue) { MostrarMensajeModal("Seleccione Estado.", false); goto ShowModalOnError; }
                // Add other validations...

                if (metaIndId > 0)
                { // Editar
                    success = metaIndDAL.ActualizarMetaIndividual(metaIndId, idMetaDep, usuario, desc, alcance, pesoN4, pesoN5, fechaIni, fechaFin, esFinalizable, idEstado);
                    actionMessage = success ? "Meta Individual actualizada." : "Error al actualizar.";
                }
                else
                { // Agregar
                    int nuevoId = metaIndDAL.InsertarMetaIndividual(idMetaDep, usuario, desc, alcance, pesoN4, pesoN5, fechaIni, fechaFin, esFinalizable, idEstado);
                    success = (nuevoId > 0);
                    actionMessage = success ? "Meta Individual agregada." : "Error al agregar.";
                }

                if (success)
                {
                    BindGrid();
                    MostrarMensaje(actionMessage, true);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideMetaIndModalScript", "hideModal('metaIndModal');", true);
                }
                else
                {
                    MostrarMensajeModal(actionMessage, false);
                    goto ShowModalOnError; // Keep modal open
                }
            }
            catch (Exception ex)
            {
                MostrarMensajeModal($"Error al guardar: {ex.Message}", false);
                Console.WriteLine($"Error btnGuardarModal_Click (MetaInd): {ex}");
                goto ShowModalOnError; // Keep modal open on exception
            }

        // Label used to jump here on validation/save error to keep modal open
        ShowModalOnError:
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ReShowMetaIndModalOnError", "showModal('metaIndModal');", true);
        }

        // Lógica para Eliminar
        private void EliminarMetaIndividual(int metaIndId)
        {
            if (!ID_ESTADO_INACTIVO_META_IND.HasValue) { MostrarMensaje("Error config: Estado inactivo no definido.", false); return; }
            try
            {
                bool eliminado = metaIndDAL.EliminarMetaIndividualLogico(metaIndId, ID_ESTADO_INACTIVO_META_IND.Value);
                if (eliminado) { BindGrid(); MostrarMensaje("Meta Individual marcada como inactiva.", true); }
                else { MostrarMensaje("Error al marcar meta individual como inactiva.", false); }
            }
            catch (InvalidOperationException ioEx) { MostrarMensaje($"Error: {ioEx.Message}", false); } // Catch specific FK error
            catch (Exception ex) { MostrarMensaje($"Error al eliminar: {ex.Message}", false); Console.WriteLine($"Error EliminarMetaIndividual: {ex}"); }
        }

        // --- Funciones auxiliares ---
        private int? GetNullableIntFromDDL(DropDownList ddl) { if (ddl != null && int.TryParse(ddl.SelectedValue, out int result) && result > 0) { return result; } return null; }
        private int? GetNullableIntFromTextBox(TextBox txt) { if (txt != null && int.TryParse(txt.Text, out int result)) { return result; } return null; }
        private DateTime? GetNullableDateTimeFromTextBox(TextBox txt) { if (txt != null && DateTime.TryParse(txt.Text, out DateTime result)) { return result; } return null; }
        private void SetSelectedValue(DropDownList ddl, int? value) { if (ddl != null && value.HasValue) { ListItem item = ddl.Items.FindByValue(value.Value.ToString()); if (item != null) { ddl.SelectedValue = value.Value.ToString(); } else { ddl.SelectedIndex = 0; } } else if (ddl != null) { ddl.SelectedIndex = 0; } }
        private void LimpiarCamposModal()
        {
            hfMetaIndId.Value = "0";
            SafeClearSelection(ddlModalMetaDep);
            // txtModalUsuario.Text = ""; // YA NO SE USA EL TEXTBOX
            SafeClearSelection(ddlModalUsuario); // <-- AÑADIR ESTA LÍNEA para resetear el nuevo DDL
            txtModalDescMetaInd.Text = "";
            txtModalAlcance.Text = "";
            txtModalPesoN4.Text = "";
            txtModalPesoN5.Text = "";
            txtModalFechaIni.Text = "";
            txtModalFechaFin.Text = "";
            chkModalFinalizable.Checked = false;
            SafeClearSelection(ddlModalEstado);
            litModalMensaje.Visible = false;
        }
        private void SafeClearSelection(DropDownList ddl) { if (ddl != null && ddl.Items.Count > 0) { ddl.ClearSelection(); if (ddl.Items[0].Value == "0") { ddl.SelectedIndex = 0; } } }
        // Helper function to truncate strings (needed for GridView display)
        protected string TruncateString(object text, int maxLength)
        {
            if (text == null) return string.Empty;
            string str = text.ToString();
            if (str.Length <= maxLength) return str;
            return str.Substring(0, maxLength) + "...";
        }
        protected string TruncateStringWithEllipsis(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
        }


    } // End Class
}