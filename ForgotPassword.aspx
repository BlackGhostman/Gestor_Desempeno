<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="Gestor_Desempeno.ForgotPassword" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Olvidó su Contraseña</title>
     <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
     <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
     <style> body { font-family: 'Inter', sans-serif; } </style>
</head>
<body class="bg-light d-flex align-items-center justify-content-center min-vh-100">
    <form id="form1" runat="server" class="card p-4" style="max-width: 400px;">
        <h3 class="text-center mb-4">Recuperar Contraseña</h3>
         <asp:Literal ID="litMensaje" runat="server" EnableViewState="false"></asp:Literal>

        <div class="mb-3">
            <label for="txtUsuarioRecuperacion" class="form-label">Usuario (ID_USUARIO):</label>
            <asp:TextBox ID="txtUsuarioRecuperacion" runat="server" CssClass="form-control" required="required"></asp:TextBox>
             <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtUsuarioRecuperacion"
                 ErrorMessage="Ingrese su ID de Usuario" CssClass="text-danger small" Display="Dynamic"></asp:RequiredFieldValidator>
        </div>
        <asp:Button ID="btnEnviarCorreo" runat="server" Text="Enviar Instrucciones por Correo" CssClass="btn btn-primary" OnClick="btnEnviarCorreo_Click" />
        <div class="mt-3 text-center">
            <asp:HyperLink ID="hlVolverLogin" runat="server" NavigateUrl="~/Login.aspx">Volver a Iniciar Sesión</asp:HyperLink>
        </div>
    </form>
</body>
</html>
