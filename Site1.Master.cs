using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient; // Para SQL Server
using System.Configuration;  // Para ConfigurationManager (si usas web.config para la connection string)
using System.IO;             // Para Path.Combine y File.Exists
using System.Data;
using System.Text;

namespace Gestor_Desempeno
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        //protected void Page_Load(object sender, EventArgs e)
        //{

        //    if (HttpContext.Current.Session["UsuarioID"] == null &&

        //        !Request.Url.AbsolutePath.EndsWith("/Login.aspx", StringComparison.OrdinalIgnoreCase) &&
        //        !Request.Url.AbsolutePath.EndsWith("/ForgotPassword.aspx", StringComparison.OrdinalIgnoreCase) &&
        //         !Request.Url.AbsolutePath.EndsWith("/CambiarContrasena.aspx", StringComparison.OrdinalIgnoreCase)
        //        )
        //    {

        //        Response.Redirect("~/Login.aspx");
        //        return; 
        //    }

        //    if (HttpContext.Current.Session["UsuarioID"] == null)
        //    {
        //        if (LoginName1 != null)
        //        {
        //            LoginName1.Visible = false;
        //        }
        //        if (btnLogoutMaster != null)
        //        {
        //            btnLogoutMaster.Visible = false;
        //        }
        //    }
        //    else
        //    {
        //        if (LoginName1 != null)
        //        {
        //            LoginName1.Visible = true;
        //        }
        //        if (btnLogoutMaster != null)
        //        {
        //            btnLogoutMaster.Visible = true;
        //        }
        //    }
        //}

        protected void Page_Load(object sender, EventArgs e)
        {
            string rutaImagenPorDefecto = ResolveUrl("~/Fotos/avatar_defecto.png"); // Ruta a tu imagen por defecto

            if (HttpContext.Current.Session["UsuarioID"] == null &&
                !Request.Url.AbsolutePath.EndsWith("/Login.aspx", StringComparison.OrdinalIgnoreCase) &&
                !Request.Url.AbsolutePath.EndsWith("/ForgotPassword.aspx", StringComparison.OrdinalIgnoreCase) &&
                !Request.Url.AbsolutePath.EndsWith("/CambiarContrasena.aspx", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }
            llenarNav();
            if (HttpContext.Current.Session["UsuarioID"] == null)
            {
                if (LoginName1 != null) LoginName1.Visible = false;
                if (btnLogoutMaster != null) btnLogoutMaster.Visible = false;
                if (imgUsuario != null) imgUsuario.Visible = false; // Ocultar también la imagen si no hay sesión
            }
            else
            {
                if (LoginName1 != null) LoginName1.Visible = true;
                if (btnLogoutMaster != null) btnLogoutMaster.Visible = true;
                if (imgUsuario != null)
                {
                    imgUsuario.Visible = true;
                    string usuarioID = HttpContext.Current.Session["UsuarioID"].ToString();
                    string nombreArchivoFoto = ObtenerLinkFotoUsuario(usuarioID); // ej: "eaguilar.png"

                    if (!string.IsNullOrEmpty(nombreArchivoFoto))
                    {
                        ObtenerMenuSecundario(Session["UsuarioID"].ToString(), "M39");
                        string rutaFotoUsuario = ResolveUrl("~/Fotos/" + nombreArchivoFoto);

                        
                        imgUsuario.ImageUrl = rutaFotoUsuario; // Asignación directa
                    }
                    else
                    {
                        imgUsuario.ImageUrl = rutaImagenPorDefecto;
                    }
                }
            }
        }

        // Asume que esta función ya existe en alguna parte o la añades aquí o en una clase de utilidad
        // Si no, puedes reemplazar GetConnectionString() directamente con:
        // ConfigurationManager.ConnectionStrings["MiConexionDB"].ConnectionString;

        public void llenarNavi()
        {
            string resultado = "";
            string cadenaA = @"<ul class='navbar-nav me-auto mb-2 mb-lg-0'>
        <li class='nav-item'>
            <asp:HyperLink ID='hlNavInicio' runat='server' CssClass='nav-link' NavigateUrl='~/Default.aspx'>INICIO</asp:HyperLink>
        </li>";

            string CadenaB = "";

            string CadenaD = "</ul>";

            DataTable dt1 = ObtenerMenuPrincipal(Session["UsuarioID"].ToString());
            for(int i = 0; i < dt1.Rows.Count; i++)
            {
                DataTable dt2 = ObtenerMenuSecundario(Session["UsuarioID"].ToString(), dt1.Rows[i]["Id_Ventana"].ToString());
                string Nomdrop = "navbarDropdown" + dt1.Rows[i]["Id_Ventana"].ToString();
                string NombreMenu = dt1.Rows[i]["Nombre"].ToString();
                string cadena = "<li class='nav-item dropdown'>" +
                                $"  <a class='nav-link dropdown-toggle' href='#' id='{Nomdrop}' role='button' data-bs-toggle='dropdown' aria-expanded='false'>{NombreMenu}</a>" +
                                $"  <ul class='dropdown-menu' aria-labelledby='{Nomdrop}'>";
                for (int j = 0; j < dt2.Rows.Count; j++)
                {
                    cadena = cadena + $"<li><asp:HyperLink ID='hlNav{dt2.Rows[j]["Id_Ventana"].ToString()}' runat='server' CssClass='dropdown-item' NavigateUrl='{dt2.Rows[j]["Url_Ventana"].ToString()}'>{dt2.Rows[j]["Nombre"].ToString()}</asp:HyperLink></li>";
                }
                cadena = cadena + @"</ul></li>";
                CadenaB = CadenaB + cadena;
            }
            resultado = cadenaA + CadenaB + CadenaD;
            divMenu.InnerHtml = resultado;

        }

        public void llenarNav()
        {
            StringBuilder sbMenu = new StringBuilder(); // Usar StringBuilder es más eficiente

            // Parte inicial del menú (Inicio)
            sbMenu.Append("<ul class='navbar-nav me-auto mb-2 mb-lg-0'>");
            sbMenu.Append("<li class='nav-item'>");
            // Usar ResolveUrl para rutas con '~/'
            sbMenu.AppendFormat("<a id='hlNavInicio' class='nav-link' href='{0}'>INICIO</a>", ResolveUrl("~/Default.aspx"));
            sbMenu.Append("</li>");

            // Obtener datos del menú principal
            // Asegúrate de que Session["UsuarioID"] no sea null y sea del tipo esperado
            string usuarioID = Session["UsuarioID"]?.ToString();
            if (string.IsNullOrEmpty(usuarioID))
            {
                // Manejar el caso donde UsuarioID no está en la sesión,
                // quizás redirigir a login o mostrar un menú por defecto.
                // Por ahora, simplemente no se agregarán más ítems.
                divMenu.InnerHtml = sbMenu.ToString() + "</ul>"; // Cierra el UL inicial
                return;
            }

            DataTable dt1 = ObtenerMenuPrincipal(usuarioID);

            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                string idVentanaPrincipal = dt1.Rows[i]["Id_Ventana"].ToString();
                DataTable dt2 = ObtenerMenuSecundario(usuarioID, idVentanaPrincipal);
                string nomDrop = "navbarDropdown" + idVentanaPrincipal;
                string nombreMenu = dt1.Rows[i]["Nombre"].ToString();

                sbMenu.Append("<li class='nav-item dropdown'>");
                sbMenu.AppendFormat("  <a class='nav-link dropdown-toggle' href='#' id='{0}' role='button' data-bs-toggle='dropdown' aria-expanded='false'>{1}</a>", nomDrop, Server.HtmlEncode(nombreMenu)); // Usar HtmlEncode por seguridad
                sbMenu.AppendFormat("  <ul class='dropdown-menu' aria-labelledby='{0}'>", nomDrop);

                for (int j = 0; j < dt2.Rows.Count; j++)
                {
                    string idVentanaSecundaria = dt2.Rows[j]["Id_Ventana"].ToString();
                    string urlVentana = dt2.Rows[j]["Url_Ventana"].ToString();
                    string nombreVentanaSecundaria = dt2.Rows[j]["Nombre"].ToString();

                    // Cambiar "Mis Respuestas" por "Reporte Individual"
                    if (nombreVentanaSecundaria == "Mis Respuestas")
                    {
                        nombreVentanaSecundaria = "Reporte Individual";
                    }

                    // Si la URL también puede ser relativa a la aplicación (ej. ~/Paginas/MiPagina.aspx)
                    // entonces también deberías usar ResolveUrl aquí.
                    // Si es una URL absoluta o completamente relativa (ej. "MiPagina.aspx" o "/app/MiPagina.aspx")
                    // ResolveUrl podría no ser necesario o comportarse diferente.
                    // Asumiremos que puede necesitar ResolveUrl:
                    string resolvedUrl = urlVentana.StartsWith("~") ? ResolveUrl(urlVentana) : urlVentana;

                    sbMenu.AppendFormat("<li><a id='hlNav{0}' class='dropdown-item' href='{1}'>{2}</a></li>",
                                        idVentanaSecundaria,
                                        resolvedUrl,
                                        Server.HtmlEncode(nombreVentanaSecundaria)); // Usar HtmlEncode
                }
                sbMenu.Append("  </ul>");
                sbMenu.Append("</li>");
            }

            sbMenu.Append("</ul>"); // Cierre del ul principal
            divMenu.InnerHtml = sbMenu.ToString();
        }


        private string GetConnectionString()
        {

            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        private string ObtenerLinkFotoUsuario(string usuarioID)
        {
            string linkFoto = null;
            // La consulta SQL para obtener solo el campo LinkFoto
            // Asegúrate de que el nombre de la tabla (Ej: TUsuarios) y la columna (Ej: ID_USUARIO) sean correctos.
            string query = "SELECT LinkFoto FROM SEG_USUARIO WHERE ID_USUARIO = @UsuarioID";

            try
            {
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);
                        con.Open();
                        object result = cmd.ExecuteScalar(); // Usamos ExecuteScalar ya que solo esperamos un valor
                        if (result != null && result != DBNull.Value)
                        {
                            linkFoto = result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar el error, por ejemplo, registrarlo
                // Console.WriteLine("Error al obtener LinkFoto: " + ex.Message); // Para depuración
                // En una aplicación real, usa un sistema de logging.
                System.Diagnostics.Trace.TraceError("Error al obtener LinkFoto para " + usuarioID + ": " + ex.Message);
            }
            return linkFoto;
        }


        private DataTable ObtenerMenuPrincipal(string usuarioID)
        {
            DataTable dt = new DataTable();
            String cnn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            SqlConnection sqlConnection1 = new SqlConnection(cnn);
            string query = @"SELECT distinct m.INICIAL as Id_Ventana,m.DSC_MODULO as Nombre,ISNULL(m.LINK, '') as Url_Ventana,'true' as EsMenuPrincipal,1 as Id_Principal,m.IONICONS as ClaseIconIonic  
                            FROM SEG_USUARIO_PERFIL up  
                            inner join SEG_USUARIO u on u.ID_USUARIO = up.ID_USUARIO  
                            inner join SEG_ROL_PERFIL rp on rp.ID_PERFIL = up.ID_PERFIL  
                            inner join SEG_ROL r on r.ID_ROL =rp.ID_ROL  
                            inner join SEG_OPCIONES_MENU om on om.ID_OPCION = r.ID_OPCION  
                            inner join SEG_MODULO m on m.ID_MODULO = om.ID_MODULO  
                            where  m.ID_SISTEMA = 5 and up.ID_USUARIO = @UsuarioID";

            try
            {

                
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;
                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
  
                reader = cmd.ExecuteReader();

                dt.Load(reader);

            }
            catch (Exception ex)
            {

                System.Diagnostics.Trace.TraceError("Error al obtener datatable para " + usuarioID + ": " + ex.Message);
            }
            sqlConnection1.Close();
            return dt;
        }

        private DataTable ObtenerMenuSecundario(string usuarioID, string Id_Padre)
        {
            DataTable dt = new DataTable();
            String cnn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            SqlConnection sqlConnection1 = new SqlConnection(cnn);

            string query = @"SELECT om.ID_MENU as Id_Ventana ,om.DSC_OPCION as Nombre ,om.LINK as Url_Ventana 
                            ,case  when (select count(ID_MODULO) as cont from SEG_OPCIONES_MENU where ID_PADRE = om.ID_MENU)  > 0 then 'true' 
                            else 'false' end as EsMenuPrincipal ,om.ID_PADRE as Id_Principal ,om.IONICONS as ClaseIconIonic   
                            FROM SEG_USUARIO_PERFIL up   
                            inner join SEG_USUARIO u on u.ID_USUARIO = up.ID_USUARIO   
                            inner join SEG_ROL_PERFIL rp on rp.ID_PERFIL = up.ID_PERFIL   
                            inner join SEG_ROL r on r.ID_ROL =rp.ID_ROL   
                            inner join SEG_OPCIONES_MENU om on om.ID_OPCION = r.ID_OPCION   
                            inner join SEG_MODULO m on m.ID_MODULO = om.ID_MODULO   
                            where  m.ID_SISTEMA = 5 and ID_PADRE = @Id_Padre and up.ID_USUARIO = @usuarioID";

            try
            {

                

                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;
                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);
                cmd.Parameters.AddWithValue("@Id_Padre", Id_Padre);
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();

                reader = cmd.ExecuteReader();

                dt.Load(reader);

            }
            catch (Exception ex)
            {

                System.Diagnostics.Trace.TraceError("Error al obtener datatable para " + usuarioID + ": " + ex.Message);
            }
            sqlConnection1.Close();
            return dt;
        }

        protected void btnLogoutMaster_Click(object sender, EventArgs e)
        {
            // Limpiar la sesión
            Session.Clear();
            Session.Abandon();

            // Opcional: Limpiar cookies de autenticación si las usaras (FormsAuthentication)
             System.Web.Security.FormsAuthentication.SignOut();

            // Redirigir a la página de Login
            Response.Redirect("~/Login.aspx");
            Context.ApplicationInstance.CompleteRequest();
        }


    }
}