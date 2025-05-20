using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using Microsoft.Reporting.WebForms;

namespace Gestor_Desempeno.Reportes
{
    public partial class ReporteRespuestaUsuario : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                //rpt_Respuesta.ProcessingMode = ProcessingMode.Remote;
                //rpt_Respuesta.ServerReport.ReportServerUrl = new Uri("http://server-bd/ReportServer");
                //rpt_Respuesta.ServerReport.ReportPath = "/Reportes_Desempeno/rpt_Respuesta";
                //rpt_Respuesta.ServerReport.Refresh();

                llamarReporte();
            }
        }


        public void llamarReporte()
        {
            try
            {
                string idUsuarioActual = "0"; 

                if ((string)Session["UsuarioID"] != null) 
                {
                    try
                    {
                        idUsuarioActual = (string)Session["UsuarioID"];
                    }
                    catch (FormatException)
                    {

                        idUsuarioActual = "0"; 

                    }
                }
                else
                {
                    idUsuarioActual = (string)Session["UsuarioID"]; 
                }


                rpt_Respuesta.ProcessingMode = ProcessingMode.Remote;
                rpt_Respuesta.ServerReport.ReportServerUrl = new Uri("http://server-bd/ReportServer");
                rpt_Respuesta.ServerReport.ReportPath = "/Reportes_Desempeno/rpt_Respuesta";

                List<ReportParameter> parametros = new List<ReportParameter>();

                parametros.Add(new ReportParameter("Usuario", idUsuarioActual));

                rpt_Respuesta.ServerReport.SetParameters(parametros);


                rpt_Respuesta.ServerReport.Refresh();
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error al configurar ReportViewer con parámetros: " + ex.ToString());

            }
        }
    }
}