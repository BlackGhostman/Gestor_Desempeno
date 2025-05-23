<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="GestionDetalleEstado.aspx.cs" Inherits="Gestor_Desempeno.GestionDetalleEstado" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
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

        .edit-control {
            width: 100%;
        }

        .filter-container {
            margin-bottom: 15px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1>Gestión de Estados Detallados</h1>
    <hr />

    <asp:Literal ID="litMensaje" runat="server" EnableViewState="false"></asp:Literal>

    <%-- Filtro por Clase --%>
    <div class="filter-container row">
        <div class="col-md-4">
            <label for="ddlClaseFiltro" class="form-label">Filtrar por Clase:</label>
            <asp:DropDownList ID="ddlClaseFiltro" runat="server" CssClass="form-select"
                DataTextField="Nombre" DataValueField="IdClase" AutoPostBack="true"
                OnSelectedIndexChanged="ddlClaseFiltro_SelectedIndexChanged">
            </asp:DropDownList>
        </div>
    </div>


    <%-- GridView para mostrar los detalles de estado --%>
    <div class="gridview-container table-responsive">
        <asp:GridView ID="gvDetalleEstado" runat="server" AutoGenerateColumns="False"
            DataKeyNames="Id_Detalle_Estado" CssClass="table table-striped table-bordered table-hover"
            OnRowEditing="gvDetalleEstado_RowEditing" OnRowUpdating="gvDetalleEstado_RowUpdating"
            OnRowCancelingEdit="gvDetalleEstado_RowCancelingEdit" OnRowDeleting="gvDetalleEstado_RowDeleting"
            AllowPaging="True" PageSize="10" OnPageIndexChanging="gvDetalleEstado_PageIndexChanging"
            EmptyDataText="No se encontraron estados para la clase seleccionada." OnRowDataBound="gvDetalleEstado_RowDataBound">
            <Columns>
                <%-- Columna ID (Opcional) --%>
                <asp:BoundField DataField="Id_Detalle_Estado" HeaderText="ID" ReadOnly="True" SortExpression="Id_Detalle_Estado" />

                <%-- Columna Clase --%>
                <asp:TemplateField HeaderText="Clase" SortExpression="NombreClase">
                    <EditItemTemplate>
                        <asp:DropDownList ID="ddlEditClase" runat="server" CssClass="form-select edit-control"
                            DataValueField="IdClase" DataTextField="Nombre">
                            <%-- ELIMINAR ESTA LÍNEA: SelectedValue='<%# Bind("IdClase") %>' --%>
                        </asp:DropDownList>
                        <%-- ... validadores si los hubiera ... --%>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:Label ID="lblClase" runat="server" Text='<%# Eval("NombreClase") %>'></asp:Label>
                        <asp:HiddenField ID="hfIdClase" runat="server" Value='<%# Eval("IdClase") %>' />
                    </ItemTemplate>
                </asp:TemplateField>

                <%-- Columna Descripción --%>
                <asp:TemplateField HeaderText="Descripción del Estado" SortExpression="Descripcion">
                    <ItemTemplate>
                        <asp:Label ID="lblDescripcion" runat="server" Text='<%# Eval("Descripcion") %>'></asp:Label>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtEditDescripcion" runat="server" Text='<%# Bind("Descripcion") %>' CssClass="form-control edit-control" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEditDescripcion" runat="server" ControlToValidate="txtEditDescripcion"
                            ErrorMessage="Descripción es requerida." CssClass="text-danger small" Display="Dynamic" ValidationGroup="EditValidation">*</asp:RequiredFieldValidator>
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Columna de Comandos --%>
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <asp:Button ID="btnEdit" runat="server" CommandName="Edit" Text="Editar" CssClass="btn btn-sm btn-outline-primary gridview-button" CausesValidation="false" />
                        <asp:Button ID="btnDelete" runat="server" CommandName="Delete" Text="Eliminar" CssClass="btn btn-sm btn-outline-danger gridview-button"
                            OnClientClick="return confirm('ADVERTENCIA: Esta acción eliminará permanentemente el estado. ¿Está seguro? Puede fallar si el estado está en uso.');" CausesValidation="false" />
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

    <%-- Sección para agregar nuevo detalle --%>
    <div class="form-container mt-4">
        <h4>Agregar Nuevo Estado Detallado</h4>
        <div class="row g-3 align-items-end">
            <div class="col-md-5">
                <label for="ddlNuevaClase" class="form-label">Clase (Opcional):</label>
                <asp:DropDownList ID="ddlNuevaClase" runat="server" CssClass="form-select" DataValueField="IdClase" DataTextField="Nombre" AppendDataBoundItems="true">
                    <asp:ListItem Text="-- Sin Clase Específica --" Value="0"></asp:ListItem>
                    <%-- Opción para NULL --%>
                </asp:DropDownList>
                <%-- No se necesita validador si es opcional --%>
            </div>
            <div class="col-md-5">
                <label for="txtNuevaDescripcion" class="form-label">Descripción Estado:</label>
                <asp:TextBox ID="txtNuevaDescripcion" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvNuevaDescripcion" runat="server" ControlToValidate="txtNuevaDescripcion"
                    ErrorMessage="Descripción requerida." Display="Dynamic" CssClass="text-danger small" ValidationGroup="NewValidation">*</asp:RequiredFieldValidator>
            </div>
            <div class="col-md-2">
                <asp:Button ID="btnAgregar" runat="server" Text="Agregar Estado" CssClass="btn btn-primary w-100" OnClick="btnAgregar_Click" ValidationGroup="NewValidation" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
</asp:Content>
