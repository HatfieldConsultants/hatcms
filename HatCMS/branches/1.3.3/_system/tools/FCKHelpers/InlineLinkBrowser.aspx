<%@ Page language="c#" Codebehind="InlineLinkBrowser.aspx.cs" AutoEventWireup="True" Inherits="HatCMS.WebEditor.Helpers.InlineLinkBrowser" %>
<html>
<head>
<style>
body { margin: 0px; padding: 0px; }
</style>
</head>

<body>
<form runat="server" id="form">
<asp:treeview id="tv_Pages" runat="server" expanddepth="1" showlines="True" OnTreeNodePopulate="tv_Pages_TreeNodePopulate"></asp:treeview>
</form>
</body>
</html>