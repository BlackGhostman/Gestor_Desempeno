<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="GestionPeriodos.aspx.cs" Inherits="Gestor_Desempeno.GestionPeriodos" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
      <%-- Estilos adicionales si son necesarios --%>
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
        .date-input { /* Estilo opcional para inputs de fecha */
            max-width: 150px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <h1>Gestión de Periodos</h1>
    <hr />

    <%-- Mensajes de estado --%>
    <asp:Literal ID="litMensaje" runat="server" EnableViewState="false"></asp:Literal>

    <%-- GridView para mostrar los periodos --%>
    <div class="gridview-container table-responsive">
        <asp:GridView ID="gvPeriodos" runat="server" AutoGenerateColumns="False"
            DataKeyNames="Id_Periodo" CssClass="table table-striped table-bordered table-hover"
            OnRowEditing="gvPeriodos_RowEditing" OnRowUpdating="gvPeriodos_RowUpdating"
            OnRowCancelingEdit="gvPeriodos_RowCancelingEdit" OnRowDeleting="gvPeriodos_RowDeleting"
            AllowPaging="True" PageSize="10" OnPageIndexChanging="gvPeriodos_PageIndexChanging">
            <Columns>
                <asp:BoundField DataField="Id_Periodo" HeaderText="ID" ReadOnly="True" SortExpression="Id_Periodo" />

                <asp:TemplateField HeaderText="Nombre" SortExpression="Nombre">
                    <ItemTemplate>
                        <asp:Label ID="lblNombre" runat="server" Text='<%# Bind("Nombre") %>'></asp:Label>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtEditNombre" runat="server" Text='<%# Bind("Nombre") %>' CssClass="form-control" MaxLength="20"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEditNombre" runat="server" ControlToValidate="txtEditNombre"
                            ErrorMessage="Nombre es requerido." CssClass="text-danger small" Display="Dynamic" ValidationGroup="EditValidation">*</asp:RequiredFieldValidator>
                    </EditItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Descripción" SortExpression="Descripcion">
                    <ItemTemplate>
                        <asp:Label ID="lblDescripcion" runat="server" Text='<%# Bind("Descripcion") %>'></asp:Label>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtEditDescripcion" runat="server" Text='<%# Bind("Descripcion") %>' CssClass="form-control" MaxLength="100"></asp:TextBox>
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Nueva Columna Fecha Inicio --%>
                <asp:TemplateField HeaderText="Fecha Inicio" SortExpression="Fecha_Inicio">
                    <ItemTemplate>
                        <asp:Label ID="lblFechaInicio" runat="server" Text='<%# Bind("Fecha_Inicio", "{0:dd/MM/yyyy}") %>'></asp:Label>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <%-- Usar TextMode="Date" para un selector de fecha nativo del navegador si es compatible y deseado --%>
                        <asp:TextBox ID="txtEditFechaInicio" runat="server" Text='<%# Bind("Fecha_Inicio", "{0:yyyy-MM-dd}") %>' CssClass="form-control date-input" TextMode="Date"></asp:TextBox>
                         <%-- Podrías agregar un CompareValidator para asegurar que es una fecha válida o un RangeValidator --%>
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Nueva Columna Fecha Final --%>
                <asp:TemplateField HeaderText="Fecha Final" SortExpression="Fecha_Final">
                    <ItemTemplate>
                        <asp:Label ID="lblFechaFinal" runat="server" Text='<%# Bind("Fecha_Final", "{0:dd/MM/yyyy}") %>'></asp:Label>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtEditFechaFinal" runat="server" Text='<%# Bind("Fecha_Final", "{0:yyyy-MM-dd}") %>' CssClass="form-control date-input" TextMode="Date"></asp:TextBox>
                        <%-- Podrías agregar un CompareValidator para asegurar que Fecha_Final es posterior a Fecha_Inicio --%>
                    </EditItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Estado" SortExpression="Estado">
                    <ItemTemplate>
                        <asp:Label ID="lblEstado" runat="server" Text='<%# (bool)Eval("Estado") ? "Activo" : "Inactivo" %>'></asp:Label>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:CheckBox ID="chkEditEstado" runat="server" Checked='<%# Bind("Estado") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <asp:Button ID="btnEdit" runat="server" CommandName="Edit" Text="Editar" CssClass="btn btn-sm btn-outline-primary gridview-button" CausesValidation="false"/>
                        <asp:Button ID="btnDelete" runat="server" CommandName="Delete" Text="Eliminar" CssClass="btn btn-sm btn-outline-danger gridview-button"
                            OnClientClick="return confirm('¿Está seguro de que desea eliminar (marcar como inactivo) este periodo?');" CausesValidation="false"/>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:Button ID="btnUpdate" runat="server" CommandName="Update" Text="Actualizar" CssClass="btn btn-sm btn-success gridview-button" ValidationGroup="EditValidation"/>
                        <asp:Button ID="btnCancel" runat="server" CommandName="Cancel" Text="Cancelar" CssClass="btn btn-sm btn-secondary gridview-button" CausesValidation="false"/>
                    </EditItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                No se encontraron periodos para mostrar.
            </EmptyDataTemplate>
            <PagerStyle CssClass="pagination-ys" />
        </asp:GridView>
    </div>

    <%-- Sección para agregar nuevo periodo --%>
    <div class="form-container mt-4">
        <h4>Agregar Nuevo Periodo</h4>
        <div class="row g-3">
            <div class="col-md-4">
                <label for="<%=txtNuevoNombre.ClientID %>" class="form-label">Nombre:</label>
                <asp:TextBox ID="txtNuevoNombre" runat="server" CssClass="form-control" MaxLength="20"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvNuevoNombre" runat="server" ControlToValidate="txtNuevoNombre"
                    ErrorMessage="El nombre es requerido." Display="Dynamic" CssClass="text-danger small" ValidationGroup="NewValidation">*</asp:RequiredFieldValidator>
            </div>
            <div class="col-md-6">
                <label for="<%=txtNuevaDescripcion.ClientID %>" class="form-label">Descripción:</label>
                <asp:TextBox ID="txtNuevaDescripcion" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
            </div>
             <div class="col-md-4">
                <label for="<%=txtNuevaFechaInicio.ClientID %>" class="form-label">Fecha Inicio:</label>
                <asp:TextBox ID="txtNuevaFechaInicio" runat="server" CssClass="form-control date-input" TextMode="Date"></asp:TextBox>
                <%-- Podrías agregar validadores aquí también --%>
            </div>
            <div class="col-md-4">
                <label for="<%=txtNuevaFechaFinal.ClientID %>" class="form-label">Fecha Final:</label>
                <asp:TextBox ID="txtNuevaFechaFinal" runat="server" CssClass="form-control date-input" TextMode="Date"></asp:TextBox>
            </div>
            <div class="col-md-2 d-flex align-items-end">
                <div class="form-check">
                    <asp:CheckBox ID="chkNuevoEstado" runat="server" Text=" Activo" Checked="true" CssClass="form-check-input"/>
                    <label class="form-check-label" for="<%=chkNuevoEstado.ClientID %>">Estado</label>
                </div>
            </div>
            <div class="col-12">
                <asp:Button ID="btnAgregar" runat="server" Text="Agregar Periodo" CssClass="btn btn-primary" OnClick="btnAgregar_Click" ValidationGroup="NewValidation"/>
            </div>
        </div>
    </div>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
</asp:Content>
