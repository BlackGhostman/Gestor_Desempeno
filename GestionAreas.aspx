<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="GestionAreas.aspx.cs" Inherits="Gestor_Desempeno.GestionAreas" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Estilos (similares a GestionPeriodos) --%>
    <style>
        .gridview-container { margin-top: 20px; }
        .form-container { margin-bottom: 20px; padding: 20px; border: 1px solid #ddd; border-radius: 5px; background-color: #f9f9f9; }
        .gridview-button { padding: 0.2rem 0.5rem; font-size: 0.875rem; margin-right: 5px; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1>Gestión de Áreas Ejecutoras</h1>
    <hr />

    <%-- Mensajes de estado --%>
    <asp:Literal ID="litMensaje" runat="server" EnableViewState="false"></asp:Literal>

    <%-- GridView para mostrar las áreas --%>
    <div class="gridview-container table-responsive">
        <asp:GridView ID="gvAreas" runat="server" AutoGenerateColumns="False"
            DataKeyNames="Id_Area_Ejecutora" CssClass="table table-striped table-bordered table-hover"
            OnRowEditing="gvAreas_RowEditing" OnRowUpdating="gvAreas_RowUpdating"
            OnRowCancelingEdit="gvAreas_RowCancelingEdit" OnRowDeleting="gvAreas_RowDeleting"
            AllowPaging="True" PageSize="10" OnPageIndexChanging="gvAreas_PageIndexChanging"
            EmptyDataText="No se encontraron áreas ejecutoras para mostrar.">
            <Columns>
                <%-- Columna ID --%>
                <asp:BoundField DataField="Id_Area_Ejecutora" HeaderText="ID" ReadOnly="True" SortExpression="Id_Area_Ejecutora" />

                <%-- Columna Nombre --%>
                <asp:TemplateField HeaderText="Nombre del Área" SortExpression="Nombre">
                    <ItemTemplate>
                        <asp:Label ID="lblNombre" runat="server" Text='<%# Bind("Nombre") %>'></asp:Label>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtEditNombre" runat="server" Text='<%# Bind("Nombre") %>' CssClass="form-control" MaxLength="150"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEditNombre" runat="server" ControlToValidate="txtEditNombre"
                            ErrorMessage="Nombre es requerido." CssClass="text-danger small" Display="Dynamic" ValidationGroup="EditValidation">*</asp:RequiredFieldValidator>
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Columna de Comandos --%>
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <asp:Button ID="btnEdit" runat="server" CommandName="Edit" Text="Editar" CssClass="btn btn-sm btn-outline-primary gridview-button" CausesValidation="false"/>
                        <asp:Button ID="btnDelete" runat="server" CommandName="Delete" Text="Eliminar" CssClass="btn btn-sm btn-outline-danger gridview-button"
                            OnClientClick="return confirm('ADVERTENCIA: Esta acción eliminará permanentemente el área ejecutora. ¿Está seguro? Puede fallar si tiene registros asociados.');" CausesValidation="false"/>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:Button ID="btnUpdate" runat="server" CommandName="Update" Text="Actualizar" CssClass="btn btn-sm btn-success gridview-button" ValidationGroup="EditValidation"/>
                        <asp:Button ID="btnCancel" runat="server" CommandName="Cancel" Text="Cancelar" CssClass="btn btn-sm btn-secondary gridview-button" CausesValidation="false"/>
                    </EditItemTemplate>
                </asp:TemplateField>
            </Columns>
             <PagerStyle CssClass="pagination-ys" />
        </asp:GridView>
    </div>

    <%-- Sección para agregar nueva área --%>
    <div class="form-container mt-4">
        <h4>Agregar Nueva Área Ejecutora</h4>
        <div class="row g-3 align-items-end">
            <div class="col-md-8">
                <label for="txtNuevoNombre" class="form-label">Nombre del Área:</label>
                <asp:TextBox ID="txtNuevoNombre" runat="server" CssClass="form-control" MaxLength="150"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvNuevoNombre" runat="server" ControlToValidate="txtNuevoNombre"
                    ErrorMessage="El nombre es requerido." Display="Dynamic" CssClass="text-danger small" ValidationGroup="NewValidation">*</asp:RequiredFieldValidator>
            </div>
            <div class="col-md-4">
                <asp:Button ID="btnAgregar" runat="server" Text="Agregar Área" CssClass="btn btn-primary w-100" OnClick="btnAgregar_Click" ValidationGroup="NewValidation"/>
            </div>
        </div>
    </div>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
</asp:Content>
