using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gestor_Desempeno
{
    public partial class GestionMetas : System.Web.UI.Page
    {
        // DALs
        private MetaDAL metaDAL = new MetaDAL();
        private ObjetivoDAL objetivoDAL = new ObjetivoDAL();
        private TipoObjetivoDAL tipoObjetivoDAL = new TipoObjetivoDAL();
        private DetalleEstadoDAL detalleEstadoDAL = new DetalleEstadoDAL();
        private UsuarioDAL usuarioDAL_Instance = new UsuarioDAL(); // For security checks

        // ID Estado Inactivo para Metas (AJUSTAR!)
        private int? ID_ESTADO_INACTIVO_META = null; // Load in Page_Load
        // Determine Clase for Meta states if needed (e.g., if states are shared)
        // ** FIXED: Removed 'const' keyword as nullable types cannot be const **
        private int? ID_CLASE_META = 2; // Example: Set to appropriate Clase ID if filtering states

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
            if (ID_ESTADO_INACTIVO_META == null)
            {
                ID_ESTADO_INACTIVO_META = detalleEstadoDAL.ObtenerIdEstadoPorDescripcion("Inactivo", ID_CLASE_META); // Pass class ID if needed
                if (ID_ESTADO_INACTIVO_META == null)
                {
                    Console.WriteLine($"CRITICAL ERROR: Could not find 'Inactivo' state ID for Metas (Clase: {ID_CLASE_META}). Logical delete will fail.");
                    // Consider showing persistent error
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
                int? objetivoFiltro = GetNullableIntFromDDL(ddlObjetivoFiltro);
                int? numMetaFiltro = GetNullableIntFromTextBox(txtNumMetaFiltro);

                List<MetaInfo> metas = metaDAL.ObtenerMetas(tipoObjFiltro, objetivoFiltro, numMetaFiltro);
                gvMetas.DataSource = metas;
                gvMetas.DataBind();
                litMensaje.Visible = false;
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error crítico al cargar metas: {ex.Message}.", false);
                Console.WriteLine($"Error en BindGrid (Metas): {ex.ToString()}");
                gvMetas.DataSource = null;
                gvMetas.DataBind();
            }
        }

        // Cargar DropDowns de Filtros
        private void LoadFilterDropdowns()
        {
            LoadTiposObjetivoDropdown(ddlTipoObjetivoFiltro, "-- Todos los Tipos --");
            LoadObjetivosDropdown(ddlObjetivoFiltro, "-- Todos los Objetivos --");
        }

        // Cargar DropDowns del Modal Add/Edit
        private void LoadModalDropdowns()
        {
            LoadObjetivosDropdown(ddlModalObjetivo, "-- Seleccione Objetivo --");
            LoadEstadosDropdown(ddlModalEstado, "-- Seleccione Estado --", ID_CLASE_META); // Pass class ID if needed
        }

        // Métodos genéricos para cargar DropDownLists
        private void LoadTiposObjetivoDropdown(DropDownList ddl, string initialText)
        {
            if (ddl == null) return; try { ddl.DataSource = tipoObjetivoDAL.ObtenerTiposObjetivo(true); ddl.DataTextField = "Nombre"; ddl.DataValueField = "Id_Tipo_Objetivo"; ddl.DataBind(); ddl.Items.Insert(0, new ListItem(initialText, "0")); } catch (Exception ex) { MostrarMensaje($"Error cargando tipos: {ex.Message}", false); }
        }
        private void LoadObjetivosDropdown(DropDownList ddl, string initialText)
        {
            if (ddl == null) return; try { ddl.DataSource = objetivoDAL.ObtenerObjetivos(); ddl.DataTextField = "Nombre"; ddl.DataValueField = "Id_Objetivo"; ddl.DataBind(); ddl.Items.Insert(0, new ListItem(initialText, "0")); } catch (Exception ex) { MostrarMensaje($"Error cargando objetivos: {ex.Message}", false); }
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
        protected void ddlTipoObjetivoFiltro_SelectedIndexChanged(object sender, EventArgs e) { gvMetas.PageIndex = 0; BindGrid(); }
        protected void ddlObjetivoFiltro_SelectedIndexChanged(object sender, EventArgs e) { gvMetas.PageIndex = 0; BindGrid(); }
        protected void btnFiltrar_Click(object sender, EventArgs e) { gvMetas.PageIndex = 0; BindGrid(); }
        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            ddlTipoObjetivoFiltro.SelectedIndex = 0;
            ddlObjetivoFiltro.SelectedIndex = 0;
            txtNumMetaFiltro.Text = "";
            gvMetas.PageIndex = 0;
            BindGrid();
        }

        // Paginación
        protected void gvMetas_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvMetas.PageIndex = e.NewPageIndex; BindGrid(); }

        // Comandos del GridView (Editar/Eliminar)
        protected void gvMetas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int metaId = 0;
            if (!string.IsNullOrEmpty(e.CommandArgument?.ToString())) { metaId = Convert.ToInt32(e.CommandArgument); }

            if (e.CommandName == "EditarMeta") { CargarDatosModalParaEditar(metaId); }
            else if (e.CommandName == "EliminarMeta") { EliminarMeta(metaId); }
        }

        // Botón "Agregar Nueva Meta"
        protected void btnAbrirModalAgregar_Click(object sender, EventArgs e)
        {
            hfMetaId.Value = "0";
            lblModalTitle.Text = "Agregar Nueva Meta";
            LimpiarCamposModal();
            litModalMensaje.Visible = false;
            //LoadModalDropdowns(); // Ensure dropdowns are loaded
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowMetaModalScript", "showModal('metaModal');", true);
        }

        // Cargar datos en modal para editar
        private void CargarDatosModalParaEditar(int metaId)
        {
            try
            {
                MetaInfo meta = metaDAL.ObtenerMetaPorId(metaId);
                if (meta != null)
                {
                    hfMetaId.Value = meta.IdMeta.ToString();
                    lblModalTitle.Text = "Editar Meta";
                    litModalMensaje.Visible = false;

                    LoadModalDropdowns(); // Load dropdowns first
                    SetSelectedValue(ddlModalObjetivo, meta.IdObjetivo);
                    SetSelectedValue(ddlModalEstado, meta.Id_Detalle_Estado);

                    txtModalNumMeta.Text = meta.NumMeta?.ToString() ?? string.Empty;
                    txtModalDescripcion.Text = meta.Descripcion;

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowMetaModalScript", "showModal('metaModal');", true);
                }
                else { MostrarMensaje("Error: Meta no encontrada.", false); }
            }
            catch (Exception ex) { MostrarMensaje($"Error al cargar datos para editar: {ex.Message}", false); Console.WriteLine($"Error CargarDatosModalParaEditar: {ex}"); }
        }

        // Botón "Guardar" del Modal
        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            Page.Validate("ModalValidation");
            if (!Page.IsValid)
            {
                MostrarMensajeModal("Corrija los errores.", false);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ReShowMetaModalOnError", "showModal('metaModal');", true);
                return;
            }

            try
            {
                int metaId = Convert.ToInt32(hfMetaId.Value);
                int? idObjetivo = GetNullableIntFromDDL(ddlModalObjetivo);
                int? numMeta = GetNullableIntFromTextBox(txtModalNumMeta);
                string descripcion = txtModalDescripcion.Text.Trim();
                int? idEstado = GetNullableIntFromDDL(ddlModalEstado);

                bool success = false; string actionMessage = "";

                // Validations
                if (!idObjetivo.HasValue) { MostrarMensajeModal("Seleccione Objetivo.", false); ScriptManager.RegisterStartupScript(this, this.GetType(), "ReShowMetaModalOnError", "showModal('metaModal');", true); return; }
                if (!idEstado.HasValue) { MostrarMensajeModal("Seleccione Estado.", false); ScriptManager.RegisterStartupScript(this, this.GetType(), "ReShowMetaModalOnError", "showModal('metaModal');", true); return; }
                // NumMeta could be optional

                if (metaId > 0)
                { // Editar
                    success = metaDAL.ActualizarMeta(metaId, idObjetivo, numMeta, descripcion, idEstado);
                    actionMessage = success ? "Meta actualizada." : "Error al actualizar.";
                }
                else
                { // Agregar
                    int nuevoId = metaDAL.InsertarMeta(idObjetivo, numMeta, descripcion, idEstado);
                    success = (nuevoId > 0);
                    actionMessage = success ? "Meta agregada." : "Error al agregar.";
                }

                if (success)
                {
                    BindGrid();
                    MostrarMensaje(actionMessage, true);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideMetaModalScript", "hideModal('metaModal');", true);
                }
                else
                {
                    MostrarMensajeModal(actionMessage, false);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ReShowMetaModalOnError", "showModal('metaModal');", true);
                }
            }
            catch (Exception ex)
            {
                MostrarMensajeModal($"Error al guardar: {ex.Message}", false);
                Console.WriteLine($"Error btnGuardarModal_Click: {ex}");
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ReShowMetaModalOnError", "showModal('metaModal');", true);
            }
        }

        // Lógica para Eliminar
        private void EliminarMeta(int metaId)
        {
            if (!ID_ESTADO_INACTIVO_META.HasValue) { MostrarMensaje("Error config: Estado inactivo no definido.", false); return; }
            try
            {
                bool eliminado = metaDAL.EliminarMetaLogico(metaId, ID_ESTADO_INACTIVO_META.Value);
                if (eliminado) { BindGrid(); MostrarMensaje("Meta marcada como inactiva.", true); }
                else { MostrarMensaje("Error al marcar meta como inactiva.", false); }
            }
            catch (InvalidOperationException ioEx) { MostrarMensaje($"Error: {ioEx.Message}", false); } // Catch specific FK error
            catch (Exception ex) { MostrarMensaje($"Error al eliminar: {ex.Message}", false); Console.WriteLine($"Error EliminarMeta: {ex}"); }
        }

        // --- Funciones auxiliares ---
        private int? GetNullableIntFromDDL(DropDownList ddl) { if (ddl != null && int.TryParse(ddl.SelectedValue, out int result) && result > 0) { return result; } return null; }
        private int? GetNullableIntFromTextBox(TextBox txt) { if (txt != null && int.TryParse(txt.Text, out int result)) { return result; } return null; }
        private void SetSelectedValue(DropDownList ddl, int? value) { if (ddl != null && value.HasValue) { ListItem item = ddl.Items.FindByValue(value.Value.ToString()); if (item != null) { ddl.SelectedValue = value.Value.ToString(); } else { ddl.SelectedIndex = 0; } } else if (ddl != null) { ddl.SelectedIndex = 0; } }
        private void LimpiarCamposModal()
        {
            hfMetaId.Value = "0";
            ddlModalObjetivo.SelectedIndex = 0;
            txtModalNumMeta.Text = "";
            txtModalDescripcion.Text = "";
            ddlModalEstado.SelectedIndex = 0;
            litModalMensaje.Visible = false;
        }

    } // End Class
}