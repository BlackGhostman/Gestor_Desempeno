<%@ Page Title="Gestión de Objetivos" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="GestionObjetivos.aspx.cs" Inherits="Gestor_Desempeno.GestionObjetivos" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
     <style>
        .gridview-container { margin-top: 20px; }
        /* Removed form-container style */
        .gridview-button { padding: 0.2rem 0.5rem; font-size: 0.875rem; margin-right: 5px; }
        .descripcion-larga { max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .modal-body .detail-label { font-weight: 600; color: #555; }
        .modal-body .detail-value { background-color: #e9ecef; padding: 0.375rem 0.75rem; border-radius: 0.25rem; min-height: calc(1.5em + 0.75rem + 2px); display: block; word-wrap: break-word; }
        .modal-body .detail-value-textarea { min-height: 80px; white-space: pre-wrap; }
        .filter-container { margin-bottom: 15px; padding: 15px; border: 1px solid #e0e0e0; border-radius: 5px; background-color: #f1f1f1;}
        .modal-body .form-label { margin-bottom: 0.25rem; }
        .modal-body .form-control, .modal-body .form-select { margin-bottom: 0.5rem; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%--<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>--%>

    <%-- ** UPDATED: Single UpdatePanel wrapping Grid, Button, and Modal ** --%>
    <asp:UpdatePanel ID="UpdatePanelPage" runat="server" UpdateMode="Conditional">
        <ContentTemplate>

            <h1>Gestión de Objetivos</h1>
            <hr />

            <asp:Literal ID="litMensaje" runat="server" EnableViewState="false"></asp:Literal>

            <%-- Filtro --%>
             <div class="filter-container row g-3 align-items-end mb-3">
                <div class="col-md-4">
                     <label for="ddlTipoObjetivoFiltro" class="form-label">Filtrar por Tipo:</label>
                     <asp:DropDownList ID="ddlTipoObjetivoFiltro" runat="server" CssClass="form-select"
                         DataTextField="Nombre" DataValueField="IdTipoObjetivo" AutoPostBack="true"
                         OnSelectedIndexChanged="ddlTipoObjetivoFiltro_SelectedIndexChanged">
                     </asp:DropDownList>
                </div>
                 <div class="col-md-2">
                     <asp:Button ID="btnLimpiarFiltros" runat="server" Text="Limpiar Filtro" CssClass="btn btn-outline-secondary w-100" OnClick="btnLimpiarFiltros_Click" CausesValidation="false"/>
                </div>
            </div>


            <%-- GridView para mostrar objetivos --%>
            <div class="gridview-container table-responsive">
                <asp:GridView ID="gvObjetivos" runat="server" AutoGenerateColumns="False"
                    DataKeyNames="Id_Objetivo" CssClass="table table-striped table-bordered table-hover table-sm"
                    AllowPaging="True" PageSize="10" OnPageIndexChanging="gvObjetivos_PageIndexChanging"
                    EmptyDataText="No se encontraron objetivos para mostrar."
                    OnRowCommand="gvObjetivos_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="NombrePeriodo" HeaderText="Periodo" SortExpression="NombrePeriodo" ReadOnly="True" />
                        <asp:BoundField DataField="NumObjetivo" HeaderText="Num." SortExpression="NumObjetivo" ReadOnly="True" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="NombreTipoObjetivo" HeaderText="Tipo" SortExpression="NombreTipoObjetivo" ReadOnly="True" />
                        <asp:BoundField DataField="Nombre" HeaderText="Nombre Objetivo" SortExpression="Nombre" ReadOnly="True" />
                        <asp:TemplateField HeaderText="Descripción">
                            <ItemTemplate>
                                 <div class="descripcion-larga" title='<%# Eval("Descripcion") %>'>
                                     <asp:Label ID="lblDescripcionGrid" runat="server" Text='<%# Eval("Descripcion") %>'></asp:Label>
                                 </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="DescripcionEstado" HeaderText="Estado" SortExpression="DescripcionEstado" ReadOnly="True" />
                        <asp:TemplateField HeaderText="Acciones">
                            <ItemTemplate>
                                <asp:Button ID="btnEditar" runat="server" CommandName="EditarObjetivo" CommandArgument='<%# Eval("Id_Objetivo") %>' Text="Editar" CssClass="btn btn-sm btn-outline-primary gridview-button" CausesValidation="false"/>
                                <asp:Button ID="btnEliminar" runat="server" CommandName="EliminarObjetivo" CommandArgument='<%# Eval("Id_Objetivo") %>' Text="Eliminar" CssClass="btn btn-sm btn-outline-danger gridview-button" CausesValidation="false" OnClientClick="return confirm('¿Está seguro de que desea eliminar (marcar como inactivo) este objetivo?');" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                     <PagerStyle CssClass="pagination-ys" />
                </asp:GridView>
            </div>

             <%-- Button to open Add modal --%>
             <div class="mt-3">
                 <asp:Button ID="btnAbrirModalAgregar" runat="server" Text="Agregar Nuevo Objetivo" CssClass="btn btn-success" OnClick="btnAbrirModalAgregar_Click" CausesValidation="false" />
             </div>

             <%-- Modal for Add/Edit (Now inside the main UpdatePanel) --%>
             <div class="modal fade" id="objetivoModal" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="objetivoModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-lg modal-dialog-centered">
                    <div class="modal-content">
                         <%-- ** REMOVED inner UpdatePanel (UpdatePanelModalContent) ** --%>
                         <%-- Content is now directly inside modal-content --%>
                        <div class="modal-header">
                            <h5 class="modal-title" id="objetivoModalLabel">
                                <asp:Label ID="lblModalTitle" runat="server" Text="Objetivo"></asp:Label>
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                        </div>
                        <div class="modal-body">
                             <asp:HiddenField ID="hfObjetivoId" runat="server" Value="0" />
                             <asp:Literal ID="litModalMensaje" runat="server" EnableViewState="false"></asp:Literal>

                             <div class="row g-3">
                                 <div class="col-md-4">
                                     <label for="ddlModalPeriodo" class="form-label">Periodo:</label>
                                     <asp:DropDownList ID="ddlModalPeriodo" runat="server" CssClass="form-select" DataValueField="IdPeriodo" DataTextField="Nombre"></asp:DropDownList>
                                     <asp:RequiredFieldValidator ID="rfvModalPeriodo" runat="server" ControlToValidate="ddlModalPeriodo" InitialValue="0" ErrorMessage="Seleccione Periodo." Display="Dynamic" CssClass="text-danger small" ValidationGroup="ModalValidation">*</asp:RequiredFieldValidator>
                                 </div>
                                 <div class="col-md-4">
                                     <label for="ddlModalTipoObj" class="form-label">Tipo Objetivo:</label>
                                     <asp:DropDownList ID="ddlModalTipoObj" runat="server" CssClass="form-select" DataValueField="IdTipoObjetivo" DataTextField="Nombre"></asp:DropDownList>
                                     <asp:RequiredFieldValidator ID="rfvModalTipoObj" runat="server" ControlToValidate="ddlModalTipoObj" InitialValue="0" ErrorMessage="Seleccione Tipo." Display="Dynamic" CssClass="text-danger small" ValidationGroup="ModalValidation">*</asp:RequiredFieldValidator>
                                 </div>
                                  <div class="col-md-4">
                                     <label for="ddlModalEstado" class="form-label">Estado:</label>
                                     <asp:DropDownList ID="ddlModalEstado" runat="server" CssClass="form-select" DataValueField="Id_Detalle_Estado" DataTextField="Descripcion"></asp:DropDownList>
                                     <asp:RequiredFieldValidator ID="rfvModalEstado" runat="server" ControlToValidate="ddlModalEstado" InitialValue="0" ErrorMessage="Seleccione Estado." Display="Dynamic" CssClass="text-danger small" ValidationGroup="ModalValidation">*</asp:RequiredFieldValidator>
                                 </div>
                                 <div class="col-md-2">
                                     <label for="txtModalNumObj" class="form-label">Num. Obj:</label>
                                     <asp:TextBox ID="txtModalNumObj" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                                     <asp:RangeValidator ID="rvModalNumObj" runat="server" ControlToValidate="txtModalNumObj" MinimumValue="1" MaximumValue="9999" Type="Integer" ErrorMessage="Num. Inválido" Display="Dynamic" CssClass="text-danger small" ValidationGroup="ModalValidation">*</asp:RangeValidator>
                                 </div>
                                 <div class="col-md-10">
                                     <label for="txtModalNombre" class="form-label">Nombre Objetivo:</label>
                                     <asp:TextBox ID="txtModalNombre" runat="server" CssClass="form-control" MaxLength="500"></asp:TextBox>
                                     <asp:RequiredFieldValidator ID="rfvModalNombre" runat="server" ControlToValidate="txtModalNombre" ErrorMessage="Nombre es requerido." Display="Dynamic" CssClass="text-danger small" ValidationGroup="ModalValidation">*</asp:RequiredFieldValidator>
                                 </div>
                                 <div class="col-12">
                                     <label for="txtModalDescripcion" class="form-label">Descripción:</label>
                                     <asp:TextBox ID="txtModalDescripcion" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                                 </div>
                             </div>
                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnGuardarModal" runat="server" Text="Guardar" CssClass="btn btn-primary" OnClick="btnGuardarModal_Click" ValidationGroup="ModalValidation" />
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        </div>
                    </div> <%-- End Modal Content --%>
                </div>
            </div> <%-- End Modal --%>

        </ContentTemplate>
         <%-- Triggers for the main UpdatePanel --%>
         <Triggers>
             <asp:AsyncPostBackTrigger ControlID="ddlTipoObjetivoFiltro" EventName="SelectedIndexChanged" />
             <asp:AsyncPostBackTrigger ControlID="btnLimpiarFiltros" EventName="Click" />
             <asp:AsyncPostBackTrigger ControlID="gvObjetivos" EventName="PageIndexChanging" />
             <asp:AsyncPostBackTrigger ControlID="gvObjetivos" EventName="RowCommand" />
             <asp:AsyncPostBackTrigger ControlID="btnAbrirModalAgregar" EventName="Click" />
             <asp:AsyncPostBackTrigger ControlID="btnGuardarModal" EventName="Click" /> <%-- Trigger for the modal save button --%>
         </Triggers>
    </asp:UpdatePanel> <%-- End UpdatePanelPage --%>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script>
        // Script to manually show/hide modal from server-side
        function showModal(modalId) {
            var modalElement = document.getElementById(modalId);
            if (modalElement) {
                var modal = bootstrap.Modal.getOrCreateInstance(modalElement);
                modal.show();
            }
        }

        function hideModal(modalId) {
            var modalElement = document.getElementById(modalId);
            if (modalElement) {
                var modal = bootstrap.Modal.getInstance(modalElement);

                // Oculta el modal si se está mostrando
                if (modal && modal._isShown) {
                    modal.hide();
                }

                // ---- INICIO DE LA CORRECCIÓN PARA LA PANTALLA OSCURA ----
                // A veces, después de un postback, el fondo no se elimina.
                // Las siguientes líneas fuerzan la limpieza para asegurar que la página vuelva a la normalidad.

                // 1. Elimina cualquier 'backdrop' (fondo oscuro) que haya quedado.
                const backdrops = document.getElementsByClassName('modal-backdrop');
                while (backdrops.length > 0) {
                    backdrops[0].parentNode.removeChild(backdrops[0]);
                }

                // 2. Quita la clase del <body> que impide el scroll.
                document.body.classList.remove('modal-open');

                // 3. Restablece los estilos del <body> que el modal modifica.
                document.body.style.overflow = '';
                document.body.style.paddingRight = '';
                // ---- FIN DE LA CORRERECCIÓN ----
            }
        }

        // Inicialización
        document.addEventListener('DOMContentLoaded', () => {
            console.log("Gestión Objetivos cargado (Modal CRUD Postback version).");
            // Reinitialize modal JS if needed after async postback
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            if (prm) {
                prm.add_endRequest(function (sender, args) {
                    // Potentially re-initialize Bootstrap components if they are inside UpdatePanel
                    // For modal outside the main panel, this might not be strictly necessary
                    // but good practice if other JS relies on elements within the panel.
                    console.log("Async Postback Ended.");
                });
            }
        });
    </script>
</asp:Content>
