using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace WebApplication1
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {

            //Why you are adding it: On a shared host, the server might "forget" its auto-generated key if it restarts or moves your site.
            //If that happens, every user on your site will get a "MAC validation failed" error (a crash) the next time they click a button. Hardcoding it prevents this.

            //Force the server to use a static key so restarts do not break ViewState validation
            //Keep this: Prevents ViewState crashes during server restarts


            //// Map path to your secret file inside App_Data
            string secretsPath = Server.MapPath("~/App_Data/machineKey.xml");

            if (File.Exists(secretsPath))
            {
                try
                {


                    // 2. Load the standalone XML file
                    System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                    xmlDoc.Load(secretsPath);

                    // 3. Declare the variables outside the if blocks so they survive
                    string customValidationKey = null;
                    string customDecryptionKey = null;

                    // 4. Select the machineKey element (works whether the file is encrypted or plain text)
                    System.Xml.XmlNode machineKeyNode = xmlDoc.SelectSingleNode("//machineKey");

                    // The file is still plain text XML, extract them directly
                    customValidationKey = machineKeyNode.Attributes["validationKey"]?.Value;
                    customDecryptionKey = machineKeyNode.Attributes["decryptionKey"]?.Value;






                    // Use Reflection to inject the keys directly into the running runtime configuration
                    var machineKeySection = (MachineKeySection)WebConfigurationManager.GetSection("system.web/machineKey");


                    var readOnlyField = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
                    readOnlyField.SetValue(machineKeySection, false);

                    machineKeySection.ValidationKey = customValidationKey;
                    machineKeySection.DecryptionKey = customDecryptionKey;
                    machineKeySection.Validation = MachineKeyValidation.SHA1;
                    machineKeySection.Decryption = "AES";

                    readOnlyField.SetValue(machineKeySection, true);
                }

                catch
                {
                    // If parsing fails, fall back silently to IIS Auto-Generation
                    //MessageBox.Show("Fail");
                }
            }


        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }









        // Nedan metod har jag lagt till själv
        // Ser till så man inte kan börja på en annan aspx-sida än startsidan
        // This method is the earliest point in the lifecycle where you can access Session data, as it has just been fully populated.
        protected void Application_PostAcquireRequestState(object sender, EventArgs e)
        {


            // 1. Ensure the HTTP context and Session state are initialized for this request
            if (Context == null || Context.Session == null)
            {
                return;
            }

            if (Request.Url.AbsolutePath.ToLower() == "/default.aspx")
                Session["UserHasVisitedHome"] = 1;


            else if (Session["UserHasVisitedHome"] == null)
            {
                Response.Redirect("~/");

            }
        }



        //Lagt till själv
        protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            if (Context.Handler is Page page)
            {
                //Hook into the Page's Init event to add the meta tag
                page.Init += (pageSender, pageInitializationEventArguments) =>
                {
                    List<HtmlMeta> htmlMetaList = new List<HtmlMeta>
                    {
                        //Instruerar sökmotorer att strunta i den här webbplatsen
                        new HtmlMeta
                        {
                            Name = "robots",
                            Content = "noindex, nofollow"
                        },

                        //Hindrar Google Chrome att uppmana till översättning eftersom det ska alltid vara på svenska
                        new HtmlMeta
                        {
                            Name = "google",
                            Content = "notranslate"

                        }
                    };

                    // 3. Loop through the list and add each tag to the page <head> section
                    if (page.Header != null)
                    {
                        foreach (HtmlMeta htmlMeta in htmlMetaList)
                        {
                            page.Header.Controls.Add(htmlMeta);
                        }
                    }
                };
            }
        }


        protected void Application_Error(object sender, EventArgs e)
        {
            
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}