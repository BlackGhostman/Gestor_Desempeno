<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="GestionMetasDep.aspx.cs" Inherits="Gestor_Desempeno.GestionMetasDep" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
     <style>
        .gridview-container { margin-top: 20px; }
        .gridview-button { padding: 0.2rem 0.5rem; font-size: 0.875rem; margin-right: 5px; }
        .descripcion-larga { max-width: 350px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .filter-container { margin-bottom: 15px; padding: 15px; border: 1px solid #e0e0e0; border-radius: 5px; background-color: #f1f1f1;}
        .modal-body .form-label { margin-bottom: 0.25rem; }
        .modal-body .form-control, .modal-body .form-select { margin-bottom: 0.5rem; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
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
                        <asp:DropDownList ID="ddlTipoObjetivoFiltro" runat="server" CssClass="form-select" DataTextField="Nombre" DataValueField="Id_Tipo_Objetivo" AutoPostBack="true" OnSelectedIndexChanged="ddlTipoObjetivoFiltro_SelectedIndexChanged"></asp:DropDownList>
                    </div>
                    <div class="col-md-3">
                        <label for="txtNumMetaFiltro" class="form-label">Num. Meta:</label>
                        <asp:TextBox ID="txtNumMetaFiltro" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                        <asp:CompareValidator ID="cvNumMetaFiltro" runat="server" ControlToValidate="txtNumMetaFiltro" Operator="DataTypeCheck" Type="Integer" ErrorMessage="Inv." CssClass="text-danger small" Display="Dynamic">*</asp:CompareValidator>
                    </div>
                    <div class="col-md-2">
                        <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn btn-secondary w-100" OnClick="btnFiltrar_Click" />
                    </div>
                    <div class="col-md-2">
                         <asp:Button ID="btnLimpiarFiltros" runat="server" Text="Limpiar" CssClass="btn btn-outline-secondary w-100" OnClick="btnLimpiarFiltros_Click" CausesValidation="false"/>
                    </div>
                </div>
            </div>

            <%-- GridView para mostrar las metas --%>
            <div class="gridview-container table-responsive">
                <asp:GridView ID="gvMetasDep" runat="server" AutoGenerateColumns="False" DataKeyNames="IdMetaDepartamental" CssClass="table table-striped table-bordered table-hover table-sm" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvMetasDep_PageIndexChanging" EmptyDataText="No se encontraron metas para los filtros seleccionados." OnRowCommand="gvMetasDep_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="DescripcionMetaPadre" HeaderText="Meta Padre" SortExpression="NombreMetaPadre" ReadOnly="True" />
                        <asp:BoundField DataField="NombreAreaEjecutora" HeaderText="Área Ejecutora" SortExpression="NombreAreaEjecutora" ReadOnly="True" />
                        <asp:TemplateField HeaderText="Descripción">
                            <ItemTemplate>
                                <div class="descripcion-larga" title='<%# Eval("Descripcion") %>'>
                                    <asp:Label ID="lblDescripcionGrid" runat="server" Text='<%# Eval("Descripcion") %>'></asp:Label>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="PesoPonderado" HeaderText="Peso" SortExpression="PesoPonderado" ReadOnly="True" />
                        <asp:BoundField DataField="DescripcionEstado" HeaderText="Estado" SortExpression="DescripcionEstado" ReadOnly="True" />
                        <asp:TemplateField HeaderText="Acciones">
                            <ItemTemplate>
                                <asp:Button ID="btnEditar" runat="server" CommandName="EditarMetaDep" CommandArgument='<%# Eval("IdMetaDepartamental") %>' Text="Editar" CssClass="btn btn-sm btn-outline-primary gridview-button" CausesValidation="false"/>
                                <asp:Button ID="btnEliminar" runat="server" CommandName="EliminarMetaDep" CommandArgument='<%# Eval("IdMetaDepartamental") %>' Text="Eliminar" CssClass="btn btn-sm btn-outline-danger gridview-button" CausesValidation="false" OnClientClick="return confirm('¿Está seguro de que desea eliminar esta meta?');" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="pagination-ys" />
                </asp:GridView>
            </div>
            
            <div class="mt-3">
                <asp:Button ID="btnAbrirModalAgregar" runat="server" Text="Agregar Nueva Meta Departamental" CssClass="btn btn-success" OnClick="btnAbrirModalAgregar_Click" CausesValidation="false" />
            </div>

            <%-- Modal for Add/Edit --%>
            <div class="modal fade" id="metaDepModal" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="metaDepModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-lg modal-dialog-centered">
                    <div class="modal-content">
                        <%-- EL UPDATEPANEL ANIDADO FUE ELIMINADO PARA CORREGIR EL REFRESH --%>
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
                                <div class="col-md-6"><label for="ddlModalMeta" class="form-label">Meta Padre:</label><asp:DropDownList ID="ddlModalMeta" runat="server" CssClass="form-select" DataValueField="IdMeta" DataTextField="DisplayText"></asp:DropDownList><asp:RequiredFieldValidator InitialValue="0" ID="rfvModalMeta" runat="server" ControlToValidate="ddlModalMeta" ErrorMessage="Requerido." CssClass="text-danger small" ValidationGroup="ModalValidation">*</asp:RequiredFieldValidator></div>
                                <div class="col-md-6"><label for="ddlModalArea" class="form-label">Área Ejecutora:</label><asp:DropDownList ID="ddlModalArea" runat="server" CssClass="form-select" DataValueField="Id_Area_Ejecutora" DataTextField="Nombre"></asp:DropDownList><asp:RequiredFieldValidator InitialValue="0" ID="rfvModalArea" runat="server" ControlToValidate="ddlModalArea" ErrorMessage="Requerido." CssClass="text-danger small" ValidationGroup="ModalValidation">*</asp:RequiredFieldValidator></div>
                                <div class="col-12"><label for="txtModalDescMetaDep" class="form-label">Descripción:</label><asp:TextBox ID="txtModalDescMetaDep" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox></div>
                                <div class="col-md-6"><label for="txtModalIndicador" class="form-label">Indicador:</label><asp:TextBox ID="txtModalIndicador" runat="server" CssClass="form-control"></asp:TextBox></div>
                                <div class="col-md-6"><label for="txtModalAlcance" class="form-label">Alcance:</label><asp:TextBox ID="txtModalAlcance" runat="server" CssClass="form-control"></asp:TextBox></div>
                                <div class="col-md-3"><label for="txtModalPeso" class="form-label">Peso (%):</label><asp:TextBox ID="txtModalPeso" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
                                <div class="col-md-3"><label for="txtModalPrioridad" class="form-label">Prioridad:</label><asp:TextBox ID="txtModalPrioridad" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
                                <div class="col-md-6"><label for="ddlModalEstado" class="form-label">Estado:</label><asp:DropDownList ID="ddlModalEstado" runat="server" CssClass="form-select" DataValueField="Id_Detalle_Estado" DataTextField="Descripcion"></asp:DropDownList><asp:RequiredFieldValidator InitialValue="0" ID="rfvModalEstado" runat="server" ControlToValidate="ddlModalEstado" ErrorMessage="Requerido." CssClass="text-danger small" ValidationGroup="ModalValidation">*</asp:RequiredFieldValidator></div>
                                <div class="col-md-6"><label for="txtModalFechaIni" class="form-label">Fecha Inicio:</label><asp:TextBox ID="txtModalFechaIni" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox></div>
                                <div class="col-md-6"><label for="txtModalFechaFin" class="form-label">Fecha Fin:</label><asp:TextBox ID="txtModalFechaFin" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox></div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnGuardarModal" runat="server" Text="Guardar" CssClass="btn btn-primary" OnClick="btnGuardarModal_Click" ValidationGroup="ModalValidation" />
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <%-- TRIGGERS DEL UPDATEPANEL PRINCIPAL --%>
            <asp:AsyncPostBackTrigger ControlID="ddlTipoObjetivoFiltro" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnFiltrar" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnLimpiarFiltros" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="gvMetasDep" EventName="PageIndexChanging" />
            <asp:AsyncPostBackTrigger ControlID="gvMetasDep" EventName="RowCommand" />
            <asp:AsyncPostBackTrigger ControlID="btnAbrirModalAgregar" EventName="Click" />
            <%-- TRIGGER DEL BOTÓN DEL MODAL AÑADIDO AQUÍ --%>
            <asp:AsyncPostBackTrigger ControlID="btnGuardarModal" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script>
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
                if (modal && modal._isShown) {
                    modal.hide();
                }
                const backdrops = document.getElementsByClassName('modal-backdrop');
                while (backdrops.length > 0) {
                    backdrops[0].parentNode.removeChild(backdrops[0]);
                }
                document.body.classList.remove('modal-open');
                document.body.style.overflow = '';
                document.body.style.paddingRight = '';
            }
        }
    </script>
    </asp:Content>
