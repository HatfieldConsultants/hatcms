<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InlineImageBrowser2.aspx.cs" Inherits="HatCMS.WebEditor.Helpers.InlineImageBrowser2" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Inline Image Browser</title>
    <style>
    body { margin: 0px; padding: 0px; }
    form { margin: 0px; padding: 0px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
<table border="1" cellpadding="2" cellspacing="0" width="100%" style="height: 100%;">
<tr>
<td valign="top" width="200">
<div style="height: <%= treeHeight() %>; width: 200px; overflow: scroll;">
    <asp:TreeView ID="FolderTreeView" runat="server" ExpandDepth="1" ShowLines="True" OnTreeNodePopulate="FolderTreeView_TreeNodePopulate" OnSelectedNodeChanged="FolderTreeView_SelectedNodeChanged">
        <SelectedNodeStyle BackColor="Black" Font-Bold="True" ForeColor="White" />
        <NodeStyle Font-Names="arial" Font-Size="9pt" ForeColor="Black" />
    </asp:TreeView>
</div>
</td>
<td valign="top">
<div style="height: <%= treeHeight() %>; width:100%; overflow: scroll;">
    <asp:PlaceHolder ID="ph_ImagePanel" runat="server"></asp:PlaceHolder>
</div>
    </td>
</tr>
<tr><td style="font-family: Arial; font-size: 9pt;">Create sub-folder:&nbsp;
    <asp:TextBox ID="tb_subFolder" runat="server" MaxLength="255" Width="150px"></asp:TextBox>&nbsp;<asp:Button
        ID="b_CreateSubFolder" runat="server" Text="create" OnClick="b_CreateSubFolder_Click" /></td>
<td style="font-family: Arial; font-size: 9pt;">Upload an image: <input type="file" runat="server" id="fileUpload" />&nbsp;<asp:Button
        ID="b_DoFileUpload" runat="server" Text="Upload" OnClick="b_DoFileUpload_Click" /></td></tr>
</table>
    </form>
</body>
</html>
