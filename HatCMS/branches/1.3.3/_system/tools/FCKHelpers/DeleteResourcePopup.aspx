<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DeleteResourcePopup.aspx.cs" Inherits="HatCMS.FCKHelpers.DeleteResourcePopup" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head runat="server">
    <title>Delete Resource - <%= FileUrl %></title>
</head>
<body>
    <% if (ResourceHasBeenDeleted)
       { %>
       <strong><%= FileUrl %> has been deleted.</strong>
       <p>
       <input type="button" value="close" onclick="opener.location.href = opener.location.href; window.close();" />
       </p>
       
    <% } else { %>
    <form id="form1" runat="server">
    <div>
    <strong>Do you really want to delete <%= FileUrl %>?</strong>
    <table width="100%" border="0">
    <tr>
        <td align="left"><input type="button" value="No - close" onclick="window.close();" /></td>
        <td align="right">
            <asp:Button ID="b_DoDelete" runat="server" OnClick="b_DoDelete_Click" Text="Yes - Delete" /></td>
    </tr>
    </table>
    </div>
    <input type="hidden" name="FileUrl" value="<%= FileUrl %>" />
    </form>
    <% OutputPageLinks(); %>
    <% }  %>
</body>
</html>
