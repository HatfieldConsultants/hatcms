<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SingleImageEditor.aspx.cs" Inherits="HatCMS.FCKHelpers.SingleImageEditor" %>
<html>
<head runat="server">
    <title>Choose an Image</title>
    <link href="../../js/_system/FCKeditor/editor/skins/default/fck_dialog.css" type="text/css" rel="stylesheet">
    <script type="text/javascript" language="javascript">
    //-- function called by embedded iframe
		function selectImage(title,url,origWidth,origHeight,thumbUrlFormat)
		{			
			document.getElementById("selectedImageName").value = title;
			document.getElementById("selectedImageUrl").value = url;			
			return false;
		}		
		
		function OkClicked()
		{
		    if (!window.opener)
		    {
		        alert("Could not find window that opened this dialog - could not update values!!");
		        return;
		    }
		    if (!window.opener.<%= formName %>UpdateDisplay)
		    {
		        alert("Could not find update display function - could not update values!!");
		        return;
		    }
		    
		    window.opener.document.getElementById('<%= formName %>ImagePath').value = document.getElementById("selectedImageUrl").value;		    
		    		    
		    window.opener.<%= formName %>UpdateDisplay();
		    window.close();
		}
    
    </script>
</head>
<body style="padding-top: 0px; margin-top: 0px;">
<table border="0" cellpadding="0" cellspacing="0">
<tr>
    <td id="TitleArea" class="PopupTitle PopupTitleBorder">Select an Image</td>
</tr>
<tr>
    <td><iframe id="InlineImageBrowserIFrame" width="650" height="410px" frameborder="1" scrolling="auto" src="../FCKHelpers/InlineImageBrowser2.aspx?SelImagePath=<%= SelImagePath %>" ></iframe></td>
</tr>
<tr>
    <td><table width="100%" cellpadding="0" cellspacing="0">
        <tr>
            <td style="font-size: 10pt;">Selected Image:</td><td><input value="<%= System.IO.Path.GetFileName(currentSingleImage.ImagePath) %>" readonly="readonly" type="text" size="60" id="selectedImageName" /> <% OutputDeleteFileLink(); %> </td>
         </tr>
    </table>
    </td>
</tr>
<tr>
<td>
<input type="button" value="OK" onclick="OkClicked();" />
<input type="button" value="Cancel" onclick="window.close();" />
<input type="hidden" id="selectedImageUrl" value="<%= (currentSingleImage.ImagePath) %>" />

</td>
</tr>
</table>
</body>
</html>
