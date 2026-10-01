using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace WebApplication1
{
    public partial class Encryption : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {


                //-----------------------------------------------------
                // Fyll i och kryptera användarnamn, lösenord och mottagare
                //-----------------------------------------------------


                // 1. Open the root Web.config file
                Configuration configuration = WebConfigurationManager.OpenWebConfiguration("~");



                // 2. Inject the values from the textboxes into memory

                // Retrieve a specific key
                if (configuration.AppSettings.Settings["SmtpUsername"] != null)
                {
                    configuration.AppSettings.Settings["SmtpUsername"].Value = TxtSmtpUsername.Text;
                }



                // Retrieve a specific key
                if (configuration.AppSettings.Settings["SmtpPassword"] != null)
                {
                    configuration.AppSettings.Settings["SmtpPassword"].Value = TxtSmtpPassword.Text;
                }



                // Retrieve a specific key
                if (configuration.AppSettings.Settings["SmtpReceiverEmailAddress"] != null)
                {
                    configuration.AppSettings.Settings["SmtpReceiverEmailAddress"].Value = TxtReceiverEmailAddress.Text;
                }






                // 2. Get the connectionStrings section
                ConfigurationSection privateMailSection = configuration.GetSection("appSettings");

                // 3. Check if it's already protected
                if (privateMailSection != null && !privateMailSection.SectionInformation.IsProtected)
                {
                    // 4. Encrypt using the server's Machine Key
                    privateMailSection.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");


                    privateMailSection.SectionInformation.ForceSave = true;



                    // 5. Save the encrypted data directly to the disk in a single transaction
                    configuration.Save();
                }





                //-----------------------------------------------------
                //Server och port för mejl
                //-----------------------------------------------------




                // 2. Get the custom section by its declared name
                ConfigurationSection publicMailSection = configuration.GetSection("mailSettingsPublic");

                if (publicMailSection != null)
                {
                    // 3. Extract the raw XML of the section to manipulate it
                    string rawXml = publicMailSection.SectionInformation.GetRawXml();

                    if (!string.IsNullOrEmpty(rawXml))
                    {
                        // 4. Load the raw section XML into an XmlDocument
                        System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                        xmlDoc.LoadXml(rawXml);

                        // 5. Find the SmtpServer and SmtpPort nodes inside this custom fragment
                        System.Xml.XmlNode serverNode = xmlDoc.SelectSingleNode("//add[@key='SmtpServer']");
                        System.Xml.XmlNode portNode = xmlDoc.SelectSingleNode("//add[@key='SmtpPort']");

                        // 6. Update the values from your UI textboxes
                        if (serverNode != null)
                        {
                            serverNode.Attributes["value"].Value = TxtSmtpServer.Text; // e.g., "smtp.gmail.com"
                        }
                        if (portNode != null)
                        {
                            portNode.Attributes["value"].Value = TxtSmtpPort.Text; // e.g., "587"
                        }

                        // 7. Inject the modified XML back into the configuration section memory
                        publicMailSection.SectionInformation.SetRawXml(xmlDoc.OuterXml);

                        // 8. Save the changes back to Web.config on the disk
                        configuration.Save();

                        Response.Write("<h2 style='color:green;'>✅ Success! Custom mailSettingsPublic section updated.</h2>");
                    }
                }
                else
                {
                    Response.Write("<h2 style='color:red;'>❌ Error: mailSettingsPublic section not found in Web.config.</h2>");
                }



            }
            catch (Exception exception)
            {

                LblMessage.Text = exception.ToString();

            }



            try
            {
                // 1. Open the Web.config configuration object root
                Configuration configuration = WebConfigurationManager.OpenWebConfiguration("~");



                // 2. Inject the values from the textboxes into memory
                configuration.ConnectionStrings.ConnectionStrings["ConnectionStringProduction"].ConnectionString = TxtConnectionString.Text;


                //Den här filen är en kryptering för connection-strängarna som en extra säkerhetsåtgärd
                //Gå till startsidan först så du inte blir redirectad dit och gå sedan till dinurl.com/encryption.aspx så körs detta
                //Efter det kan denna fil raderas från servern



                // 2. Get the connectionStrings section
                ConfigurationSection connectionStringsSection = configuration.GetSection("connectionStrings");

                // 3. Check if it's already protected
                if (connectionStringsSection != null && !connectionStringsSection.SectionInformation.IsProtected)
                {
                    // 4. Encrypt using the server's Machine Key
                    connectionStringsSection.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                    connectionStringsSection.SectionInformation.ForceSave = true;


                    //5.Save the changes back to App_Data\settings.config
                    configuration.Save();








                    Response.Write("<h2 style='color:green;'>✅ Success!</h2>");


                    LblMessage.ForeColor = System.Drawing.Color.Green;
                    LblMessage.Text = "Configuration encrypted and saved successfully! Self-destructing page...";

                    // Law 2: Self-destruct the setup files immediately so they can never be reused
                    System.IO.File.Delete(Server.MapPath("~/Encryption.aspx"));
                    System.IO.File.Delete(Server.MapPath("~/Encryption.aspx.cs"));
                    System.IO.File.Delete(Server.MapPath("~/Encryption.aspx.designer.cs"));

                }
                else
                {
                    Response.Write("<h2 style='color:blue;'>ℹ️ Already Encrypted</h2>");
                    Response.Write("<p>The section was already protected by the server.</p>");

                }
            }
            catch (Exception exception)
            {
                Response.Write("<h2 style='color:red;'>❌ Error</h2>");
                Response.Write("<p>" + exception.Message + "</p>");
            }
        }
    }
}