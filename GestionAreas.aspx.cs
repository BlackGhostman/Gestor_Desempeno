using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gestor_Desempeno
{
    public partial class GestionAreas : System.Web.UI.Page
    {
        // DAL instance
        private AreaEjecutoraDAL areaDAL = new AreaEjecutoraDAL();

        protected void Page_Load(object sender, EventArgs e)
        {
            // --- Security Checks (same as other pages) ---
            if (Session["UsuarioID"] == null)
            {
                Response.Redirect("~/Login.aspx?mensaje=SesionExpirada");
                return;
            }

            // Check if password change is required
            bool necesitaCambiar = false;
            string idUsuario = Session["UsuarioID"].ToString();
            UsuarioDAL usuarioDal = new UsuarioDAL(); // Need User DAL
            UsuarioInfo infoActual = usuarioDal.ObtenerInfoUsuario(idUsuario);
            if (infoActual != null && infoActual.NecesitaCambiarContrasena)
            {
                necesitaCambiar = true;
            }

            if (necesitaCambiar)
            {
                Response.Redirect("~/CambiarContrasena.aspx");
                return;
            }
            // --- End Security Checks ---

            if (!IsPostBack)
            {
                BindGrid();
            }
            // Hide message on postbacks unless an operation sets it
            if (IsPostBack)
            {
                litMensaje.Visible = false;
            }
        }

        // Method to load/reload GridView data
        private void BindGrid()
        {
            try
            {
                List<AreaEjecutoraInfo> areas = areaDAL.ObtenerAreas();

                if (areas != null)
                {
                    gvAreas.DataSource = areas;
                    gvAreas.DataBind();
                    litMensaje.Visible = (areas.Count == 0); // Show message only if grid is empty
                }
                else
                {
                    gvAreas.DataSource = null;
                    gvAreas.DataBind();
                    MostrarMensaje("Error: No se pudo obtener la lista de áreas (resultado nulo).", false);
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error crítico al cargar las áreas: {ex.Message}. Verifique la conexión y configuración.", false);
                Console.WriteLine($"Error en BindGrid (Areas): {ex.ToString()}");
                gvAreas.DataSource = null;
                gvAreas.DataBind();
            }
        }

        // Method to display messages
        private void MostrarMensaje(string texto, bool esExito)
        {
            litMensaje.Text = $"<div class='alert alert-{(esExito ? "success" : "danger")} alert-dismissible fade show' role='alert'>" +
                              $"{Server.HtmlEncode(texto)}" +
                              "<button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>" +
                              "</div>";
            litMensaje.Visible = true;
        }

        // Event to enter edit mode
        protected void gvAreas_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvAreas.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        // Event to cancel editing
        protected void gvAreas_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvAreas.EditIndex = -1;
            BindGrid();
        }

        // Event to update a row
        protected void gvAreas_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int idArea = Convert.ToInt32(gvAreas.DataKeys[e.RowIndex].Value);
                TextBox txtEditNombre = (TextBox)gvAreas.Rows[e.RowIndex].FindControl("txtEditNombre");

                if (txtEditNombre != null)
                {
                    if (string.IsNullOrWhiteSpace(txtEditNombre.Text))
                    {
                        MostrarMensaje("El nombre del área no puede estar vacío.", false);
                        // Keep the row in edit mode
                        // gvAreas.EditIndex = e.RowIndex; // Optional: Keep row in edit mode
                        // BindGrid(); // Rebind if needed to show error with edit controls
                        return;
                    }

                    string nombre = txtEditNombre.Text.Trim();
                    bool actualizado = areaDAL.ActualizarArea(idArea, nombre);

                    if (actualizado)
                    {
                        gvAreas.EditIndex = -1;
                        BindGrid();
                        MostrarMensaje("Área Ejecutora actualizada correctamente.", true);
                    }
                    else
                    {
                        MostrarMensaje("Error: No se pudo actualizar el Área Ejecutora.", false);
                    }
                }
                else
                {
                    MostrarMensaje("Error: No se encontró el control de edición de nombre.", false);
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al actualizar el área: {ex.Message}", false);
                gvAreas.EditIndex = -1; // Exit edit mode on error
                BindGrid(); // Rebind grid
            }
        }

        // Event to delete a row (physically)
        protected void gvAreas_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int idArea = Convert.ToInt32(gvAreas.DataKeys[e.RowIndex].Value);
                bool eliminado = areaDAL.EliminarArea(idArea); // Attempt physical delete

                if (eliminado)
                {
                    BindGrid();
                    MostrarMensaje("Área Ejecutora eliminada correctamente.", true);
                }
                else
                {
                    // This part might not be reached if DAL throws an exception on constraint violation
                    MostrarMensaje("Error: No se pudo eliminar el Área Ejecutora.", false);
                }
            }
            catch (InvalidOperationException ioEx) // Catch specific exception for FK violation from DAL
            {
                MostrarMensaje($"Error: {ioEx.Message}", false);
            }
            catch (Exception ex) // Catch other general errors
            {
                MostrarMensaje($"Error al eliminar el área: {ex.Message}", false);
            }
        }

        // Event for paging
        protected void gvAreas_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAreas.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        // Event for "Agregar Área" button
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (Page.IsValid) // Check validation group "NewValidation"
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(txtNuevoNombre.Text))
                    {
                        MostrarMensaje("El nombre del área no puede estar vacío.", false);
                        return;
                    }

                    string nombre = txtNuevoNombre.Text.Trim();
                    bool insertado = areaDAL.InsertarArea(nombre);

                    if (insertado)
                    {
                        BindGrid();
                        MostrarMensaje("Nueva Área Ejecutora agregada correctamente.", true);
                        txtNuevoNombre.Text = ""; // Clear the input field
                    }
                    else
                    {
                        MostrarMensaje("Error: No se pudo agregar el Área Ejecutora (verifique si ya existe).", false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje($"Error al agregar el área: {ex.Message}", false);
                }
            }
            else
            {
                MostrarMensaje("Por favor, corrija los errores en el formulario.", false);
            }
        }
    }
}