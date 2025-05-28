<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="RevisionMetasSubordinados.aspx.cs" Inherits="Gestor_Desempeno.RevisionMetasSubordinados" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Estilos base para los ítems de metas (puedes reutilizar/adaptar de Desempeno.aspx) */
        .meta-revision-item {
            display: flex;
            align-items: center;
            border-left-width: 5px !important;
            border-left-style: solid !important;
            border-left-color: #6c757d !important; /* Un gris neutro para "pendiente de revisión" */
            transition: background-color 0.2s ease-in-out, border-left-color 0.2s ease-in-out;
            padding: 0.85rem 1.15rem;
            margin-bottom: 0.6rem;
            border-radius: 0.375rem;
            cursor: pointer;
            border: 1px solid #dee2e6;
            background-color: #f8f9fa; /* Fondo ligeramente diferente */
            box-shadow: 0 1px 3px rgba(0,0,0,0.05);
        }

            .meta-revision-item:hover {
                background-color: #e9ecef;
            }

        .meta-descripcion-principal {
            font-weight: 500;
            flex-grow: 1;
            display: -webkit-box;
            -webkit-line-clamp: 2;
            -webkit-box-orient: vertical;
            overflow: hidden;
            text-overflow: ellipsis;
            line-height: 1.4;
            margin-right: 1rem;
        }

        .meta-usuario-subordinado {
            font-size: 0.85em;
            color: #495057;
            margin-right: 1rem;
            white-space: nowrap;
        }

        .meta-fecha-final {
            font-size: 0.8em;
            color: #6c757d;
            white-space: nowrap;
            margin-left: auto; /* Empuja a la derecha */
        }

        .meta-badge-tipo {
            font-size: 0.75em;
            font-weight: 600;
            padding: 0.3em 0.55em;
            border-radius: 0.25rem;
            min-width: 60px;
            text-align: center;
            margin-right: 0.75rem !important;
            background-color: #0d6efd; /* Azul para tipo de objetivo */
            color: white;
        }

        .respondida-tarde-indicator {
            color: #dc3545; /* Rojo para indicar tarde */
            font-weight: bold;
            font-size: 0.8em;
            margin-left: 0.5rem;
        }

        .historial-respuesta-item {
            border-bottom: 1px solid #eee;
            padding-bottom: 0.75rem;
            margin-bottom: 0.75rem;
        }

            .historial-respuesta-item:last-child {
                border-bottom: none;
                margin-bottom: 0;
            }

        .historial-fecha {
            font-size: 0.8em;
            color: #6c757d;
            display: block;
            margin-bottom: 0.25rem;
        }

        .historial-observacion {
            font-size: 0.9em;
            white-space: pre-wrap; /* Para respetar saltos de línea en la observación */
        }
        /* Estilos para el modal y sus componentes */
        #modalMetaDescriptionDisplay, #modalHistorialRespuestasContent {
            font-size: 0.95rem;
            background-color: #f8f9fa;
            padding: 1rem;
            border-radius: 0.25rem;
            border: 1px solid #e9ecef;
            margin-bottom: 1rem;
            max-height: 150px; /* Ajusta según necesidad */
            overflow-y: auto;
            white-space: pre-wrap;
        }

        #modalHistorialRespuestasContent {
            max-height: 200px; /* Más espacio para el historial */
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdatePanelPagina" runat="server">
        <ContentTemplate>
            <asp:HiddenField ID="hfSelectedMetaIdRevision" runat="server" Value="0" />
            <%-- Podrías necesitar un HiddenField para el estado actual si lo usas en JS o C# --%>
            <%-- <asp:HiddenField ID="hfEstadoActualMeta" runat="server" Value="0" /> --%>

            <div class="card shadow-lg mx-auto" style="max-width: 70rem;">
                <div class="card-body p-4 p-md-5">
                    <h1 class="card-title h2 mb-4 border-bottom pb-3">Revisión de Metas Respondidas</h1>

                    <asp:Literal ID="litMensajeGeneral" runat="server" EnableViewState="false"></asp:Literal>

                    <div class="mt-4">
                        <asp:Repeater ID="rptMetasRespondidasParaRevision" runat="server">
                            <HeaderTemplate>
                                <div class="list-group">
                            </HeaderTemplate>
                            <ItemTemplate>
                                <div class='meta-revision-item'
                                    data-meta-id='<%# Eval("Meta.IdMetaIndividual") %>'
                                    data-meta-descripcion='<%# Server.HtmlEncode(Eval("Meta.Descripcion").ToString()) %>'
                                    onclick="abrirModalRevision(this);">

                                    <span class="meta-badge-tipo" title='<%# Server.HtmlEncode(Eval("Meta.NombreTipoObjetivo").ToString()) %>'>
                                        <%# Server.HtmlEncode(Eval("Meta.NombreTipoObjetivo").ToString().Length > 3 ? Eval("Meta.NombreTipoObjetivo").ToString().Substring(0,3).ToUpper() : Eval("Meta.NombreTipoObjetivo").ToString().ToUpper()) %>
                                    </span>
                                    <span class="meta-descripcion-principal" title='<%# Server.HtmlEncode(Eval("Meta.Descripcion").ToString()) %>'>
                                        <%# Server.HtmlEncode(Eval("Meta.Descripcion").ToString()) %>
                                    </span>
                                    <span class="meta-usuario-subordinado">
                                        <img src="https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/user.svg" alt="Usr" style="width: 1em; height: 1em; vertical-align: text-bottom; margin-right: 0.2rem;" />
                                        <%# Eval("Meta.Usuario") %>
                                    </span>
                                    <asp:Panel Visible='<%# (bool)Eval("RespondidaFueraDeTiempo") %>' runat="server" CssClass="respondida-tarde-indicator">
                                        <img src="https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/alert-triangle.svg" alt="Tarde" style="width: 1em; height: 1em; vertical-align: text-bottom; margin-right: 0.2rem;" />
                                        Tarde
                                    </asp:Panel>
                                    <small class="meta-fecha-final">Finaliza: <%# Eval("Meta.FechaFinal", "{0:dd/MM/yyyy}") ?? "N/A" %>
                                    </small>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate></div></FooterTemplate>
                            <%-- NO hay EmptyDataTemplate aquí --%>
                        </asp:Repeater>

                        <%-- Panel para mostrar cuando no hay datos --%>
                        <asp:Panel ID="pnlNoHayMetasParaRevision" runat="server" Visible="false" CssClass="mt-3">
                            <div class="alert alert-info">No hay metas pendientes de revisión por el momento.</div>
                        </asp:Panel>
                    </div>
                </div>
            </div>

            <div class="modal fade" id="revisionMetaModal" tabindex="-1" aria-labelledby="revisionMetaModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered modal-lg">
                    <div class="modal-content">
                        <asp:UpdatePanel ID="UpdatePanelModal" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="modal-header">
                                    <h5 class="modal-title" id="revisionMetaModalLabel">Revisar Meta</h5>
                                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                                </div>
                                <div class="modal-body">
                                    <div class="mb-3">
                                        <label class="form-label fw-semibold">Descripción de la Meta:</label>
                                        <div id="modalMetaDescriptionDisplay" class="p-2 border rounded bg-light" style="min-height: 60px;">Cargando...</div>
                                    </div>

                                    <hr />
                                    <h6 class="fw-semibold">Historial de Respuestas del Colaborador:</h6>
                                    <div id="modalHistorialRespuestasContent" class="p-2 border rounded bg-light mb-3" style="min-height: 100px;">
                                        <%-- El historial se cargará aquí mediante JavaScript --%>
                                        <span class="text-muted small">Cargando historial...</span>
                                    </div>
                                    <%-- NUEVA SECCIÓN PARA DOCUMENTOS --%>
                                    <hr />
                                    <h6 class="fw-semibold">Documentos Adjuntos (Última Respuesta del Colaborador):</h6>
                                    <div id="modalDocumentosUltimaRespuestaContent" class="p-2 border rounded bg-light mb-3" style="min-height: 50px;">
                                        <span class="text-muted small">Cargando documentos...</span>
                                    </div>
                                    <hr />
                                    <div class="mt-3">
                                        <label for="<%=txtComentarioJefeModal.ClientID %>" class="form-label fw-semibold">Comentario del Jefe (Feedback):</label>
                                        <textarea class="form-control" id="txtComentarioJefeModal" rows="3" placeholder="Ingrese su comentario aquí..." runat="server"></textarea>
                                        <small id="comentarioError" class="text-danger d-none">El comentario es obligatorio para devolver la meta.</small>
                                    </div>
                                </div>
                                <div class="modal-footer">
                                    <asp:Button ID="btnDevolverMetaModal" runat="server" CssClass="btn btn-warning me-2" Text="Devolver Meta" OnClick="btnDevolverMeta_Click" />
                                    <asp:Button ID="btnCompletarGestionModal" runat="server" CssClass="btn btn-success" Text="Completar Gestión" OnClick="btnCompletarGestion_Click" />
                                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                                </div>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="btnDevolverMetaModal" EventName="Click" />
                                <asp:AsyncPostBackTrigger ControlID="btnCompletarGestionModal" EventName="Click" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script>
        let revisionModal = null; // Instancia del modal de Bootstrap
        const hfSelectedMetaIdRevisionJS = document.getElementById('<%= hfSelectedMetaIdRevision.ClientID %>');
        const modalMetaDescriptionDisplayJS = document.getElementById('modalMetaDescriptionDisplay');
        const modalHistorialRespuestasContentJS = document.getElementById('modalHistorialRespuestasContent');
        const txtComentarioJefeModalJS = document.getElementById('<%= txtComentarioJefeModal.ClientID %>');
        const comentarioErrorJS = document.getElementById('comentarioError');

        const btnDevolverMetaModalJS = document.getElementById('<%= btnDevolverMetaModal.ClientID %>');
        //const btnCompletarGestionModalJS = document.getElementById('<%= btnCompletarGestionModal.ClientID %>');


        document.addEventListener('DOMContentLoaded', function () {
            const revisionMetaModalElement = document.getElementById('revisionMetaModal');
            if (revisionMetaModalElement) {
                revisionModal = new bootstrap.Modal(revisionMetaModalElement);

                // Limpiar campos cuando el modal se oculte
                revisionMetaModalElement.addEventListener('hidden.bs.modal', function () {
                    hfSelectedMetaIdRevisionJS.value = '0';
                    modalMetaDescriptionDisplayJS.innerHTML = 'Cargando...';
                    modalHistorialRespuestasContentJS.innerHTML = '<span class="text-muted small">Cargando historial...</span>';
                    txtComentarioJefeModalJS.value = '';
                    comentarioErrorJS.classList.add('d-none');
                });
            }
            // Adjuntar validación al botón "Devolver" si no se hace postback completo para validación
            if (btnDevolverMetaModalJS) {
                btnDevolverMetaModalJS.addEventListener('click', function (event) {
                    if (txtComentarioJefeModalJS.value.trim() === '') {
                        comentarioErrorJS.classList.remove('d-none');
                        // Prevenir el postback si la validación del lado del cliente falla
                        // Esto es útil si el botón NO está dentro de un UpdatePanel o si quieres validación extra.
                        // Si el botón está en UpdatePanel y causa postback, la validación C# es prioritaria.
                        // event.preventDefault(); // Descomentar si es necesario prevenir postback
                    } else {
                        comentarioErrorJS.classList.add('d-none');
                    }
                });
            }
        });

        function abrirModalRevision(element) {
            if (!revisionModal) {
                console.error("Modal de revisión no inicializado.");
                return;
            }
            const metaId = element.getAttribute('data-meta-id');
            const metaDescripcion = element.getAttribute('data-meta-descripcion');

            hfSelectedMetaIdRevisionJS.value = metaId;
            document.getElementById('revisionMetaModalLabel').textContent = 'Revisar Meta ID: ' + metaId;
            modalMetaDescriptionDisplayJS.textContent = metaDescripcion || 'Descripción no disponible.';

            cargarHistorialRespuestas(metaId);
            revisionModal.show();
        }

        // En RevisionMetasSubordinados.aspx, dentro de la etiqueta <script>

        // En RevisionMetasSubordinados.aspx, dentro de la etiqueta <script>

        function cargarHistorialRespuestas(metaId) {
            modalHistorialRespuestasContentJS.innerHTML = '<div class="text-center small p-2"><div class="spinner-border spinner-border-sm" role="status"><span class="visually-hidden">Cargando...</span></div> Cargando historial...</div>';

            // La sección separada para documentos de la "última respuesta" ya no es necesaria,
            // así que puedes limpiar/ocultar ese div si aún existe en tu HTML.
            const divDocsSeparado = document.getElementById('modalDocumentosUltimaRespuestaContent');
            if (divDocsSeparado) {
                divDocsSeparado.innerHTML = ''; // Limpiar
                divDocsSeparado.style.display = 'none'; // Ocultar
            }

            PageMethods.GetHistorialRespuestasMeta(metaId,
                function (response) {
                    let result = response.d || response; // Manejar el wrapper 'd'

                    console.log("Respuesta de GetHistorialRespuestasMeta (con docs por item):", result);

                    if (result && result.success && result.data) { // 'result.data' es ahora la lista de historial
                        if (result.data.length > 0) {
                            let historialHtml = '';
                            result.data.forEach(function (respuestaItem) {
                                historialHtml += '<div class="historial-respuesta-item">';
                                const fechaEntregado = respuestaItem.FechaEntregado ? new Date(respuestaItem.FechaEntregado).toLocaleString('es-CR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit', hour12: true }) : 'Fecha no disponible';
                                historialHtml += '<span class="historial-fecha">' + fechaEntregado + '</span>';
                                historialHtml += '<div class="historial-observacion mb-2">' + (respuestaItem.Descripcion || '<em>Sin observación</em>') + '</div>';

                                // --- Renderizar Documentos para ESTE item del historial ---
                                if (respuestaItem.DatosDocs && respuestaItem.DatosDocs.trim() !== "") {
                                    historialHtml += '<div class="mb-2"><small class="fw-semibold">Documentos Adjuntos:</small>'; // Título más pequeño
                                    historialHtml += '<div class="d-flex flex-wrap mt-1">'; // Contenedor para los links/botones
                                    const documentosArray = respuestaItem.DatosDocs.split('|');

                                    documentosArray.forEach(function (docEntry) {
                                        if (docEntry) {
                                            const partes = docEntry.split(',');
                                            if (partes.length >= 1) {
                                                const urlDoc = partes[0]; // Ej: "Doc.aspx?num=IDE_OBJ_ENCODED"
                                                const nombreDoc = partes.length > 1 ? partes.slice(1).join(',') : 'Documento';

                                                historialHtml += `
                                            <a href="javascript:void(0);"
                                               class="btn btn-sm btn-outline-secondary me-1 mb-1 doc-link-popup-revision" 
                                               style="font-size: 0.75em; line-height: 1.1; padding: 0.15rem 0.3rem;"
                                               data-doc-url="${urlDoc}" 
                                               title="Ver documento: ${nombreDoc}">
                                               <img src="https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/file-text.svg" alt="doc" style="width:0.8em; height:0.8em; vertical-align: text-bottom; margin-right: 0.15rem;"/>
                                               <span>${nombreDoc}</span>
                                            </a>`;
                                            }
                                        }
                                    });



                                    historialHtml += '</div></div>'; // Cierre de d-flex y de contenedor de documentos adjuntos
                                }
                                historialHtml += '</div>'; // Cierre de historial-respuesta-item
                            });
                            modalHistorialRespuestasContentJS.innerHTML = historialHtml;
                        } else {
                            modalHistorialRespuestasContentJS.innerHTML = '<span class="text-muted small p-2">No hay historial de respuestas para esta meta.</span>';
                        }
                    } else {
                        const errorMsg = (result && result.message) ? result.message : 'Respuesta inesperada del servidor.';
                        modalHistorialRespuestasContentJS.innerHTML = '<span class="text-danger small p-2">Error al cargar historial: ' + errorMsg + '</span>';
                    }
                },
                function (error) { // Error de la llamada AJAX
                    console.error("Error AJAX en GetHistorialRespuestasMeta:", error);
                    const errorMsg = (error && error.get_message) ? error.get_message() : "Error de red o servidor desconocido.";
                    modalHistorialRespuestasContentJS.innerHTML = '<span class="text-danger small p-2">Error de red/servidor (historial): ' + errorMsg + '</span>';
                }
            );
        }



        // Necesitarás un event listener para los nuevos links de documentos si usas una clase diferente (ej. 'doc-link-popup-revision')
        // o asegúrate que el listener global que ya tenías funcione para estos también.
        // Si el listener de 'Desempeno.aspx' es global y basado en la clase 'doc-link-popup',
        // y aquí usaste 'doc-link-popup-revision', necesitarías duplicar o generalizar ese listener.
        // Por simplicidad, si usas la misma clase 'doc-link-popup' y el listener es global, debería funcionar.
        // Si no, aquí un ejemplo adaptado:

        document.addEventListener('click', function (event) {
            // Asegurarse que el click provenga de dentro del modal de revisión
            const modalElement = document.getElementById('revisionMetaModal');
            if (!modalElement || !modalElement.classList.contains('show')) { // Solo si el modal está visible
                return;
            }

            const linkClickeado = event.target.closest('.doc-link-popup-revision'); // O la clase que uses
            if (linkClickeado) {
                event.preventDefault();
                const urlParaAbrir = linkClickeado.dataset.docUrl;
                if (urlParaAbrir) {
                    // La URL ya viene como "Doc.aspx?num=..." por lo que debería ser relativa a la raíz del sitio.
                    // Si Doc.aspx no está en la raíz, necesitarás ajustar esto.
                    // Ejemplo: const urlCompleta = window.location.origin + '/' + urlParaAbrir;
                    const urlCompleta = urlParaAbrir; // Asumiendo que es correcta como está.

                    const nuevaVentana = window.open(urlCompleta, 'visorDocumento', 'width=800,height=600,resizable=yes,scrollbars=yes,status=yes,noopener,noreferrer');
                    if (!nuevaVentana) {
                        //alert('Parece que tu navegador bloqueó la ventana emergente para ver el documento. Por favor, permite las ventanas emergentes para este sitio.');
                    }
                } else {
                    alert('Error: URL del documento no encontrada en el link.');
                }
            }
        });

        // Para manejar mensajes del servidor después de un postback dentro del UpdatePanel
        function pageLoaded(sender, args) {
            if (args.get_isPartialLoad()) { // Si es un postback parcial del UpdatePanel
                var feedbackLit = document.getElementById('<%= litMensajeGeneral.ClientID %>');
                if (feedbackLit && feedbackLit.innerHTML.trim() !== '') {
                    // Si quieres que los mensajes desaparezcan después de un tiempo
                    setTimeout(function () {
                        if (feedbackLit) feedbackLit.innerHTML = '';
                    }, 7000); // 7 segundos
                }

                // Re-inicializar el modal de Bootstrap si es necesario después de un postback parcial
                // (aunque con el listener 'hidden.bs.modal' debería ser suficiente)
                const revisionMetaModalElement = document.getElementById('revisionMetaModal');
                if (revisionMetaModalElement && !revisionModal) { // Si se perdió la instancia
                    revisionModal = new bootstrap.Modal(revisionMetaModalElement);
                }
            }
        }

        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageLoaded);
        }

    </script>
</asp:Content>
