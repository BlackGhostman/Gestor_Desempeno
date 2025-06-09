using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data; // Necesario para DataTable si el servicio devuelve eso.
using System.Linq; // Si el servicio devuelve una colección que necesita ser procesada.
using System.Globalization;

namespace Gestor_Desempeno
{
    public partial class MetasRapidas : System.Web.UI.Page
    {
        // Cadena de conexión a la base de datos.
        string connectionString = ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
        // Referencia al servicio web de RRHH.
        wsRRHH.WS_RecursosHumanos servicioRRHH;

        protected void Page_Load(object sender, EventArgs e)
        {
            servicioRRHH = new wsRRHH.WS_RecursosHumanos();
            // Podrías configurar la URL del servicio aquí si es dinámica:
            // servicioRRHH.Url = ConfigurationManager.AppSettings["UrlServicioRRHH"];

            if (Session["UsuarioID"] == null)
            {
                Response.Redirect("Login.aspx");
            }

            if (!IsPostBack)
            {
                CargarUsuariosJefe();
            }
        }

        // Método para cargar los colaboradores del jefe actual en el DropDownList.
        private void CargarUsuariosJefe()
        {
            try
            {
                string usuarioJefeID = Session["UsuarioID"]?.ToString();
                if (string.IsNullOrEmpty(usuarioJefeID))
                {
                    MostrarMensaje("Error: No se pudo obtener el ID del usuario jefe de la sesión.", false);
                    return;
                }

                // Llamada al WebService para obtener los colaboradores.
                // Asumiendo que el servicio podría devolver DataTable o un array/lista.
                // Si devuelve DataTable:
                var colaboradoresResult = servicioRRHH.ObtenerColaboradoresUsuarioJefe(usuarioJefeID);

                // Verificar si el resultado es un DataTable y si tiene filas.
                if (colaboradoresResult is DataTable colaboradoresDataTable && colaboradoresDataTable != null && colaboradoresDataTable.Rows.Count > 0)
                {
                    ddlUsuario.DataSource = colaboradoresDataTable;
                    // Asegúrate de que "NombreCompleto" y "Identificador" sean los nombres correctos de las COLUMNAS
                    // en el DataTable devuelto por tu servicio web.
                    // El campo "Usuario" en la tabla Meta_Individual es nvarchar(20),
                    // por lo que DataValueField debe ser la columna que contenga ese identificador.
                    ddlUsuario.DataTextField = "Nombre"; // Columna para mostrar en el dropdown.
                    ddlUsuario.DataValueField = "Id_Colaborador";  // Columna con el valor nvarchar(20) para guardar en BD.
                    ddlUsuario.DataBind();
                }
                // Si el servicio devuelve un array u otra colección IEnumerable<T> (y no un DataTable)
                // else if (colaboradoresResult is System.Collections.IEnumerable && !(colaboradoresResult is DataTable) && ((System.Collections.IEnumerable)colaboradoresResult).Cast<object>().Any())
                // {
                //     ddlUsuario.DataSource = colaboradoresResult;
                //     ddlUsuario.DataTextField = "NombreCompleto"; // Propiedad del objeto en la colección
                //     ddlUsuario.DataValueField = "Identificador";  // Propiedad del objeto en la colección
                //     ddlUsuario.DataBind();
                // }
                else
                {
                    MostrarMensaje("No se encontraron colaboradores para el jefe actual o el formato de datos no es el esperado.", true);
                    ddlUsuario.Items.Clear();
                }

                ddlUsuario.Items.Insert(0, new ListItem("Seleccione un colaborador...", ""));
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al cargar la lista de colaboradores: " + ex.Message, false);
            }
        }

        // Evento click del botón Guardar.
        protected void btnGuardarMetaRapida_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                MostrarMensaje("Por favor, corrija los errores en el formulario.", false);
                return;
            }
            try
            {
                const int idMetaDepartamental = 8;
                const int idDetalleEstado = 7;

                string usuarioSeleccionado = ddlUsuario.SelectedValue;
                string descripcion = txtDescripcion.Text.Trim();
                DateTime fechaInicial = DateTime.Parse(txtFechaInicial.Text);
                DateTime fechaFinal = DateTime.Parse(txtFechaFinal.Text);
                bool esFinalizable = true;

                if (string.IsNullOrEmpty(usuarioSeleccionado))
                {
                    MostrarMensaje("Debe seleccionar un colaborador.", false);
                    return;
                }
                if (fechaFinal < fechaInicial)
                {
                    MostrarMensaje("La fecha final no puede ser anterior a la fecha inicial.", false);
                    return;
                }

                MetaIndividualDAL.InsertarMetaRapidaIndividual(
                    idMetaDepartamental,
                    usuarioSeleccionado,
                    descripcion,
                    fechaInicial,
                    fechaFinal,
                    esFinalizable,
                    idDetalleEstado,
                    connectionString
                );

                MostrarMensaje("Meta rápida guardada exitosamente.", true);
                LimpiarFormulario();
                CargarUsuariosJefe();
            }
            catch (FormatException fx)
            {
                MostrarMensaje("Error en el formato de una de las fechas: " + fx.Message, false);
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al guardar la meta rápida: " + ex.Message, false);
            }
        }

        // Método para limpiar los campos del formulario.
        private void LimpiarFormulario()
        {
            ddlUsuario.ClearSelection();
            if (ddlUsuario.Items.FindByValue("") == null)
            {
                ddlUsuario.Items.Insert(0, new ListItem("Seleccione un colaborador...", ""));
            }
            txtDescripcion.Text = string.Empty;
            txtFechaInicial.Text = string.Empty;
            txtFechaFinal.Text = string.Empty;
            chkEsFinalizable.Checked = false;
        }

        // Método para mostrar mensajes al usuario.
        private void MostrarMensaje(string mensaje, bool esExito)
        {
            lblMensaje.Text = mensaje;
            if (esExito)
            {
                pnlMensaje.CssClass = "alert alert-success alert-dismissible fade show mb-3";
            }
            else
            {
                pnlMensaje.CssClass = "alert alert-danger alert-dismissible fade show mb-3";
            }
            pnlMensaje.Visible = true;
        }
    }
}