using System;
using System.IO;
using System.Web;
using System.Web.UI;

namespace WebApplication1
{
    public partial class WebForm2 : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Default.downloadInvoiceAfterCreation == 1)
            {
                Response.ContentType = "Application/pdf";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlDecode(Request.QueryString.ToString()) + ".pdf;");
                Response.WriteFile(Server.MapPath("~/wwwroot/Ny_faktura.pdf"));











                //string filePath = Server.MapPath("~/wwwroot/Ny_faktura.pdf");

                //if (File.Exists(filePath))
                //{
                //    // 1. Read all bytes into server RAM immediately and release the file hook
                //    byte[] fileBytes = File.ReadAllBytes(filePath);

                //    // 2. Clear the response buffer
                //    Response.Clear();
                //    Response.ClearHeaders();
                //    Response.ClearContent();

                //    // 3. Set your headers (with the custom dynamic filename trick from above)
                //    string uniqueDownloadName = "Faktura_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                //    Response.ContentType = "application/pdf";
                //    Response.AddHeader("Content-Disposition", "attachment; filename=" + uniqueDownloadName + ".pdf");
                //    Response.AddHeader("Content-Length", fileBytes.Length.ToString());

                //    // 4. Write the binary data directly to the stream and close it
                //    Response.BinaryWrite(fileBytes);
                //    Response.Flush();
                //    Response.End();
                //}
            }
        }
    }
}