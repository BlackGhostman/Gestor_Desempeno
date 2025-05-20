<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CambiarContrasena.aspx.cs" Inherits="Gestor_Desempeno.CambiarContrasena" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Cambiar Contraseña</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
    <style> body { font-family: 'Inter', sans-serif; } </style>
</head>
<body class="bg-light d-flex align-items-center justify-content-center min-vh-100">
    <form id="form1" runat="server" class="card p-4 shadow" style="max-width: 450px;">
        <h3 class="text-center mb-4">Establecer Nueva Contraseña</h3>

        <asp:Literal ID="litMensajeCambio" runat="server" EnableViewState="false"></asp:Literal>

        <div class="mb-3">
            <label for="txtNuevaClave" class="form-label">Nueva Contraseña:</label>
            <asp:TextBox ID="txtNuevaClave" runat="server" CssClass="form-control" TextMode="Password" required="required"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtNuevaClave"
                ErrorMessage="Ingrese la nueva contraseña." CssClass="text-danger small" Display="Dynamic" ValidationGroup="CambioPass"></asp:RequiredFieldValidator>
             <%-- Opcional: Agregar validador de complejidad (RegularExpressionValidator) --%>
        </div>
        <div class="mb-3">
            <label for="txtConfirmarClave" class="form-label">Confirmar Nueva Contraseña:</label>
            <asp:TextBox ID="txtConfirmarClave" runat="server" CssClass="form-control" TextMode="Password" required="required"></asp:TextBox>
             <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="txtConfirmarClave"
                ErrorMessage="Confirme la nueva contraseña." CssClass="text-danger small" Display="Dynamic" ValidationGroup="CambioPass"></asp:RequiredFieldValidator>
            <asp:CompareValidator ID="CompareValidator1" runat="server" ControlToValidate="txtConfirmarClave" ControlToCompare="txtNuevaClave"
                 Operator="Equal" ErrorMessage="Las contraseñas no coinciden." CssClass="text-danger small" Display="Dynamic" ValidationGroup="CambioPass"></asp:CompareValidator>
        </div>

        <asp:Button ID="btnCambiarClave" runat="server" Text="Guardar Nueva Contraseña" CssClass="btn btn-primary w-100" OnClick="btnCambiarClave_Click" ValidationGroup="CambioPass" />

         <div class="mt-3 text-center">
             <asp:HyperLink ID="hlVolverLoginLink" runat="server" NavigateUrl="~/Login.aspx" Text="Cancelar e ir a Inicio de Sesión" Visible="false"></asp:HyperLink>
         </div>
    </form>
</body>
</html>
