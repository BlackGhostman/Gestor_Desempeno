<%@ Page Title="Desempeño" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Desempeno.aspx.cs" Inherits="Gestor_Desempeno.Desempeno" %>
<%@ Import Namespace="System.Globalization" %> <%-- Needed for CalendarWeekRule --%>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Font face para Iconos Lucide */
         @font-face { font-family: 'LucideIcons'; src: url(https://cdn.jsdelivr.net/npm/lucide-static@latest/font/Lucide.ttf) format('truetype'); }
         body { font-family: 'Inter', sans-serif; background-color: #f8f9fa; display: flex; flex-direction: column; min-height: 100vh; }
         main { flex: 1; }
         .header-border { border-top: 5px solid #FD8204; }
         .banner { background-color: #FD8204; height: 100px; border-bottom: 2px solid #fff; display: flex; justify-content: space-between; align-items: center; padding: 0 1rem; }
         .banner img { max-height: 80px; max-width: 200px; height: auto; width: auto; }
         .navbar-custom { background-color: #FD8204; }
         .navbar-dark .navbar-nav .nav-link { font-weight: bold; color: white !important; transition: background-color 0.2s ease, border-color 0.2s ease; padding-bottom: 0.3rem; border-bottom: 2px solid transparent; margin: 0 0.5rem; border-radius: 0.25rem 0.25rem 0 0; }
         .navbar-dark .navbar-nav .nav-link:hover,
         .navbar-dark .navbar-nav .nav-link.active { border-bottom-color: white; background-color: rgba(255, 255, 255, 0.1); }
         .navbar-dark .navbar-nav .nav-link.active:not(:hover) { background-color: transparent; }
         /* Lista 1: Metas Finalizables (Orange theme) */
         .meta-finalizable-item { background-color: #FFF7ED; border-left: 4px solid #F97316 !important; border-top: 1px solid #dee2e6; border-right: 1px solid #dee2e6; border-bottom: 1px solid #dee2e6; transition: background-color 0.2s ease-in-out; padding: 0.75rem 1rem; margin-bottom: 0.5rem; border-radius: 0.375rem; cursor: pointer; /* Make clickable */ }
         .meta-finalizable-item:hover { background-color: #FED7AA; }
         /* Lista 2: Metas Semanales (Sky theme) */
         .meta-semanal-item { background-color: #E0F2FE; border-left-color: #0EA5E9 !important; border-top: 1px solid #dee2e6; border-right: 1px solid #dee2e6; border-bottom: 1px solid #dee2e6; transition: background-color 0.2s ease-in-out; padding: 0.75rem 1rem; margin-bottom: 0.5rem; border-radius: 0.375rem; cursor: pointer; /* Make clickable */ }
         .meta-semanal-item:hover { background-color: #BAE6FD; }
         /* Common styles */
         .view-btn-bs { font-size: 0.75rem; padding: 0.25rem 0.75rem; }
         .task-description-bs { font-size: 0.875rem; color: #495057; flex-grow: 1; display: -webkit-box; -webkit-line-clamp: 1; -webkit-box-orient: vertical; overflow: hidden; text-overflow: ellipsis; margin-left: 0.75rem; }
         .meta-type-badge { font-size: 0.7em; vertical-align: middle; min-width: 35px; text-align: center; }
         /* Tabs */
         .nav-tabs { border-bottom: 1px solid #dee2e6; }
         .nav-tabs .nav-link { color: #6c757d; border: 1px solid transparent; border-bottom: none; margin-bottom: -1px; border-top-left-radius: 0.375rem; border-top-right-radius: 0.375rem; padding: 0.5rem 1rem; }
         .nav-tabs .nav-link.active { color: #F97316; background-color: #fff; border-color: #dee2e6 #dee2e6 #fff; font-weight: 600; }
         .nav-tabs .nav-link:hover:not(.active):not(.disabled) { border-color: #e9ecef #e9ecef #dee2e6; isolation: isolate; color: #495057; background-color: #f8f9fa; }
         .nav-tabs .nav-link.disabled { color: #adb5bd; background-color: transparent; border-color: transparent; cursor: default; }
         .tab-content { border: 1px solid #dee2e6; border-top: none; padding: 1.5rem; border-bottom-left-radius: 0.375rem; border-bottom-right-radius: 0.375rem; background-color: #fff; min-height: 150px; }
         /* Feedback & Modal */
         .feedback-message { display: none; margin-top: 1rem; }
         #modalDescriptionDisplay { font-size: 0.9rem; color: #212529; margin-top: 0.5rem; white-space: pre-wrap; max-height: 250px; overflow-y: auto; background-color: #f8f9fa; padding: 1rem; border-radius: 0.25rem; border: 1px solid #dee2e6; margin-bottom: 1rem; }
         .file-name-display-bs { color: #6c757d; font-style: italic; font-size: 0.8rem; }
         .card { margin-bottom: 2rem; }
         .hidden-server-button { display: none; }
         /* Ensure list items fill width */
         .list-group-item { width: 100%; }
         /* Style for empty data message panels */
         .empty-data-panel { margin-top: 1rem; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%--<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>--%>
    <asp:UpdatePanel ID="UpdatePanelModal" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:HiddenField ID="hfSelectedMetaId" runat="server" Value="0" />
            <asp:HiddenField ID="hfSelectedIsFinalizable" runat="server" Value="false" />
            <asp:HiddenField ID="hfActiveWeekNumber" runat="server" Value="0" />
            <input id="hdGuardado" type="hidden" runat="server" />

             <div class="card shadow-lg mx-auto" style="max-width: 56rem;">
                <div class="card-body p-4 p-md-5">
                    <h1 class="card-title h2 mb-4 border-bottom pb-3">Desempeño</h1>

                    <asp:Literal ID="litFeedback" runat="server" EnableViewState="false"></asp:Literal>
                    <div id="feedbackMessageJS" class="feedback-message alert" role="alert"></div>

                    <%-- Lista 1: Metas Finalizables --%>
                    <section id="metasFinalizables" class="mb-5">
                        <h2 class="h4 mb-3 text-primary">Metas Finalizables</h2>
                        <asp:Repeater ID="rptMetasFinalizables" runat="server">
                            <HeaderTemplate><div class="list-group"></HeaderTemplate>
                            <ItemTemplate>
                                 <div class="list-group-item d-flex align-items-center meta-finalizable-item"
                                      data-meta-id='<%# Eval("IdMetaIndividual") %>'
                                      data-description='<%# Eval("Descripcion") %>' <%-- Store original description --%>
                                      data-es-finalizable='true'
                                      data-meta-type='<%# Eval("NombreTipoObjetivo") %>'>
                                    <span class="badge bg-secondary me-2 meta-type-badge"><%# Eval("NombreTipoObjetivo") %></span>
                                    <span class="task-description-bs flex-grow-1"><%# Eval("Descripcion") %></span> <%-- Display original description --%>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate></div></FooterTemplate>
                        </asp:Repeater>
                        <asp:Panel ID="pnlEmptyMetasFinalizables" runat="server" Visible="false" CssClass="empty-data-panel">
                             <div class="alert alert-secondary small">No hay metas finalizables asignadas.</div>
                        </asp:Panel>
                    </section>

                    <%-- Lista 2: Metas Semanales --%>
                    <section id="metasSemanales" class="mb-4">
                        <h2 class="h4 mb-3 text-primary">Metas Semanales</h2>
                        <nav>
                            <div class="nav nav-tabs mb-0" id="nav-tab" role="tablist">
                                <button class="nav-link" id="nav-week-1-tab" data-bs-toggle="tab" data-bs-target="#nav-week-1" type="button" role="tab" aria-controls="nav-week-1" aria-selected="false" data-week-code-num="1">Semana 1</button>
                                <button class="nav-link" id="nav-week-2-tab" data-bs-toggle="tab" data-bs-target="#nav-week-2" type="button" role="tab" aria-controls="nav-week-2" aria-selected="false" data-week-code-num="2">Semana 2</button>
                                <button class="nav-link" id="nav-week-3-tab" data-bs-toggle="tab" data-bs-target="#nav-week-3" type="button" role="tab" aria-controls="nav-week-3" aria-selected="false" data-week-code-num="3">Semana 3</button>
                                <button class="nav-link" id="nav-week-4-tab" data-bs-toggle="tab" data-bs-target="#nav-week-4" type="button" role="tab" aria-controls="nav-week-4" aria-selected="false" data-week-code-num="4">Semana 4</button>
                                <button class="nav-link" id="nav-week-5-tab" data-bs-toggle="tab" data-bs-target="#nav-week-5" type="button" role="tab" aria-controls="nav-week-5" aria-selected="false" data-week-code-num="5">Semana 5</button>
                            </div>
                        </nav>
                        <div class="tab-content" id="nav-tabContent">
                            <%-- Repeaters for weeks 1 to 5 --%>
                            <div class="tab-pane fade" id="nav-week-1" role="tabpanel" aria-labelledby="nav-week-1-tab" tabindex="0">
                                <asp:Repeater ID="rptSemana1" runat="server" ItemType="Gestor_Desempeno.MetaIndividualInfo">
                                     <HeaderTemplate><div class="list-group mt-3"></HeaderTemplate>
                                     <ItemTemplate>
                                         <div class="list-group-item d-flex align-items-center meta-semanal-item"
                                              data-meta-id='<%# Item.IdMetaIndividual %>'
                                              data-description='<%# Item.Descripcion %>' <%-- Store original description --%>
                                              data-es-finalizable='false'
                                              data-meta-type='<%# Item.NombreTipoObjetivo %>'>
                                             <span class="badge bg-info me-2 meta-type-badge"><%# Item.NombreTipoObjetivo %></span>
                                             <span class="task-description-bs flex-grow-1"><%# Item.DisplayTextLista2 %></span>
                                         </div>
                                     </ItemTemplate>
                                     <FooterTemplate></div></FooterTemplate>
                                </asp:Repeater>
                                <asp:Panel ID="pnlEmptySemana1" runat="server" Visible="false" CssClass="empty-data-panel">
                                    <div class="alert alert-light small mt-3">No hay metas asignadas para esta semana.</div>
                                </asp:Panel>
                            </div>
                             <div class="tab-pane fade" id="nav-week-2" role="tabpanel" aria-labelledby="nav-week-2-tab" tabindex="0">
                                <asp:Repeater ID="rptSemana2" runat="server" ItemType="Gestor_Desempeno.MetaIndividualInfo">
                                     <HeaderTemplate><div class="list-group mt-3"></HeaderTemplate>
                                     <ItemTemplate>
                                         <div class="list-group-item d-flex align-items-center meta-semanal-item" data-meta-id='<%# Item.IdMetaIndividual %>' data-description='<%# Item.Descripcion %>' data-es-finalizable='false' data-meta-type='<%# Item.NombreTipoObjetivo %>'>
                                             <span class="badge bg-info me-2 meta-type-badge"><%# Item.NombreTipoObjetivo %></span> <span class="task-description-bs flex-grow-1"><%# Item.DisplayTextLista2 %></span>
                                         </div>
                                     </ItemTemplate>
                                     <FooterTemplate></div></FooterTemplate>
                                </asp:Repeater>
                                 <asp:Panel ID="pnlEmptySemana2" runat="server" Visible="false" CssClass="empty-data-panel">
                                     <div class="alert alert-light small mt-3">No hay metas asignadas para esta semana.</div>
                                 </asp:Panel>
                            </div>
                             <div class="tab-pane fade" id="nav-week-3" role="tabpanel" aria-labelledby="nav-week-3-tab" tabindex="0">
                                <asp:Repeater ID="rptSemana3" runat="server" ItemType="Gestor_Desempeno.MetaIndividualInfo">
                                     <HeaderTemplate><div class="list-group mt-3"></HeaderTemplate>
                                     <ItemTemplate>
                                         <div class="list-group-item d-flex align-items-center meta-semanal-item" data-meta-id='<%# Item.IdMetaIndividual %>' data-description='<%# Item.Descripcion %>' data-es-finalizable='false' data-meta-type='<%# Item.NombreTipoObjetivo %>'>
                                             <span class="badge bg-info me-2 meta-type-badge"><%# Item.NombreTipoObjetivo %></span> <span class="task-description-bs flex-grow-1"><%# Item.DisplayTextLista2 %></span>
                                         </div>
                                     </ItemTemplate>
                                     <FooterTemplate></div></FooterTemplate>
                                </asp:Repeater>
                                 <asp:Panel ID="pnlEmptySemana3" runat="server" Visible="false" CssClass="empty-data-panel">
                                     <div class="alert alert-light small mt-3">No hay metas asignadas para esta semana.</div>
                                 </asp:Panel>
                            </div>
                             <div class="tab-pane fade" id="nav-week-4" role="tabpanel" aria-labelledby="nav-week-4-tab" tabindex="0">
                                <asp:Repeater ID="rptSemana4" runat="server" ItemType="Gestor_Desempeno.MetaIndividualInfo">
                                     <HeaderTemplate><div class="list-group mt-3"></HeaderTemplate>
                                     <ItemTemplate>
                                         <div class="list-group-item d-flex align-items-center meta-semanal-item" data-meta-id='<%# Item.IdMetaIndividual %>' data-description='<%# Item.Descripcion %>' data-es-finalizable='false' data-meta-type='<%# Item.NombreTipoObjetivo %>'>
                                             <span class="badge bg-info me-2 meta-type-badge"><%# Item.NombreTipoObjetivo %></span> <span class="task-description-bs flex-grow-1"><%# Item.DisplayTextLista2 %></span>
                                         </div>
                                     </ItemTemplate>
                                     <FooterTemplate></div></FooterTemplate>
                                </asp:Repeater>
                                 <asp:Panel ID="pnlEmptySemana4" runat="server" Visible="false" CssClass="empty-data-panel">
                                     <div class="alert alert-light small mt-3">No hay metas asignadas para esta semana.</div>
                                 </asp:Panel>
                            </div>
                             <div class="tab-pane fade" id="nav-week-5" role="tabpanel" aria-labelledby="nav-week-5-tab" tabindex="0">
                                <asp:Repeater ID="rptSemana5" runat="server" ItemType="Gestor_Desempeno.MetaIndividualInfo">
                                     <HeaderTemplate><div class="list-group mt-3"></HeaderTemplate>
                                     <ItemTemplate>
                                         <div class="list-group-item d-flex align-items-center meta-semanal-item" data-meta-id='<%# Item.IdMetaIndividual %>' data-description='<%# Item.Descripcion %>' data-es-finalizable='false' data-meta-type='<%# Item.NombreTipoObjetivo %>'>
                                             <span class="badge bg-info me-2 meta-type-badge"><%# Item.NombreTipoObjetivo %></span> <span class="task-description-bs flex-grow-1"><%# Item.DisplayTextLista2 %></span>
                                         </div>
                                     </ItemTemplate>
                                     <FooterTemplate></div></FooterTemplate>
                                </asp:Repeater>
                                 <asp:Panel ID="pnlEmptySemana5" runat="server" Visible="false" CssClass="empty-data-panel">
                                     <div class="alert alert-light small mt-3">No hay metas asignadas para esta semana.</div>
                                 </asp:Panel>
                            </div>
                        </div>
                    </section>
                </div> <%-- fin card-body --%>
            </div> <%-- fin card --%>

             <%-- Modal Structure --%>
            <div class="modal fade" id="respuestaModal" tabindex="-1" aria-labelledby="respuestaModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title d-flex align-items-center" id="respuestaModalLabel">
                                <img id="modalIcon" src="https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/file-text.svg" alt="Icono" class="me-2" style="width: 24px; height: 24px;"/>
                                <span id="modalTitle">Registrar Respuesta</span>
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                        </div>
                        <div class="modal-body">
                            <div class="mb-3">
                                <label class="form-label fw-semibold">Descripción de la Meta:</label>
                                <div id="modalDescriptionDisplay" class="p-3 border rounded bg-light" style="min-height: 50px;">Cargando descripción...</div>
                            </div>
                            <hr/>
                            <div class="mt-3">
                                <label for="modalObservacion" class="form-label fw-semibold">Respuesta / Observación:</label>
                                <textarea class="form-control" id="modalObservacion" rows="4" placeholder="Ingrese su respuesta u observación aquí..." runat="server"></textarea>
                            </div>
                            <div class="mt-3">
                                <label for="<%= fileUploadControl.ClientID %>" class="form-label fw-semibold">Adjuntar Archivo (Opcional):</label>
                                <asp:FileUpload ID="fileUploadControl" runat="server" CssClass="form-control" title="Funcionalidad de subida pendiente"/>
                                <span class="file-name-display-bs d-block mt-1" id="modalFileNameDisplay">Subida de archivos pendiente de implementación.</span>
                            </div>
                            <div id="ParaIconos" class="modal-nav-icons">
                                    <%--<a href="Default.aspx" class="btn btn-sm btn-outline-primary" title="Ir a la página principal de Desempeño">
                                        <img src="https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/notepad-text.svg" alt="Ir a Inicio"/>
                                        <span class="ms-1">Ir a Inicio</span>
                                    </a> --%>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <%--<button type="button" class="btn btn-primary" id="modalGuardarBtnJS">Guardar Respuesta</button>--%>
                            <asp:Button ID="btModalGuardar" runat="server" CssClass="btn btn-primary" Text="Guardar Respuesta" OnClick="btModalGuardar_Click" />
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        </div>
                    </div>
                </div>
            </div>

        </ContentTemplate>
        <Triggers>
        <asp:PostBackTrigger ControlID="btModalGuardar" />
    </Triggers>
    </asp:UpdatePanel>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
  
<script>
    // --- References ---
    const feedbackMessageJS = document.getElementById('feedbackMessageJS');
    const mainContentContainer = document.querySelector('.card-body'); // Asegúrate que este selector sea el correcto para tu layout
    const hfSelectedMetaId = document.getElementById('<%= hfSelectedMetaId.ClientID %>');
    const hfSelectedIsFinalizable = document.getElementById('<%= hfSelectedIsFinalizable.ClientID %>');
    const hfActiveWeekNumber = document.getElementById('<%= hfActiveWeekNumber.ClientID %>');

    const respuestaModalElement = document.getElementById('respuestaModal');
    let respuestaModal = null;
    if (respuestaModalElement) {
        respuestaModal = new bootstrap.Modal(respuestaModalElement);
    } else { console.error("Modal (#respuestaModal) not found."); }

    const modalIcon = document.getElementById('modalIcon');
    const modalTitle = document.getElementById('modalTitle');
    const modalDescriptionDisplay = document.getElementById('modalDescriptionDisplay');
    const modalObservacionTextarea = document.getElementById('<%= modalObservacion.ClientID %>');
    const modalArchivoInput = document.getElementById('<%= fileUploadControl.ClientID %>');
    const modalFileNameDisplay = document.getElementById('modalFileNameDisplay');
    const modalGuardarBtnJS = document.getElementById('modalGuardarBtnJS'); // Si todavía usas un botón JS para guardar además del de ASP.NET

    const lucideBaseUrl = 'https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/';

    // --- Utility Functions ---
    function showFeedback(message, type = 'success') {
        if (!feedbackMessageJS) return;
        feedbackMessageJS.innerHTML = ''; feedbackMessageJS.textContent = message;
        feedbackMessageJS.className = `feedback-message alert alert-${type} alert-dismissible fade show`;
        feedbackMessageJS.style.display = 'block'; feedbackMessageJS.setAttribute('role', 'alert');
        if (!feedbackMessageJS.querySelector('.btn-close')) { const b = document.createElement('button'); b.type = 'button'; b.className = 'btn-close'; b.setAttribute('data-bs-dismiss', 'alert'); b.setAttribute('aria-label', 'Cerrar'); feedbackMessageJS.appendChild(b); }
        setTimeout(() => { const i = bootstrap.Alert.getOrCreateInstance(feedbackMessageJS); if (i) i.close(); else feedbackMessageJS.style.display = 'none';}, 5000);
    }

    function resetModalFields() {
        if (modalObservacionTextarea) modalObservacionTextarea.value = '';
        if (modalArchivoInput) modalArchivoInput.value = null;
        if (modalFileNameDisplay) modalFileNameDisplay.textContent = 'Subida de archivos pendiente.';
        if (hfSelectedMetaId) hfSelectedMetaId.value = '0';
        if (hfSelectedIsFinalizable) hfSelectedIsFinalizable.value = 'false';
        if (modalDescriptionDisplay) modalDescriptionDisplay.textContent = 'Cargando descripción...';
    }

    function showRespuestaModal(title, descriptionMeta, iconName = 'file-text', metaId = 0, esFinalizable = false, existingObservacion = '', datosDocs = '') {
        if (!respuestaModal || !modalTitle || !modalDescriptionDisplay || !modalIcon || !modalObservacionTextarea || !hfSelectedMetaId || !hfSelectedIsFinalizable) {
            console.error("Modal elements not found or modal not initialized.");
            alert(`No se pueden mostrar los detalles.\nDescripción de la Meta:\n${descriptionMeta}`);
            return;
        }

        modalTitle.textContent = title || 'Registrar Respuesta';
        modalDescriptionDisplay.textContent = descriptionMeta || 'No hay descripción disponible.';
        if (typeof lucideBaseUrl !== 'undefined' && modalIcon) {
            modalIcon.src = `${lucideBaseUrl}${iconName}.svg`;
            modalIcon.alt = `Icono ${title || 'respuesta'}`;
        } else if (modalIcon) {
            console.warn("lucideBaseUrl no está definido.");
            modalIcon.alt = `Icono ${title || 'respuesta'}`;
        }
        hfSelectedMetaId.value = metaId;
        hfSelectedIsFinalizable.value = esFinalizable.toString();

        if (modalObservacionTextarea) modalObservacionTextarea.value = '';
        if (modalArchivoInput) modalArchivoInput.value = null;
        if (modalFileNameDisplay) modalFileNameDisplay.textContent = 'Subida de archivos pendiente.';

        var contenedorDocumentos = document.getElementById('ParaIconos');
        if (contenedorDocumentos) {
            contenedorDocumentos.innerHTML = ''; // Limpiar contenido previo
        } else {
            console.error("El elemento con ID 'ParaIconos' no fue encontrado. Los documentos no se mostrarán.");
        }

        if (modalObservacionTextarea) {
            modalObservacionTextarea.value = existingObservacion;
        }

        // --- Sección Modificada para mostrar documentos como Enlaces Simples ---
        let htmlDocumentos = "";
        if (datosDocs && datosDocs !== "") {
            htmlDocumentos = '<h6 class="mt-3 mb-2">Documentos Adjuntos:</h6><div>'; // Iniciar un div contenedor
            const documentosArray = datosDocs.split('|');

            documentosArray.forEach(docEntry => {
                if (docEntry) {
                    const partes = docEntry.split(',');
                    const urli = partes[0];
                    const nombre = partes.length > 1 ? partes.slice(1).join(',') : 'Documento sin nombre';

                    // Generar un enlace simple con icono
                    htmlDocumentos += `
                        <a href="javascript:void(0);" 
                           class="d-inline-block border rounded px-2 py-1 me-2 mb-2 text-decoration-none text-secondary doc-link-popup" 
                           style="font-size: 0.9em; line-height: 1.4;"
                           data-doc-url="${urli}" 
                           title="Ver documento: ${nombre}">
                            <img src="https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/file-text.svg" alt="doc" style="width:1em; height:1em; vertical-align: text-bottom; margin-right: 0.3rem;"/>
                            <span>${nombre}</span>
                        </a>`;
                }
            });


            htmlDocumentos += '</div>'; // Cerrar el div contenedor
            if (contenedorDocumentos) {
                 contenedorDocumentos.insertAdjacentHTML('beforeend', htmlDocumentos);
            }
        } else if (contenedorDocumentos) {
            contenedorDocumentos.insertAdjacentHTML('beforeend', '<p class="mt-3 small text-muted">No hay documentos adjuntos.</p>');
        }
        // --- Fin Sección Modificada ---


        if (respuestaModal && typeof respuestaModal.show === 'function') {
            respuestaModal.show();
        } else {
            console.error("'respuestaModal' no está definido o no tiene un método 'show()'.");
        }
    }


    function setActiveWeekTab() {
        try {
            const today = new Date();
            const dayOfMonth = today.getDate();
            const currentWeek = Math.min(Math.floor((dayOfMonth - 1) / 7) + 1, 5);

            // Establecer el valor del HiddenField para la semana actual
            if (hfActiveWeekNumber) {
                hfActiveWeekNumber.value = currentWeek.toString();
                console.log(`Active week number set to: ${currentWeek} in HiddenField`);
            } else {
                console.error("HiddenField hfActiveWeekNumber not found.");
            }

            const defaultActiveTab = document.querySelector('#nav-tab .nav-link.active');
            const defaultActivePane = document.querySelector('#nav-tabContent .tab-pane.active');
            const targetTab = document.getElementById(`nav-week-${currentWeek}-tab`);
            const targetPane = document.getElementById(`nav-week-${currentWeek}`);

            if (targetTab && targetPane && (!defaultActiveTab || defaultActiveTab !== targetTab)) {
                if (defaultActiveTab) {
                    defaultActiveTab.classList.remove('active');
                    defaultActiveTab.setAttribute('aria-selected', 'false');
                }
                if (defaultActivePane) {
                    defaultActivePane.classList.remove('show', 'active');
                }
                targetTab.classList.add('active');
                targetTab.setAttribute('aria-selected', 'true');
                targetPane.classList.add('show', 'active');
                console.log(`Active tab set to Week ${currentWeek}`);
            } else if (targetTab && targetPane && defaultActiveTab === targetTab) {
                if (!targetPane.classList.contains('show')) targetPane.classList.add('show');
                if (!targetPane.classList.contains('active')) targetPane.classList.add('active');
            } else {
                console.warn(`Could not find tab/pane for week ${currentWeek}. Activating Week 1.`);
                const w1t = document.getElementById('nav-week-1-tab');
                const w1p = document.getElementById('nav-week-1');
                if (w1t && w1p && (!defaultActiveTab || defaultActiveTab !== w1t)) {
                    if (defaultActiveTab) {
                        defaultActiveTab.classList.remove('active');
                        defaultActiveTab.setAttribute('aria-selected', 'false');
                    }
                    if (defaultActivePane) {
                        defaultActivePane.classList.remove('show', 'active');
                    }
                    w1t.classList.add('active');
                    w1t.setAttribute('aria-selected', 'true');
                    w1p.classList.add('show', 'active');
                    // Si se activa la semana 1 por defecto y no es la semana actual, actualizar el hidden field
                    if (hfActiveWeekNumber && currentWeek !== 1) {
                        hfActiveWeekNumber.value = "1";
                        console.log(`Active week number (fallback) set to: 1 in HiddenField`);
                    }
                }
            }
        } catch (error) {
            console.error("Error setting active week tab:", error);
        }
    }

    function disableWeek5IfNeeded() { /* ... (tu código existente) ... */
        try {
            const today = new Date(); const year = today.getFullYear(); const month = today.getMonth();
            const daysInMonth = new Date(year, month + 1, 0).getDate();
            if (daysInMonth < 29) { const w5t = document.getElementById('nav-week-5-tab'); if (w5t) { w5t.classList.add('disabled'); w5t.setAttribute('aria-disabled', 'true'); w5t.setAttribute('tabindex', '-1'); console.log(`Week 5 tab disabled (${daysInMonth} days).`); } }
        } catch (error) { console.error("Error disabling week 5 tab:", error); }
    }

    function getCodigoSemanaJS(weekNumber) {
        const today = new Date();
        const month = today.getMonth() + 1; // JavaScript months are 0-indexed
        const year = today.getFullYear();
        const monthStr = month < 10 ? '0' + month : month.toString();
        const weekStr = weekNumber ? weekNumber.toString() : '0';
        return `${weekStr}${monthStr}${year}`;
    }

    // --- Event Listener para el click en las metas (para abrir respuestaModal) ---
    if (mainContentContainer) {
        mainContentContainer.addEventListener('click', function (event) {
            const clickedListItem = event.target.closest('.meta-finalizable-item, .meta-semanal-item');
            if (clickedListItem) {
                event.stopPropagation();
                const descriptionMeta = clickedListItem.dataset.description || 'Detalle no encontrado.';
                const metaId = parseInt(clickedListItem.dataset.metaId || '0', 10);
                const esFinalizable = clickedListItem.dataset.esFinalizable === 'true';
                // const metaType = clickedListItem.dataset.metaType || ''; // No se usa actualmente
                let modalTitleText = 'Registrar Respuesta';
                let modalIconName = 'edit';
                let codigoSemanaDeLaPestana = null;

                if (!esFinalizable) {
                    const activeTabButton = document.querySelector('#nav-tab .nav-link.active');
                    if (activeTabButton && activeTabButton.dataset.weekCodeNum) {
                        codigoSemanaDeLaPestana = getCodigoSemanaJS(activeTabButton.dataset.weekCodeNum);
                        console.log("Clicked weekly meta from tab with CodigoSemana:", codigoSemanaDeLaPestana);
                    } else {
                        console.warn("Could not determine active week tab's code for weekly meta.");
                    }
                }

                if (metaId > 0) {
                    PageMethods.GetRespuestaDetalles(metaId, esFinalizable, codigoSemanaDeLaPestana,
                        (response) => {
                            let existingObservacion = '';
                            let datosDocsDesdeServidor = ''; // Inicializar
                            if (response && response.success && response.data) {
                                existingObservacion = response.data.ObservacionGuardada || '';
                                // let descripcionDesdeServidor = response.data.DescripcionMeta; // No se usa actualmente
                                datosDocsDesdeServidor = response.data.DatosDocs || '';
                            } else if (response && !response.success) {
                                console.warn("GetRespuestaDetalles no tuvo éxito:", response.message);
                            }
                            showRespuestaModal(modalTitleText, descriptionMeta, modalIconName, metaId, esFinalizable, existingObservacion, datosDocsDesdeServidor);
                        },
                        (error) => {
                            console.error("Error fetching response details:", error);
                            showFeedback('Error al cargar datos de respuesta previa.', 'danger');
                            showRespuestaModal(modalTitleText, descriptionMeta, modalIconName, metaId, esFinalizable, '', ''); // Sin datos de documentos en caso de error
                        }
                    );
                } else {
                    console.error("Meta ID not found on clicked item.");
                    showFeedback('Error: No se pudo identificar la meta seleccionada.', 'danger');
                }
            }
        });
    } else { console.error("Main content container (.card-body) not found for meta click listener."); }

    // --- Event Listener para abrir documentos en Popup ---
    if (respuestaModalElement) { // Adjuntar al cuerpo de la modal para delegación
        respuestaModalElement.addEventListener('click', function(event) {
            const linkClickeado = event.target.closest('.doc-link-popup');
            if (linkClickeado) {
                event.preventDefault();
                const urlParaAbrir = linkClickeado.dataset.docUrl;
                const nombreVentana = 'visorDocumento';
                const opcionesVentana = 'width=800,height=600,resizable=yes,scrollbars=yes,status=yes';

                console.log('Intentando abrir popup con URL:', urlParaAbrir);

                if (urlParaAbrir) {
                    // Verificar si la URL es absoluta o construirla si es necesario
                    let finalUrl = urlParaAbrir;
                    // Ajusta esta lógica si 'urlParaAbrir' no es la URL completa o correcta para Doc.aspx
                    if (!urlParaAbrir.startsWith('http://') && !urlParaAbrir.startsWith('https://') && !urlParaAbrir.startsWith('Doc.aspx')) {
                        finalUrl = urlParaAbrir.startsWith('?') ? 'Doc.aspx' + urlParaAbrir : 'Doc.aspx?' + urlParaAbrir.replace(/^Doc\.aspx\??/, '');
                         if (!finalUrl.includes('num=')) {
                            console.warn("La URL del documento parece incompleta, intentando construirla: ", finalUrl);
                         }
                    } else if (urlParaAbrir.startsWith('Doc.aspx')) {
                        // Ya es Doc.aspx?num=... o Doc.aspx, está bien.
                    }

                    const nuevaVentana = window.open(finalUrl, nombreVentana, opcionesVentana);
                    if (!nuevaVentana || nuevaVentana.closed || typeof nuevaVentana.closed == 'undefined') {
                        alert('No se pudo abrir la ventana del documento. Por favor, revisa si tu navegador está bloqueando ventanas emergentes y permite pop-ups para este sitio.');
                    }
                } else {
                    console.error('El atributo data-doc-url está vacío o no se encontró en el enlace.');
                    alert('Error: No se pudo obtener la URL del documento para abrir.');
                }
            }
        });
    } else {
        console.warn("El elemento de la modal 'respuestaModal' no fue encontrado. El listener para abrir popups de documentos no se adjuntará.");
    }


    // Listener for file input change (optional)
    if (modalArchivoInput && modalFileNameDisplay) {
        modalArchivoInput.addEventListener('change', function(event) { modalFileNameDisplay.textContent = event.target.files.length > 0 ? event.target.files[0].name : 'Subida de archivos pendiente.'; });
    }

    // Listener for Save button in modal (JS) - Si lo usas
    if (modalGuardarBtnJS) {
        modalGuardarBtnJS.addEventListener('click', function () {
            const metaId = parseInt(hfSelectedMetaId.value, 10);
            const esFinalizable = hfSelectedIsFinalizable.value === 'true';
            const observacion = modalObservacionTextarea ? modalObservacionTextarea.value : '';
            const fileName = null; // File upload deferred

            let activeWeekNumClient = 0; // Esta variable JS contiene el número de semana del cliente
            if (hfActiveWeekNumber && hfActiveWeekNumber.value) {
                if (isNaN(parseInt(hfActiveWeekNumber.value, 10))) { // Comprobación más robusta con isNaN
                    console.warn("El valor de hfActiveWeekNumber no es un entero válido:", hfActiveWeekNumber.value);
                    const today = new Date();
                    const dayOfMonth = today.getDate();
                    activeWeekNumClient = Math.min(Math.floor((dayOfMonth - 1) / 7) + 1, 5);
                } else {
                    activeWeekNumClient = parseInt(hfActiveWeekNumber.value, 10);
                }
            } else {
                console.error("hfActiveWeekNumber no encontrado o sin valor.");
                const today = new Date();
                const dayOfMonth = today.getDate();
                activeWeekNumClient = Math.min(Math.floor((dayOfMonth - 1) / 7) + 1, 5);
                console.log("Usando semana calculada en JS como fallback:", activeWeekNumClient);
            }

            if (metaId <= 0) { showFeedback('Error: Meta no válida.', 'danger'); return; }

            PageMethods.GuardarRespuesta(metaId, observacion, fileName, esFinalizable, activeWeekNumClient,
                (response) => {
                    if (response && response.success) {
                        showFeedback(response.message || 'Respuesta guardada.', 'success');
                        if (respuestaModal) respuestaModal.hide();
                        __doPostBack('<%= UpdatePanelModal.ClientID %>', ''); // Trigger partial postback
                    } else { showFeedback(response.message || 'Error al guardar.', 'danger'); }
                },
                (error) => {
                    console.error("Error calling GuardarRespuesta:", error);
                    let errorMsg = 'Error de comunicación.';
                    if (error.get_message) { errorMsg = error.get_message(); }
                    else if (error.responseText) { try { const errObj = JSON.parse(error.responseText); if (errObj && errObj.Message) errorMsg = errObj.Message; } catch (e) { /* ignore */ } }
                    showFeedback(errorMsg, 'danger');
                }
            );
        });
    }

    // --- Initialization ---
    document.addEventListener('DOMContentLoaded', () => {
        console.log("DOM loaded. Init Desempeño.");
        // La instancia de respuestaModal ya se crea arriba si el elemento existe.
        if (respuestaModalElement) {
            respuestaModalElement.addEventListener('hidden.bs.modal', resetModalFields);
        }
        disableWeek5IfNeeded();
        setActiveWeekTab(); // Esto ya establece el valor inicial del HiddenField

        // NUEVO: Listener para actualizar el HiddenField cuando cambia la pestaña de semana
        const weekTabButtons = document.querySelectorAll('#nav-tab .nav-link[data-bs-toggle="tab"]');
        weekTabButtons.forEach(tabButton => {
            tabButton.addEventListener('shown.bs.tab', function (event) {
                const weekNumber = event.target.dataset.weekCodeNum; // Obtiene el número de 'data-week-code-num'
                if (hfActiveWeekNumber && weekNumber) {
                    hfActiveWeekNumber.value = weekNumber;
                    console.log(`Active week number updated to: ${weekNumber} in HiddenField`);
                } else {
                    if (!weekNumber) console.error("data-week-code-num not found on tab:", event.target);
                    if (!hfActiveWeekNumber) console.error("HiddenField hfActiveWeekNumber not found for tab change.");
                }
            });
        });
        console.log("Init Desempeño complete.");
    });

</script>

</asp:Content>
