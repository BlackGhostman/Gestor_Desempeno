using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;

namespace Gestor_Desempeno
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            ScriptManager.ScriptResourceMapping.AddDefinition("jquery",
        new ScriptResourceDefinition
        {
            Path = "~/Scripts/jquery-3.x.x.min.js", // <-- ¡¡IMPORTANTE!! Reemplaza '3.x.x' con la versión EXACTA de jQuery que instalaste. Verifica el nombre del archivo en tu carpeta /Scripts.
            DebugPath = "~/Scripts/jquery-3.x.x.js", // <-- Reemplaza '3.x.x' con la versión EXACTA.
            CdnPath = "https://ajax.aspnetcdn.com/ajax/jQuery/jquery-3.x.x.min.js", // <-- Reemplaza '3.x.x' (Opcional pero bueno tenerlo)
            CdnDebugPath = "https://ajax.aspnetcdn.com/ajax/jQuery/jquery-3.x.x.js" // <-- Reemplaza '3.x.x' (Opcional)
        });
        }
    }
}