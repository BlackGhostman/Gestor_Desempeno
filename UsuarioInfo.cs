using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gestor_Desempeno
{
    public class UsuarioInfo
    {
        public string IdUsuario { get; set; }
        public string ContrasenaAlmacenada { get; set; }
        public string NombreCompleto { get; set; }
        public bool EsExterno { get; set; }
        public bool EstaActivo { get; set; }
        public string Correo { get; set; } // Nuevo campo
        public bool NecesitaCambiarContrasena { get; set; } // Nuevo campo
    }
}