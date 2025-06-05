<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CambiarContrasena.aspx.cs" Inherits="Gestor_Desempeno.CambiarContrasena" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Cambiar Contraseña - Gestor Desempeño</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" xintegrity="sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH" crossorigin="anonymous">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
    <style>
        body {
            font-family: 'Inter', sans-serif;
            background-color: #f8f9fa; /* bg-light */
        }
        .change-password-container {
            max-width: 28rem; /* Ajustado para consistencia con login ~450px */
            width: 100%;
        }
        .btn-custom-orange { /* Reutilizando el estilo del login para consistencia si se desea */
            background-color: #F97316;
            border-color: #F97316;
            color: white;
        }
        .btn-custom-orange:hover {
            background-color: #EA580C;
            border-color: #EA580C;
            color: white;
        }
        .link-custom-orange {
            color: #F97316;
            text-decoration: none;
        }
        .link-custom-orange:hover {
            color: #EA580C;
        }
         /* Estilo para los iconos dentro de los input-group, similar al login */
        .input-group-text {
            background-color: #e9ecef;
            border-right: none;
        }
        .input-group > .form-control {
            border-left: 1px solid #ced4da; /* Borde por defecto */
        }
        .input-group > .input-group-text + .form-control {
           border-left: none; /* Quitar borde izquierdo si el icono está antes */
        }
        .form-control:focus {
            border-color: #fdba74; /* Naranja claro de Bootstrap */
            box-shadow: 0 0 0 0.25rem rgba(249, 115, 22, 0.25);
            position: relative;
            z-index: 3;
        }
        .input-group:focus-within .input-group-text {
            border-color: #fdba74;
            border-right: none;
        }
        .input-group:focus-within .input-group-text + .form-control {
           border-left: none;
        }

    </style>
</head>
<body class="d-flex align-items-center justify-content-center min-vh-100 px-3 py-4">
    <form id="formCambioContrasena" runat="server" class="change-password-container card shadow-lg rounded-4 p-4 p-md-5">
        <div class="text-center mb-4">
            <%-- Puedes agregar un logo aquí si lo deseas, similar a la página de login --%>
            <%-- <asp:Image ID="ImageLogo" runat="server" ImageUrl="URL_DEL_LOGO" AlternateText="Logo" CssClass="img-fluid mb-3" Style="max-height: 70px;" /> --%>
            <h3 class="h4 fw-semibold text-secondary">Establecer Nueva Contraseña</h3>
        </div>

        <asp:Literal ID="litMensajeCambio" runat="server" EnableViewState="false"></asp:Literal>

        <%-- CAMPO OPCIONAL: Contraseña Actual --%>
        <%-- Descomenta este bloque si necesitas pedir la contraseña actual (ej. para user.ChangePassword() en AD) --%>
        
        <div class="mb-3">
            <label for="txtContrasenaActual" class="form-label small fw-medium text-secondary">CONTRASEÑA ACTUAL</label>
            <div class="input-group">
                <span class="input-group-text">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-key-round text-secondary"><path d="M2 18v3c0 .6.4 1 1 1h4v-3h3v-3h2l1.4-1.4a6.5 6.5 0 1 0-4-4Z"/><circle cx="16.5" cy="7.5" r=".5"/></svg>
                </span>
                <asp:TextBox ID="txtContrasenaActual" runat="server" CssClass="form-control py-2" TextMode="Password" placeholder="Ingrese su contraseña actual"></asp:TextBox>
            </div>
            <asp:RequiredFieldValidator ID="RequiredFieldValidatorContrasenaActual" runat="server"
                ControlToValidate="txtContrasenaActual" ErrorMessage="Ingrese su contraseña actual."
                CssClass="text-danger small" Display="Dynamic" ValidationGroup="CambioPass"></asp:RequiredFieldValidator>
        </div>
        

        <div class="mb-3">
            <label for="txtNuevaClave" class="form-label small fw-medium text-secondary">NUEVA CONTRASEÑA</label>
            <div class="input-group">
                 <span class="input-group-text">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-lock text-secondary"><rect width="18" height="11" x="3" y="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                </span>
                <asp:TextBox ID="txtNuevaClave" runat="server" CssClass="form-control py-2" TextMode="Password" placeholder="Ingrese la nueva contraseña"></asp:TextBox>
            </div>
            <asp:RequiredFieldValidator ID="RequiredFieldValidatorNuevaClave" runat="server" ControlToValidate="txtNuevaClave"
                ErrorMessage="Ingrese la nueva contraseña." CssClass="text-danger small" Display="Dynamic" ValidationGroup="CambioPass"></asp:RequiredFieldValidator>
            <%-- Opcional: Agregar validador de complejidad (RegularExpressionValidator) para feedback inmediato al cliente --%>
            <%-- 
            <asp:RegularExpressionValidator ID="RegexValidatorNuevaClave" runat="server"
                ControlToValidate="txtNuevaClave"
                ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"
                ErrorMessage="Debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un símbolo."
                CssClass="text-danger small" Display="Dynamic" ValidationGroup="CambioPass">
            </asp:RegularExpressionValidator>
            --%>
        </div>

        <div class="mb-4">
            <label for="txtConfirmarClave" class="form-label small fw-medium text-secondary">CONFIRMAR NUEVA CONTRASEÑA</label>
            <div class="input-group">
                 <span class="input-group-text">
                     <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-lock-keyhole text-secondary"><circle cx="12" cy="16" r="1"/><rect x="3" y="10" width="18" height="12" rx="2"/><path d="M7 10V7a5 5 0 0 1 9.33-2.5"/></svg>
                </span>
                <asp:TextBox ID="txtConfirmarClave" runat="server" CssClass="form-control py-2" TextMode="Password" placeholder="Confirme la nueva contraseña"></asp:TextBox>
            </div>
            <asp:RequiredFieldValidator ID="RequiredFieldValidatorConfirmarClave" runat="server" ControlToValidate="txtConfirmarClave"
                ErrorMessage="Confirme la nueva contraseña." CssClass="text-danger small" Display="Dynamic" ValidationGroup="CambioPass"></asp:RequiredFieldValidator>
            <asp:CompareValidator ID="CompareValidatorClaves" runat="server" ControlToValidate="txtConfirmarClave" ControlToCompare="txtNuevaClave"
                Operator="Equal" ErrorMessage="Las contraseñas no coinciden." CssClass="text-danger small" Display="Dynamic" ValidationGroup="CambioPass"></asp:CompareValidator>
        </div>

        <asp:Button ID="btnCambiarClave" runat="server" Text="GUARDAR NUEVA CONTRASEÑA" CssClass="btn btn-custom-orange w-100 fw-semibold py-2 shadow" OnClick="btnCambiarClave_Click" ValidationGroup="CambioPass" />

        <div class="mt-4 text-center">
            <asp:HyperLink ID="hlVolverLoginLink" runat="server" NavigateUrl="~/Login.aspx" CssClass="link-custom-orange small" Visible="false">Cancelar e ir a Inicio de Sesión</asp:HyperLink>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js" xintegrity="sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz" crossorigin="anonymous"></script>
</body>
</html>