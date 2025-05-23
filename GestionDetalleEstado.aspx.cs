using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gestor_Desempeno
{
    public partial class GestionDetalleEstado : System.Web.UI.Page
    {
        // DALs
        private DetalleEstadoDAL detalleEstadoDAL = new DetalleEstadoDAL();
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
                LoadClaseFilterDropdown(); // Cargar filtro primero
                BindGrid(); // Cargar grid inicial (filtrado o no)
                LoadNuevaClaseDropdown(); // Cargar dropdown del formulario 'Agregar'
            }
            if (IsPostBack) { litMensaje.Visible = false; }
        }

        // Cargar GridView según filtro
        private void BindGrid()
        {
            try
            {
                int? idClaseFiltro = GetNullableIntFromDDL(ddlClaseFiltro); // Obtener filtro seleccionado
                List<DetalleEstadoInfo> detalles = detalleEstadoDAL.ObtenerDetallesEstado(idClaseFiltro);
                gvDetalleEstado.DataSource = detalles;
                gvDetalleEstado.DataBind();
                litMensaje.Visible = false;
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error crítico al cargar estados: {ex.Message}.", false);
                Console.WriteLine($"Error en BindGrid (DetalleEstado): {ex.ToString()}");
                gvDetalleEstado.DataSource = null;
                gvDetalleEstado.DataBind();
            }
        }

        // Cargar DropDown de Filtro de Clases
        private void LoadClaseFilterDropdown()
        {
            try
            {
                ddlClaseFiltro.DataSource = claseDAL.ObtenerClases();
                ddlClaseFiltro.DataTextField = "Nombre";
                ddlClaseFiltro.DataValueField = "IdClase";
                ddlClaseFiltro.DataBind();
                // Añadir opción "Todos"
                ddlClaseFiltro.Items.Insert(0, new ListItem("-- Todas las Clases --", "0"));
            }
            catch (Exception ex) { MostrarMensaje($"Error cargando filtro de clases: {ex.Message}", false); }
        }

        // Cargar DropDown de Clases para formulario 'Agregar'
        private void LoadNuevaClaseDropdown()
        {
            try
            {
                ddlNuevaClase.DataSource = claseDAL.ObtenerClases();
                ddlNuevaClase.DataTextField = "Nombre";
                ddlNuevaClase.DataValueField = "IdClase";
                ddlNuevaClase.DataBind();
                // El item "-- Sin Clase Específica --" ya está en el ASPX
                // ddlNuevaClase.Items.Insert(0, new ListItem("-- Sin Clase Específica --", "0")); // Ya definido en ASPX
            }
            catch (Exception ex) { MostrarMensaje($"Error cargando clases (nuevo): {ex.Message}", false); }
        }

        // Cargar DropDown de Clases para fila en edición
        private void LoadEditClaseDropdown(DropDownList ddl)
        {
            if (ddl == null) return;
            try
            {
                ddl.DataSource = claseDAL.ObtenerClases();
                ddl.DataTextField = "Nombre";
                ddl.DataValueField = "IdClase";
                ddl.DataBind();
                // Añadir opción para NULL/Sin clase
                ddl.Items.Insert(0, new ListItem("-- Sin Clase Específica --", "0"));
            }
            catch (Exception ex) { MostrarMensaje($"Error cargando clases (edición): {ex.Message}", false); }
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

        // Evento cuando cambia el filtro de clase
        protected void ddlClaseFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            gvDetalleEstado.PageIndex = 0; // Resetear a la primera página al filtrar
            BindGrid(); // Recargar el grid con el nuevo filtro
        }


        // Poblar dropdown en modo edición
        protected void gvDetalleEstado_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow && gvDetalleEstado.EditIndex == e.Row.RowIndex)
            {
                DropDownList ddlEditClase = e.Row.FindControl("ddlEditClase") as DropDownList;
                if (ddlEditClase != null)
                {
                    // 1. Poblar el DropDownList (esto ya lo haces y está bien)
                    LoadEditClaseDropdown(ddlEditClase);

                    // 2. Establecer el valor seleccionado correctamente usando e.Row.DataItem
                    object dataItem = e.Row.DataItem;
                    if (dataItem != null)
                    {
                        string idClaseDelRegistroActual = null;
                        // Usar DataBinder.Eval para obtener de forma segura la propiedad IdClase del objeto de datos
                        object idClaseObj = DataBinder.Eval(dataItem, "IdClase");

                        // Convertir el IdClase (que puede ser null) a string para buscar en el DropDownList
                        if (idClaseObj != null && idClaseObj != DBNull.Value)
                        {
                            idClaseDelRegistroActual = idClaseObj.ToString();
                        }

                        ListItem itemParaSeleccionar;

                        if (!string.IsNullOrEmpty(idClaseDelRegistroActual))
                        {
                            // Si hay un IdClase específico para este registro
                            itemParaSeleccionar = ddlEditClase.Items.FindByValue(idClaseDelRegistroActual);
                        }
                        else
                        {
                            // Si IdClase es NULL en la base de datos, seleccionamos el item "0" 
                            // que representa "-- Sin Clase Específica --"
                            itemParaSeleccionar = ddlEditClase.Items.FindByValue("0");
                        }

                        if (itemParaSeleccionar != null)
                        {
                            ddlEditClase.ClearSelection(); // Buena práctica para evitar selecciones múltiples accidentales
                            itemParaSeleccionar.Selected = true;
                        }
                        else
                        {
                            // Fallback: Si por alguna razón el IdClase (o "0" para NULLs) no se encuentra,
                            // se podría seleccionar el item "0" por defecto si existe.
                            // Esto no debería ocurrir si los datos son consistentes y LoadEditClaseDropdown funciona como se espera.
                            ListItem defaultItem = ddlEditClase.Items.FindByValue("0");
                            if (defaultItem != null)
                            {
                                ddlEditClase.ClearSelection();
                                defaultItem.Selected = true;
                            }
                            // Considera registrar un warning si itemParaSeleccionar fue null inicialmente,
                            // ya que podría indicar un problema de datos.
                            // System.Diagnostics.Debug.WriteLine($"Advertencia: No se pudo encontrar el ítem para IdClase '{idClaseDelRegistroActual ?? "NULL"}' en ddlEditClase durante RowDataBound.");
                        }
                    }
                }
            }
        }


        // Entrar en modo edición
        protected void gvDetalleEstado_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvDetalleEstado.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        // Cancelar edición
        protected void gvDetalleEstado_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvDetalleEstado.EditIndex = -1;
            BindGrid();
        }

        // Actualizar fila
        protected void gvDetalleEstado_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int idDetalle = Convert.ToInt32(gvDetalleEstado.DataKeys[e.RowIndex].Value);

                DropDownList ddlEditClase = (DropDownList)gvDetalleEstado.Rows[e.RowIndex].FindControl("ddlEditClase");
                TextBox txtEditDescripcion = (TextBox)gvDetalleEstado.Rows[e.RowIndex].FindControl("txtEditDescripcion");

                if (ddlEditClase != null && txtEditDescripcion != null)
                {
                    int? idClase = GetNullableIntFromDDL(ddlEditClase); // Obtener ID de clase (puede ser null)
                    string descripcion = txtEditDescripcion.Text.Trim();

                    if (string.IsNullOrWhiteSpace(descripcion))
                    {
                        MostrarMensaje("La descripción es requerida.", false);
                        return; // Mantener en modo edición
                    }

                    // Verificar duplicados antes de actualizar
                    if (detalleEstadoDAL.ExisteDetalleEstado(descripcion, idClase, idDetalle))
                    {
                        MostrarMensaje("Ya existe un estado con la misma descripción para esa clase (o sin clase).", false);
                        return; // Mantener en modo edición
                    }


                    bool actualizado = detalleEstadoDAL.ActualizarDetalleEstado(idDetalle, idClase, descripcion);

                    if (actualizado)
                    {
                        gvDetalleEstado.EditIndex = -1;
                        BindGrid();
                        MostrarMensaje("Estado actualizado correctamente.", true);
                    }
                    else
                    {
                        MostrarMensaje("Error: No se pudo actualizar el estado.", false);
                    }
                }
                else
                {
                    MostrarMensaje("Error: No se encontraron los controles de edición.", false);
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al actualizar el estado: {ex.Message}", false);
                gvDetalleEstado.EditIndex = -1;
                BindGrid();
            }
        }

        // Eliminar (físicamente) fila
        protected void gvDetalleEstado_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int idDetalle = Convert.ToInt32(gvDetalleEstado.DataKeys[e.RowIndex].Value);
                bool eliminado = detalleEstadoDAL.EliminarDetalleEstado(idDetalle);

                if (eliminado)
                {
                    BindGrid();
                    MostrarMensaje("Estado eliminado correctamente.", true);
                }
                else
                {
                    // Podría no alcanzarse si el DAL lanza excepción por FK
                    MostrarMensaje("Error: No se pudo eliminar el estado.", false);
                }
            }
            catch (InvalidOperationException ioEx) // Capturar error específico de FK
            {
                MostrarMensaje($"Error: {ioEx.Message}", false);
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al eliminar el estado: {ex.Message}", false);
            }
        }

        // Paginación
        protected void gvDetalleEstado_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvDetalleEstado.PageIndex = e.NewPageIndex;
            BindGrid(); // Asegurarse de que el filtro se mantenga al paginar
        }

        // Botón "Agregar Estado"
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (Page.IsValid) // Verifica validadores del grupo "NewValidation"
            {
                try
                {
                    int? idClase = GetNullableIntFromDDL(ddlNuevaClase); // Puede ser null
                    string descripcion = txtNuevaDescripcion.Text.Trim();

                    if (string.IsNullOrWhiteSpace(descripcion))
                    {
                        MostrarMensaje("La descripción es requerida.", false);
                        return;
                    }

                    // Verificar duplicados antes de insertar
                    if (detalleEstadoDAL.ExisteDetalleEstado(descripcion, idClase))
                    {
                        MostrarMensaje("Ya existe un estado con la misma descripción para esa clase (o sin clase).", false);
                        return;
                    }

                    int nuevoId = detalleEstadoDAL.InsertarDetalleEstado(idClase, descripcion);

                    if (nuevoId > 0)
                    {
                        BindGrid();
                        MostrarMensaje("Nuevo estado agregado correctamente.", true);
                        // Limpiar formulario
                        ddlNuevaClase.SelectedIndex = 0;
                        txtNuevaDescripcion.Text = "";
                    }
                    else
                    {
                        MostrarMensaje("Error: No se pudo agregar el nuevo estado.", false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje($"Error al agregar el estado: {ex.Message}", false);
                }
            }
            else
            {
                MostrarMensaje("Por favor, corrija los errores en el formulario.", false);
            }
        }

        // --- Funciones auxiliares ---
        private int? GetNullableIntFromDDL(DropDownList ddl)
        {
            // Devuelve null si el valor seleccionado es "0" o no es un entero válido
            if (ddl != null && int.TryParse(ddl.SelectedValue, out int result) && result > 0)
            {
                return result;
            }
            return null;
        }
    }
}