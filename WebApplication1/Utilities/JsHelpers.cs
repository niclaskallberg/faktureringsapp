using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WebApplication1.Utilities
{
    //Static är tillagt av mig
    //In C#, static basically means "belongs to the class itself, not to a specific object."
    public static class JsHelpers
    {
        





        //Funktion för att kalla valfri JavaScript
        public static void DoAny(string jsCode)
        {
            // string.IsNullOrWhiteSpace: This handles null, empty strings, and strings that 
            // are just spaces in one go, preventing a NullReferenceException
            if (string.IsNullOrWhiteSpace(jsCode)) return;

            jsCode = jsCode.Trim();

            // Ensure the script ends with a semicolon
            // Last() will throw an error if the string is empty, whereas EndsWith simply returns false.
            if (!jsCode.EndsWith(";")) jsCode += ';';





            if (!(HttpContext.Current?.Handler is System.Web.UI.Page currentPage)) return;

            // Check if there is a ScriptManager on the current page
            ScriptManager scriptManager = ScriptManager.GetCurrent(currentPage);

            if (scriptManager != null)
            {
                //GUID = Globally Unique Identifier
                //Används för att generera slumpmässig identifierare som används som key för scripts
                ScriptManager.RegisterStartupScript(currentPage, currentPage.GetType(), Guid.NewGuid().ToString(), jsCode, true);

            }
            else
            {
                // Fallback to standard ClientScript for regular pages
                currentPage.ClientScript.RegisterStartupScript(
                    currentPage.GetType(),
                    Guid.NewGuid().ToString(),
                    jsCode,
                    true
                );
            }
        }





        public static void Alert(string message = "")
        {
            //Om meddelandet är null eller bara innehåller mellanslag
            if (string.IsNullOrWhiteSpace(message)) message = "";

            //Modern encoding: escapes quotes, backticks, <, >, and non-ASCII chars
            message = JavaScriptEncoder.Default.Encode(message);

            DoAny($"alert('{message}')");

        }

    }
}