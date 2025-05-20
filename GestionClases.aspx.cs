using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gestor_Desempeno
{
    public partial class GestionClases : System.Web.UI.Page
    {
        private ClaseDAL claseDAL = new ClaseDAL();

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
                List<ClaseInfo> clases = claseDAL.ObtenerClases();
                gvClases.DataSource = clases;
                gvClases.DataBind();
                litMensaje.Visible = false;
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error crítico al cargar clases: {ex.Message}.", false);
                Console.WriteLine($"Error en BindGrid (Clases): {ex.ToString()}");
                gvClases.DataSource = null;
                gvClases.DataBind();
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
        protected void gvClases_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvClases.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        // Cancelar edición
        protected void gvClases_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvClases.EditIndex = -1;
            BindGrid();
        }

        // Actualizar fila
        protected void gvClases_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int idClase = Convert.ToInt32(gvClases.DataKeys[e.RowIndex].Value);
                TextBox txtEditNombre = (TextBox)gvClases.Rows[e.RowIndex].FindControl("txtEditNombre");

                if (txtEditNombre != null)
                {
                    string nombre = txtEditNombre.Text.Trim();
                    if (string.IsNullOrWhiteSpace(nombre))
                    {
                        MostrarMensaje("El nombre es requerido.", false);
                        return; // Keep in edit mode
                    }

                    // Intenta actualizar (DAL ahora verifica duplicados)
                    bool actualizado = claseDAL.ActualizarClase(idClase, nombre);

                    if (actualizado)
                    {
                        gvClases.EditIndex = -1;
                        BindGrid();
                        MostrarMensaje("Clase actualizada correctamente.", true);
                    }
                    else
                    {
                        // This might not be reached if DAL throws exception on duplicate
                        MostrarMensaje("Error: No se pudo actualizar la clase.", false);
                    }
                }
                else
                {
                    MostrarMensaje("Error: No se encontró el control de edición.", false);
                }
            }
            catch (InvalidOperationException opEx) // Catch specific duplicate error
            {
                MostrarMensaje($"Error: {opEx.Message}", false);
                // Keep in edit mode to allow correction
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al actualizar la clase: {ex.Message}", false);
                gvClases.EditIndex = -1; // Exit edit mode on other errors
                BindGrid();
            }
        }

        // Eliminar (físicamente) fila
        protected void gvClases_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int idClase = Convert.ToInt32(gvClases.DataKeys[e.RowIndex].Value);
                bool eliminado = claseDAL.EliminarClase(idClase);

                if (eliminado)
                {
                    BindGrid();
                    MostrarMensaje("Clase eliminada correctamente.", true);
                }
                else
                {
                    // This might not be reached if DAL throws exception
                    MostrarMensaje("Error: No se pudo eliminar la clase.", false);
                }
            }
            catch (InvalidOperationException ioEx) // Catch FK violation error
            {
                MostrarMensaje($"Error: {ioEx.Message}", false);
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al eliminar la clase: {ex.Message}", false);
            }
        }

        // Paginación
        protected void gvClases_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvClases.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        // Botón "Agregar Clase"
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (Page.IsValid) // Verifica validadores del grupo "NewValidation"
            {
                try
                {
                    string nombre = txtNuevoNombre.Text.Trim();
                    if (string.IsNullOrWhiteSpace(nombre))
                    {
                        MostrarMensaje("El nombre es requerido.", false);
                        return;
                    }

                    // Intenta insertar (DAL ahora verifica duplicados)
                    int nuevoId = claseDAL.InsertarClase(nombre);

                    if (nuevoId > 0)
                    {
                        BindGrid();
                        MostrarMensaje("Nueva clase agregada correctamente.", true);
                        txtNuevoNombre.Text = ""; // Limpiar input
                    }
                    else
                    {
                        // This might not be reached if DAL throws exception on duplicate
                        MostrarMensaje("Error: No se pudo agregar la nueva clase.", false);
                    }
                }
                catch (InvalidOperationException opEx) // Catch specific duplicate error
                {
                    MostrarMensaje($"Error: {opEx.Message}", false);
                }
                catch (Exception ex)
                {
                    MostrarMensaje($"Error al agregar la clase: {ex.Message}", false);
                }
            }
            else
            {
                MostrarMensaje("Por favor, corrija los errores en el formulario.", false);
            }
        }
    }
}