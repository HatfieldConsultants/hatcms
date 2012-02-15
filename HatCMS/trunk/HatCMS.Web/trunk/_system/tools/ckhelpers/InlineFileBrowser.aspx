<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeBehind="InlineFileBrowser.aspx.cs" Inherits="HatCMS.ckhelpers.InlineFileBrowser" %>
<%@ Import Namespace="HatCMS" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Browse for a File</title>
   <link rel="STYLESHEET" type="text/css" href="../../../js/_system/dhtmlx/dhtmlxTree/codebase/dhtmlxtree.css" />
	
	<script  src="../../../js/_system/dhtmlx/dhtmlxTree/codebase/dhtmlxcommon.js"></script>
	<script  src="../../../js/_system/dhtmlx/dhtmlxTree/codebase/dhtmlxtree.js"></script>	
    <style> body</style>
</head>
<body>  
<div style="height: 340px; border: 1px solid #CCC; overflow: scroll;">
    <div id="treeboxbox_tree" style="height: 320px;"></div>
</div>  
<div style="height: 50px; border: 1px solid #CCC;">
<form style="padding: 0px; margin: 0px" enctype="multipart/form-data" runat="server">
Upload file <% OutputMaxFileSize(); %>: <input type="file" id="fileUpload" runat="server" /> <input type="submit" value="Upload" runat="server" id="b_Upload" onserverclick="Submit1_ServerClick" />
<span id="uploadInto"></span>
<br />
Create sub-folder <input type="text" size="20" maxlength="200" runat="server" id="txt_SubFolderName" /> <input type="submit" value="Create" runat="server" id="b_CreateFolder" onserverclick="b_CreateFolder_ServerClick" />
<span id="createUnder"></span>
<input type="hidden" name="uploadPath" id="uploadPath" />
</form>
<script type="text/javascript">
			tree=new dhtmlXTreeObject({
				skin:"dhx_skyblue",
				parent:"treeboxbox_tree",
				image_path:"../../../js/_system/dhtmlx/dhtmlxTree/codebase/imgs/",
				checkbox:false
			});

        var currUrl = window.parent.CKEDITOR.dialog.getCurrent().getValueOf('info', 'url');
        
        <% OutputCurrUrl(); %>
        
        tree.loadXML("<%= CmsContext.ApplicationPath %>_system/tools/ckhelpers/dhtmlxFiles_xml.ashx?currurl="+encodeURI(currUrl), function(){                
                
                tree.focusItem(tree.getSelectedItemId());
                tree.selectItem(tree.getSelectedItemId(), false, false);  
                setTimeout("doSelect(tree.getSelectedItemId())", 500);  
            });
        
        
        function doSelect(id)
        {
                // alert(tree.getSelectedItemId());
                if (id == '')
                    return;
                tree.focusItem(id);
                var url = tree.getUserData(id,"url"); 
                var dirurl = '<%= CmsContext.ApplicationPath %>' + tree.getUserData(id,"dirurl");
                
                if (url){                             
                    window.parent.CKEDITOR.dialog.getCurrent().setValueOf('info', 'url', encodeURI(url));
                    window.parent.CKEDITOR.dialog.getCurrent().setValueOf('target', 'linkTargetType', '_blank');                     
                }
                if (dirurl) {
                    document.getElementById('uploadPath').value = dirurl;
                    var dispUrl = dirurl.substring(<%= (CmsContext.ApplicationPath + "UserFiles/").Length -1 %>);
                    document.getElementById('uploadInto').innerHTML = ' into '+dispUrl;   
                    document.getElementById('createUnder').innerHTML = ' under '+dispUrl;   
                                                                                    
                    document.getElementById('<%= b_Upload.ClientID %>').disabled = false;
                    document.getElementById('<%= b_CreateFolder.ClientID %>').disabled = false;
                }
                else {
                    document.getElementById('<%= b_Upload.ClientID %>').disabled = true;
                    document.getElementById('<%= b_CreateFolder.ClientID %>').disabled = true;
                }        
        }
                        
        tree.attachEvent("onSelect", function(id)
            { 
                doSelect(id);
            });

        

	</script>    
</div>            
</body>
</html>
