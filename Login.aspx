<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Gestor_Desempeno.Login" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Login - Municipalidad de Curridabat</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH" crossorigin="anonymous">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
    <style>
        body { font-family: 'Inter', sans-serif; }
        .login-container-bs { max-width: 26rem; width: 100%; }
        .btn-custom-orange { background-color: #F97316; border-color: #F97316; color: white; }
        .btn-custom-orange:hover { background-color: #EA580C; border-color: #EA580C; color: white; }
        .link-custom-orange { color: #F97316; text-decoration: none; }
        .link-custom-orange:hover { color: #EA580C; }
        .input-group-text { background-color: #e9ecef; border-right: none; }
        /* Ajuste para que el borde izquierdo del input no se quite */
        .input-group > .form-control { border-left: 1px solid #ced4da; }
        .input-group > .form-control:focus {
             border-color: #fdba74;
             box-shadow: 0 0 0 0.25rem rgba(249, 115, 22, 0.25);
             /* Mantiene el borde izquierdo */
             border-left: 1px solid #fdba74;
             position: relative; /* Ayuda con el z-index */
             z-index: 3;
        }
         /* Asegurar que el span tenga borde derecho al enfocar el input */
        .input-group:focus-within .input-group-text {
             border-color: #fdba74;
             border-right: none; /* Mantiene sin borde derecho */
        }
         /* Quitar el borde izquierdo SOLO del input que sigue al span */
         .input-group > .input-group-prepend > .input-group-text + .form-control,
         .input-group > .input-group-text + .form-control {
            border-left: none;
         }
         .input-group:focus-within .input-group-text + .form-control {
             border-left: none; /* Mantener sin borde izquierdo al enfocar */
         }

    </style>
</head>
<body class="bg-light d-flex align-items-center justify-content-center min-vh-100 px-3 py-4">
    <form id="form1" runat="server" class="login-container-bs card shadow-lg rounded-4 p-4 p-md-5 text-center">

        <asp:Image ID="Image1" runat="server" ImageUrl="https://www.curridabat.go.cr/wp-content/uploads/2025/04/banner1.png" AlternateText="Banner Municipalidad"
             CssClass="img-fluid mb-4 rounded" onerror="this.onerror=null; this.src='https://placehold.co/300x80/eeeeee/cccccc?text=Banner+Muni'; this.alt='Placeholder Banner';" />

         <h2 class="h3 mb-4 fw-semibold text-secondary">Iniciar Sesión</h2>

         <%-- Mensaje de Error --%>
         <asp:Literal ID="litError" runat="server" Visible="false" EnableViewState="false"></asp:Literal>

         <div class="mb-3 text-start">
             <label for="txtUsuario" class="form-label small fw-medium text-secondary">USUARIO</label>
             <div class="input-group">
                 <span class="input-group-text">
                     <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-user text-secondary"><path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                 </span>
                 <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control py-2" placeholder="Ingrese su usuario" required="required"></asp:TextBox>
             </div>
             <asp:RequiredFieldValidator ID="RequiredFieldValidatorUsuario" runat="server"
                ControlToValidate="txtUsuario" ErrorMessage="Por favor ingrese su usuario." Display="Dynamic"
                CssClass="text-danger small" ValidationGroup="LoginValidation"></asp:RequiredFieldValidator>
         </div>

         <div class="mb-4 text-start">
             <label for="txtClave" class="form-label small fw-medium text-secondary">CLAVE</label>
             <div class="input-group">
                 <span class="input-group-text">
                      <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-lock text-secondary"><rect width="18" height="11" x="3" y="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                 </span>
                 <asp:TextBox ID="txtClave" runat="server" CssClass="form-control py-2" TextMode="Password" placeholder="Ingrese su clave" required="required"></asp:TextBox>
             </div>
              <asp:RequiredFieldValidator ID="RequiredFieldValidatorClave" runat="server"
                ControlToValidate="txtClave" ErrorMessage="Por favor ingrese su clave." Display="Dynamic"
                CssClass="text-danger small" ValidationGroup="LoginValidation"></asp:RequiredFieldValidator>
         </div>

         <asp:Button ID="btnIngresar" runat="server" Text="INGRESAR" CssClass="btn btn-custom-orange w-100 fw-semibold py-2 shadow" OnClick="btnIngresar_Click" ValidationGroup="LoginValidation" />

         <div class="mt-4 text-center">
             <%-- Enlace a la página de recuperación de contraseña --%>
             <asp:HyperLink ID="hlOlvidoClave" runat="server" NavigateUrl="~/ForgotPassword.aspx" CssClass="link-custom-orange small">¿Olvidó su contraseña?</asp:HyperLink>
         </div>

    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js" integrity="sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz" crossorigin="anonymous"></script>

    <%-- Script para validación Bootstrap (opcional, ASP.NET Validators ya hacen algo similar) --%>
    <%-- <script>
         (function () {
           'use strict'
           var forms = document.querySelectorAll('.needs-validation') // Usar una clase diferente si usas la de ASP.NET
           Array.prototype.slice.call(forms)
             .forEach(function (form) {
               form.addEventListener('submit', function (event) {
                 if (!form.checkValidity()) {
                   event.preventDefault()
                   event.stopPropagation()
                 }
                 form.classList.add('was-validated')
               }, false)
             })
         })()
     </script> --%>
</body>
</html>
