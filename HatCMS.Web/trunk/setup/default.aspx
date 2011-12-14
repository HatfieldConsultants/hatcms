<%@ Page language="c#" Codebehind="default.aspx.cs" AutoEventWireup="True" Inherits="HatCMS.setup.setupPage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
	<head>
		<title>Setup HatCMS</title>
		<link rel="stylesheet" type="text/css" href="../css/_system/Setup.css" />
	</head>
	<body>
		<div id="navigation"><div class="wrapper"><img align="absmiddle" src="../images/_system/hatCms_logo.png" style="float: left; margin-top: 8px; margin-right: 10px;" /> <h1>HatCMS Setup</h1></div></div>
	    <div class="wrapper">
		<form id="Form1" method="post" runat="server">
			
			<p><asp:Label id="l_msg" runat="server" BackColor="Yellow" Font-Bold="True" Font-Size="Large"></asp:Label></p>
			
				<h2>Step 1: Edit the web.config file</h2>
            <p>
                Ensure that configuration items are configured properly. These especially include
                the following keys:</p>
            <ul>                
                <li>SiteName</li>
                <li>languages</li>
                <li>LinkMacrosIncludeLanguage</li>
            </ul>
            <p>
                Ensure also that the Author and Administrator roles are configured properly.</p>
            <p>
            </p>
            <h2>
                Step 2: Create the Database</h2>
				<p>Fill in the following information to create the new database:<br />
				<strong>Note: the database should *not* be exist before this step is run (the script will create the database)</strong>
				</p>
				<table>
			        <tr>
			            <td>Database server host name: <span class="required">[required]</span></td>
				        <td><asp:TextBox id="db_host" runat="server" Columns="40">localhost</asp:TextBox></td>
				    </tr>
				    <tr>
				        <td>New Database Name: <span class="required">[required, must not already exist]</span></td>
				        <td><asp:TextBox id="tb_DbName" runat="server" Columns="40">hatcms_db</asp:TextBox></td>
				    </tr>
				    <tr>
				        <td>Database access username: <span class="required">[required]</span></td>
				        <td><asp:TextBox id="db_un" runat="server" Columns="40">username</asp:TextBox></td>
				    </tr>
				    <tr>
				        <td>Database access password:<span class="required">[required]</span></td>
				        <td><asp:TextBox id="db_pw" runat="server" Columns="40">password</asp:TextBox></td>
				    </tr>
				</table>
			
                
                    
                    
			
			<p>
				<asp:Button id="b_db" runat="server" Text="create the database" onclick="b_db_Click"></asp:Button></p>
			<h2>Step 3: Update the ConnectionString</h2>
            <p>
                &nbsp;<asp:Label ID="l_NewConnStr" runat="server"></asp:Label>
                <strong><u></u></strong>
            </p>
			<h2>
                Step 4: Create Standard Pages</h2>
            <p>
                Create standard pages - including the home page and other pages needed for the CMS
                to function.</p>
            <h2>
                <asp:Button ID="b_CreatePages" runat="server" OnClick="b_CreatePages_Click" Text="Create Standard Pages" />&nbsp;</h2>
            <h2>
                Step 5: Verify the configuration</h2>
            <p>
                Some directories need to be made writable by the webserver.</p>
            <p>
				<asp:Button id="b_verifyConfig" runat="server" Text="verify configuration" onclick="b_verifyConfig_Click"></asp:Button> </p>
				<h2>
                    Step 6: Delete the setup directory</h2>
            <p>
                After the configuration has been verified, please delete the "setup" directory.
                You can always verify your configuration through the "Admin Tools"</p>
            <h2>
                Step 7: Customize Templates and Controls</h2>
            <p>
                Your site is now installed, but is quite bland. You can customize your site by creating
                custom templates and controls.</p>
            <p>
                Check out your new
                <asp:LinkButton ID="link_HomePage" runat="server" OnClick="link_HomePage_Click">Home Page</asp:LinkButton>.</p>
            <p>
                &nbsp;</p>
		</form>
		</div>
	</body>
</html>
