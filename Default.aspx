<%@ Page Title="Inicio" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Gestor_Desempeno.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Puedes agregar CSS o meta tags específicos para esta página aquí si es necesario --%>
    <style>
        /* Estilos solo para Default.aspx */
        .welcome-message {
            font-size: 1.5rem;
            color: #333;
        }
    </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
     <%-- El contenido de tu página va aquí. Ya no necesitas <html>, <head>, <body>, ni el header/nav repetido --%>
    <div class="container mt-4"> <%-- Puedes usar contenedores Bootstrap si quieres --%>
        <h1>Página Principal</h1>
        <hr /> <%-- Una línea divisoria --%>

 
<li class="nav-item">
    <asp:HyperLink ID="hlGestionPeriodos" runat="server" CssClass="nav-link" NavigateUrl="~/GestionPeriodos.aspx">GESTIÓN PERIODOS</asp:HyperLink>
</li>
<li class="nav-item">
    <asp:HyperLink ID="hlGestionAreas" runat="server" CssClass="nav-link" NavigateUrl="~/GestionAreas.aspx">GESTIÓN ÁREAS</asp:HyperLink>
</li>
 <li class="nav-item">
    <asp:HyperLink ID="hlGestionEncargados" runat="server" CssClass="nav-link" NavigateUrl="~/GestionEncargados.aspx">GESTIÓN ENCARGADOS</asp:HyperLink>
</li>
<li class="nav-item">
    <asp:HyperLink ID="hlGestionObjetivos" runat="server" CssClass="nav-link" NavigateUrl="~/GestionObjetivos.aspx">GESTIÓN OBJETIVOS</asp:HyperLink>
</li>
 <li class="nav-item">
    <asp:HyperLink ID="hlGestionTiposObj" runat="server" CssClass="nav-link" NavigateUrl="~/GestionTiposObjetivo.aspx">GESTIÓN TIPOS OBJETIVO</asp:HyperLink>
</li>
 <li class="nav-item">
    <asp:HyperLink ID="hlGestionDetalleEstado" runat="server" CssClass="nav-link" NavigateUrl="~/GestionDetalleEstado.aspx">GESTIÓN ESTADOS</asp:HyperLink>
</li>
 <li class="nav-item">
    <asp:HyperLink ID="hlGestionClases" runat="server" CssClass="nav-link" NavigateUrl="~/GestionClases.aspx">GESTIÓN CLASES</asp:HyperLink>
</li>
<li class="nav-item">
    <asp:HyperLink ID="hlGestionMetas" runat="server" CssClass="nav-link" NavigateUrl="~/GestionMetas.aspx">GESTIÓN METAS</asp:HyperLink>
</li>
<li class="nav-item">
    <asp:HyperLink ID="HyperLink1" runat="server" CssClass="nav-link" NavigateUrl="~/GestionClases.aspx">GESTIÓN CLASES</asp:HyperLink>
</li>
<li class="nav-item">
    <asp:HyperLink ID="HyperLink2" runat="server" CssClass="nav-link" NavigateUrl="~/GestionMetas.aspx">GESTIÓN METAS</asp:HyperLink>
</li>
<li class="nav-item">
    <asp:HyperLink ID="hlGestionMetasDep" runat="server" CssClass="nav-link" NavigateUrl="~/GestionMetasDep.aspx">GESTIÓN METAS DEP.</asp:HyperLink>
</li>
        <li class="nav-item">
    <asp:HyperLink ID="hlGestionMetasInd" runat="server" CssClass="nav-link" NavigateUrl="~/GestionMetasInd.aspx">GESTIÓN METAS IND.</asp:HyperLink>
</li>

        <p class="welcome-message">
            <%-- El Label para el mensaje de bienvenida ya estaba en tu Default.aspx.cs --%>
            <asp:Label ID="lblBienvenida" runat="server" Text=""></asp:Label>
        </p>
        <p>Este es el contenido específico de la página de inicio.</p>
        <p>La cabecera y la navegación vienen de la Página Maestra (Site.Master).</p>

        <%-- El botón de Logout ahora está en la Master Page, así que puedes quitarlo de aquí si quieres,
             o dejarlo como una opción adicional en la página principal.
             Si lo quitas, asegúrate de quitar también su evento Click del Default.aspx.cs
        <asp:Button ID="btnLogout" runat="server" Text="Cerrar Sesión (desde Default)" OnClick="btnLogout_Click" CssClass="btn btn-warning mt-3" />
        --%>
        <asp:Button ID="btnLogout" runat="server" Text="Cerrar Sesión" OnClick="btnLogout_Click" CssClass="btn btn-warning mt-3" />

    </div>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <%-- Puedes agregar JavaScript específico para esta página aquí si es necesario --%>
    <script>
        console.log("Página Default.aspx cargada usando Site.Master");
    </script>
</asp:Content>
