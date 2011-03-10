<%@ Control Language="c#" AutoEventWireup="True" Codebehind="Header.ascx.cs" Inherits="HatCMS.Controls.header" TargetSchema="http://schemas.microsoft.com/intellisense/ie3-2nav3-0" %>
<%@ Import Namespace="Hatfield.Web.Portal"%>
<!-- BEGIN Header.ascx -->
<!-- Begin Main Layout Table -->
<table cellspacing="0" cellpadding="0" border="0" align="center" width="902" class="layoutTable">
<tr>
    <td><div class="TopTools">
    <form action="/search.aspx" method="get">
    <a href="#">About</a> |
    <a href="#">Glossary</a> |
    <a href="#">Documents</a> |
    <a href="#">Tools</a> |
    <a href="#">Links</a> |
    <a href="#">Contact</a>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
    <span style="color: Black;">Search:</span>
    <input type="text" name="q" />
    </form>
</div></td>
</tr>
<tr>
	<td><table cellspacing="0" cellpadding="0" border="0" align="center">
		<tr>
			<td><a href="/"><img src="<%= PageUtils.ApplicationPath %>images/banner/logo.jpg" width="240" height="139" /></td>
			<td><a href="/"><img src="<%= PageUtils.ApplicationPath %>images/banner/banner.jpg" width="662" height="139" /></td>
		</tr>
		</table>
	</td>
</tr>
<tr>
	<td>
