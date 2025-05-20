using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gestor_Desempeno
{
    public partial class GestionPeriodos : System.Web.UI.Page
    {
        // Instancia del DAL
        private PeriodoDAL periodoDAL = new PeriodoDAL();

        protected void Page_Load(object sender, EventArgs e)
        {
            // --- Verificaciones de seguridad (igual que en Default.aspx.cs) ---
            if (Session["UsuarioID"] == null)
            {
                Response.Redirect("~/Login.aspx?mensaje=SesionExpirada");
                return;
            }

            // Verificar si necesita cambiar contraseña
            bool necesitaCambiar = false;
            string idUsuario = Session["UsuarioID"].ToString();
            // Necesitas acceso a UsuarioDAL aquí también
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
            // --- Fin Verificaciones ---


            if (!IsPostBack)
            {
                // Cargar los datos en el GridView solo la primera vez que carga la página
                BindGrid();
                litMensaje.Visible = false; // Ocultar mensajes al inicio
            }
        }

        // Método para cargar/recargar los datos del GridView
        private void BindGrid()
        {
            try
            {
                // Obtener solo periodos activos para mostrar por defecto
                // **DEBUG:** Considera temporalmente cambiar a soloActivos: false para ver si hay datos inactivos
                // List<PeriodoInfo> periodos = periodoDAL.ObtenerPeriodos(soloActivos: false);
                List<PeriodoInfo> periodos = periodoDAL.ObtenerPeriodos(soloActivos: true);

                if (periodos != null) // Verificar si la lista no es nula
                {
                    gvPeriodos.DataSource = periodos;
                    gvPeriodos.DataBind();

                    // **FIX:** Mostrar mensaje si la lista está vacía, incluso si la carga fue "exitosa"
                    if (periodos.Count == 0)
                    {
                        // Usar EmptyDataText del GridView (configurado en el ASPX) o mostrar un mensaje literal
                        // MostrarMensaje("No se encontraron periodos activos para mostrar.", true); // Mensaje informativo
                        litMensaje.Visible = false; // Ocultar mensajes de error/éxito si no hay datos
                    }
                    else
                    {
                        litMensaje.Visible = false; // Ocultar mensajes si la carga es exitosa y hay datos
                    }
                }
                else
                {
                    // Si el DAL devuelve null (inesperado, pero posible)
                    gvPeriodos.DataSource = null; // Limpiar datasource
                    gvPeriodos.DataBind(); // Reflejar el estado vacío
                    MostrarMensaje("Error: No se pudo obtener la lista de periodos (resultado nulo).", false);
                }

            }
            catch (Exception ex)
            {
                // Mostrar un error más detallado si falla la carga
                MostrarMensaje($"Error crítico al cargar los periodos: {ex.Message}. Verifique la conexión y la configuración.", false);
                // Podrías querer registrar el error detallado para depuración interna
                Console.WriteLine($"Error en BindGrid: {ex.ToString()}");
                gvPeriodos.DataSource = null; // Asegurarse que el grid esté vacío en caso de error
                gvPeriodos.DataBind();
            }
        }

        // Método para mostrar mensajes al usuario
        private void MostrarMensaje(string texto, bool esExito)
        {
            litMensaje.Text = $"<div class='alert alert-{(esExito ? "success" : "danger")} alert-dismissible fade show' role='alert'>" +
                              $"{Server.HtmlEncode(texto)}" + // Codificar para prevenir XSS
                              "<button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>" +
                              "</div>";
            litMensaje.Visible = true;
        }

        // Evento para entrar en modo edición
        protected void gvPeriodos_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvPeriodos.EditIndex = e.NewEditIndex; // Establecer la fila a editar
            BindGrid(); // Recargar el grid para mostrar los controles de edición
        }

        // Evento para cancelar la edición
        protected void gvPeriodos_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvPeriodos.EditIndex = -1; // Salir del modo edición
            BindGrid(); // Recargar el grid
        }

        // Evento para actualizar una fila
        protected void gvPeriodos_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                // Obtener el ID del periodo desde DataKeys
                int Id_Periodo = Convert.ToInt32(gvPeriodos.DataKeys[e.RowIndex].Value);

                // Encontrar los controles dentro de la fila en modo edición
                TextBox txtEditNombre = (TextBox)gvPeriodos.Rows[e.RowIndex].FindControl("txtEditNombre");
                TextBox txtEditDescripcion = (TextBox)gvPeriodos.Rows[e.RowIndex].FindControl("txtEditDescripcion");
                CheckBox chkEditEstado = (CheckBox)gvPeriodos.Rows[e.RowIndex].FindControl("chkEditEstado");

                // Validar que los controles se encontraron (aunque deberían existir)
                if (txtEditNombre != null && txtEditDescripcion != null && chkEditEstado != null)
                {
                    string nombre = txtEditNombre.Text.Trim();
                    string descripcion = txtEditDescripcion.Text.Trim();
                    bool estado = chkEditEstado.Checked;

                    // Llamar al DAL para actualizar
                    bool actualizado = periodoDAL.ActualizarPeriodo(Id_Periodo, nombre, descripcion, estado);

                    if (actualizado)
                    {
                        gvPeriodos.EditIndex = -1; // Salir del modo edición
                        BindGrid(); // Recargar grid
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

        // Evento para eliminar (lógicamente) una fila
        protected void gvPeriodos_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                // Obtener el ID del periodo desde DataKeys
                int Id_Periodo = Convert.ToInt32(gvPeriodos.DataKeys[e.RowIndex].Value);

                // Llamar al DAL para la eliminación lógica
                bool eliminado = periodoDAL.EliminarPeriodoLogico(Id_Periodo);

                if (eliminado)
                {
                    // Si la eliminación fue exitosa, recargar el grid (el periodo ya no aparecerá si solo mostramos activos)
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

        // Evento para manejar la paginación
        protected void gvPeriodos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPeriodos.PageIndex = e.NewPageIndex; // Establecer la nueva página
            BindGrid(); // Recargar el grid con la página correcta
        }

        // Evento del botón "Agregar Periodo"
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            // Validar que los campos no estén vacíos (aunque RequiredFieldValidator ayuda)
            if (Page.IsValid) // Verifica validadores del grupo "NewValidation"
            {
                try
                {
                    string nombre = txtNuevoNombre.Text.Trim();
                    string descripcion = txtNuevaDescripcion.Text.Trim();
                    bool estado = chkNuevoEstado.Checked;

                    // Llamar al DAL para insertar
                    bool insertado = periodoDAL.InsertarPeriodo(nombre, descripcion, estado);

                    if (insertado)
                    {
                        BindGrid(); // Recargar el grid para mostrar el nuevo periodo
                        MostrarMensaje("Nuevo periodo agregado correctamente.", true);
                        // Limpiar los campos del formulario de agregar
                        txtNuevoNombre.Text = "";
                        txtNuevaDescripcion.Text = "";
                        chkNuevoEstado.Checked = true; // Estado por defecto activo
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