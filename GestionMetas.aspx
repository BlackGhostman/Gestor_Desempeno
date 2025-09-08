<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="GestionMetas.aspx.cs" Inherits="Gestor_Desempeno.GestionMetas" ResponseEncoding="utf-8" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .gridview-container { margin-top: 20px; }
        /* Removed form-container style */
        .gridview-button { padding: 0.2rem 0.5rem; font-size: 0.875rem; margin-right: 5px; }
        .edit-control { width: 100%; }
        .descripcion-larga { max-width: 350px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .filter-container { margin-bottom: 15px; padding: 15px; border: 1px solid #e0e0e0; border-radius: 5px; background-color: #f1f1f1;}
         /* Ensure modal labels are aligned */
        .modal-body .form-label { margin-bottom: 0.25rem; }
        .modal-body .form-control, .modal-body .form-select { margin-bottom: 0.5rem; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
     <%--<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>--%>

    <asp:UpdatePanel ID="UpdatePanelPage" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <h1>Gestión de Metas</h1>
            <hr />

            <asp:Literal ID="litMensaje" runat="server" EnableViewState="false"></asp:Literal>

            <%-- Filtros --%>
            <div class="filter-container">
                <h5>Filtros</h5>
                <div class="row g-3 align-items-end">
                    <div class="col-md-4">
                        <label for="ddlTipoObjetivoFiltro" class="form-label">Tipo Objetivo:</label>
                        <asp:DropDownList ID="ddlTipoObjetivoFiltro" runat="server" CssClass="form-select"
                            DataTextField="Nombre" DataValueField="IdTipoObjetivo" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlTipoObjetivoFiltro_SelectedIndexChanged">
                        </asp:DropDownList>

                    </div>
                     <div class="col-md-4">
                        <label for="ddlObjetivoFiltro" class="form-label">Objetivo:</label>
                        <%--<asp:DropDownList ID="ddlObjetivoFiltro" runat="server" CssClass="form-select" DataTextField="Nombre" DataValueField="Id_Objetivo" AutoPostBack="true" OnSelectedIndexChanged="ddlObjetivoFiltro_SelectedIndexChanged">
                        </asp:DropDownList>--%>
                         <asp:DropDownList ID="ddlObjetivoFiltro" runat="server" CssClass="form-select" DataTextField="Nombre" DataValueField="Id_Objetivo" AutoPostBack="true" OnSelectedIndexChanged="ddlObjetivoFiltro_SelectedIndexChanged"></asp:DropDownList>
                    </div>
                     <div class="col-md-2">
                        <label for="txtNumMetaFiltro" class="form-label">Num. Meta:</label>
                        <asp:TextBox ID="txtNumMetaFiltro" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                        <asp:CompareValidator ID="cvNumMetaFiltro" runat="server" ControlToValidate="txtNumMetaFiltro" Operator="DataTypeCheck" Type="Integer"
                            ErrorMessage="Inv." CssClass="text-danger small" Display="Dynamic">*</asp:CompareValidator>
                    </div>
                    <div class="col-md-2">
                        <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn btn-secondary w-100" OnClick="btnFiltrar_Click" />
                         <asp:Button ID="btnLimpiarFiltros" runat="server" Text="Limpiar" CssClass="btn btn-outline-secondary w-100 mt-1" OnClick="btnLimpiarFiltros_Click" CausesValidation="false"/>
                    </div>
                </div>
            </div>


            <%-- GridView para mostrar las metas --%>
            <div class="gridview-container table-responsive">
                <asp:GridView ID="gvMetas" runat="server" AutoGenerateColumns="False"
                    DataKeyNames="IdMeta" CssClass="table table-striped table-bordered table-hover table-sm"
                    AllowPaging="True" PageSize="10" OnPageIndexChanging="gvMetas_PageIndexChanging"
                    EmptyDataText="No se encontraron metas para los filtros seleccionados."
                    OnRowCommand="gvMetas_RowCommand"> <%-- Keep RowCommand --%>
                    <Columns>
                        <%-- Columna Objetivo --%>
                        <asp:BoundField DataField="NombreObjetivo" HeaderText="Nombre Objetivo" SortExpression="NombreObjetivo" ReadOnly="True" />

                                                <asp:TemplateField HeaderText="Descripción Objetivo">
                            <ItemTemplate>
                                <div class="descripcion-larga" title='<%# Eval("DescripcionObjetivo") %>'>
                                    <asp:Label ID="lblDescripcionObjetivoGrid" runat="server" Text='<%# Eval("DescripcionObjetivo") %>'></asp:Label>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                

                         <%-- Columna Num Meta --%>
                        <asp:BoundField DataField="NumMeta" HeaderText="Num." SortExpression="NumMeta" ReadOnly="True" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center"/>

                        <%-- Columna Descripción Meta --%>
                        <asp:TemplateField HeaderText="Meta">
                            <ItemTemplate>
                                 <div class="descripcion-larga" title='<%# Eval("Descripcion") %>'>
                                     <asp:Label ID="lblDescripcionGrid" runat="server" Text='<%# Eval("Descripcion") %>'></asp:Label>
                                 </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                         <%-- Columna Estado --%>
                         <asp:BoundField DataField="DescripcionEstado" HeaderText="Estado" SortExpression="DescripcionEstado" ReadOnly="True" />

                        <%-- Columna Ficha Técnica --%>
                        <asp:TemplateField HeaderText="Ficha">
                            <ItemTemplate>
                                <asp:Button ID="btnVerFicha" runat="server" CommandName="VerFicha" CommandArgument='<%# Eval("IdMeta") %>' Text="Ver" CssClass="btn btn-sm btn-info gridview-button" CausesValidation="false" />
                            </ItemTemplate>
                        </asp:TemplateField>

                        <%-- ** UPDATED: Actions Column ** --%>
                        <asp:TemplateField HeaderText="Acciones">
                            <ItemTemplate>
                                <asp:Button ID="btnEditar" runat="server" CommandName="EditarMeta" CommandArgument='<%# Eval("IdMeta") %>' Text="Editar" CssClass="btn btn-sm btn-outline-primary gridview-button" CausesValidation="false"/>
                                <asp:Button ID="btnEliminar" runat="server" CommandName="EliminarMeta" CommandArgument='<%# Eval("IdMeta") %>' Text="Eliminar" CssClass="btn btn-sm btn-outline-danger gridview-button" CausesValidation="false" OnClientClick="return confirm('¿Está seguro de que desea eliminar (marcar como inactiva) esta meta?');" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                     <PagerStyle CssClass="pagination-ys" />
                </asp:GridView>
            </div>

             <%-- ** ADDED: Button to open Add modal ** --%>
             <div class="mt-3">
                 <asp:Button ID="btnAbrirModalAgregar" runat="server" Text="Agregar Nueva Meta" CssClass="btn btn-success" OnClick="btnAbrirModalAgregar_Click" CausesValidation="false" />
             </div>

             <%-- ** ADDED: Modal for Add/Edit ** --%>
             <div class="modal fade" id="metaModal" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="metaModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-lg modal-dialog-centered">
                    <div class="modal-content">
                        
                                <div class="modal-header">
                                    <h5 class="modal-title" id="metaModalLabel">
                                        <asp:Label ID="lblModalTitle" runat="server" Text="Meta"></asp:Label>
                                    </h5>
                                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                                </div>
                                <div class="modal-body">
                                     <asp:HiddenField ID="hfMetaId" runat="server" Value="0" />
                                     <asp:Literal ID="litModalMensaje" runat="server" EnableViewState="false"></asp:Literal>

                                     <div class="row g-3">
                                         <div class="col-md-8">
                                             <label for="ddlModalObjetivo" class="form-label">Objetivo Asociado:</label>
                                             <asp:DropDownList ID="ddlModalObjetivo" runat="server" CssClass="form-select" DataValueField="Id_Objetivo" DataTextField="Nombre"></asp:DropDownList>
                                             <asp:RequiredFieldValidator ID="rfvModalObjetivo" runat="server" ControlToValidate="ddlModalObjetivo" InitialValue="0" ErrorMessage="Seleccione Objetivo." Display="Dynamic" CssClass="text-danger small" ValidationGroup="ModalValidation">*</asp:RequiredFieldValidator>
                                         </div>
                                         <div class="col-md-4">
                                             <label for="txtModalNumMeta" class="form-label">Num. Meta:</label>
                                             <asp:TextBox ID="txtModalNumMeta" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                                             <asp:RangeValidator ID="rvModalNumMeta" runat="server" ControlToValidate="txtModalNumMeta" MinimumValue="1" MaximumValue="9999" Type="Integer" ErrorMessage="Inv." Display="Dynamic" CssClass="text-danger small" ValidationGroup="ModalValidation">*</asp:RangeValidator>
                                         </div>
                                         <div class="col-12">
                                             <label for="txtModalDescripcion" class="form-label">Descripción Meta:</label>
                                             <asp:TextBox ID="txtModalDescripcion" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                                              <%-- No es requerido usualmente --%>
                                         </div>
                                         <div class="col-12">
                                             <label for="txtModalFichaTecnica" class="form-label">Ficha Técnica:</label>
                                             <asp:TextBox ID="txtModalFichaTecnica" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5"></asp:TextBox>
                                         </div>
                                          <div class="col-md-6">
                                             <label for="ddlModalEstado" class="form-label">Estado:</label>
                                             <asp:DropDownList ID="ddlModalEstado" runat="server" CssClass="form-select" DataValueField="Id_Detalle_Estado" DataTextField="Descripcion"></asp:DropDownList>
                                             <asp:RequiredFieldValidator ID="rfvModalEstado" runat="server" ControlToValidate="ddlModalEstado" InitialValue="0" ErrorMessage="Seleccione Estado." Display="Dynamic" CssClass="text-danger small" ValidationGroup="ModalValidation">*</asp:RequiredFieldValidator>
                                         </div>
                                     </div>
                                </div>
                                <div class="modal-footer">
                                    <asp:Button ID="btnGuardarModal" runat="server" Text="Guardar" CssClass="btn btn-primary" OnClick="btnGuardarModal_Click" ValidationGroup="ModalValidation" />
                                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                                </div>
                            
                    </div>
                </div>
            </div> <%-- End Modal --%>

            <%-- Modal para ver Ficha Técnica --%>
            <div class="modal fade" id="fichaModal" tabindex="-1" aria-labelledby="fichaModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="fichaModalLabel">Ficha Técnica</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                        </div>
                        <div class="modal-body">
                            <asp:Literal ID="litFichaTecnicaContenido" runat="server"></asp:Literal>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
                        </div>
                    </div>
                </div>
            </div>

        </ContentTemplate>
         <%-- Triggers for the main UpdatePanel --%>
         <Triggers>
             <asp:AsyncPostBackTrigger ControlID="ddlTipoObjetivoFiltro" EventName="SelectedIndexChanged" />
             <asp:AsyncPostBackTrigger ControlID="ddlObjetivoFiltro" EventName="SelectedIndexChanged" /> <%-- Added trigger --%>
             <asp:AsyncPostBackTrigger ControlID="btnFiltrar" EventName="Click" />
             <asp:AsyncPostBackTrigger ControlID="btnLimpiarFiltros" EventName="Click" />
             <asp:AsyncPostBackTrigger ControlID="gvMetas" EventName="PageIndexChanging" />
             <asp:AsyncPostBackTrigger ControlID="gvMetas" EventName="RowCommand" />
             <asp:AsyncPostBackTrigger ControlID="btnAbrirModalAgregar" EventName="Click" />
             <asp:AsyncPostBackTrigger ControlID="btnGuardarModal" EventName="Click" />
             <%-- btnGuardarModal is inside its own UpdatePanel --%>
         </Triggers>
    </asp:UpdatePanel> <%-- End UpdatePanelPage --%>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script>
         // Script to manually show/hide modal from server-side if needed
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
                // ---- FIN DE LA CORRECCIÓN ----
            }
        }

         // Inicialización
         document.addEventListener('DOMContentLoaded', () => {
             console.log("Gestión Metas cargado (Modal CRUD version).");
             var prm = Sys.WebForms.PageRequestManager.getInstance();
             if (prm) {
                prm.add_endRequest(function (sender, args) {
                    console.log("Async Postback Ended (Metas).");
                });
             }
         });
    </script>
</asp:Content>
