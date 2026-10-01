<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Encryption.aspx.cs" Inherits="WebApplication1.Encryption" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>App Initialization</title>

    
    <style>
    /* Spacing between the separate fieldset boxes */
    fieldset {
        margin-bottom: 20px;
        padding: 15px;
        border: 1px solid #ccc;
        border-radius: 4px;
    }
    legend {
        font-weight: bold;
        padding: 0 5px;
    }
    .form-group {
        margin-bottom: 15px;
        display: flex;
        flex-direction: column;
        align-items: flex-start;
    }
    .form-group label {
        margin-bottom: 5px;
    }
</style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
                <main>

        <!-- Main Page/Section Title -->
        <!--<h1>System Configuration</h1>-->
        <h1>Secure Configuration Setup</h1>

        <asp:Label ID="LblMessage" runat="server" ForeColor="Red"></asp:Label><br /><br />

        <!-- Category 1: Database Connections -->
        <fieldset>
            <legend>Database</legend>

            <div class="form-group">
            <label runat="server">Production Connection String:</label><br />
            <asp:TextBox ID="TxtConnectionString" runat="server" Width="400px" TextMode="MultiLine" Rows="3"></asp:TextBox><br /><br />
            </div>
         </fieldset>

        <!-- Category 2: Email Configuration -->

        <fieldset>
            <legend>Error E-Mail</legend>

            <div class="form-group">
                <label>SMTP E-mail address:</label><br />
                <asp:TextBox ID="TxtSmtpUsername" runat="server" Width="400px" TextMode="Email"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>SMTP Password:</label><br />
                <asp:TextBox ID="TxtSmtpPassword" runat="server" Width="400px" TextMode="Password"></asp:TextBox>
            </div>
            
            <div class="form-group">
                <label>SMTP Receiver E-mail Address:</label><br />
                <asp:TextBox ID="TxtReceiverEmailAddress" runat="server" Width="400px" TextMode="Email"></asp:TextBox>
            </div>
        </fieldset>

                    
            <div class="form-group">
                <label>SMTP Server:</label><br />
                <asp:TextBox ID="TxtSmtpServer" runat="server" Width="400px" TextMode="SingleLine"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>SMTP Port:</label><br />
                <asp:TextBox ID="TxtSmtpPort" runat="server" Width="400px" TextMode="SingleLine"></asp:TextBox>
            </div>


            <asp:Button ID="Button1" runat="server" Text="Save and Encrypt Config" OnClick="Button1_Click" />



    </main>
        </div>
    </form>
</body>
</html>