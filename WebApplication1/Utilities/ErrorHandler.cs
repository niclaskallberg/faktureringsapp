using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace WebApplication1.Utilities
{
    public class ErrorHandler
    {

        public enum ErrorTypes
        {
            RetrieveFailed,
            DeleteFailed,
            InvoiceCreationFailed
        }


        public static string GenerateErrorMessage(ErrorTypes errorType, Exception exception)
        {

            Dictionary<ErrorTypes, string> ErrorMessages = new Dictionary<ErrorTypes, string>()
            {
                { ErrorTypes.RetrieveFailed, "Kunde ej ansluta till kundregistret!" },
                { ErrorTypes.DeleteFailed, "Något gick fel vid radering!" },
                { ErrorTypes.InvoiceCreationFailed, "Kunde ej ansluta till kundregistret!\nInformationen har inte sparats." }
            };

            //Hämta meddelande, om null -> Använd generellt meddelande
            string errorMessage = ErrorMessages.TryGetValue(errorType, out string value) ? value : "Ett okänt fel uppstod!";

            //Lägg till exception-meddelande om det finns ett
            if (exception != null)
                errorMessage += $"\n\nFelmeddelande:\n\n{exception.Message}";



            return errorMessage;
        }





        public static void HandleError(ErrorTypes errorType, Exception exception)
        {


            // MySQL connection string syntax issues usually don't have a standard server error code number, 
            // so checking the message text is the most reliable way.
            if (exception.Message.Contains("Incorrect value in Connection String"))
            {
                string safeMessage = "Felmeddelande: Kunde ej ansluta till databasen på grund av ett konfigurationsfel.";
                JsHelpers.Alert(safeMessage);
            }
            else
            {
                // For other MySQL errors (e.g., Access Denied, Table not found), show the whole exception
                JsHelpers.Alert(GenerateErrorMessage(errorType, exception));

            }










            LogToFile(exception);




            SendEmail(exception);

        }




        //----------------------------------------------------------------------------------



        //-----------------------------
        //Logga felet till fil
        //-----------------------------

        //Starting a variable name with an underscore (like _fileLock or _balance) is a standard C# coding convention used to denote a private field at the class level.
        //_fileLock används för att 2 personer inte ska kunna skriva till logg-filen samma millisekund och därmed korrumpera den
        private static readonly object _errorLogLock = new object();
        
        private static void LogToFile(Exception exception)
        {

            if (Default.canLogErrors != 1) return;

            try
            {
                string logEntry = $"--------------------------------------------------{Environment.NewLine}" +
                                        $"Date: {DateTime.Now}{Environment.NewLine}" +
                                        $"Message: {exception.Message}{Environment.NewLine}" +
                                        $"Stack Trace: {exception.StackTrace}{Environment.NewLine}" +
                                        $"--------------------------------------------------{Environment.NewLine}";

                string filePath = HostingEnvironment.MapPath("~/error_log.txt");

                //För att undvika flaskhals används så lite kod som möjligt inom själva lås-blocket
                lock (_errorLogLock)
                {
                    File.AppendAllText(filePath, logEntry);
                }
            }
            catch
            {
                JsHelpers.Alert("Kunde ej logga felet!");
            }
        }




        //-----------------------------------------------------------------------------------------





        //-------------------------------
        // Skicka felmeddelande via mejl
        //-------------------------------

        //Initiera variabel för tid för senaste skickade mejl utanför metoden där den används så värdet behålls över postbacks
        private static DateTime _lastEmailSent = DateTime.MinValue;

        private static void SendEmail(Exception exception)
        {


            //Ställ in cooldown
            TimeSpan cooldown = TimeSpan.FromMinutes(60);

            //Om funktionen för att mejla om avvikelser inte är på eller om cooldownen inte är klar, stanna här
            if (Default.canEmailErrors != 1 || (DateTime.Now - _lastEmailSent) < cooldown) return;




            /*
             2-Step Verification Enabled: They must have 2-Factor Authentication turned on, or Google will not allow them to create an App Password.Personal @gmail.com Account: It works perfectly for standard, free personal accounts.
             */

            try
            {





                // Read the custom section as a NameValueCollection
                NameValueCollection publicSettings = (NameValueCollection)(ConfigurationManager.GetSection("mailSettingsPublic") ?? throw new NullReferenceException("Server and port settings are missing. Cannot send error e-mail."));


                string smtpServer = publicSettings["SmtpServer"];
                int smtpPort = int.Parse(publicSettings["SmtpPort"]);







                string smtpUser = ConfigurationManager.AppSettings["SmtpUsername"];
                string smtpPass = ConfigurationManager.AppSettings["SmtpPassword"]; //App pasword
                string receiverEmailAddress = ConfigurationManager.AppSettings["SmtpReceiverEmailAddress"];

                //// 2. Validate that we have the credentials before sending
                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass) || string.IsNullOrEmpty(receiverEmailAddress))
                {
                    // Fail silently or log locally if credentials aren't set up yet (e.g. on GitHub testing)
                    return;
                }

                string subject = "Critical Web App Error Alert";
                string body = $"Time: {DateTime.Now}\n\nMessage: {exception.Message}\n\nStack Trace:\n{exception.StackTrace}";






                MailAddress fromAddress = new MailAddress(smtpUser, "Application Error Monitor");








                //Set up and send the mail
                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = fromAddress;
                    mailMessage.To.Add(receiverEmailAddress); // Your admin inbox
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = false;

                    using (SmtpClient smtp = new SmtpClient(smtpServer, smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                        smtp.EnableSsl = true;
                        smtp.Send(mailMessage);
                    }
                }


                // Update the timestamp ONLY after a successful send
                _lastEmailSent = DateTime.Now;
            }

            catch (Exception emailSenderException)
            {
                // Om e-posten inte skickas, logga misslyckandet att skicka e-post
                JsHelpers.Alert(emailSenderException.Message);
                LogToFile(emailSenderException);
            }
        }
    }
}