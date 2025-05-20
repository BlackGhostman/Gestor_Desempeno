<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="GestionMetasDep.aspx.cs" Inherits="Gestor_Desempeno.GestionMetasDep" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
     <style>
        .gridview-container { margin-top: 20px; }
        /* Removed form-container style */
        .gridview-button { padding: 0.2rem 0.5rem; font-size: 0.875rem; margin-right: 5px; }
        .edit-control { width: 100%; }
        /* Adjusted max-width for truncation */
        .descripcion-larga-50 { max-width: 50ch; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; display: inline-block; vertical-align: middle;}
        .descripcion-larga { max-width: 250px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .filter-container { margin-bottom: 15px; padding: 15px; border: 1px solid #e0e0e0; border-radius: 5px; background-color: #f1f1f1;}
        .date-textbox { /* Date input styles */ }
         /* Ensure modal labels are aligned */
        .modal-body .form-label { margin-bottom: 0.25rem; }
        .modal-body .form-control, .modal-body .form-select { margin-bottom: 0.5rem; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%--<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>--%>

    <asp:UpdatePanel ID="UpdatePanelPage" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
             <h1>Gestión de Metas Departamentales</h1>
            <hr />

            <asp:Literal ID="litMensaje" runat="server" EnableViewState="false"></asp:Literal>

             <%-- Filtros --%>
            <div class="filter-container">
                <h5>Filtros</h5>
                <div class="row g-3 align-items-end">
                    <div class="col-md-5">
                        <label for="ddlTipoObjetivoFiltro" class="form-label">Tipo Objetivo:</label>
                        <asp:DropDownList ID="ddlTipoObjetivoFiltro" runat="server" CssClass="form-select"
                            DataTextField="Nombre" DataValueField="IdTipoObjetivo" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlTipoObjetivoFiltro_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>
                     <div class="col-md-3">
                        <label for="txtNumMetaFiltro" class="form-label">Num. Meta Padre:</label>
                        <asp:TextBox ID="txtNumMetaFiltro" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                        <asp:CompareValidator ID="cvNumMetaFiltro" runat="server" ControlToValidate="txtNumMetaFiltro" Operator="DataTypeCheck" Type="Integer"
                            ErrorMessage="Inv." CssClass="text-danger small" Display="Dynamic">*</asp:CompareValidator>
                    </div>
                    <div class="col-md-2">
                        <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn btn-secondary w-100" OnClick="btnFiltrar_Click" />
                    </div>
                     <div class="col-md-2">
                         <asp:Button ID="btnLimpiarFiltros" runat="server" Text="Limpiar" CssClass="btn btn-outline-secondary w-100" OnClick="btnLimpiarFiltros_Click" CausesValidation="false"/>
                    </div>
                </div>
            </div>

            <%-- GridView --%>
            <div class="gridview-container table-responsive">
                <asp:GridView ID="gvMetasDep" runat="server" AutoGenerateColumns="False"
                    DataKeyNames="IdMetaDepartamental" CssClass="table table-striped table-bordered table-hover table-sm"
                    AllowPaging="True" PageSize="10" OnPageIndexChanging="gvMetasDep_PageIndexChanging"
                    EmptyDataText="No se encontraron metas departamentales para los filtros seleccionados."
                    OnRowCommand="gvMetasDep_RowCommand">
                    <Columns>
                        <%-- Columna Meta Padre (Truncada a 50 aprox) --%>
                        <asp:TemplateField HeaderText="Meta Padre" SortExpression="NumMetaPadre">
                            <ItemTemplate>
                                <%-- Combine NumMeta and Description, truncate Description part --%>
                                <span class="descripcion-larga-50" title='<%# Eval("DescripcionMetaPadre") %>'>
                                    <%# Eval("NumMetaPadre") %>. <%# TruncateString(Eval("DescripcionMetaPadre"), 50) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <%-- Columna Area Ejecutora --%>
                        <asp:BoundField DataField="NombreAreaEjecutora" HeaderText="Área Ejec." SortExpression="NombreAreaEjecutora" ReadOnly="True" />

                        <%-- Columna Descripción Meta Dep (Truncada) --%>
                        <asp:TemplateField HeaderText="Desc. Meta Dep.">
                             <ItemTemplate>
                                 <div class="descripcion-larga" title='<%# Eval("Descripcion") %>'>
                                     <asp:Label ID="lblDescMetaDepGrid" runat="server" Text='<%# Eval("Descripcion") %>'></asp:Label>
                                 </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <%-- ** UPDATED: Actions Column ** --%>
                        <asp:TemplateField HeaderText="Acciones">
                            <ItemTemplate>
                                <asp:Button ID="btnEditar" runat="server" CommandName="EditarMetaDep" CommandArgument='<%# Eval("IdMetaDepartamental") %>' Text="Editar" CssClass="btn btn-sm btn-outline-primary gridview-button" CausesValidation="false"/>
                                <asp:Button ID="btnEliminar" runat="server" CommandName="EliminarMetaDep" CommandArgument='<%# Eval("IdMetaDepartamental") %>' Text="Eliminar" CssClass="btn btn-sm btn-outline-danger gridview-button" CausesValidation="false" OnClientClick="return confirm('¿Está seguro de eliminar (marcar inactiva) esta meta departamental?');" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                     <PagerStyle CssClass="pagination-ys" />
                </asp:GridView>
            </div>

             <%-- Botón para abrir modal de agregar --%>
             <div class="mt-3">
                 <asp:Button ID="btnAbrirModalAgregar" runat="server" Text="Agregar Nueva Meta Departamental" CssClass="btn btn-success" OnClick="btnAbrirModalAgregar_Click" CausesValidation="false" />
             </div>

             <%-- ** ADDED: Modal for Add/Edit ** --%>
             <div class="modal fade" id="metaDepModal" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="metaDepModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-lg modal-dialog-centered">
                    <div class="modal-content">
                        <asp:UpdatePanel ID="UpdatePanelModalContent" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="modal-header">
                                    <h5 class="modal-title" id="metaDepModalLabel">
                                        <asp:Label ID="lblModalTitle" runat="server" Text="Meta Departamental"></asp:Label>
                                    </h5>
                                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                                </div>
                                <div class="modal-body">
                                     <asp:HiddenField ID="hfMetaDepId" runat="server" Value="0" />
                                     <asp:Literal ID="litModalMensaje" runat="server" EnableViewState="false"></asp:Literal>

                                     <div class="row g-3">
                                         <%-- Meta Padre --%>
                                         <div class="col-md-6">
                                             <label class="form-label">Meta Padre:</label>
                                             <asp:DropDownList ID="ddlModalMeta" runat="server" CssClass="form-select" DataValueField="IdMeta" DataTextField="DisplayText"></asp:DropDownList>
                                             <asp:RequiredFieldValidator InitialValue="0" ControlToValidate="ddlModalMeta" ID="rfvModalMeta" runat="server" ErrorMessage="*" CssClass="text-danger small" ValidationGroup="ModalValidation"></asp:RequiredFieldValidator>
                                         </div>
                                         <%-- Area Ejecutora --%>
                                         <div class="col-md-6">
                                             <label class="form-label">Área Ejecutora:</label>
                                             <asp:DropDownList ID="ddlModalArea" runat="server" CssClass="form-select" DataValueField="Id_Area_Ejecutora" DataTextField="Nombre"></asp:DropDownList>
                                             <asp:RequiredFieldValidator InitialValue="0" ControlToValidate="ddlModalArea" ID="rfvModalArea" runat="server" ErrorMessage="*" CssClass="text-danger small" ValidationGroup="ModalValidation"></asp:RequiredFieldValidator>
                                         </div>
                                         <%-- Descripcion Meta Dep --%>
                                         <div class="col-12">
                                             <label class="form-label">Descripción Meta Departamental:</label>
                                             <asp:TextBox ID="txtModalDescMetaDep" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox>
                                         </div>
                                         <%-- Peso Ponderado --%>
                                         <div class="col-md-3">
                                             <label class="form-label">Peso Ponderado:</label>
                                             <asp:TextBox ID="txtModalPeso" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                                             <asp:RangeValidator Type="Integer" MinimumValue="0" MaximumValue="100" ControlToValidate="txtModalPeso" ID="rvModalPeso" runat="server" ErrorMessage="0-100" CssClass="text-danger small" ValidationGroup="ModalValidation"></asp:RangeValidator>
                                         </div>
                                         <%-- Prioridad --%>
                                         <div class="col-md-3">
                                             <label class="form-label">Prioridad:</label>
                                             <asp:TextBox ID="txtModalPrioridad" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                                             <asp:RangeValidator Type="Integer" MinimumValue="1" MaximumValue="10" ControlToValidate="txtModalPrioridad" ID="rvModalPrioridad" runat="server" ErrorMessage="1-10" CssClass="text-danger small" ValidationGroup="ModalValidation"></asp:RangeValidator>
                                         </div>
                                         <%-- Estado --%>
                                         <div class="col-md-6">
                                             <label class="form-label">Estado:</label>
                                             <asp:DropDownList ID="ddlModalEstado" runat="server" CssClass="form-select" DataValueField="Id_Detalle_Estado" DataTextField="Descripcion"></asp:DropDownList>
                                             <asp:RequiredFieldValidator InitialValue="0" ControlToValidate="ddlModalEstado" ID="rfvModalEstado" runat="server" ErrorMessage="*" CssClass="text-danger small" ValidationGroup="ModalValidation"></asp:RequiredFieldValidator>
                                         </div>
                                          <%-- Fecha Inicial --%>
                                         <div class="col-md-6">
                                             <label class="form-label">Fecha Inicial (defecto hoy):</label>
                                             <asp:TextBox ID="txtModalFechaIni" runat="server" CssClass="form-control date-textbox" TextMode="Date"></asp:TextBox>
                                         </div>
                                         <%-- Fecha Final --%>
                                         <div class="col-md-6">
                                             <label class="form-label">Fecha Final (Opcional):</label>
                                             <asp:TextBox ID="txtModalFechaFin" runat="server" CssClass="form-control date-textbox" TextMode="Date"></asp:TextBox>
                                         </div>
                                         <%-- Indicador --%>
                                         <div class="col-12">
                                             <label class="form-label">Indicador:</label>
                                             <asp:TextBox ID="txtModalIndicador" runat="server" CssClass="form-control" MaxLength="1000"></asp:TextBox>
                                         </div>
                                         <%-- Alcance --%>
                                         <div class="col-12">
                                             <label class="form-label">Alcance:</label>
                                             <asp:TextBox ID="txtModalAlcance" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox>
                                         </div>
                                     </div>
                                </div>
                                <div class="modal-footer">
                                    <asp:Button ID="btnGuardarModal" runat="server" Text="Guardar" CssClass="btn btn-primary" OnClick="btnGuardarModal_Click" ValidationGroup="ModalValidation" />
                                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                                </div>
                            </ContentTemplate>
                             <Triggers>
                                 <asp:AsyncPostBackTrigger ControlID="btnGuardarModal" EventName="Click" />
                             </Triggers>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div> <%-- End Modal --%>

        </ContentTemplate>
         <%-- Triggers for the main UpdatePanel --%>
         <Triggers>
             <asp:AsyncPostBackTrigger ControlID="ddlTipoObjetivoFiltro" EventName="SelectedIndexChanged" />
             <asp:AsyncPostBackTrigger ControlID="btnFiltrar" EventName="Click" />
             <asp:AsyncPostBackTrigger ControlID="btnLimpiarFiltros" EventName="Click" />
             <asp:AsyncPostBackTrigger ControlID="gvMetasDep" EventName="PageIndexChanging" />
             <asp:AsyncPostBackTrigger ControlID="gvMetasDep" EventName="RowCommand" />
             <asp:AsyncPostBackTrigger ControlID="btnAbrirModalAgregar" EventName="Click" />
             <%-- btnGuardarModal is inside its own UpdatePanel --%>
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
                var modal = bootstrap.Modal.getInstance(modalElement); // Get existing instance
                if (modal && modal._isShown) { // Check if modal is shown before trying to hide
                    modal.hide();
                }
            }
        }

        // Inicialización
        document.addEventListener('DOMContentLoaded', () => {
            console.log("Gestión Metas Dep. cargado (Modal CRUD version).");
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            if (prm) {
                prm.add_endRequest(function (sender, args) {
                    console.log("Async Postback Ended (Metas Dep).");
                });
            }
        });
</script>
    </asp:Content>
