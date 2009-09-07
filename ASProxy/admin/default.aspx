<%@ Page Language="C#" MasterPageFile="~/admin/AdminUI.master" Title="Default Page" %>

<script runat="server">

</script>

<asp:Content ID="plhHead" ContentPlaceHolderID="PageHead" Runat="Server">
     <style type="text/css">
     <!--[if IE 6]>
     #IE6Support{ display:block !important;}
     <![endif]-->
     </style>
</asp:Content>
<asp:Content ID="plhHeader" ContentPlaceHolderID="Header" Runat="Server">
</asp:Content>
<asp:Content ID="plhContent" ContentPlaceHolderID="Content" Runat="Server">
    <div id="IE6Support" style="display:none; color:Red; font-weight:bold;">
        Warning, IE6 is not supported by ASProxy Administration UI. Please use
        <a href="http://www.getfirefox.com/" target="_blank">Firefox</a> instead.</div>
</asp:Content>

