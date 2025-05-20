<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="GestionTiposObjetivo.aspx.cs" Inherits="Gestor_Desempeno.GestionTiposObjetivo" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .gridview-container { margin-top: 20px; }
        .form-container { margin-bottom: 20px; padding: 20px; border: 1px solid #ddd; border-radius: 5px; background-color: #f9f9f9; }
        .gridview-button { padding: 0.2rem 0.5rem; font-size: 0.875rem; margin-right: 5px; }
        .descripcion-larga { max-width: 400px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1>Gestión de Tipos de Objetivo</h1>
    <hr />

    <asp:Literal ID="litMensaje" runat="server" EnableViewState="false"></asp:Literal>

    <%-- GridView para mostrar tipos de objetivo --%>
    <div class="gridview-container table-responsive">
        <asp:GridView ID="gvTiposObjetivo" runat="server" AutoGenerateColumns="False"
            DataKeyNames="Id_Tipo_Objetivo" CssClass="table table-striped table-bordered table-hover"
            OnRowEditing="gvTiposObjetivo_RowEditing" OnRowUpdating="gvTiposObjetivo_RowUpdating"
            OnRowCancelingEdit="gvTiposObjetivo_RowCancelingEdit" OnRowDeleting="gvTiposObjetivo_RowDeleting"
            AllowPaging="True" PageSize="10" OnPageIndexChanging="gvTiposObjetivo_PageIndexChanging"
            EmptyDataText="No se encontraron tipos de objetivo activos para mostrar.">
            <Columns>
                <%-- Columna ID (Opcional) --%>
                <asp:BoundField DataField="Id_Tipo_Objetivo" HeaderText="ID" ReadOnly="True" SortExpression="Id_Tipo_Objetivo" />

                <%-- Columna Nombre --%>
                <asp:TemplateField HeaderText="Nombre (Max 10)" SortExpression="Nombre">
                    <ItemTemplate>
                        <asp:Label ID="lblNombre" runat="server" Text='<%# Eval("Nombre") %>'></asp:Label>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtEditNombre" runat="server" Text='<%# Bind("Nombre") %>' CssClass="form-control" MaxLength="10"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEditNombre" runat="server" ControlToValidate="txtEditNombre"
                            ErrorMessage="Nombre es requerido." CssClass="text-danger small" Display="Dynamic" ValidationGroup="EditValidation">*</asp:RequiredFieldValidator>
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Columna Descripción --%>
                <asp:TemplateField HeaderText="Descripción (Max 100)">
                     <ItemTemplate>
                         <div class="descripcion-larga" title='<%# Eval("Descripcion") %>'>
                             <asp:Label ID="lblDescripcion" runat="server" Text='<%# Eval("Descripcion") %>'></asp:Label>
                         </div>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtEditDescripcion" runat="server" Text='<%# Bind("Descripcion") %>' CssClass="form-control" MaxLength="100" TextMode="MultiLine" Rows="2"></asp:TextBox>
                    </EditItemTemplate>
                </asp:TemplateField>

                 <%-- Columna Estado --%>
                 <asp:TemplateField HeaderText="Estado" SortExpression="Estado">
                     <ItemTemplate>
                         <asp:Label ID="lblEstado" runat="server" Text='<%# (bool)Eval("Estado") ? "Activo" : "Inactivo" %>'></asp:Label>
                     </ItemTemplate>
                     <EditItemTemplate>
                         <asp:CheckBox ID="chkEditEstado" runat="server" Checked='<%# Bind("Estado") %>' /> Activo
                     </EditItemTemplate>
                 </asp:TemplateField>

                <%-- Columna de Comandos --%>
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <asp:Button ID="btnEdit" runat="server" CommandName="Edit" Text="Editar" CssClass="btn btn-sm btn-outline-primary gridview-button" CausesValidation="false"/>
                        <asp:Button ID="btnDelete" runat="server" CommandName="Delete" Text="Eliminar" CssClass="btn btn-sm btn-outline-danger gridview-button"
                            OnClientClick="return confirm('¿Está seguro de que desea eliminar (marcar como inactivo) este tipo de objetivo?');" CausesValidation="false"/>
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

    <%-- Sección para agregar nuevo tipo --%>
    <div class="form-container mt-4">
        <h4>Agregar Nuevo Tipo de Objetivo</h4>
        <div class="row g-3">
            <div class="col-md-3">
                <label for="txtNuevoNombre" class="form-label">Nombre (Max 10):</label>
                <asp:TextBox ID="txtNuevoNombre" runat="server" CssClass="form-control" MaxLength="10"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvNuevoNombre" runat="server" ControlToValidate="txtNuevoNombre"
                    ErrorMessage="Nombre es requerido." Display="Dynamic" CssClass="text-danger small" ValidationGroup="NewValidation">*</asp:RequiredFieldValidator>
            </div>
            <div class="col-md-6">
                <label for="txtNuevaDescripcion" class="form-label">Descripción (Max 100):</label>
                <asp:TextBox ID="txtNuevaDescripcion" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
            </div>
             <div class="col-md-3 d-flex align-items-end">
                 <div class="form-check mb-2">
                     <asp:CheckBox ID="chkNuevoEstado" runat="server" Text="&nbsp;Activo" Checked="true" CssClass="form-check-input"/>
                     <label class="form-check-label" for="<%= chkNuevoEstado.ClientID %>">Estado</label>
                 </div>
            </div>
            <div class="col-12">
                <asp:Button ID="btnAgregar" runat="server" Text="Agregar Tipo" CssClass="btn btn-primary" OnClick="btnAgregar_Click" ValidationGroup="NewValidation"/>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
</asp:Content>
