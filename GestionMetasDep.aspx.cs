using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gestor_Desempeno
{
    public partial class GestionMetasDep : System.Web.UI.Page
    {
        // DALs
        private MetaDepartamentalDAL metaDepDAL = new MetaDepartamentalDAL();
        private MetaDAL metaDAL = new MetaDAL(); // Needed for dropdown
        private AreaEjecutoraDAL areaDAL = new AreaEjecutoraDAL(); // Needed for dropdown
        private DetalleEstadoDAL detalleEstadoDAL = new DetalleEstadoDAL(); // Needed for dropdown
        private TipoObjetivoDAL tipoObjetivoDAL = new TipoObjetivoDAL(); // Needed for filter
        private UsuarioDAL usuarioDAL_Instance = new UsuarioDAL(); // For security checks


        // ID Estado Inactivo para Metas Departamentales (Clase 3 - AJUSTAR!)
        private int? ID_ESTADO_INACTIVO_META_DEP = null;
        private const int ID_CLASE_META_DEP = 3; // Clase para estados de Meta Departamental

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

            // Load Inactive State ID once
            if (ID_ESTADO_INACTIVO_META_DEP == null)
            {
                ID_ESTADO_INACTIVO_META_DEP = detalleEstadoDAL.ObtenerIdEstadoPorDescripcion("Inactivo", ID_CLASE_META_DEP);
                if (ID_ESTADO_INACTIVO_META_DEP == null)
                {
                    Console.WriteLine($"CRITICAL ERROR: Could not find 'Inactivo' state ID for Clase {ID_CLASE_META_DEP}. Logical delete will fail.");
                }
            }

            if (!IsPostBack)
            {
                LoadFilterDropdowns();
                LoadModalDropdowns(); // Load dropdowns for the modal
                BindGrid();
            }
            if (IsPostBack && !ScriptManager.GetCurrent(this.Page).IsInAsyncPostBack)
            {
                litMensaje.Visible = false;
                litModalMensaje.Visible = false;
            }
        }

        // Cargar GridView aplicando filtros
        private void BindGrid()
        {
            try
            {
                int? tipoObjFiltro = GetNullableIntFromDDL(ddlTipoObjetivoFiltro);
                int? numMetaFiltro = GetNullableIntFromTextBox(txtNumMetaFiltro);

                List<MetaDepartamentalInfo> metasDep = metaDepDAL.ObtenerMetasDepartamentales(Session["UsuarioID"].ToString(), tipoObjFiltro, numMetaFiltro);
                gvMetasDep.DataSource = metasDep;
                gvMetasDep.DataBind();
                litMensaje.Visible = false;
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error crítico al cargar metas departamentales: {ex.Message}.", false);
                Console.WriteLine($"Error en BindGrid (MetasDep): {ex.ToString()}");
                gvMetasDep.DataSource = null;
                gvMetasDep.DataBind();
            }
        }

        // Cargar DropDowns de Filtros
        private void LoadFilterDropdowns()
        {
            LoadTiposObjetivoDropdown(ddlTipoObjetivoFiltro, "-- Todos los Tipos --");
        }

        // Cargar DropDowns del Modal Add/Edit
        private void LoadModalDropdowns()
        {
            LoadMetasDropdown(ddlModalMeta, "-- Seleccione Meta Padre --");
            LoadAreasDropdown(ddlModalArea, "-- Seleccione Área Ejecutora --");
            LoadEstadosDropdown(ddlModalEstado, "-- Seleccione Estado --", ID_CLASE_META_DEP); // Filter by Class 3
        }

        // Métodos genéricos para cargar DropDownLists
        private void LoadTiposObjetivoDropdown(DropDownList ddl, string initialText)
        {
            if (ddl == null) return; try { ddl.DataSource = tipoObjetivoDAL.ObtenerTiposObjetivo(true); ddl.DataTextField = "Nombre"; ddl.DataValueField = "Id_Tipo_Objetivo"; ddl.DataBind(); ddl.Items.Insert(0, new ListItem(initialText, "0")); } catch (Exception ex) { MostrarMensaje($"Error cargando tipos: {ex.Message}", false); }
        }
        private void LoadMetasDropdown(DropDownList ddl, string initialText)
        {
            if (ddl == null) return; try
            {
                var metas = metaDAL.ObtenerMetas();
                var displayList = metas.Select(m => new {
                    IdMeta = m.IdMeta,
                    // Use the combined display text property from MetaInfo
                    DisplayText = TruncateStringWithEllipsis($"{m.NumMeta ?? 0}. {m.Descripcion ?? ""}", 70) // Truncate for dropdown
                }).ToList();
                ddl.DataSource = displayList;
                ddl.DataTextField = "DisplayText"; ddl.DataValueField = "IdMeta";
                ddl.DataBind(); ddl.Items.Insert(0, new ListItem(initialText, "0"));
            }
            catch (Exception ex) { MostrarMensaje($"Error cargando metas padre: {ex.Message}", false); }
        }
        private void LoadAreasDropdown(DropDownList ddl, string initialText)
        {
            if (ddl == null) return; try { ddl.DataSource = areaDAL.ObtenerAreas(Session["UsuarioID"].ToString()); ddl.DataTextField = "Nombre"; ddl.DataValueField = "Id_Area_Ejecutora"; ddl.DataBind(); ddl.Items.Insert(0, new ListItem(initialText, "0")); } catch (Exception ex) { MostrarMensaje($"Error cargando áreas: {ex.Message}", false); }
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
        protected void ddlTipoObjetivoFiltro_SelectedIndexChanged(object sender, EventArgs e) { gvMetasDep.PageIndex = 0; BindGrid(); }
        protected void btnFiltrar_Click(object sender, EventArgs e) { gvMetasDep.PageIndex = 0; BindGrid(); }
        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            ddlTipoObjetivoFiltro.SelectedIndex = 0;
            txtNumMetaFiltro.Text = "";
            gvMetasDep.PageIndex = 0;
            BindGrid();
        }

        // Paginación
        protected void gvMetasDep_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvMetasDep.PageIndex = e.NewPageIndex; BindGrid(); }

        // Comandos del GridView (Editar/Eliminar)
        protected void gvMetasDep_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int metaDepId = 0;
            if (!string.IsNullOrEmpty(e.CommandArgument?.ToString()) && int.TryParse(e.CommandArgument.ToString(), out metaDepId))
            {
                if (e.CommandName == "EditarMetaDep") { CargarDatosModalParaEditar(metaDepId); }
                else if (e.CommandName == "EliminarMetaDep") { EliminarMetaDepartamental(metaDepId); }
            }
            else if (e.CommandName == "EditarMetaDep" || e.CommandName == "EliminarMetaDep")
            {
                MostrarMensaje("Error: No se pudo obtener el ID de la meta departamental.", false);
            }
        }

        // Botón "Agregar Nueva Meta Departamental"
        protected void btnAbrirModalAgregar_Click(object sender, EventArgs e)
        {
            hfMetaDepId.Value = "0";
            lblModalTitle.Text = "Agregar Nueva Meta Departamental";
            LimpiarCamposModal();
            litModalMensaje.Visible = false;
            //LoadModalDropdowns();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowMetaDepModalScript", "showModal('metaDepModal');", true);
        }

        // Cargar datos en modal para editar
        private void CargarDatosModalParaEditar(int metaDepId)
        {
            try
            {
                MetaDepartamentalInfo metaDep = metaDepDAL.ObtenerMetaDepartamentalPorId(metaDepId);
                if (metaDep != null)
                {
                    hfMetaDepId.Value = metaDep.IdMetaDepartamental.ToString();
                    lblModalTitle.Text = "Editar Meta Departamental";
                    litModalMensaje.Visible = false;

                    LoadModalDropdowns();
                    SetSelectedValue(ddlModalMeta, metaDep.IdMeta);
                    SetSelectedValue(ddlModalArea, metaDep.Id_Area_Ejecutora);
                    SetSelectedValue(ddlModalEstado, metaDep.Id_Detalle_Estado);

                    txtModalDescMetaDep.Text = metaDep.Descripcion;
                    txtModalPeso.Text = metaDep.PesoPonderado?.ToString() ?? string.Empty;
                    txtModalIndicador.Text = metaDep.Indicador;
                    txtModalAlcance.Text = metaDep.Alcance;
                    txtModalPrioridad.Text = metaDep.Prioridad?.ToString() ?? string.Empty;
                    txtModalFechaIni.Text = metaDep.FechaInicial?.ToString("yyyy-MM-dd") ?? string.Empty;
                    txtModalFechaFin.Text = metaDep.FechaFinal?.ToString("yyyy-MM-dd") ?? string.Empty;

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowMetaDepModalScript", "showModal('metaDepModal');", true);
                }
                else { MostrarMensaje("Error: Meta Departamental no encontrada.", false); }
            }
            catch (Exception ex) { MostrarMensaje($"Error al cargar datos para editar: {ex.Message}", false); Console.WriteLine($"Error CargarDatosModalParaEditar (MetaDep): {ex}"); }
        }

        // Botón "Guardar" del Modal
        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            Page.Validate("ModalValidation");
            if (!Page.IsValid)
            {
                MostrarMensajeModal("Corrija los errores.", false);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ReShowMetaDepModalOnError", "showModal('metaDepModal');", true);
                return;
            }

            try
            {
                int metaDepId = Convert.ToInt32(hfMetaDepId.Value);
                int? idMeta = GetNullableIntFromDDL(ddlModalMeta);
                int? idArea = GetNullableIntFromDDL(ddlModalArea);
                string desc = txtModalDescMetaDep.Text.Trim();
                int? peso = GetNullableIntFromTextBox(txtModalPeso);
                string indicador = txtModalIndicador.Text.Trim();
                string alcance = txtModalAlcance.Text.Trim();
                int? prioridad = GetNullableIntFromTextBox(txtModalPrioridad);
                DateTime? fechaIni = GetNullableDateTimeFromTextBox(txtModalFechaIni);
                DateTime? fechaFin = GetNullableDateTimeFromTextBox(txtModalFechaFin);
                int? idEstado = GetNullableIntFromDDL(ddlModalEstado);

                bool success = false; string actionMessage = "";

                // Validations
                if (!idMeta.HasValue) { MostrarMensajeModal("Seleccione Meta Padre.", false); goto ShowModalOnError; }
                if (!idArea.HasValue) { MostrarMensajeModal("Seleccione Área Ejecutora.", false); goto ShowModalOnError; }
                if (!idEstado.HasValue) { MostrarMensajeModal("Seleccione Estado.", false); goto ShowModalOnError; }

                if (metaDepId > 0)
                { // Editar
                    success = metaDepDAL.ActualizarMetaDepartamental(metaDepId, idMeta, idArea, desc, peso, indicador, alcance, prioridad, fechaIni, fechaFin, idEstado);
                    actionMessage = success ? "Meta Departamental actualizada." : "Error al actualizar.";
                }
                else
                { // Agregar
                    int nuevoId = metaDepDAL.InsertarMetaDepartamental(idMeta, idArea, desc, peso, indicador, alcance, prioridad, fechaIni, fechaFin, idEstado);
                    success = (nuevoId > 0);
                    actionMessage = success ? "Meta Departamental agregada." : "Error al agregar.";
                }

                if (success)
                {
                    BindGrid();
                    MostrarMensaje(actionMessage, true);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideMetaDepModalScript", "hideModal('metaDepModal');", true);
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
                Console.WriteLine($"Error btnGuardarModal_Click (MetaDep): {ex}");
                goto ShowModalOnError; // Keep modal open on exception
            }

        // Label used to jump here on validation/save error to keep modal open
        ShowModalOnError:
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ReShowMetaDepModalOnError", "showModal('metaDepModal');", true);
        }

        // Lógica para Eliminar
        private void EliminarMetaDepartamental(int metaDepId)
        {
            if (!ID_ESTADO_INACTIVO_META_DEP.HasValue) { MostrarMensaje("Error config: Estado inactivo no definido.", false); return; }
            try
            {
                bool eliminado = metaDepDAL.EliminarMetaDepartamentalLogico(metaDepId, ID_ESTADO_INACTIVO_META_DEP.Value);
                if (eliminado) { BindGrid(); MostrarMensaje("Meta Departamental marcada como inactiva.", true); }
                else { MostrarMensaje("Error al marcar meta departamental como inactiva.", false); }
            }
            catch (InvalidOperationException ioEx) { MostrarMensaje($"Error: {ioEx.Message}", false); } // Catch specific FK error
            catch (Exception ex) { MostrarMensaje($"Error al eliminar: {ex.Message}", false); Console.WriteLine($"Error EliminarMetaDepartamental: {ex}"); }
        }

        // --- Funciones auxiliares ---
        private int? GetNullableIntFromDDL(DropDownList ddl) { if (ddl != null && int.TryParse(ddl.SelectedValue, out int result) && result > 0) { return result; } return null; }
        private int? GetNullableIntFromTextBox(TextBox txt) { if (txt != null && int.TryParse(txt.Text, out int result)) { return result; } return null; }
        private DateTime? GetNullableDateTimeFromTextBox(TextBox txt) { if (txt != null && DateTime.TryParse(txt.Text, out DateTime result)) { return result; } return null; }
        private void SetSelectedValue(DropDownList ddl, int? value) { if (ddl != null && value.HasValue) { ListItem item = ddl.Items.FindByValue(value.Value.ToString()); if (item != null) { ddl.SelectedValue = value.Value.ToString(); } else { ddl.SelectedIndex = 0; } } else if (ddl != null) { ddl.SelectedIndex = 0; } }
        private void LimpiarCamposModal()
        {
            hfMetaDepId.Value = "0";
            SafeClearSelection(ddlModalMeta);
            SafeClearSelection(ddlModalArea);
            txtModalDescMetaDep.Text = "";
            txtModalPeso.Text = "";
            txtModalIndicador.Text = "";
            txtModalAlcance.Text = "";
            txtModalPrioridad.Text = "";
            txtModalFechaIni.Text = "";
            txtModalFechaFin.Text = "";
            SafeClearSelection(ddlModalEstado);
            litModalMensaje.Visible = false;
        }
        private void SafeClearSelection(DropDownList ddl) { if (ddl != null && ddl.Items.Count > 0) { ddl.ClearSelection(); if (ddl.Items[0].Value == "0") { ddl.SelectedIndex = 0; } } }
        // Helper function to truncate strings
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