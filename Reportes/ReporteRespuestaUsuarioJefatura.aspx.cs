using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;

namespace Gestor_Desempeno.Reportes
{
	public partial class ReporteRespuestaUsuarioJefatura : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
            if (!IsPostBack)
            {

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


                rpt_Respuesta_Jefatura.ProcessingMode = ProcessingMode.Remote;
                rpt_Respuesta_Jefatura.ServerReport.ReportServerUrl = new Uri("http://server-bd/ReportServer");
                rpt_Respuesta_Jefatura.ServerReport.ReportPath = "/Reportes_Desempeno/rpt_Repuesta_Colaborador";

                List<ReportParameter> parametros = new List<ReportParameter>();

                parametros.Add(new ReportParameter("Usuario", idUsuarioActual));

                rpt_Respuesta_Jefatura.ServerReport.SetParameters(parametros);


                rpt_Respuesta_Jefatura.ServerReport.Refresh();
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error al configurar ReportViewer con parámetros: " + ex.ToString());

            }
        }

    }
}