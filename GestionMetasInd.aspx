<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="GestionMetasInd.aspx.cs" Inherits="Gestor_Desempeno.GestionMetasInd" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .gridview-container {
            margin-top: 20px;
        }
        /* Removed form-container style as the form is now in modal */
        .gridview-button {
            padding: 0.2rem 0.5rem;
            font-size: 0.875rem;
            margin-right: 5px;
        }

        .edit-control {
            width: 100%;
        }
        /* Adjusted max-width for truncation */
        .descripcion-larga-50 {
            max-width: 50ch;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
            display: inline-block;
            vertical-align: middle;
        }
        /* Truncate to approx 50 chars */
        .descripcion-larga {
            max-width: 250px;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        .filter-container {
            margin-bottom: 15px;
            padding: 15px;
            border: 1px solid #e0e0e0;
            border-radius: 5px;
            background-color: #f1f1f1;
        }

        /* Ensure modal labels are aligned */
        .modal-body .form-label {
            margin-bottom: 0.25rem;
        }

        .modal-body .form-control, .modal-body .form-select {
            margin-bottom: 0.5rem;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdatePanelPage" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <h1>Gestión de Metas Individuales</h1>
            <hr />
            <asp:Literal ID="litMensaje" runat="server" EnableViewState="false"></asp:Literal>

            <%-- Filtros --%>
            <asp:Panel ID="pnlFiltros" runat="server" DefaultButton="btnFiltrar">
            <div class="filter-container">
                <h5>Filtros</h5>
                <div class="row g-3 align-items-end">
                    <div class="col-md-3"><label class="form-label">Tipo Objetivo:</label><asp:DropDownList ID="ddlTipoObjetivoFiltro" runat="server" CssClass="form-select" DataTextField="Nombre" DataValueField="IdTipoObjetivo" AutoPostBack="true" OnSelectedIndexChanged="ddlTipoObjetivoFiltro_SelectedIndexChanged"></asp:DropDownList></div>
                    <div class="col-md-3"><label class="form-label">Área Ejecutora:</label><asp:DropDownList ID="ddlAreaFiltro" runat="server" CssClass="form-select" DataTextField="Nombre" DataValueField="IdAreaEjecutora" AutoPostBack="true" OnSelectedIndexChanged="ddlAreaFiltro_SelectedIndexChanged"></asp:DropDownList></div>
                    <div class="col-md-2"><label class="form-label">Num. Meta Padre:</label><asp:TextBox ID="txtNumMetaFiltro" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
                    <div class="col-md-3"><label class="form-label" for="<%= ddlUsuarioFiltro.ClientID %>">Usuario Colaborador:</label><asp:DropDownList ID="ddlUsuarioFiltro" runat="server" CssClass="form-select" DataTextField="Nombre" DataValueField="Id_Colaborador" AutoPostBack="true" OnSelectedIndexChanged="ddlUsuarioFiltro_SelectedIndexChanged"></asp:DropDownList></div>
                    <div class="col-md-2 d-grid gap-1"><asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn btn-secondary w-100" OnClick="btnFiltrar_Click" /><asp:Button ID="btnLimpiarFiltros" runat="server" Text="Limpiar" CssClass="btn btn-outline-secondary w-100" OnClick="btnLimpiarFiltros_Click" CausesValidation="false" /></div>
                </div>
            </div>
            </asp:Panel>

            <%-- GridView --%>
            <div class="gridview-container table-responsive">
                <asp:GridView ID="gvMetasInd" runat="server" AutoGenerateColumns="False" DataKeyNames="IdMetaIndividual" CssClass="table table-striped table-bordered table-hover table-sm" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvMetasInd_PageIndexChanging" EmptyDataText="No se encontraron metas individuales." OnRowCommand="gvMetasInd_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="Meta Departamental" SortExpression="NombreAreaEjecutora"><ItemTemplate><span class="descripcion-larga-50" title='<%# Eval("NombreAreaEjecutora") + " - " + Eval("DescripcionMetaDepartamental") %>'><%# TruncateStringWithEllipsis( (Eval("NombreAreaEjecutora") == DBNull.Value ? "Sin Área" : Eval("NombreAreaEjecutora").ToString()) + " - " + (Eval("DescripcionMetaDepartamental") == DBNull.Value ? "Sin Desc." : Eval("DescripcionMetaDepartamental").ToString()), 50) %></span></ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="Usuario" HeaderText="Usuario" SortExpression="Usuario" />
                        <asp:TemplateField HeaderText="Acciones"><ItemTemplate><asp:Button ID="btnEditar" runat="server" CommandName="EditarMetaInd" CommandArgument='<%# Eval("IdMetaIndividual") %>' Text="Editar" CssClass="btn btn-sm btn-outline-primary gridview-button" CausesValidation="false" /><asp:Button ID="btnEliminar" runat="server" CommandName="EliminarMetaInd" CommandArgument='<%# Eval("IdMetaIndividual") %>' Text="Eliminar" CssClass="btn btn-sm btn-outline-danger gridview-button" CausesValidation="false" OnClientClick="return confirm('¿Está seguro de eliminar esta meta individual?');" /></ItemTemplate></asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="pagination-ys" />
                </asp:GridView>
            </div>
            
            <div class="mt-3">
                <asp:Button ID="btnAbrirModalAgregar" runat="server" Text="Agregar Nueva Meta Individual" CssClass="btn btn-success" OnClick="btnAbrirModalAgregar_Click" CausesValidation="false" />
            </div>

             <%-- Modal (AHORA DENTRO DEL UPDATEPANEL PRINCIPAL) --%>
            <div class="modal fade" id="metaIndModal" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="metaIndModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-lg modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="metaIndModalLabel"><asp:Label ID="lblModalTitle" runat="server" Text="Meta Individual"></asp:Label></h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                        </div>
                        <div class="modal-body">
                            <asp:HiddenField ID="hfMetaIndId" runat="server" Value="0" />
                            <asp:Literal ID="litModalMensaje" runat="server" EnableViewState="false"></asp:Literal>
                            <div class="row g-3">
                                <div class="col-md-8"><label class="form-label">Meta Departamental Padre:</label><asp:DropDownList ID="ddlModalMetaDep" runat="server" CssClass="form-select" DataValueField="IdMetaDepartamental" DataTextField="DisplayTextForDropdown"></asp:DropDownList><asp:RequiredFieldValidator InitialValue="0" ControlToValidate="ddlModalMetaDep" ID="rfvModalMetaDep" runat="server" ErrorMessage="*" CssClass="text-danger small" ValidationGroup="ModalValidation"></asp:RequiredFieldValidator></div>
                                <div class="col-md-4"><label class="form-label">Usuario Asignado:</label><asp:DropDownList ID="ddlModalUsuario" runat="server" CssClass="form-select"></asp:DropDownList><asp:RequiredFieldValidator ControlToValidate="ddlModalUsuario" ID="rfvModalUsuario" runat="server" InitialValue="0" ErrorMessage="*" CssClass="text-danger small" ValidationGroup="ModalValidation"></asp:RequiredFieldValidator></div>
                                <div class="col-12"><label class="form-label">Descripción Meta Individual:</label><asp:TextBox ID="txtModalDescMetaInd" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox></div>
                                <div class="col-md-3"><label class="form-label">Peso N4:</label><asp:TextBox ID="txtModalPesoN4" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
                                <div class="col-md-3"><label class="form-label">Peso N5:</label><asp:TextBox ID="txtModalPesoN5" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
                                <div class="col-md-6"><label class="form-label">Estado:</label><asp:DropDownList ID="ddlModalEstado" runat="server" CssClass="form-select" DataValueField="Id_Detalle_Estado" DataTextField="Descripcion"></asp:DropDownList><asp:RequiredFieldValidator InitialValue="0" ControlToValidate="ddlModalEstado" ID="rfvModalEstado" runat="server" ErrorMessage="*" CssClass="text-danger small" ValidationGroup="ModalValidation"></asp:RequiredFieldValidator></div>
                                <div class="col-md-4"><label class="form-label">Fecha Inicial:</label><asp:TextBox ID="txtModalFechaIni" runat="server" CssClass="form-control date-textbox" TextMode="Date"></asp:TextBox></div>
                                <div class="col-md-4"><label class="form-label">Fecha Final (Opcional):</label><asp:TextBox ID="txtModalFechaFin" runat="server" CssClass="form-control date-textbox" TextMode="Date"></asp:TextBox></div>
                                <div class="col-md-4"><div class="form-group mb-3"><label class="form-label d-block">&nbsp;</label><div class="form-check"><asp:CheckBox ID="chkModalFinalizable" runat="server" CssClass="form-check-input" Checked="true" Visible="false" /></div></div></div>
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
            <%-- Triggers del UpdatePanel Principal --%>
            <asp:AsyncPostBackTrigger ControlID="ddlTipoObjetivoFiltro" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="ddlAreaFiltro" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="ddlUsuarioFiltro" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnFiltrar" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnLimpiarFiltros" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="gvMetasInd" EventName="PageIndexChanging" />
            <asp:AsyncPostBackTrigger ControlID="gvMetasInd" EventName="RowCommand" />
            <asp:AsyncPostBackTrigger ControlID="btnAbrirModalAgregar" EventName="Click" />
            <%-- TRIGGER DEL BOTÓN DEL MODAL AÑADIDO AQUÍ --%>
            <asp:AsyncPostBackTrigger ControlID="btnGuardarModal" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script>
        $(document).ready(function () {
            initializeSelect2();
        });

        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function () {
            initializeSelect2();
        });

        function initializeSelect2() {
            $('#<%= ddlModalMetaDep.ClientID %>').select2({
                theme: "bootstrap-5",
                dropdownParent: $('#metaIndModal')
            });
        }

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
