<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="GestionEncargados.aspx.cs" Inherits="Gestor_Desempeno.GestionEncargados" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Estilos (similares a páginas anteriores) --%>
    <style>
        .gridview-container {
            margin-top: 20px;
        }

        .form-container {
            margin-bottom: 20px;
            padding: 20px;
            border: 1px solid #ddd;
            border-radius: 5px;
            background-color: #f9f9f9;
        }

        .gridview-button {
            padding: 0.2rem 0.5rem;
            font-size: 0.875rem;
            margin-right: 5px;
        }
        /* Asegurar que el dropdown en modo edición ocupe el ancho */
        .edit-dropdown {
            width: 100%;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1>Gestión de Encargados por Área</h1>
    <hr />

    <%-- Mensajes de estado --%>
    <asp:Literal ID="litMensaje" runat="server" EnableViewState="false"></asp:Literal>

    <%-- GridView para mostrar las asignaciones --%>
    <div class="gridview-container table-responsive">
        <asp:GridView ID="gvEncargados" runat="server" AutoGenerateColumns="False"
            DataKeyNames="IdEncargadoArea" CssClass="table table-striped table-bordered table-hover"
            OnRowEditing="gvEncargados_RowEditing" OnRowUpdating="gvEncargados_RowUpdating"
            OnRowCancelingEdit="gvEncargados_RowCancelingEdit" OnRowDeleting="gvEncargados_RowDeleting"
            AllowPaging="True" PageSize="10" OnPageIndexChanging="gvEncargados_PageIndexChanging"
            OnRowDataBound="gvEncargados_RowDataBound" EmptyDataText="No se encontraron encargados asignados a áreas.">
            <Columns>
                <%-- Columna ID (Oculta usualmente, usada como DataKey) --%>
                <%-- <asp:BoundField DataField="IdEncargadoArea" HeaderText="ID" ReadOnly="True" SortExpression="IdEncargadoArea" /> --%>

                <%-- Columna Área Ejecutora --%>
                <asp:TemplateField HeaderText="Área Ejecutora" SortExpression="NombreAreaEjecutora">
                    <ItemTemplate>
                        <asp:Label ID="lblAreaNombre" runat="server" Text='<%# Eval("NombreAreaEjecutora") %>'></asp:Label>
                        <%-- Guardar el ID del área en un control oculto para la edición --%>
                        <asp:HiddenField ID="hfIdAreaEjecutora" runat="server" Value='<%# Eval("Id_Area_Ejecutora") %>' />
                    </ItemTemplate>
                    <EditItemTemplate>
                        <%-- DropDownList para seleccionar el área en modo edición --%>
                        <asp:DropDownList ID="ddlEditArea" runat="server" CssClass="form-select edit-dropdown"
                            DataValueField="Id_Area_Ejecutora" DataTextField="Nombre">
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvEditArea" runat="server" ControlToValidate="ddlEditArea"
                            InitialValue="0" ErrorMessage="Seleccione un área." CssClass="text-danger small" Display="Dynamic" ValidationGroup="EditValidation">*</asp:RequiredFieldValidator>
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Columna Usuario --%>
                <asp:TemplateField HeaderText="Usuario Encargado" SortExpression="Usuario">
                    <ItemTemplate>
                        <asp:Label ID="lblUsuario" runat="server" Text='<%# Eval("Usuario") %>'></asp:Label>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <%--<asp:TextBox ID="txtEditUsuario" runat="server" Text='<%# Bind("Usuario") %>' CssClass="form-control" MaxLength="20"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEditUsuario" runat="server" ControlToValidate="txtEditUsuario"
                            ErrorMessage="Usuario es requerido." CssClass="text-danger small" Display="Dynamic" ValidationGroup="EditValidation">*</asp:RequiredFieldValidator>--%>
                        <asp:DropDownList ID="ddlEditUsuario" runat="server" CssClass="form-select edit-dropdown">
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvEditUsuario" runat="server" ControlToValidate="ddlEditUsuario"
                            InitialValue="0" ErrorMessage="Seleccione un usuario." CssClass="text-danger small" Display="Dynamic" ValidationGroup="EditValidation">*</asp:RequiredFieldValidator>
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Columna de Comandos --%>
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <asp:Button ID="btnEdit" runat="server" CommandName="Edit" Text="Editar" CssClass="btn btn-sm btn-outline-primary gridview-button" CausesValidation="false" />
                        <asp:Button ID="btnDelete" runat="server" CommandName="Delete" Text="Eliminar" CssClass="btn btn-sm btn-outline-danger gridview-button"
                            OnClientClick="return confirm('¿Está seguro de que desea eliminar esta asignación de encargado?');" CausesValidation="false" />
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:Button ID="btnUpdate" runat="server" CommandName="Update" Text="Actualizar" CssClass="btn btn-sm btn-success gridview-button" ValidationGroup="EditValidation" />
                        <asp:Button ID="btnCancel" runat="server" CommandName="Cancel" Text="Cancelar" CssClass="btn btn-sm btn-secondary gridview-button" CausesValidation="false" />
                    </EditItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle CssClass="pagination-ys" />
        </asp:GridView>
    </div>

    <%-- Sección para agregar nueva asignación --%>
    <div class="form-container mt-4">
        <h4>Asignar Nuevo Encargado a Área</h4>
        <div class="row g-3 align-items-end">
            <div class="col-md-5">
                <label for="ddlAreas" class="form-label">Área Ejecutora:</label>
                <asp:DropDownList ID="ddlAreas" runat="server" CssClass="form-select" DataValueField="Id_Area_Ejecutora" DataTextField="Nombre">
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="rfvArea" runat="server" ControlToValidate="ddlAreas"
                    InitialValue="0" ErrorMessage="Seleccione un área." Display="Dynamic" CssClass="text-danger small" ValidationGroup="NewValidation">*</asp:RequiredFieldValidator>
            </div>
            <div class="col-md-4">
                <label for="<%= ddlNuevoUsuario.ClientID %>" class="form-label">Usuario Encargado:</label>
                <%-- Etiqueta actualizada --%>
                <asp:DropDownList ID="ddlNuevoUsuario" runat="server" CssClass="form-select">
                    <%-- Los ítems se cargarán desde el code-behind --%>
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="rfvNuevoUsuario" runat="server" ControlToValidate="ddlNuevoUsuario"
                    InitialValue="0" ErrorMessage="Seleccione un usuario." CssClass="text-danger small" Display="Dynamic" ValidationGroup="NewValidation">*</asp:RequiredFieldValidator>
            </div>
            <div class="col-md-3">
                <asp:Button ID="btnAgregar" runat="server" Text="Asignar Encargado" CssClass="btn btn-primary w-100" OnClick="btnAgregar_Click" ValidationGroup="NewValidation" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
</asp:Content>
