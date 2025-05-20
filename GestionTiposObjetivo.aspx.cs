using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gestor_Desempeno
{
    public partial class GestionTiposObjetivo : System.Web.UI.Page
    {
        private TipoObjetivoDAL tipoObjetivoDAL = new TipoObjetivoDAL();

        protected void Page_Load(object sender, EventArgs e)
        {
            // --- Security Checks ---
            if (Session["UsuarioID"] == null) { Response.Redirect("~/Login.aspx?mensaje=SesionExpirada"); return; }
            bool necesitaCambiar = false;
            string idUsuario = Session["UsuarioID"].ToString();
            UsuarioDAL usuarioDal = new UsuarioDAL();
            UsuarioInfo infoActual = usuarioDal.ObtenerInfoUsuario(idUsuario);
            if (infoActual != null && infoActual.NecesitaCambiarContrasena) { necesitaCambiar = true; }
            if (necesitaCambiar) { Response.Redirect("~/CambiarContrasena.aspx"); return; }
            // --- End Security Checks ---

            if (!IsPostBack)
            {
                BindGrid();
            }
            if (IsPostBack) { litMensaje.Visible = false; }
        }

        // Cargar GridView
        private void BindGrid()
        {
            try
            {
                // Mostrar solo los activos por defecto
                List<TipoObjetivoInfo> tipos = tipoObjetivoDAL.ObtenerTiposObjetivo(true);
                gvTiposObjetivo.DataSource = tipos;
                gvTiposObjetivo.DataBind();
                litMensaje.Visible = false;
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error crítico al cargar tipos de objetivo: {ex.Message}.", false);
                Console.WriteLine($"Error en BindGrid (Tipos Objetivo): {ex.ToString()}");
                gvTiposObjetivo.DataSource = null;
                gvTiposObjetivo.DataBind();
            }
        }

        // Mostrar mensajes
        private void MostrarMensaje(string texto, bool esExito)
        {
            litMensaje.Text = $"<div class='alert alert-{(esExito ? "success" : "danger")} alert-dismissible fade show' role='alert'>" +
                              $"{Server.HtmlEncode(texto)}" +
                              "<button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>" +
                              "</div>";
            litMensaje.Visible = true;
        }

        // Entrar en modo edición
        protected void gvTiposObjetivo_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvTiposObjetivo.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        // Cancelar edición
        protected void gvTiposObjetivo_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvTiposObjetivo.EditIndex = -1;
            BindGrid();
        }

        // Actualizar fila
        protected void gvTiposObjetivo_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int idTipo = Convert.ToInt32(gvTiposObjetivo.DataKeys[e.RowIndex].Value);

                TextBox txtEditNombre = (TextBox)gvTiposObjetivo.Rows[e.RowIndex].FindControl("txtEditNombre");
                TextBox txtEditDescripcion = (TextBox)gvTiposObjetivo.Rows[e.RowIndex].FindControl("txtEditDescripcion");
                CheckBox chkEditEstado = (CheckBox)gvTiposObjetivo.Rows[e.RowIndex].FindControl("chkEditEstado");

                if (txtEditNombre != null && chkEditEstado != null) // Descripcion puede ser null
                {
                    string nombre = txtEditNombre.Text.Trim();
                    string descripcion = txtEditDescripcion?.Text.Trim(); // Safely get text
                    bool estado = chkEditEstado.Checked;

                    if (string.IsNullOrWhiteSpace(nombre))
                    {
                        MostrarMensaje("El nombre es requerido.", false);
                        return; // Keep in edit mode
                    }

                    bool actualizado = tipoObjetivoDAL.ActualizarTipoObjetivo(idTipo, nombre, descripcion, estado);

                    if (actualizado)
                    {
                        gvTiposObjetivo.EditIndex = -1;
                        BindGrid();
                        MostrarMensaje("Tipo de Objetivo actualizado correctamente.", true);
                    }
                    else
                    {
                        MostrarMensaje("Error: No se pudo actualizar el Tipo de Objetivo.", false);
                    }
                }
                else
                {
                    MostrarMensaje("Error: No se encontraron los controles de edición.", false);
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al actualizar el tipo: {ex.Message}", false);
                gvTiposObjetivo.EditIndex = -1;
                BindGrid();
            }
        }

        // Eliminar (lógicamente) fila
        protected void gvTiposObjetivo_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int idTipo = Convert.ToInt32(gvTiposObjetivo.DataKeys[e.RowIndex].Value);
                bool eliminado = tipoObjetivoDAL.EliminarTipoObjetivoLogico(idTipo);

                if (eliminado)
                {
                    BindGrid(); // Recargar para que desaparezca
                    MostrarMensaje("Tipo de Objetivo marcado como inactivo correctamente.", true);
                }
                else
                {
                    MostrarMensaje("Error: No se pudo marcar el tipo como inactivo.", false);
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al eliminar el tipo: {ex.Message}", false);
            }
        }

        // Paginación
        protected void gvTiposObjetivo_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvTiposObjetivo.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        // Botón "Agregar Tipo"
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (Page.IsValid) // Verifica validadores del grupo "NewValidation"
            {
                try
                {
                    string nombre = txtNuevoNombre.Text.Trim();
                    string descripcion = txtNuevaDescripcion.Text.Trim();
                    bool estado = chkNuevoEstado.Checked;

                    if (string.IsNullOrWhiteSpace(nombre))
                    {
                        MostrarMensaje("El nombre es requerido.", false);
                        return;
                    }

                    int nuevoId = tipoObjetivoDAL.InsertarTipoObjetivo(nombre, descripcion, estado);

                    if (nuevoId > 0)
                    {
                        BindGrid();
                        MostrarMensaje("Nuevo Tipo de Objetivo agregado correctamente.", true);
                        // Limpiar formulario
                        txtNuevoNombre.Text = "";
                        txtNuevaDescripcion.Text = "";
                        chkNuevoEstado.Checked = true;
                    }
                    else
                    {
                        MostrarMensaje("Error: No se pudo agregar el Tipo de Objetivo (verifique si ya existe).", false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje($"Error al agregar el tipo: {ex.Message}", false);
                }
            }
            else
            {
                MostrarMensaje("Por favor, corrija los errores en el formulario.", false);
            }
        }
    }
}