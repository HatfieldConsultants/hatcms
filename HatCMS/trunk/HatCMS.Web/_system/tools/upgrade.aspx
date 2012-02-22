<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="upgrade.aspx.cs" Inherits="HatCMS._system.tools.UpgradePage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
	<head>
		<title>Setup HatCMS</title>
		<link rel="stylesheet" type="text/css" href="../../css/_system/Setup.css" />
		<script language="javascript" type="text/javascript" src="../../js/_system/jquery/jquery-1.4.1.min.js"></script>
		<script language="javascript" type="text/javascript" src="../../js/_system/UpdateTool/UpdateTool.js"></script>
	</head>
	<body>
		<div id="navigation"><div class="wrapper"><img align="absmiddle" src="../../images/_system/hatCms_logo.png" style="float: left; margin-top: 8px; margin-right: 10px;" /> 
		<h1>HatCMS Upgrade</h1></div></div>
		<div id="updatecontent" runat="server"></div>
	    <div class="wrapper" style="left: 0px; top: 0px">
		<form id="Form1" method="post" runat="server">
        <br />
        <strong>
        As a security check, please copy the current ConnectionString from your web.config file here:
        <br />
        <asp:TextBox ID="tb_TestConnectionString" runat="server" Columns="50"></asp:TextBox>   
        </strong>
        <p>&nbsp;</p>
        <asp:Button runat="server" ID="b_ValidateConfig" Text="Validate Current Configuration" OnClick="b_ValidateConfig_Click" />
        <br />
        <asp:PlaceHolder ID="ph_ValidationErrors" runat="server"></asp:PlaceHolder>
        
        <p>&nbsp;</p>
        <asp:Button runat="server" ID="b_UpdateDatabase" Text="Update Database" OnClick="b_UpdateDatabase_Click"  />
        <br />
        <asp:PlaceHolder ID="ph_UpdateDatabaseMessage" runat="server"></asp:PlaceHolder>
        </form>
        </div>
</body>
</html>
