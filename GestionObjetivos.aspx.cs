using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services; // Para WebMethod
using System.Web.Script.Serialization; // Para JSON

namespace Gestor_Desempeno
{
    public partial class GestionObjetivos : System.Web.UI.Page
    {
        // Instancias de DALs
        private ObjetivoDAL objetivoDAL = new ObjetivoDAL();
        private PeriodoDAL periodoDAL = new PeriodoDAL();
        private TipoObjetivoDAL tipoObjetivoDAL = new TipoObjetivoDAL();
        private DetalleEstadoDAL detalleEstadoDAL = new DetalleEstadoDAL();
        private UsuarioDAL usuarioDAL_Instance = new UsuarioDAL(); // For security checks
        private int Id_Detalle_Objetivo_Activo = 1;

        public const int id_claseObjetivos = 1;

        // Constante o configuración para el ID del estado "Inactivo" (AJUSTAR!)
        // ** IMPORTANT: Find the correct ID for 'Inactivo' state, likely from Detalle_Estado where Descripcion = 'Inactivo' **
        private int? ID_ESTADO_INACTIVO = null; // Load in Page_Load

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
            if (ID_ESTADO_INACTIVO == null)
            {
                ID_ESTADO_INACTIVO = detalleEstadoDAL.ObtenerIdEstadoPorDescripcion("Inactivo"); // Assuming "Inactivo" is the description
                if (ID_ESTADO_INACTIVO == null)
                {
                    Console.WriteLine("CRITICAL ERROR: Could not find 'Inactivo' state ID. Logical delete will fail.");
                    // Consider showing a persistent error message
                }
            }


            if (!IsPostBack)
            {
                LoadDropdowns(); // Cargar todos los dropdowns iniciales (filtros y agregar)
                BindGrid();
            }
            // Ocultar mensajes en postbacks normales
            if (IsPostBack && !ScriptManager.GetCurrent(this.Page).IsInAsyncPostBack)
            {
                litMensaje.Visible = false;
                litModalMensaje.Visible = false; // Hide modal message too
            }
        }

        // Cargar GridView aplicando el filtro seleccionado
        private void BindGrid()
        {
            try
            {
                int? idTipoFiltro = GetNullableIntFromDDL(ddlTipoObjetivoFiltro);
                List<ObjetivoInfo> objetivos = objetivoDAL.ObtenerObjetivos(Id_Detalle_Objetivo_Activo,idTipoFiltro);

                gvObjetivos.DataSource = objetivos;
                gvObjetivos.DataBind();
                // Don't hide litMensaje here, let MostrarMensaje handle it
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error crítico al cargar objetivos: {ex.Message}.", false);
                Console.WriteLine($"Error en BindGrid (Objetivos): {ex.ToString()}");
                gvObjetivos.DataSource = null;
                gvObjetivos.DataBind();
            }
        }

        // Cargar todos los DropDownLists necesarios (filtros, agregar y modal)
        private void LoadDropdowns()
        {
            LoadTipoObjetivoFilterDropdown();
            // Load dropdowns for the Add/Edit Modal
            LoadPeriodosDropdown(ddlModalPeriodo);
            LoadTiposObjetivoDropdown(ddlModalTipoObj);
            LoadEstadosDropdown(ddlModalEstado, id_claseObjetivos);
            // Load dropdowns for the old Add form (can be removed if form is gone)
            // LoadPeriodosDropdown(ddlNuevoPeriodo); // No longer needed if form removed
            // LoadTiposObjetivoDropdown(ddlNuevoTipoObj); // No longer needed if form removed
            // LoadEstadosDropdown(ddlNuevoEstado); // No longer needed if form removed
        }

        // Cargar DropDownList de Filtro Tipo Objetivo
        private void LoadTipoObjetivoFilterDropdown()
        {
            try
            {
                ddlTipoObjetivoFiltro.DataSource = tipoObjetivoDAL.ObtenerTiposObjetivo(true);
                ddlTipoObjetivoFiltro.DataTextField = "Nombre";
                ddlTipoObjetivoFiltro.DataValueField = "Id_Tipo_Objetivo";
                ddlTipoObjetivoFiltro.DataBind();
                ddlTipoObjetivoFiltro.Items.Insert(0, new ListItem("-- Todos los Tipos --", "0"));
            }
            catch (Exception ex) { MostrarMensaje($"Error cargando filtro de tipos: {ex.Message}", false); }
        }

        // Métodos genéricos para cargar DropDownLists (reutilizados)
        private void LoadPeriodosDropdown(DropDownList ddl)
        {
            if (ddl == null) return; try { ddl.DataSource = periodoDAL.ObtenerPeriodos(true); ddl.DataTextField = "Nombre"; ddl.DataValueField = "Id_Periodo"; ddl.DataBind(); ddl.Items.Insert(0, new ListItem("-- Periodo --", "0")); } catch (Exception ex) { MostrarMensaje($"Error cargando periodos: {ex.Message}", false); }
        }
        private void LoadTiposObjetivoDropdown(DropDownList ddl)
        {
            if (ddl == null) return; try { ddl.DataSource = tipoObjetivoDAL.ObtenerTiposObjetivo(true); ddl.DataTextField = "Nombre"; ddl.DataValueField = "Id_Tipo_Objetivo"; ddl.DataBind(); ddl.Items.Insert(0, new ListItem("-- Tipo Objetivo --", "0")); } catch (Exception ex) { MostrarMensaje($"Error cargando tipos: {ex.Message}", false); }
        }
        private void LoadEstadosDropdown(DropDownList ddl, int? idClaseFiltro = null)
        {
            if (ddl == null) return; try { ddl.DataSource = detalleEstadoDAL.ObtenerDetallesEstado(idClaseFiltro); ddl.DataTextField = "Descripcion"; ddl.DataValueField = "Id_Detalle_Estado"; ddl.DataBind(); ddl.Items.Insert(0, new ListItem("-- Estado --", "0")); } catch (Exception ex) { MostrarMensaje($"Error cargando estados: {ex.Message}", false); }
        }


        // Mostrar mensajes (en la página principal)
        private void MostrarMensaje(string texto, bool esExito)
        {
            litMensaje.Text = $"<div class='alert alert-{(esExito ? "success" : "danger")} alert-dismissible fade show' role='alert'>{Server.HtmlEncode(texto)}<button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button></div>";
            litMensaje.Visible = true;
        }
        // Mostrar mensajes (dentro del modal)
        private void MostrarMensajeModal(string texto, bool esExito)
        {
            litModalMensaje.Text = $"<div class='alert alert-{(esExito ? "success" : "danger")} mb-3'>{Server.HtmlEncode(texto)}</div>"; // No dismiss button needed inside modal usually
            litModalMensaje.Visible = true;
        }


        // Evento cuando cambia el filtro de tipo
        protected void ddlTipoObjetivoFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            gvObjetivos.PageIndex = 0;
            BindGrid();
        }
        // Evento Botón Limpiar Filtros
        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            ddlTipoObjetivoFiltro.SelectedIndex = 0;
            gvObjetivos.PageIndex = 0;
            BindGrid();
        }


        // Manejador de Comandos del GridView (Editar y Eliminar)
        protected void gvObjetivos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int objetivoId = 0;
            if (!string.IsNullOrEmpty(e.CommandArgument?.ToString()))
            {
                objetivoId = Convert.ToInt32(e.CommandArgument);
            }

            if (e.CommandName == "EditarObjetivo")
            {
                CargarDatosModalParaEditar(objetivoId);
            }
            else if (e.CommandName == "EliminarObjetivo")
            {
                EliminarObjetivo(objetivoId);
            }
        }

        // Paginación
        protected void gvObjetivos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvObjetivos.PageIndex = e.NewPageIndex;
            BindGrid(); // Rebind with filter applied
        }

        // Botón "Agregar Nuevo Objetivo" - Abre el modal vacío
        protected void btnAbrirModalAgregar_Click(object sender, EventArgs e)
        {
            hfObjetivoId.Value = "0"; // Indicar que es nuevo
            lblModalTitle.Text = "Agregar Nuevo Objetivo";
            LimpiarCamposModal();
            litModalMensaje.Visible = false;
            // Cargar dropdowns por si acaso no se cargaron inicialmente
            //LoadPeriodosDropdown(ddlModalPeriodo);
            //LoadTiposObjetivoDropdown(ddlModalTipoObj);
            //LoadEstadosDropdown(ddlModalEstado);
            // Mostrar el modal usando JavaScript
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowObjetivoModalScript", "showModal('objetivoModal');", true);
        }


        // Carga datos en el modal para edición
        private void CargarDatosModalParaEditar(int objetivoId)
        {
            try
            {
                ObjetivoInfo objetivo = objetivoDAL.ObtenerObjetivoPorId(objetivoId);
                if (objetivo != null)
                {
                    hfObjetivoId.Value = objetivo.Id_Objetivo.ToString();
                    lblModalTitle.Text = "Editar Objetivo";
                    litModalMensaje.Visible = false;

                    // Cargar y seleccionar dropdowns
                    LoadPeriodosDropdown(ddlModalPeriodo);
                    SetSelectedValue(ddlModalPeriodo, objetivo.Id_Periodo);

                    LoadTiposObjetivoDropdown(ddlModalTipoObj);
                    SetSelectedValue(ddlModalTipoObj, objetivo.Id_Tipo_Objetivo);

                    LoadEstadosDropdown(ddlModalEstado, id_claseObjetivos);
                    SetSelectedValue(ddlModalEstado, objetivo.Id_Detalle_Estado);

                    // Poblar textboxes
                    txtModalNumObj.Text = objetivo.NumObjetivo?.ToString() ?? string.Empty;
                    txtModalNombre.Text = objetivo.Nombre;
                    txtModalDescripcion.Text = objetivo.Descripcion;

                    // Mostrar el modal
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowObjetivoModalScript", "showModal('objetivoModal');", true);
                }
                else
                {
                    MostrarMensaje("Error: Objetivo no encontrado.", false);
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al cargar datos para editar: {ex.Message}", false);
                Console.WriteLine($"Error en CargarDatosModalParaEditar: {ex.ToString()}");
            }
        }

        // Botón "Guardar" dentro del Modal
        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            // Validar en el servidor (además de validadores ASP.NET)
            Page.Validate("ModalValidation");
            if (!Page.IsValid)
            {
                MostrarMensajeModal("Por favor corrija los errores.", false);
                // ** REMOVED: UpdatePanelModalContent.Update(); **
                // Re-ejecutar script para asegurar que el modal siga visible
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ReShowModalOnError", "showModal('objetivoModal');", true);
                return;
            }


            try
            {
                int objetivoId = Convert.ToInt32(hfObjetivoId.Value);
                int? idPeriodo = GetNullableIntFromDDL(ddlModalPeriodo);
                int? idTipoObj = GetNullableIntFromDDL(ddlModalTipoObj);
                int? idEstado = GetNullableIntFromDDL(ddlModalEstado);
                int? numObj = GetNullableIntFromTextBox(txtModalNumObj);
                string nombre = txtModalNombre.Text.Trim();
                string descripcion = txtModalDescripcion.Text.Trim();

                bool success = false;
                string actionMessage = "";

                if (objetivoId > 0) // Editar
                {
                    success = objetivoDAL.ActualizarObjetivo(objetivoId, idTipoObj, idPeriodo, nombre, descripcion, numObj, idEstado);
                    actionMessage = success ? "Objetivo actualizado correctamente." : "Error al actualizar el objetivo.";
                }
                else // Agregar
                {
                    int nuevoId = objetivoDAL.InsertarObjetivo(idTipoObj, idPeriodo, nombre, descripcion, numObj, idEstado);
                    success = (nuevoId > 0);
                    actionMessage = success ? "Objetivo agregado correctamente." : "Error al agregar el objetivo.";
                }

                if (success)
                {
                    BindGrid(); // Recargar el grid principal
                    MostrarMensaje(actionMessage, true); // Mostrar mensaje en la página principal
                    // Ocultar el modal usando JavaScript
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideObjetivoModalScript", "hideModal('objetivoModal');", true);
                }
                else
                {
                    MostrarMensajeModal(actionMessage, false); // Mostrar error dentro del modal
                    // Mantener modal abierto
                    // ** REMOVED: UpdatePanelModalContent.Update(); **
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ReShowModalOnError", "showModal('objetivoModal');", true);
                }
            }
            catch (Exception ex)
            {
                MostrarMensajeModal($"Error al guardar: {ex.Message}", false);
                Console.WriteLine($"Error en btnGuardarModal_Click: {ex.ToString()}");
                // Mantener modal abierto
                // ** REMOVED: UpdatePanelModalContent.Update(); **
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ReShowModalOnError", "showModal('objetivoModal');", true);
            }
        }

        // Lógica para eliminar (llamada desde RowCommand)
        private void EliminarObjetivo(int objetivoId)
        {
            if (!ID_ESTADO_INACTIVO.HasValue)
            {
                MostrarMensaje("Error de configuración: No se puede eliminar lógicamente sin un estado inactivo definido.", false);
                return;
            }

            try
            {
                bool eliminado = objetivoDAL.EliminarObjetivoLogico(objetivoId, ID_ESTADO_INACTIVO.Value);
                if (eliminado)
                {
                    BindGrid();
                    MostrarMensaje("Objetivo marcado como inactivo.", true);
                }
                else
                {
                    MostrarMensaje("Error al marcar el objetivo como inactivo.", false);
                }
            }
            catch (InvalidOperationException ioEx) { MostrarMensaje($"Error: {ioEx.Message}", false); } // Catch specific FK error if DAL throws it
            catch (Exception ex)
            {
                MostrarMensaje($"Error al eliminar: {ex.Message}", false);
                Console.WriteLine($"Error en EliminarObjetivo: {ex.ToString()}");
            }
        }


        // --- Funciones auxiliares ---
        private int? GetNullableIntFromDDL(DropDownList ddl) { if (ddl != null && int.TryParse(ddl.SelectedValue, out int result) && result > 0) { return result; } return null; }
        private int? GetNullableIntFromTextBox(TextBox txt) { if (txt != null && int.TryParse(txt.Text, out int result)) { return result; } return null; }
        // Helper para seleccionar valor en DropDownList de forma segura
        private void SetSelectedValue(DropDownList ddl, int? value)
        {
            if (ddl != null && value.HasValue)
            {
                ListItem item = ddl.Items.FindByValue(value.Value.ToString());
                if (item != null) { ddl.SelectedValue = value.Value.ToString(); }
                else { ddl.SelectedIndex = 0; } // Valor no encontrado, seleccionar item inicial
            }
            else if (ddl != null)
            {
                ddl.SelectedIndex = 0; // Valor es null, seleccionar item inicial
            }
        }
        // Limpia los controles dentro del modal
        private void LimpiarCamposModal()
        {
            hfObjetivoId.Value = "0";
            ddlModalPeriodo.SelectedIndex = 0;
            ddlModalTipoObj.SelectedIndex = 0;
            ddlModalEstado.SelectedIndex = 0;
            txtModalNumObj.Text = "";
            txtModalNombre.Text = "";
            txtModalDescripcion.Text = "";
            litModalMensaje.Visible = false;
        }

        // ** REMOVED: WebMethod GetObjetivoDetalles **

    } // End of Page Class
}