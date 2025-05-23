using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization; // Necesario para ParseExact si se usa

namespace Gestor_Desempeno
{
    public partial class GestionPeriodos : System.Web.UI.Page
    {
        private PeriodoDAL periodoDAL = new PeriodoDAL();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UsuarioID"] == null)
            {
                Response.Redirect("~/Login.aspx?mensaje=SesionExpirada");
                return;
            }

            // ... (resto de tus verificaciones de seguridad) ...
            bool necesitaCambiar = false;
            string idUsuario = Session["UsuarioID"].ToString();
            UsuarioDAL usuarioDal = new UsuarioDAL(); // Asumiendo que tienes esta clase
            UsuarioInfo infoActual = usuarioDal.ObtenerInfoUsuario(idUsuario); // Asumiendo que tienes esta clase/método
            if (infoActual != null && infoActual.NecesitaCambiarContrasena)
            {
                necesitaCambiar = true;
            }

            if (necesitaCambiar)
            {
                Response.Redirect("~/CambiarContrasena.aspx");
                return;
            }


            if (!IsPostBack)
            {
                BindGrid();
                litMensaje.Visible = false;
            }
        }

        private void BindGrid()
        {
            try
            {
                List<PeriodoInfo> periodos = periodoDAL.ObtenerPeriodos(soloActivos: true);
                if (periodos != null)
                {
                    gvPeriodos.DataSource = periodos;
                    gvPeriodos.DataBind();
                    if (periodos.Count == 0)
                    {
                        litMensaje.Visible = false;
                    }
                    else
                    {
                        litMensaje.Visible = false;
                    }
                }
                else
                {
                    gvPeriodos.DataSource = null;
                    gvPeriodos.DataBind();
                    MostrarMensaje("Error: No se pudo obtener la lista de periodos (resultado nulo).", false);
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error crítico al cargar los periodos: {ex.Message}. Verifique la conexión y la configuración.", false);
                Console.WriteLine($"Error en BindGrid: {ex.ToString()}");
                gvPeriodos.DataSource = null;
                gvPeriodos.DataBind();
            }
        }

        private void MostrarMensaje(string texto, bool esExito)
        {
            litMensaje.Text = $"<div class='alert alert-{(esExito ? "success" : "danger")} alert-dismissible fade show' role='alert'>" +
                              $"{Server.HtmlEncode(texto)}" +
                              "<button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>" +
                              "</div>";
            litMensaje.Visible = true;
        }

        protected void gvPeriodos_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvPeriodos.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        protected void gvPeriodos_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvPeriodos.EditIndex = -1;
            BindGrid();
        }

        protected void gvPeriodos_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int Id_Periodo = Convert.ToInt32(gvPeriodos.DataKeys[e.RowIndex].Value);

                TextBox txtEditNombre = (TextBox)gvPeriodos.Rows[e.RowIndex].FindControl("txtEditNombre");
                TextBox txtEditDescripcion = (TextBox)gvPeriodos.Rows[e.RowIndex].FindControl("txtEditDescripcion");
                CheckBox chkEditEstado = (CheckBox)gvPeriodos.Rows[e.RowIndex].FindControl("chkEditEstado");
                TextBox txtEditFechaInicio = (TextBox)gvPeriodos.Rows[e.RowIndex].FindControl("txtEditFechaInicio");
                TextBox txtEditFechaFinal = (TextBox)gvPeriodos.Rows[e.RowIndex].FindControl("txtEditFechaFinal");

                if (txtEditNombre != null && txtEditDescripcion != null && chkEditEstado != null && txtEditFechaInicio != null && txtEditFechaFinal != null)
                {
                    string nombre = txtEditNombre.Text.Trim();
                    string descripcion = txtEditDescripcion.Text.Trim();
                    bool estado = chkEditEstado.Checked;

                    DateTime? fechaInicio = null;
                    if (!string.IsNullOrWhiteSpace(txtEditFechaInicio.Text))
                    {
                        // El input type="date" debería devolver en formato yyyy-MM-dd
                        if (DateTime.TryParse(txtEditFechaInicio.Text, out DateTime parsedFechaInicio))
                        {
                            fechaInicio = parsedFechaInicio;
                        }
                        else
                        {
                            MostrarMensaje("Formato de Fecha Inicio inválido. Use YYYY-MM-DD.", false);
                            return;
                        }
                    }

                    DateTime? fechaFinal = null;
                    if (!string.IsNullOrWhiteSpace(txtEditFechaFinal.Text))
                    {
                        if (DateTime.TryParse(txtEditFechaFinal.Text, out DateTime parsedFechaFinal))
                        {
                            fechaFinal = parsedFechaFinal;
                        }
                        else
                        {
                            MostrarMensaje("Formato de Fecha Final inválido. Use YYYY-MM-DD.", false);
                            return;
                        }
                    }

                    // Validación adicional: Fecha Final no puede ser anterior a Fecha Inicio
                    if (fechaInicio.HasValue && fechaFinal.HasValue && fechaFinal < fechaInicio)
                    {
                        MostrarMensaje("La Fecha Final no puede ser anterior a la Fecha Inicio.", false);
                        return;
                    }


                    bool actualizado = periodoDAL.ActualizarPeriodo(Id_Periodo, nombre, descripcion, estado, fechaInicio, fechaFinal);

                    if (actualizado)
                    {
                        gvPeriodos.EditIndex = -1;
                        BindGrid();
                        MostrarMensaje("Periodo actualizado correctamente.", true);
                    }
                    else
                    {
                        MostrarMensaje("Error: No se pudo actualizar el periodo.", false);
                    }
                }
                else
                {
                    MostrarMensaje("Error: No se encontraron los controles de edición.", false);
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al actualizar el periodo: {ex.Message}", false);
            }
        }

        protected void gvPeriodos_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int Id_Periodo = Convert.ToInt32(gvPeriodos.DataKeys[e.RowIndex].Value);
                bool eliminado = periodoDAL.EliminarPeriodoLogico(Id_Periodo);

                if (eliminado)
                {
                    BindGrid();
                    MostrarMensaje("Periodo marcado como inactivo correctamente.", true);
                }
                else
                {
                    MostrarMensaje("Error: No se pudo eliminar (marcar como inactivo) el periodo.", false);
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al eliminar el periodo: {ex.Message}", false);
            }
        }

        protected void gvPeriodos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPeriodos.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                try
                {
                    string nombre = txtNuevoNombre.Text.Trim();
                    string descripcion = txtNuevaDescripcion.Text.Trim();
                    bool estado = chkNuevoEstado.Checked;

                    DateTime? fechaInicio = null;
                    if (!string.IsNullOrWhiteSpace(txtNuevaFechaInicio.Text))
                    {
                        if (DateTime.TryParse(txtNuevaFechaInicio.Text, out DateTime parsedFechaInicio))
                        {
                            fechaInicio = parsedFechaInicio;
                        }
                        else
                        {
                            MostrarMensaje("Formato de Fecha Inicio inválido para nuevo periodo. Use YYYY-MM-DD.", false);
                            return;
                        }
                    }

                    DateTime? fechaFinal = null;
                    if (!string.IsNullOrWhiteSpace(txtNuevaFechaFinal.Text))
                    {
                        if (DateTime.TryParse(txtNuevaFechaFinal.Text, out DateTime parsedFechaFinal))
                        {
                            fechaFinal = parsedFechaFinal;
                        }
                        else
                        {
                            MostrarMensaje("Formato de Fecha Final inválido para nuevo periodo. Use YYYY-MM-DD.", false);
                            return;
                        }
                    }

                    // Validación adicional: Fecha Final no puede ser anterior a Fecha Inicio
                    if (fechaInicio.HasValue && fechaFinal.HasValue && fechaFinal < fechaInicio)
                    {
                        MostrarMensaje("La Fecha Final no puede ser anterior a la Fecha Inicio al agregar.", false);
                        return;
                    }

                    bool insertado = periodoDAL.InsertarPeriodo(nombre, descripcion, estado, fechaInicio, fechaFinal);

                    if (insertado)
                    {
                        BindGrid();
                        MostrarMensaje("Nuevo periodo agregado correctamente.", true);
                        txtNuevoNombre.Text = "";
                        txtNuevaDescripcion.Text = "";
                        chkNuevoEstado.Checked = true;
                        txtNuevaFechaInicio.Text = ""; // Limpiar campo
                        txtNuevaFechaFinal.Text = "";  // Limpiar campo
                    }
                    else
                    {
                        MostrarMensaje("Error: No se pudo agregar el nuevo periodo.", false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje($"Error al agregar el periodo: {ex.Message}", false);
                }
            }
        }
    }
}