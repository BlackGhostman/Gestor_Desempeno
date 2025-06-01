using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;

namespace Gestor_Desempeno
{
    
    public partial class GestionEncargados : System.Web.UI.Page
    {

        public wsRRHH.WS_RecursosHumanos apiRH = new wsRRHH.WS_RecursosHumanos();
        // Instancias de los DAL necesarios
        private EncargadoAreaDAL encargadoDAL = new EncargadoAreaDAL();
        private AreaEjecutoraDAL areaDAL = new AreaEjecutoraDAL(); // Para poblar DropDownList

        protected void Page_Load(object sender, EventArgs e)
        {
            // --- Security Checks ---
            if (Session["UsuarioID"] == null)
            {
                Response.Redirect("~/Login.aspx?mensaje=SesionExpirada");
                return;
            }
            bool necesitaCambiar = false;
            string idUsuario = Session["UsuarioID"].ToString();
            UsuarioDAL usuarioDal = new UsuarioDAL();
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
                LoadAreasDropdown(); // Cargar dropdown antes de BindGrid
                CargarColaboradoresDropDown(ddlNuevoUsuario); // <-- AÑADIR ESTA LÍNEA para poblar el nuevo DDL de usuarios
                BindGrid();
            }
            if (IsPostBack)
            {
                litMensaje.Visible = false;
            }
        }

        private void CargarColaboradoresDropDown(DropDownList ddl)
        {
            if (ddl == null) return;
            try
            {
                System.Data.DataTable dtColaboradores = apiRH.ObtenerColaboradores();

                if (dtColaboradores != null && dtColaboradores.Rows.Count > 0)
                {
                    ddl.DataSource = dtColaboradores;
                    ddl.DataTextField = "nombre";         // Columna del DataTable con el nombre a mostrar
                    ddl.DataValueField = "Id_Colaborador"; // Columna del DataTable con el ID a guardar
                    ddl.DataBind();
                }
                else
                {
                    ddl.Items.Clear(); // Asegurar que esté vacío si no hay datos
                }
                // Agregar un ítem inicial "Seleccione..."
                ddl.Items.Insert(0, new ListItem("-- Seleccione Usuario --", "0")); // Usa "0" o string.Empty según tu validador
            }
            catch (System.Net.WebException webEx) // Específico para errores de comunicación con el WS
            {
                MostrarMensaje($"Error de red al contactar el servicio de RRHH: {webEx.Message}", false);
                Console.WriteLine($"Error de WebService en CargarColaboradoresDropDown: {webEx.ToString()}");
                ddl.Items.Clear();
                ddl.Items.Insert(0, new ListItem("-- Error WS --", "0"));
            }
            catch (Exception ex) // Para cualquier otro error
            {
                MostrarMensaje($"Error crítico al cargar la lista de colaboradores: {ex.Message}", false);
                Console.WriteLine($"Error en CargarColaboradoresDropDown: {ex.ToString()}");
                ddl.Items.Clear();
                ddl.Items.Insert(0, new ListItem("-- Error Carga --", "0"));
            }
        }

        // Cargar datos en el GridView
        private void BindGrid()
        {
            try
            {
                List<EncargadoAreaInfo> encargados = encargadoDAL.ObtenerEncargados();
                gvEncargados.DataSource = encargados;
                gvEncargados.DataBind();
                // No mostrar mensaje si está vacío, usar EmptyDataText
                litMensaje.Visible = false;
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error crítico al cargar los encargados: {ex.Message}.", false);
                Console.WriteLine($"Error en BindGrid (Encargados): {ex.ToString()}");
                gvEncargados.DataSource = null;
                gvEncargados.DataBind();
            }
        }

        // Cargar el DropDownList de Áreas Ejecutoras
        private void LoadAreasDropdown()
        {
            try
            {
                List<AreaEjecutoraInfo> areas = areaDAL.ObtenerAreas(Session["UsuarioID"].ToString()); // Obtener todas las áreas
                ddlAreas.DataSource = areas;
                ddlAreas.DataTextField = "Nombre";
                ddlAreas.DataValueField = "Id_Area_Ejecutora";
                ddlAreas.DataBind();
                // Agregar un item inicial "Seleccione..."
                ddlAreas.Items.Insert(0, new ListItem("-- Seleccione un Área --", "0"));
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al cargar las áreas para el dropdown: {ex.Message}", false);
                Console.WriteLine($"Error en LoadAreasDropdown: {ex.ToString()}");
            }
        }

        // Cargar el DropDownList de Áreas para la fila en edición
        private void LoadEditAreasDropdown(DropDownList ddl)
        {
            if (ddl == null) return;
            try
            {
                List<AreaEjecutoraInfo> areas = areaDAL.ObtenerAreas(Session["UsuarioID"].ToString());
                ddl.DataSource = areas;
                ddl.DataTextField = "Nombre";
                ddl.DataValueField = "Id_Area_Ejecutora";
                ddl.DataBind();
                ddl.Items.Insert(0, new ListItem("-- Seleccione Área --", "0"));
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al cargar áreas (edición): {ex.Message}", false);
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

        // Evento RowDataBound para poblar el DropDownList en modo edición
        protected void gvEncargados_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (gvEncargados.EditIndex == e.Row.RowIndex) // Si la fila está en modo edición
                {
                    // --- Manejo del DropDownList de Áreas Ejecutoras ---
                    DropDownList ddlEditArea = (DropDownList)e.Row.FindControl("ddlEditArea");
                    if (ddlEditArea != null)
                    {
                        string idAreaActualDelRegistro = null;
                        if (e.Row.DataItem != null)
                        {
                            object idAreaObj = DataBinder.Eval(e.Row.DataItem, "Id_Area_Ejecutora");
                            if (idAreaObj != null && idAreaObj != DBNull.Value)
                            {
                                idAreaActualDelRegistro = idAreaObj.ToString();
                            }
                        }

                        LoadEditAreasDropdown(ddlEditArea); // Poblar el DDL

                        if (!string.IsNullOrEmpty(idAreaActualDelRegistro))
                        {
                            ListItem itemExistente = ddlEditArea.Items.FindByValue(idAreaActualDelRegistro);
                            if (itemExistente != null)
                            {
                                ddlEditArea.ClearSelection();
                                itemExistente.Selected = true;
                            }
                            else
                            {
                                MostrarMensaje($"Advertencia: El Área Ejecutora (ID: {idAreaActualDelRegistro}) previamente asignada no es una opción válida o ha sido eliminada. Por favor, seleccione un área de la lista.", false);
                                if (ddlEditArea.Items.FindByValue("0") != null)
                                {
                                    ddlEditArea.SelectedValue = "0";
                                }
                            }
                        }
                        else
                        {
                            if (ddlEditArea.Items.FindByValue("0") != null)
                            {
                                ddlEditArea.SelectedValue = "0";
                            }
                        }
                    }

                    // --- Manejo del DropDownList de Usuarios (Colaboradores) ---
                    DropDownList ddlEditUsuario = (DropDownList)e.Row.FindControl("ddlEditUsuario");
                    if (ddlEditUsuario != null)
                    {
                        string idUsuarioActualDelRegistro = null;
                        if (e.Row.DataItem != null)
                        {
                            object idUsuarioObj = DataBinder.Eval(e.Row.DataItem, "Usuario");
                            if (idUsuarioObj != null && idUsuarioObj != DBNull.Value)
                            {
                                idUsuarioActualDelRegistro = idUsuarioObj.ToString();
                            }
                        }

                        CargarColaboradoresDropDown(ddlEditUsuario); // Poblar el DDL

                        if (!string.IsNullOrEmpty(idUsuarioActualDelRegistro))
                        {
                            ListItem itemExistente = ddlEditUsuario.Items.FindByValue(idUsuarioActualDelRegistro);
                            if (itemExistente != null)
                            {
                                ddlEditUsuario.ClearSelection();
                                itemExistente.Selected = true;
                            }
                            else
                            {
                                MostrarMensaje($"Advertencia: El Usuario (ID: {idUsuarioActualDelRegistro}) previamente asignado no es una opción válida o ha sido eliminado. Por favor, seleccione un usuario de la lista.", false);
                                if (ddlEditUsuario.Items.FindByValue("0") != null)
                                {
                                    ddlEditUsuario.SelectedValue = "0";
                                }
                            }
                        }
                        else
                        {
                            if (ddlEditUsuario.Items.FindByValue("0") != null)
                            {
                                ddlEditUsuario.SelectedValue = "0";
                            }
                        }
                    }
                }
            }
        }


        // Entrar en modo edición
        protected void gvEncargados_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvEncargados.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        // Cancelar edición
        protected void gvEncargados_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvEncargados.EditIndex = -1;
            BindGrid();
        }

        // Actualizar fila
        protected void gvEncargados_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int idEncargadoKey = Convert.ToInt32(gvEncargados.DataKeys[e.RowIndex].Value); // ID de la fila Encargado_Area

                DropDownList ddlEditArea = (DropDownList)gvEncargados.Rows[e.RowIndex].FindControl("ddlEditArea");
                DropDownList ddlEditUsuario = (DropDownList)gvEncargados.Rows[e.RowIndex].FindControl("ddlEditUsuario"); // El nuevo DropDownList

                if (ddlEditArea != null && ddlEditUsuario != null)
                {
                    int idAreaSeleccionada = Convert.ToInt32(ddlEditArea.SelectedValue);
                    string idColaboradorSeleccionado = ddlEditUsuario.SelectedValue; // Este es el Id_Colaborador a guardar

                    // Validaciones
                    if (idAreaSeleccionada <= 0) // Asumiendo "0" es "no seleccionado" para áreas
                    {
                        MostrarMensaje("Debe seleccionar un Área Ejecutora válida.", false);
                        e.Cancel = true; // Mantiene la fila en modo edición
                        return;
                    }
                    if (string.IsNullOrEmpty(idColaboradorSeleccionado) || idColaboradorSeleccionado == "0") // Asumiendo "0" es "no seleccionado"
                    {
                        MostrarMensaje("Debe seleccionar un Usuario Encargado válido.", false);
                        e.Cancel = true; // Mantiene la fila en modo edición
                        return;
                    }

                    // Llamar al DAL. El parámetro 'usuario' ahora es 'idColaboradorSeleccionado'.
                    // Las firmas de los métodos DAL ('ActualizarEncargado', 'ExisteAsignacion') no cambian.
                    if (encargadoDAL.ExisteAsignacion(idAreaSeleccionada, idColaboradorSeleccionado, idEncargadoKey))
                    {
                        MostrarMensaje("Esta asignación (Área y Usuario ID) ya existe.", false);
                        e.Cancel = true;
                        return;
                    }

                    bool actualizado = encargadoDAL.ActualizarEncargado(idEncargadoKey, idAreaSeleccionada, idColaboradorSeleccionado);

                    if (actualizado)
                    {
                        gvEncargados.EditIndex = -1; // Salir del modo edición
                        BindGrid(); // Recargar el GridView
                        MostrarMensaje("Asignación actualizada correctamente.", true);
                    }
                    else
                    {
                        MostrarMensaje("Error: No se pudo actualizar la asignación.", false);
                        e.Cancel = true; // Mantener en modo edición si falla
                    }
                }
                else
                {
                    MostrarMensaje("Error crítico: No se encontraron los controles de edición.", false);
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al actualizar la asignación: {ex.Message}", false);
                e.Cancel = true; // Es buena práctica mantener el modo edición si hay un error no manejado aquí.
            }
        }

        // Eliminar fila
        protected void gvEncargados_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int idEncargado = Convert.ToInt32(gvEncargados.DataKeys[e.RowIndex].Value);
                bool eliminado = encargadoDAL.EliminarEncargado(idEncargado);

                if (eliminado)
                {
                    BindGrid();
                    MostrarMensaje("Asignación eliminada correctamente.", true);
                }
                else
                {
                    MostrarMensaje("Error: No se pudo eliminar la asignación.", false);
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al eliminar la asignación: {ex.Message}", false);
            }
        }

        // Paginación
        protected void gvEncargados_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvEncargados.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        // Botón "Asignar Encargado"

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (Page.IsValid) // Verifica validadores del grupo "NewValidation"
            {
                try
                {
                    int idAreaSeleccionada = Convert.ToInt32(ddlAreas.SelectedValue);
                    // CAMBIO: Leer del DropDownList en lugar del TextBox
                    string nuevoUsuarioIdColaborador = ddlNuevoUsuario.SelectedValue;

                    // Validaciones actualizadas
                    if (idAreaSeleccionada <= 0)
                    {
                        MostrarMensaje("Debe seleccionar un Área Ejecutora.", false);
                        return;
                    }
                    // CAMBIO: Validación para el DropDownList
                    if (string.IsNullOrEmpty(nuevoUsuarioIdColaborador) || nuevoUsuarioIdColaborador == "0") // "0" es el valor de "--Seleccione--"
                    {
                        MostrarMensaje("Debe seleccionar un Usuario Encargado.", false);
                        return;
                    }

                    if (encargadoDAL.ExisteAsignacion(idAreaSeleccionada, nuevoUsuarioIdColaborador))
                    {
                        MostrarMensaje("Esta asignación (Área y Usuario ID) ya existe.", false);
                        return;
                    }

                    int nuevoIdRegistrado = encargadoDAL.InsertarEncargado(idAreaSeleccionada, nuevoUsuarioIdColaborador);

                    if (nuevoIdRegistrado > 0)
                    {
                        BindGrid();
                        MostrarMensaje("Nueva asignación agregada correctamente.", true);
                        ddlAreas.SelectedIndex = 0;
                        // CAMBIO: Limpiar el DropDownList de usuario
                        ddlNuevoUsuario.SelectedIndex = 0;
                    }
                    else
                    {
                        MostrarMensaje("Error: No se pudo agregar la nueva asignación. Verifique los datos.", false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje($"Error al agregar la asignación: {ex.Message}", false);
                    // Considera loggear el ex.ToString() para más detalles en caso de errores inesperados
                    Console.WriteLine($"Error en btnAgregar_Click: {ex.ToString()}");
                }
            }
            else // Page.IsValid es false
            {
                MostrarMensaje("Por favor, corrija los errores en el formulario.", false);
            }
        }

    }
}