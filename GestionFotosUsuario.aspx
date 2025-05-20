<%-- 
  Página para la Gestión de Fotos de Perfil de Usuarios.
  Muestra un listado de usuarios y permite editar su foto de perfil.
  Incluye filtro por nombre.
--%>
<%@ Page Title="Gestión de Fotos de Usuario" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="GestionFotosUsuario.aspx.cs" Inherits="Gestor_Desempeno.GestionFotosUsuario" %>

<%-- Este ContentPlaceHolder es para estilos o metatags en el <head> --%>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .profile-pic-grid {
            max-width: 50px;
            max-height: 50px;
            border-radius: 50%;
            object-fit: cover; 
        }
        .profile-pic-edit-section { /* Estilo para la foto actual en la sección de edición */
            max-width: 150px;
            max-height: 150px;
            border-radius: 8px;
            display: block;
            margin-left: auto;
            margin-right: auto;
            margin-bottom: 10px;
            object-fit: cover;
        }
        .preview-pic-edit-section { /* Estilo para la previsualización en la sección de edición */
            max-width: 150px;
            max-height: 150px;
            border-radius: 8px;
            display: block;
            margin-left: auto;
            margin-right: auto;
            margin-top: 10px;
            border: 1px solid #ddd;
            object-fit: cover;
        }
        .edit-section-centered-text {
            text-align: center;
        }
        .filter-container {
            margin-bottom: 20px;
            padding: 15px;
            background-color: #f8f9fa;
            border-radius: 8px;
        }
        .edit-photo-section { /* Contenedor para la sección de edición */
            margin-bottom: 20px; /* Ajustado margen inferior ya que ahora está arriba del filtro */
            padding: 20px;
            border: 1px solid #ddd;
            border-radius: 8px;
            background-color: #f9f9f9;
        }
    </style>
</asp:Content>

<%-- Este ContentPlaceHolder es para el contenido principal de la página --%>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="container mt-4">
        <h2 class="mb-4">Gestión de Fotos de Perfil de Usuarios</h2>

        <asp:Panel ID="pnlMensaje" runat="server" Visible="false" CssClass="mb-3">
            <button type="button" class="btn-close float-end" data-bs-dismiss="alert" aria-label="Close" onclick="document.getElementById('<%= pnlMensaje.ClientID %>').style.display='none'; return false;"></button>
            <asp:Label ID="lblMensaje" runat="server" Text=""></asp:Label>
        </asp:Panel>

        <%-- Sección para Editar Foto (AHORA ARRIBA DEL FILTRO) --%>
        <asp:Panel ID="pnlEditarFotoSeccion" runat="server" Visible="false" CssClass="edit-photo-section">
            <div class="edit-section-centered-text">
                <h5 class="mb-3">Editar Foto de Perfil</h5>
                <asp:HiddenField ID="hfIdUsuarioModal" runat="server" />
                <p><strong>Usuario:</strong> <asp:Label ID="lblIdUsuarioModal" runat="server"></asp:Label></p>
                <p><strong>Nombre:</strong> <asp:Label ID="lblNombreUsuarioModal" runat="server"></asp:Label></p>
                
                <h6>Foto Actual:</h6>
                <asp:Image ID="imgFotoActualModal" runat="server" CssClass="profile-pic-edit-section" AlternateText="Foto Actual" />

                <hr />
                <h6>Seleccionar Nueva Foto:</h6>
                <asp:FileUpload ID="fuNuevaFotoModal" runat="server" CssClass="form-control mb-2" accept=".png,.jpg,.jpeg" />
                <asp:RegularExpressionValidator ID="revTipoArchivo" runat="server"
                    ControlToValidate="fuNuevaFotoModal"
                    ValidationExpression="^.*\.([jJ][pP][gG]|[jJ][pP][eE][gG]|[pP][nN][gG])$"
                    ErrorMessage="Solo se permiten archivos .png, .jpg o .jpeg." Display="Dynamic" CssClass="text-danger" />
                
                <h6>Previsualización Nueva Foto:</h6>
                <asp:Image ID="imgNuevaFotoPreviewModal" runat="server" CssClass="preview-pic-edit-section" Visible="false" />
            </div>
            <div class="mt-3 text-center">
                <asp:Button ID="btnGuardarFotoSeccion" runat="server" Text="Guardar Foto" CssClass="btn btn-primary me-2" OnClick="btnGuardarFotoSeccion_Click" />
                <asp:Button ID="btnCancelarEdicionSeccion" runat="server" Text="Cancelar" CssClass="btn btn-secondary" OnClick="btnCancelarEdicionSeccion_Click" CausesValidation="false"/>
            </div>
        </asp:Panel>
        <%-- Fin Sección para Editar Foto --%>

        <div class="filter-container">
            <div class="row g-3 align-items-center">
                <div class="col-auto">
                    <label for="<%= txtFiltroNombre.ClientID %>" class="col-form-label">Filtrar por Nombre:</label>
                </div>
                <div class="col">
                    <asp:TextBox ID="txtFiltroNombre" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-auto">
                    <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn btn-info" OnClick="btnFiltrar_Click" />
                </div>
                <div class="col-auto">
                    <asp:Button ID="btnLimpiarFiltro" runat="server" Text="Limpiar" CssClass="btn btn-secondary" OnClick="btnLimpiarFiltro_Click" />
                </div>
            </div>
        </div>

        <asp:GridView ID="gvUsuarios" runat="server" AutoGenerateColumns="False" CssClass="table table-hover table-striped"
            DataKeyNames="ID_USUARIO" OnRowCommand="gvUsuarios_RowCommand" AllowPaging="true" PageSize="10" 
            OnPageIndexChanging="gvUsuarios_PageIndexChanging" OnRowDataBound="gvUsuarios_RowDataBound">
            <Columns>
                <asp:BoundField DataField="ID_USUARIO" HeaderText="ID Usuario" SortExpression="ID_USUARIO" />
                <asp:TemplateField HeaderText="Nombre Completo" SortExpression="NOMBRE_COMPLETO">
                    <ItemTemplate>
                        <%# Eval("NOMBRE_COMPLETO") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Foto Actual">
                    <ItemTemplate>
                        <asp:Image ID="imgFotoGrid" runat="server" CssClass="profile-pic-grid" 
                                     AlternateText="Foto de perfil" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <asp:Button ID="btnEditarFoto" runat="server" Text="Editar Foto" CssClass="btn btn-sm btn-primary"
                            CommandName="EditarFoto" CommandArgument='<%# Eval("ID_USUARIO") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle CssClass="pagination-ys" />
            <EmptyDataTemplate>
                <div class="alert alert-info" role="alert">
                    No se encontraron usuarios que coincidan con el filtro.
                </div>
            </EmptyDataTemplate>
        </asp:GridView>

    </div>

</asp:Content>

<%-- ESTE ES EL LUGAR CORRECTO PARA LOS SCRIPTS DE LA PÁGINA --%>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
     <script type="text/javascript">
         //<![CDATA[
         function previewImage(fileInputId, previewImageId) {
             const fileInput = document.getElementById(fileInputId);
             const previewImage = document.getElementById(previewImageId);

             if (fileInput.files && fileInput.files[0]) {
                 const allowedTypes = ['image/png', 'image/jpeg'];
                 if (!allowedTypes.includes(fileInput.files[0].type)) {
                     alert('Tipo de archivo no permitido. Solo PNG o JPG.');
                     fileInput.value = ''; // Limpiar el input
                     previewImage.setAttribute('src', '#');
                     previewImage.style.display = 'none';
                     return;
                 }

                 const reader = new FileReader();
                 reader.onload = function (e) {
                     previewImage.setAttribute('src', e.target.result);
                     previewImage.style.display = 'block';
                 }
                 reader.readAsDataURL(fileInput.files[0]);
             } else {
                 previewImage.setAttribute('src', '#');
                 previewImage.style.display = 'none';
             }
         }

         function setupFileUploadPreview() {
             try {
                 const fu = document.getElementById('<%= fuNuevaFotoModal.ClientID %>');
                if (fu) {
                    fu.onchange = function () {
                        previewImage('<%= fuNuevaFotoModal.ClientID %>', '<%= imgNuevaFotoPreviewModal.ClientID %>');
                     };
                 } else {
                     // console.warn("FileUpload 'fuNuevaFotoModal' no encontrado para setupFileUploadPreview.");
                 }
             } catch (e) {
                 // console.error("Error en setupFileUploadPreview:", e);
             }
         }

         // Asegurar que el DOM esté listo antes de adjuntar eventos
         if (document.readyState === "loading") {
             document.addEventListener("DOMContentLoaded", setupFileUploadPreview);
         } else {
             // DOMContentLoaded ya ha disparado
             setupFileUploadPreview();
         }
        //]]>
     </script>
</asp:Content>
