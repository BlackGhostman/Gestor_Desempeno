<%@ Page Title="Desempeño" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Desempeno.aspx.cs" Inherits="Gestor_Desempeno.Desempeno" %>

<%@ Import Namespace="System.Globalization" %>
<%-- Needed for CalendarWeekRule --%>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">

    <style>
        /* Estilos para la página de Desempeño */

        /* Estilos base para todos los ítems de metas en las listas */
        .meta-item-base {
            display: flex;
            align-items: center;
            border-left-width: 5px !important; /* Un borde izquierdo más grueso */
            border-left-style: solid !important;
            transition: background-color 0.2s ease-in-out, border-left-color 0.2s ease-in-out;
            padding: 0.85rem 1.15rem; /* Ajuste de padding */
            margin-bottom: 0.6rem; /* Un poco más de espacio entre ítems */
            border-radius: 0.375rem; /* Esquinas redondeadas estándar de Bootstrap */
            cursor: pointer;
            border: 1px solid #dee2e6; /* Borde general sutil */
            box-shadow: 0 1px 3px rgba(0,0,0,0.05); /* Sombra suave para profundidad */
        }

            .meta-item-base:hover {
                /* box-shadow: 0 2px 5px rgba(0,0,0,0.1); */ /* Sombra más pronunciada al hacer hover */
            }

        /* Estado: Meta Vencida (ROJO) */
        .meta-vencida {
            border-left-color: #a51829 !important;
            background-color: #f8d7da !important;
            color: #721c24 !important;
        }

            .meta-vencida:hover {
                background-color: #f1cacc !important;
                border-left-color: #8a1320 !important;
            }

            .meta-vencida .task-description-bs, .meta-vencida small {
                color: #721c24 !important;
            }

        /* Estado: Meta Vence Hoy (AMARILLO) */
        .meta-hoy {
            border-left-color: #dda800 !important;
            background-color: #fff3cd !important;
            color: #664d03 !important;
        }

            .meta-hoy:hover {
                background-color: #ffeeBA !important;
                border-left-color: #c69500 !important;
            }

            .meta-hoy .task-description-bs, .meta-hoy small {
                color: #664d03 !important;
            }

        /* Estado: Meta A Tiempo (VERDE) */
        .meta-a-tiempo {
            border-left-color: #10703d !important;
            background-color: #d1e7dd !important;
            color: #0f5132 !important;
        }

            .meta-a-tiempo:hover {
                background-color: #bfdeca !important;
                border-left-color: #0d5c33 !important;
            }

            .meta-a-tiempo .task-description-bs, .meta-a-tiempo small {
                color: #0f5132 !important;
            }

        /* Estado: Meta Semanal Original (AZUL - para no finalizables) */
        .meta-semanal-original {
            border-left-color: #0b80b6 !important;
            background-color: #e0f2fe !important;
            color: #084298 !important;
        }

            .meta-semanal-original:hover {
                background-color: #bae6fd !important;
                border-left-color: #096a9a !important;
            }

            .meta-semanal-original .task-description-bs, .meta-semanal-original small {
                color: #084298 !important;
            }

        .meta-type-badge {
            font-size: 0.75em;
            font-weight: 600;
            padding: 0.3em 0.55em;
            border-radius: 0.25rem;
            min-width: 40px;
            text-align: center;
            margin-right: 0.75rem !important;
        }

        .task-description-bs {
            font-size: 0.9rem;
            flex-grow: 1;
            display: -webkit-box;
            -webkit-line-clamp: 2;
            -webkit-box-orient: vertical;
            overflow: hidden;
            text-overflow: ellipsis;
            line-height: 1.4;
        }

        .meta-item-base small {
            font-size: 0.78rem;
            white-space: nowrap;
            margin-left: auto;
            padding-left: 0.5rem;
        }

        .nav-tabs .nav-link.active {
            font-weight: 600;
            color: #F97316;
            border-color: #dee2e6 #dee2e6 #fff;
        }

        .nav-tabs .nav-link:hover:not(.active):not(.disabled) {
            border-color: #e9ecef #e9ecef #dee2e6;
            isolation: isolate;
            color: #495057;
            background-color: #f8f9fa;
        }

        .tab-content {
            border: 1px solid #dee2e6;
            border-top: none;
            padding: 1.5rem;
            border-bottom-left-radius: 0.375rem;
            border-bottom-right-radius: 0.375rem;
            background-color: #fff;
            min-height: 200px;
        }

        .section-metas { /* Contenedor para cada sección (Vencidas y Plan Semanal) */
            background-color: #fff;
            padding: 1.5rem;
            border-radius: 0.375rem;
            border: 1px solid #dee2e6;
            margin-bottom: 1.5rem; /* Espacio entre la sección de vencidas y el plan semanal */
        }


        .empty-data-panel .alert {
            font-size: 0.85rem;
            padding: 0.6rem 1rem;
            margin-top: 0; /* Ajustado ya que el panel tiene padding */
        }

        #modalDescriptionDisplay {
            font-size: 0.95rem;
            background-color: #f8f9fa;
            padding: 1rem;
            border-radius: 0.25rem;
            border: 1px solid #e9ecef;
            margin-bottom: 1.5rem;
            max-height: 200px;
            overflow-y: auto;
            white-space: pre-wrap;
        }

        .modal-footer .btn {
            min-width: 120px;
        }

        .feedback-message {
            display: none;
            margin-top: 1rem;
        }
    </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <asp:UpdatePanel ID="UpdatePanelModal" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
      <asp:HiddenField ID="hfSelectedMetaId" runat="server" Value="0" />
      <asp:HiddenField ID="hfSelectedIsFinalizable" runat="server" Value="false" />
      <%-- hfActiveWeekNumber almacenará "overdue", "1", "2", "3", "4", o "5" --%>
      <asp:HiddenField ID="hfActiveWeekNumber" runat="server" Value="1" />
      <input id="hdGuardado" type="hidden" runat="server" value="false" />

      <div class="card shadow-lg mx-auto" style="max-width: 60rem;">
        <div class="card-body p-4 p-md-5">
          <h1 class="card-title h2 mb-4 border-bottom pb-3">Gestión de Desempeño</h1>

          <asp:Literal ID="litFeedback" runat="server" EnableViewState="false"></asp:Literal>
          <div id="feedbackMessageJS" class="feedback-message alert" role="alert"></div>

          <%-- SECCIÓN DE METAS VENCIDAS (SIEMPRE VISIBLE) --%>
          <section id="metasVencidasSection" class="section-metas">
            <h2 class="h4 mb-3 text-danger">Metas Vencidas</h2>
            <asp:Repeater ID="rptMetasVencidas" runat="server" ItemType="Gestor_Desempeno.MetaIndividualInfoViewModel">
              <HeaderTemplate><div class="list-group"></HeaderTemplate> <%-- No mt-3 aquí, el padding de section-metas lo maneja --%>
              <ItemTemplate>
                <div class='list-group-item meta-item-base <%# Item.EstadoColorCss %>'
                  data-meta-id='<%# Item.Meta.IdMetaIndividual %>'
                  data-description='<%# Server.HtmlEncode(Item.Meta.Descripcion) %>'
                  data-es-finalizable='<%# Item.Meta.EsFinalizable.ToString().ToLower() %>'
                  data-meta-type='<%# Server.HtmlEncode(Item.Meta.NombreTipoObjetivo) %>'>
                  <span class="badge meta-type-badge" style='<%# Item.BadgeStyle %>'><%# Server.HtmlEncode(Item.Meta.NombreTipoObjetivo) %></span>
                  <span class="task-description-bs flex-grow-1"><%# Server.HtmlEncode(Item.Meta.Descripcion) %></span>
                  <small class="ms-2 text-muted"><%# Server.HtmlEncode(Item.MensajeTiempo) %></small>
                </div>
              </ItemTemplate>
              <FooterTemplate></div></FooterTemplate>
            </asp:Repeater>
            <asp:Panel ID="pnlEmptyMetasVencidas" runat="server" Visible="false" CssClass="empty-data-panel">
              <div class="alert alert-secondary small">No hay metas vencidas pendientes.</div>
            </asp:Panel>
          </section>

          <%-- SECCIÓN DE PLAN SEMANAL (PESTAÑAS) --%>
          <section id="planSemanalSection" class="mb-4">
            <h2 class="h4 mb-3 text-primary">Plan Semanal</h2>
            <nav>
              <div class="nav nav-tabs mb-0" id="nav-tab" role="tablist">
                <%-- El botón de Vencidas se ha movido --%>
                <button class="nav-link" id="nav-week-1-tab" data-bs-toggle="tab" data-bs-target="#nav-week-1" type="button" role="tab" aria-controls="nav-week-1" aria-selected="false" data-week-code-num="1">Semana 1</button>
                <button class="nav-link" id="nav-week-2-tab" data-bs-toggle="tab" data-bs-target="#nav-week-2" type="button" role="tab" aria-controls="nav-week-2" aria-selected="false" data-week-code-num="2">Semana 2</button>
                <button class="nav-link" id="nav-week-3-tab" data-bs-toggle="tab" data-bs-target="#nav-week-3" type="button" role="tab" aria-controls="nav-week-3" aria-selected="false" data-week-code-num="3">Semana 3</button>
                <button class="nav-link" id="nav-week-4-tab" data-bs-toggle="tab" data-bs-target="#nav-week-4" type="button" role="tab" aria-controls="nav-week-4" aria-selected="false" data-week-code-num="4">Semana 4</button>
                <button class="nav-link" id="nav-week-5-tab" data-bs-toggle="tab" data-bs-target="#nav-week-5" type="button" role="tab" aria-controls="nav-week-5" aria-selected="false" data-week-code-num="5">Semana 5</button>
              </div>
            </nav>
            <div class="tab-content" id="nav-tabContent">
              <%-- El panel de Vencidas se ha movido --%>

              <%-- Pestañas para Semanas 1 a 5 --%>
              <%-- Semana 1 --%>
              <div class="tab-pane fade" id="nav-week-1" role="tabpanel" aria-labelledby="nav-week-1-tab" tabindex="0">
                <asp:Repeater ID="rptSemana1" runat="server" ItemType="Gestor_Desempeno.MetaIndividualInfoViewModel">
                  <HeaderTemplate><div class="list-group mt-3"></HeaderTemplate>
                  <ItemTemplate>
                    <div class='list-group-item meta-item-base <%# Item.EstadoColorCss %>'
                      data-meta-id='<%# Item.Meta.IdMetaIndividual %>'
                      data-description='<%# Server.HtmlEncode(Item.Meta.Descripcion) %>'
                      data-es-finalizable='<%# Item.Meta.EsFinalizable.ToString().ToLower() %>'
                      data-meta-type='<%# Server.HtmlEncode(Item.Meta.NombreTipoObjetivo) %>'>
                      <span class="badge meta-type-badge" style='<%# Item.BadgeStyle %>'><%# Server.HtmlEncode(Item.Meta.NombreTipoObjetivo) %></span>
                      <span class="task-description-bs flex-grow-1">
                        <%# Item.Meta.EsFinalizable == true ? Server.HtmlEncode(Item.Meta.Descripcion) : Server.HtmlEncode(Item.Meta.DisplayTextLista2) %>
                      </span>
                      <small class="ms-2 text-muted"><%# Server.HtmlEncode(Item.MensajeTiempo) %></small>
                    </div>
                  </ItemTemplate>
                  <FooterTemplate></div></FooterTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlEmptySemana1" runat="server" Visible="false" CssClass="empty-data-panel">
                  <div class="alert alert-light small mt-3">No hay metas asignadas para esta semana.</div>
                </asp:Panel>
              </div>
              <%-- Semana 2 --%>
              <div class="tab-pane fade" id="nav-week-2" role="tabpanel" aria-labelledby="nav-week-2-tab" tabindex="0">
                <asp:Repeater ID="rptSemana2" runat="server" ItemType="Gestor_Desempeno.MetaIndividualInfoViewModel">
                  <HeaderTemplate><div class="list-group mt-3"></HeaderTemplate>
                  <ItemTemplate>
                    <div class='list-group-item meta-item-base <%# Item.EstadoColorCss %>'
                      data-meta-id='<%# Item.Meta.IdMetaIndividual %>'
                      data-description='<%# Server.HtmlEncode(Item.Meta.Descripcion) %>'
                      data-es-finalizable='<%# Item.Meta.EsFinalizable.ToString().ToLower() %>'
                      data-meta-type='<%# Server.HtmlEncode(Item.Meta.NombreTipoObjetivo) %>'>
                      <span class="badge meta-type-badge" style='<%# Item.BadgeStyle %>'><%# Server.HtmlEncode(Item.Meta.NombreTipoObjetivo) %></span>
                      <span class="task-description-bs flex-grow-1">
                        <%# Item.Meta.EsFinalizable == true ? Server.HtmlEncode(Item.Meta.Descripcion) : Server.HtmlEncode(Item.Meta.DisplayTextLista2) %>
                      </span>
                      <small class="ms-2 text-muted"><%# Server.HtmlEncode(Item.MensajeTiempo) %></small>
                    </div>
                  </ItemTemplate>
                  <FooterTemplate></div></FooterTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlEmptySemana2" runat="server" Visible="false" CssClass="empty-data-panel">
                  <div class="alert alert-light small mt-3">No hay metas asignadas para esta semana.</div>
                </asp:Panel>
              </div>
              <%-- Semana 3 --%>
              <div class="tab-pane fade" id="nav-week-3" role="tabpanel" aria-labelledby="nav-week-3-tab" tabindex="0">
                <asp:Repeater ID="rptSemana3" runat="server" ItemType="Gestor_Desempeno.MetaIndividualInfoViewModel">
                  <HeaderTemplate><div class="list-group mt-3"></HeaderTemplate>
                  <ItemTemplate>
                    <div class='list-group-item meta-item-base <%# Item.EstadoColorCss %>'
                      data-meta-id='<%# Item.Meta.IdMetaIndividual %>'
                      data-description='<%# Server.HtmlEncode(Item.Meta.Descripcion) %>'
                      data-es-finalizable='<%# Item.Meta.EsFinalizable.ToString().ToLower() %>'
                      data-meta-type='<%# Server.HtmlEncode(Item.Meta.NombreTipoObjetivo) %>'>
                      <span class="badge meta-type-badge" style='<%# Item.BadgeStyle %>'><%# Server.HtmlEncode(Item.Meta.NombreTipoObjetivo) %></span>
                      <span class="task-description-bs flex-grow-1">
                        <%# Item.Meta.EsFinalizable == true ? Server.HtmlEncode(Item.Meta.Descripcion) : Server.HtmlEncode(Item.Meta.DisplayTextLista2) %>
                      </span>
                      <small class="ms-2 text-muted"><%# Server.HtmlEncode(Item.MensajeTiempo) %></small>
                    </div>
                  </ItemTemplate>
                  <FooterTemplate></div></FooterTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlEmptySemana3" runat="server" Visible="false" CssClass="empty-data-panel">
                  <div class="alert alert-light small mt-3">No hay metas asignadas para esta semana.</div>
                </asp:Panel>
              </div>
              <%-- Semana 4 --%>
              <div class="tab-pane fade" id="nav-week-4" role="tabpanel" aria-labelledby="nav-week-4-tab" tabindex="0">
                <asp:Repeater ID="rptSemana4" runat="server" ItemType="Gestor_Desempeno.MetaIndividualInfoViewModel">
                  <HeaderTemplate><div class="list-group mt-3"></HeaderTemplate>
                  <ItemTemplate>
                    <div class='list-group-item meta-item-base <%# Item.EstadoColorCss %>'
                      data-meta-id='<%# Item.Meta.IdMetaIndividual %>'
                      data-description='<%# Server.HtmlEncode(Item.Meta.Descripcion) %>'
                      data-es-finalizable='<%# Item.Meta.EsFinalizable.ToString().ToLower() %>'
                      data-meta-type='<%# Server.HtmlEncode(Item.Meta.NombreTipoObjetivo) %>'>
                      <span class="badge meta-type-badge" style='<%# Item.BadgeStyle %>'><%# Server.HtmlEncode(Item.Meta.NombreTipoObjetivo) %></span>
                      <span class="task-description-bs flex-grow-1">
                        <%# Item.Meta.EsFinalizable == true ? Server.HtmlEncode(Item.Meta.Descripcion) : Server.HtmlEncode(Item.Meta.DisplayTextLista2) %>
                      </span>
                      <small class="ms-2 text-muted"><%# Server.HtmlEncode(Item.MensajeTiempo) %></small>
                    </div>
                  </ItemTemplate>
                  <FooterTemplate></div></FooterTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlEmptySemana4" runat="server" Visible="false" CssClass="empty-data-panel">
                  <div class="alert alert-light small mt-3">No hay metas asignadas para esta semana.</div>
                </asp:Panel>
              </div>
              <%-- Semana 5 --%>
              <div class="tab-pane fade" id="nav-week-5" role="tabpanel" aria-labelledby="nav-week-5-tab" tabindex="0">
                <asp:Repeater ID="rptSemana5" runat="server" ItemType="Gestor_Desempeno.MetaIndividualInfoViewModel">
                  <HeaderTemplate><div class="list-group mt-3"></HeaderTemplate>
                  <ItemTemplate>
                    <div class='list-group-item meta-item-base <%# Item.EstadoColorCss %>'
                      data-meta-id='<%# Item.Meta.IdMetaIndividual %>'
                      data-description='<%# Server.HtmlEncode(Item.Meta.Descripcion) %>'
                      data-es-finalizable='<%# Item.Meta.EsFinalizable.ToString().ToLower() %>'
                      data-meta-type='<%# Server.HtmlEncode(Item.Meta.NombreTipoObjetivo) %>'>
                      <span class="badge meta-type-badge" style='<%# Item.BadgeStyle %>'><%# Server.HtmlEncode(Item.Meta.NombreTipoObjetivo) %></span>
                      <span class="task-description-bs flex-grow-1">
                        <%# Item.Meta.EsFinalizable == true ? Server.HtmlEncode(Item.Meta.Descripcion) : Server.HtmlEncode(Item.Meta.DisplayTextLista2) %>
                      </span>
                      <small class="ms-2 text-muted"><%# Server.HtmlEncode(Item.MensajeTiempo) %></small>
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
        </div>
      </div>

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
                <label for="<%= modalObservacion.ClientID %>" class="form-label fw-semibold">Respuesta / Observación:</label>
                <textarea class="form-control" id="modalObservacion" rows="4" placeholder="Ingrese su respuesta u observación aquí..." runat="server"></textarea>
              </div>
              <div class="mt-3">
                <label for="<%= fileUploadControl.ClientID %>" class="form-label fw-semibold">Adjuntar Archivo (Opcional):</label>
                <asp:FileUpload ID="fileUploadControl" runat="server" CssClass="form-control" />
                <span class="form-text" id="modalFileNameDisplay">Ningún archivo seleccionado.</span>
              </div>
              <div id="ParaIconos" class="mt-3">
              </div>
            </div>
            <div class="modal-footer">
              <asp:Panel ID="pnlBotonesMetaFinalizable" runat="server" style="display:none;">
                <asp:Button ID="btModalGuardarAvance" runat="server" CssClass="btn btn-info me-2" Text="Guardar Avance" OnClick="btModalGuardarAvance_Click" />
                <asp:Button ID="btModalFinalizarMeta" runat="server" CssClass="btn btn-success" Text="Finalizar Meta" OnClick="btModalFinalizarMeta_Click" />
              </asp:Panel>
              <asp:Panel ID="pnlBotonMetaSemanal" runat="server" style="display:none;">
                <asp:Button ID="btModalGuardarSemanal" runat="server" CssClass="btn btn-primary" Text="Guardar Respuesta" OnClick="btModalGuardarSemanal_Click" />
              </asp:Panel>
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
            </div>
          </div>
        </div>
      </div>

    </ContentTemplate>
    <Triggers>
      <asp:PostBackTrigger ControlID="btModalGuardarAvance" />
      <asp:PostBackTrigger ControlID="btModalFinalizarMeta" />
      <asp:PostBackTrigger ControlID="btModalGuardarSemanal" />
    </Triggers>
  </asp:UpdatePanel>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">

    <script>
        // --- References ---
        const feedbackMessageJS = document.getElementById('feedbackMessageJS');
        const mainContentContainer = document.querySelector('.card-body'); // Se usará para delegar eventos de click en metas
        const hfSelectedMetaId = document.getElementById('<%= hfSelectedMetaId.ClientID %>');
        const hfSelectedIsFinalizable = document.getElementById('<%= hfSelectedIsFinalizable.ClientID %>');
        const hfActiveWeekNumber = document.getElementById('<%= hfActiveWeekNumber.ClientID %>'); // Almacena "overdue" o el número de semana "1"-"5"

        const respuestaModalElement = document.getElementById('respuestaModal');
        let respuestaModal = null; // Instancia del modal de Bootstrap

        // Función para inicializar el modal de Bootstrap
        function initializeModal() {
            if (respuestaModalElement && typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                if (!respuestaModal) {
                    respuestaModal = new bootstrap.Modal(respuestaModalElement);
                    console.log("Bootstrap Modal initialized.");
                }
            } else if (!respuestaModalElement) {
                console.error("Modal element (#respuestaModal) not found for initialization.");
            } else if (typeof bootstrap === 'undefined' || typeof bootstrap.Modal === 'undefined') {
                console.error("Bootstrap or Bootstrap.Modal is not defined. Modal cannot be initialized.");
            }
        }

        // Inicializar modal al cargar el DOM y después de cada PostBack asíncrono de ASP.NET
        document.addEventListener('DOMContentLoaded', initializeModal);
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initializeModal);
        }

        // Referencias a elementos dentro del modal
        const modalIcon = document.getElementById('modalIcon');
        const modalTitle = document.getElementById('modalTitle');
        const modalDescriptionDisplay = document.getElementById('modalDescriptionDisplay');
        const modalObservacionTextarea = document.getElementById('<%= modalObservacion.ClientID %>');
    const modalArchivoInput = document.getElementById('<%= fileUploadControl.ClientID %>');
    const modalFileNameDisplay = document.getElementById('modalFileNameDisplay');
   
    const pnlBotonesMetaFinalizable = document.getElementById('<%= pnlBotonesMetaFinalizable.ClientID %>');
    const pnlBotonMetaSemanal = document.getElementById('<%= pnlBotonMetaSemanal.ClientID %>');

    const lucideBaseUrl = 'https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/';

    // Muestra un mensaje de feedback (éxito o error)
    function showFeedback(message, type = 'success') {
      if (!feedbackMessageJS) return;
      feedbackMessageJS.innerHTML = '';
      const textNode = document.createTextNode(message);
      feedbackMessageJS.appendChild(textNode);
      feedbackMessageJS.className = `feedback-message alert alert-${type} alert-dismissible fade show`;
      feedbackMessageJS.style.display = 'block';
      feedbackMessageJS.setAttribute('role', 'alert');
     
      if (!feedbackMessageJS.querySelector('.btn-close')) {
        const b = document.createElement('button'); b.type = 'button'; b.className = 'btn-close';
        b.setAttribute('data-bs-dismiss', 'alert'); b.setAttribute('aria-label', 'Cerrar');
        feedbackMessageJS.appendChild(b);
      }
      setTimeout(() => {
        if (typeof bootstrap !== 'undefined' && bootstrap.Alert && typeof bootstrap.Alert.getOrCreateInstance === 'function') {
          const alertInstance = bootstrap.Alert.getOrCreateInstance(feedbackMessageJS);
          if (alertInstance) {
            try { alertInstance.close(); } catch(e) { feedbackMessageJS.style.display = 'none'; }
          } else { feedbackMessageJS.style.display = 'none'; }
        } else {
          feedbackMessageJS.style.display = 'none';
        }
      }, 7000);
    }

    // Resetea los campos del modal a su estado inicial
    function resetModalFields() {
      if (modalObservacionTextarea) modalObservacionTextarea.value = '';
      if (modalArchivoInput) {
        modalArchivoInput.value = null;
        if (typeof $ === 'function' && $(modalArchivoInput).data('bs.fileinput')) {
          $(modalArchivoInput).fileinput('clear');
        }
      }
      if (modalFileNameDisplay) modalFileNameDisplay.textContent = 'Ningún archivo seleccionado.';
      if (hfSelectedMetaId) hfSelectedMetaId.value = '0';
      if (hfSelectedIsFinalizable) hfSelectedIsFinalizable.value = 'false';
      if (modalDescriptionDisplay) modalDescriptionDisplay.textContent = 'Cargando descripción...';
      var contenedorDocumentos = document.getElementById('ParaIconos');
      if(contenedorDocumentos) contenedorDocumentos.innerHTML = '<p class="mt-3 small text-muted">No hay documentos adjuntos.</p>';

      if(pnlBotonesMetaFinalizable) pnlBotonesMetaFinalizable.style.display = 'none';
      if(pnlBotonMetaSemanal) pnlBotonMetaSemanal.style.display = 'none';
    }

    // Muestra y configura el modal de respuesta
    function showRespuestaModal(title, descriptionMeta, iconName = 'file-text', metaId = 0, esFinalizable = false, existingObservacion = '', datosDocs = '') {
      console.log("Attempting to show modal. esFinalizable:", esFinalizable, "MetaID:", metaId, "DatosDocs:", datosDocs);

      if (!respuestaModalElement || !modalTitle || !modalDescriptionDisplay || !modalIcon || !modalObservacionTextarea || !hfSelectedMetaId || !hfSelectedIsFinalizable) {
        console.error("Modal core elements not found. Cannot show modal.");
        showFeedback(`No se pueden mostrar los detalles. Elementos del modal no encontrados.`, 'danger');
        return;
      }
     
      if (!respuestaModal) {
        console.error("Bootstrap Modal object (respuestaModal) is not initialized. Attempting to initialize now.");
        initializeModal();
        if (!respuestaModal) {
          showFeedback('Error crítico: El diálogo modal no se pudo inicializar.', 'danger');
          return;
        }
      }

      resetModalFields();

      modalTitle.textContent = title || 'Registrar Respuesta';
      modalDescriptionDisplay.textContent = descriptionMeta || 'No hay descripción disponible.';
      if (typeof lucideBaseUrl !== 'undefined' && modalIcon) {
        modalIcon.src = `${lucideBaseUrl}${iconName}.svg`;
        modalIcon.alt = `Icono ${title || 'respuesta'}`;
      }

      hfSelectedMetaId.value = metaId.toString();
      hfSelectedIsFinalizable.value = esFinalizable.toString().toLowerCase();

      if (modalObservacionTextarea) modalObservacionTextarea.value = existingObservacion;

      if (pnlBotonesMetaFinalizable && pnlBotonMetaSemanal) {
        if (esFinalizable) {
          pnlBotonesMetaFinalizable.style.display = 'flex';
          pnlBotonMetaSemanal.style.display = 'none';
        } else {
          pnlBotonesMetaFinalizable.style.display = 'none';
          pnlBotonMetaSemanal.style.display = 'block';
        }
      } else {
        console.error("Paneles de botones del modal (pnlBotonesMetaFinalizable o pnlBotonMetaSemanal) no encontrados en el DOM.");
      }
     
      var contenedorDocumentos = document.getElementById('ParaIconos');
      if (contenedorDocumentos) {
        contenedorDocumentos.innerHTML = '';
        try {
          let htmlDocumentos = "";
          if (datosDocs && datosDocs.trim() !== "") {
            console.log("Processing datosDocs:", datosDocs);
            htmlDocumentos = '<h6 class="mt-3 mb-2">Documentos Adjuntos:</h6><div class="d-flex flex-wrap">';
            const documentosArray = datosDocs.split('|');
            documentosArray.forEach(docEntry => {
              if (docEntry) {
                const partes = docEntry.split(',');
                if (partes.length >= 1) {
                  const urli = partes[0];
                  const nombre = partes.length > 1 ? partes.slice(1).join(',') : 'Documento';
                  htmlDocumentos += `
                    <a href="javascript:void(0);"
                     class="btn btn-sm btn-outline-secondary me-2 mb-2 doc-link-popup"
                     style="font-size: 0.85em; line-height: 1.4;"
                     data-doc-url="${urli}"
                     title="Ver documento: ${nombre}">
                      <img src="${lucideBaseUrl}file-text.svg" alt="doc" style="width:1em; height:1em; vertical-align: text-bottom; margin-right: 0.3rem;"/>
                      <span>${nombre}</span>
                    </a>`;
                } else {
                  console.warn("Entrada de documento inválida (sin partes suficientes):", docEntry);
                }
              }
            });
            htmlDocumentos += '</div>';
            contenedorDocumentos.insertAdjacentHTML('beforeend', htmlDocumentos);
          } else {
            contenedorDocumentos.insertAdjacentHTML('beforeend', '<p class="mt-3 small text-muted">No hay documentos adjuntos.</p>');
          }
        } catch (exDoc) {
          console.error("Error procesando datos de documentos:", exDoc, "DatosDocs problemáticos:", datosDocs);
          contenedorDocumentos.innerHTML = '<p class="mt-3 small text-danger">Error al cargar la lista de documentos adjuntos.</p>';
        }
      } else {
        console.error("Contenedor de documentos 'ParaIconos' no encontrado.");
      }

      try {
        if (respuestaModal && typeof respuestaModal.show === 'function') {
          console.log("Calling respuestaModal.show()");
          respuestaModal.show();
        } else {
          console.error("'respuestaModal.show()' no es una función o respuestaModal no está definido. Modal no se mostrará.");
          showFeedback("Error: No se pudo mostrar el diálogo modal. Verifique la consola para más detalles.", "danger");
        }
      } catch (exShow) {
        console.error("Excepción al intentar mostrar el modal:", exShow);
        showFeedback("Error inesperado al intentar mostrar el diálogo modal.", "danger");
      }
    }

    // Obtiene el número de semana del mes para una fecha dada (1 a 5)
    function GetWeekOfMonthJS(date) {
      const d = new Date(date.getFullYear(), date.getMonth(), date.getDate());
      const firstDayOfMonth = new Date(d.getFullYear(), d.getMonth(), 1);
      const dayOfWeekOfFirst = firstDayOfMonth.getDay() === 0 ? 7 : firstDayOfMonth.getDay(); // Lunes=1, Domingo=7
      const dayInMonth = d.getDate();
      const weekNumber = Math.ceil((dayInMonth + (dayOfWeekOfFirst - 1)) / 7);
      return Math.min(weekNumber, 5); // Limita a un máximo de 5 semanas
    }

    // Establece la pestaña activa (basada en la semana actual o el hash de la URL)
    function setActiveWeekTab() {
      try {
        const today = new Date();
        const currentWeekNumberInMonth = GetWeekOfMonthJS(today);
        let activeTabValue = currentWeekNumberInMonth.toString(); // Valor para hfActiveWeekNumber
        let targetTabButtonId = `nav-week-${currentWeekNumberInMonth}-tab`;
       
        const currentHash = window.location.hash;
        let preSelectedTab = null;

        if (currentHash && currentHash !== '#nav-overdue') { // Ignorar hash de vencidas ya que no es pestaña
          preSelectedTab = document.querySelector(`.nav-link[data-bs-target="${currentHash}"]`);
        }
       
        if (preSelectedTab) { // Si hay un hash válido para una pestaña semanal
          targetTabButtonId = preSelectedTab.id;
          activeTabValue = preSelectedTab.dataset.weekCodeNum || activeTabValue;
        }
       
        if (hfActiveWeekNumber) {
          hfActiveWeekNumber.value = activeTabValue; // Setea el hidden field para la semana
        }

        const allTabs = document.querySelectorAll('#nav-tab .nav-link');
        const allPanes = document.querySelectorAll('#nav-tabContent .tab-pane');
        allTabs.forEach(t => { t.classList.remove('active'); t.setAttribute('aria-selected', 'false');});
        allPanes.forEach(p => p.classList.remove('show', 'active'));

        const targetTabButton = document.getElementById(targetTabButtonId);
        const targetPaneId = targetTabButton ? targetTabButton.getAttribute('data-bs-target') : null;
        const targetPane = targetPaneId ? document.querySelector(targetPaneId) : null;

        if (targetTabButton && targetPane) {
          targetTabButton.classList.add('active');
          targetTabButton.setAttribute('aria-selected', 'true');
          targetPane.classList.add('show', 'active');
        } else {// Fallback si la pestaña calculada o por hash no existe/es inválida
          const firstAvailableWeekTab = document.querySelector('#nav-tab .nav-link:not(.disabled)');
          if(firstAvailableWeekTab){
            firstAvailableWeekTab.classList.add('active');
            firstAvailableWeekTab.setAttribute('aria-selected', 'true');
            const firstPaneId = firstAvailableWeekTab.getAttribute('data-bs-target');
            if(firstPaneId) document.querySelector(firstPaneId).classList.add('show', 'active');
            if (hfActiveWeekNumber) {
             hfActiveWeekNumber.value = firstAvailableWeekTab.dataset.weekCodeNum || "1";
            }
          }
        }
      } catch (error) {
        console.error("Error setting active week tab:", error);
      }
    }
   
    // Deshabilita la pestaña "Semana 5" si el mes actual no tiene 5 semanas completas
    function disableWeek5IfNeeded() {
      try {
        const today = new Date();
        const year = today.getFullYear();
        const month = today.getMonth();
        const daysInMonth = new Date(year, month + 1, 0).getDate();
        const week5TabButton = document.getElementById('nav-week-5-tab');
        if (week5TabButton) {
          if (daysInMonth < 29) { // Asume que se necesita al menos el día 29 para justificar una semana 5
            week5TabButton.classList.add('disabled','pe-none');
            week5TabButton.setAttribute('aria-disabled', 'true');
            week5TabButton.setAttribute('tabindex', '-1');
          } else {
            week5TabButton.classList.remove('disabled','pe-none');
            week5TabButton.setAttribute('aria-disabled', 'false');
            week5TabButton.removeAttribute('tabindex');
          }
        }
      } catch (error) { console.error("Error en disableWeek5IfNeeded:", error); }
    }

    // Genera el código de semana en formato WMMYYYY para el backend
    function getCodigoSemanaServidorFormato(weekNumberStr) {
      if (!weekNumberStr || isNaN(parseInt(weekNumberStr,10))) return null;
      const today = new Date();
      const month = today.getMonth() + 1;
      const year = today.getFullYear();
      const monthStr = month < 10 ? '0' + month : month.toString();
      return `${weekNumberStr}${monthStr}${year}`;
    }

    // Manejador de eventos para clicks en los ítems de metas (delegado al contenedor principal)
    if (mainContentContainer) {
      mainContentContainer.addEventListener('click', function (event) {
        const clickedListItem = event.target.closest('.meta-item-base');
        if (clickedListItem) {
          event.stopPropagation(); // Prevenir que el evento se propague más allá
          const descriptionMeta = clickedListItem.dataset.description || 'Detalle no encontrado.';
          const metaId = parseInt(clickedListItem.dataset.metaId || '0', 10);
          const esFinalizable = clickedListItem.dataset.esFinalizable === 'true';
          let modalTitleText = esFinalizable ? 'Registrar Avance / Finalizar Meta' : 'Registrar Respuesta Semanal';
          let modalIconName = esFinalizable ? 'edit-3' : 'calendar-check-2';
         
          let activeValueForHiddenField = ""; // Para hfActiveWeekNumber
          let codigoSemanaParaDetalles = null; // Para PageMethods.GetRespuestaDetalles

          if (clickedListItem.closest('#metasVencidasSection')) { // Si el click fue en la sección de vencidas
            activeValueForHiddenField = "overdue";
            codigoSemanaParaDetalles = "0000000"; // Código especial para vencidas en backend
          } else { // Si el click fue en una pestaña semanal
            const activeTabButton = document.querySelector('#nav-tab .nav-link.active');
            if (activeTabButton && activeTabButton.dataset.weekCodeNum) {
              activeValueForHiddenField = activeTabButton.dataset.weekCodeNum;
              codigoSemanaParaDetalles = getCodigoSemanaServidorFormato(activeValueForHiddenField);
            } else {
              console.warn("No se pudo determinar la pestaña activa o su número de semana. Usando '1' por defecto.");
              activeValueForHiddenField = "1"; // Fallback
              codigoSemanaParaDetalles = getCodigoSemanaServidorFormato("1");
            }
          }

          if (hfActiveWeekNumber) {
            hfActiveWeekNumber.value = activeValueForHiddenField;
            console.log("hfActiveWeekNumber set to: " + activeValueForHiddenField);
          }

          if (metaId > 0) {
            if (typeof PageMethods !== 'undefined' && PageMethods.GetRespuestaDetalles) {
              PageMethods.GetRespuestaDetalles(metaId, esFinalizable, codigoSemanaParaDetalles,
                (response) => {
                  let existingObservacion = ''; let datosDocsDesdeServidor = '';
                  if (response && response.success && response.data) {
                    existingObservacion = response.data.ObservacionGuardada || '';
                    datosDocsDesdeServidor = response.data.DatosDocs || '';
                    console.log("DatosDocs recibidos del servidor:", datosDocsDesdeServidor);
                  } else if (response && !response.success) {
                    console.warn("GetRespuestaDetalles no tuvo éxito:", response.message);
                    showFeedback(response.message || 'No se pudieron cargar datos previos.', 'warning');
                  }
                  showRespuestaModal(modalTitleText, descriptionMeta, modalIconName, metaId, esFinalizable, existingObservacion, datosDocsDesdeServidor);
                },
                (error) => {
                  console.error("Error AJAX GetRespuestaDetalles:", error);
                  showFeedback('Error al cargar datos de respuesta previa.', 'danger');
                  showRespuestaModal(modalTitleText, descriptionMeta, modalIconName, metaId, esFinalizable, '', ''); // Mostrar modal incluso si falla carga de datos previos
                }
              );
            } else {
              console.error("PageMethods o PageMethods.GetRespuestaDetalles no definido.");
              showFeedback('Error de configuración para cargar detalles.', 'danger');
              showRespuestaModal(modalTitleText, descriptionMeta, modalIconName, metaId, esFinalizable, '', '');
            }
          } else {
            showFeedback('Error: No se pudo identificar la meta.', 'danger');
          }
        }
      });
    }

    // Manejador para abrir documentos adjuntos en una nueva ventana (pop-up)
    if (respuestaModalElement) {
      respuestaModalElement.addEventListener('click', function(event) {
        const linkClickeado = event.target.closest('.doc-link-popup');
        if (linkClickeado) {
          event.preventDefault();
          const urlParaAbrir = linkClickeado.dataset.docUrl;
          if (urlParaAbrir) {
            const nuevaVentana = window.open(urlParaAbrir, 'visorDocumento', 'width=800,height=600,resizable=yes,scrollbars=yes,status=yes,noopener,noreferrer');
            if (!nuevaVentana) { 
              // No usar alert() aquí, usar el feedback visual si es posible o un mensaje en consola.
              console.warn('Bloqueador de pop-ups activado. Permita pop-ups para este sitio.');
              showFeedback('Bloqueador de pop-ups activado. Permita pop-ups para ver el documento.', 'warning');
            }
          } else {
            console.error('Error: URL del documento no encontrada.');
            showFeedback('Error: URL del documento no encontrada.', 'danger');
          }
        }
      });
    }

    // Actualiza el nombre del archivo seleccionado en el input de carga
    if (modalArchivoInput && modalFileNameDisplay) {
      modalArchivoInput.addEventListener('change', function(event) {
        modalFileNameDisplay.textContent = event.target.files.length > 0 ? event.target.files[0].name : 'Ningún archivo seleccionado.';
      });
    }
   
    // Eventos que se ejecutan una vez que el DOM está completamente cargado
    document.addEventListener('DOMContentLoaded', () => {
      initializeModal();// Asegura que el modal esté listo

      if (respuestaModalElement) { // Resetea campos cuando el modal se oculta
        respuestaModalElement.addEventListener('hidden.bs.modal', resetModalFields);
      }
      disableWeek5IfNeeded();// Deshabilita Semana 5 si es necesario
      setActiveWeekTab(); // Establece la pestaña activa

      // Actualiza el hidden field hfActiveWeekNumber cuando se cambia de pestaña
      const allTabButtons = document.querySelectorAll('#nav-tab .nav-link[data-bs-toggle="tab"]');
      allTabButtons.forEach(tabButton => {
        tabButton.addEventListener('shown.bs.tab', function (event) {
          let activeValueForHiddenField = "";
          // La pestaña 'overdue' ya no existe, así que no se necesita esa condición aquí.
          activeValueForHiddenField = event.target.dataset.weekCodeNum;
         
          if (hfActiveWeekNumber && activeValueForHiddenField) {
            hfActiveWeekNumber.value = activeValueForHiddenField;
            console.log("Tab changed, hfActiveWeekNumber set to: " + activeValueForHiddenField);
          }
        });
      });

      // Si se guardó algo (indicado por hdGuardado), oculta el modal y resetea la bandera.
      const hdGuardadoFlag = document.getElementById('<%= hdGuardado.ClientID %>');
        if (hdGuardadoFlag && hdGuardadoFlag.value === "true") {
            if (respuestaModal && typeof respuestaModal.hide === 'function') {
                respuestaModal.hide();
            }
            hdGuardadoFlag.value = "false"; // Reset flag
        }
    });
    </script>

</asp:Content>
