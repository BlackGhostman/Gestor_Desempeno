<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="MetasRapidas.aspx.cs" Inherits="Gestor_Desempeno.MetasRapidas" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Estilos adicionales si son necesarios */
        .form-label {
            font-weight: bold;
        }
        .container-form {
            max-width: 700px; /* Ancho máximo del contenedor del formulario */
            margin-top: 20px;
            margin-bottom: 20px;
            padding: 20px;
            background-color: #f8f9fa; /* Un color de fondo suave */
            border-radius: 8px; /* Bordes redondeados */
            box-shadow: 0 0 10px rgba(0,0,0,0.1); /* Sombra ligera */
        }
        .button-container {
            text-align: right; /* Alinea el botón a la derecha */
            margin-top: 20px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container container-form">
        <h2 class="mb-4 text-center">Ingreso Rápido de Metas Individuales</h2>

        <%-- Placeholder para mensajes de éxito o error --%>
        <asp:Panel ID="pnlMensaje" runat="server" Visible="false" CssClass="mb-3">
            <button type="button" class="btn-close float-end" data-bs-dismiss="alert" aria-label="Close" onclick="document.getElementById('<%= pnlMensaje.ClientID %>').style.display='none'; return false;"></button>
            <asp:Label ID="lblMensaje" runat="server" Text=""></asp:Label>
        </asp:Panel>

        <%-- Campo: Usuario (Colaborador) --%>
        <div class="mb-3">
            <label for="<%= ddlUsuario.ClientID %>" class="form-label">Colaborador Asignado:</label>
            <asp:DropDownList ID="ddlUsuario" runat="server" CssClass="form-select" AppendDataBoundItems="true">
                <asp:ListItem Text="Seleccione un colaborador..." Value=""></asp:ListItem>
            </asp:DropDownList>
            <asp:RequiredFieldValidator ID="rfvUsuario" runat="server" ControlToValidate="ddlUsuario"
                ErrorMessage="Debe seleccionar un colaborador." CssClass="text-danger" Display="Dynamic" InitialValue="">
            </asp:RequiredFieldValidator>
        </div>

        <%-- Campo: Descripción de la Meta --%>
        <div class="mb-3">
            <label for="<%= txtDescripcion.ClientID %>" class="form-label">Descripción de la Meta:</label>
            <asp:TextBox ID="txtDescripcion" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
            <asp:RequiredFieldValidator ID="rfvDescripcion" runat="server" ControlToValidate="txtDescripcion"
                ErrorMessage="La descripción es obligatoria." CssClass="text-danger" Display="Dynamic">
            </asp:RequiredFieldValidator>
        </div>

        <%-- Campo: Fecha Inicial --%>
        <div class="row mb-3">
            <div class="col-md-6">
                <label for="<%= txtFechaInicial.ClientID %>" class="form-label">Fecha Inicial:</label>
                <asp:TextBox ID="txtFechaInicial" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvFechaInicial" runat="server" ControlToValidate="txtFechaInicial"
                    ErrorMessage="La fecha inicial es obligatoria." CssClass="text-danger" Display="Dynamic">
                </asp:RequiredFieldValidator>
                <asp:CompareValidator ID="cvFechaInicial" runat="server" ControlToValidate="txtFechaInicial" 
                    Type="Date" Operator="DataTypeCheck" ErrorMessage="Formato de fecha inicial inválido." 
                    CssClass="text-danger" Display="Dynamic"></asp:CompareValidator>
            </div>
        
            <%-- Campo: Fecha Final --%>
            <div class="col-md-6">
                <label for="<%= txtFechaFinal.ClientID %>" class="form-label">Fecha Final:</label>
                <asp:TextBox ID="txtFechaFinal" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvFechaFinal" runat="server" ControlToValidate="txtFechaFinal"
                    ErrorMessage="La fecha final es obligatoria." CssClass="text-danger" Display="Dynamic">
                </asp:RequiredFieldValidator>
                 <asp:CompareValidator ID="cvFechaFinal" runat="server" ControlToValidate="txtFechaFinal" 
                    Type="Date" Operator="DataTypeCheck" ErrorMessage="Formato de fecha final inválido." 
                    CssClass="text-danger" Display="Dynamic"></asp:CompareValidator>
                <asp:CompareValidator ID="cvFechasRango" runat="server" ControlToValidate="txtFechaFinal" 
                    ControlToCompare="txtFechaInicial" Operator="GreaterThanEqual" Type="Date"
                    ErrorMessage="La fecha final debe ser mayor o igual a la fecha inicial." 
                    CssClass="text-danger" Display="Dynamic"></asp:CompareValidator>
            </div>
        </div>

        <%-- Campo: Es Finalizable --%>
        <div class="mb-3 form-check">
            <asp:CheckBox ID="chkEsFinalizable" runat="server" CssClass="form-check-input" Checked="false" Visible="false" />
            
        </div>

        <%-- Botón para Guardar --%>
        <div class="button-container">
            <asp:Button ID="btnGuardarMetaRapida" runat="server" Text="Guardar Meta Rápida" CssClass="btn btn-primary" OnClick="btnGuardarMetaRapida_Click" />
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
</asp:Content>
