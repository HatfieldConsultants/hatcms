<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InlinePageBrowser.aspx.cs" Inherits="HatCMS.ckhelpers.InlinePageBrowser" %>
<%@ Import Namespace="HatCMS" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Browse for Page</title>
   <link rel="STYLESHEET" type="text/css" href="../../js/_system/dhtmlx/dhtmlxTree/codebase/dhtmlxtree.css" />
	
	<script  src="../../js/_system/dhtmlx/dhtmlxTree/codebase/dhtmlxcommon.js"></script>
	<script  src="../../js/_system/dhtmlx/dhtmlxTree/codebase/dhtmlxtree.js"></script>	
    
</head>
<body>    
    <div id="treeboxbox_tree" />    
    
<script type="text/javascript">
			tree=new dhtmlXTreeObject({
				skin:"dhx_skyblue",
				parent:"treeboxbox_tree",
				image_path:"../../js/_system/dhtmlx/dhtmlxTree/codebase/imgs/",
				checkbox:false
			});

        var currUrl = window.parent.CKEDITOR.dialog.getCurrent().getValueOf('info', 'url');
        
        tree.loadXML("<%= CmsContext.ApplicationPath %>_system/ckhelpers/dhtmlxPages_xml.ashx?currurl="+encodeURI(currUrl), function(){                
                tree.focusItem(tree.getSelectedItemId());
                tree.selectItem(tree.getSelectedItemId(), false, false);        
            });
                                
        tree.attachEvent("onSelect", function(id)
            { 
                tree.focusItem(id);
                var url = tree.getUserData(id,"url"); 
                if (url){                             
                    window.parent.CKEDITOR.dialog.getCurrent().setValueOf('info', 'url', encodeURI(url));
                    window.parent.CKEDITOR.dialog.getCurrent().setValueOf('target', 'linkTargetType', 'notSet');                            
                }
            });


	</script>    
    
</body>
</html>
