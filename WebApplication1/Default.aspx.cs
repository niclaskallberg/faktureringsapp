using MySql.Data.MySqlClient;
using QRCoder;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Texts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication1.Models;
using WebApplication1.Utilities;



namespace WebApplication1
{
    public partial class Default : Page
    {
        //Används för funktioner, Vid publicering ska alla sättas på 1
        //Dessa är lagda här eftersom denna fil är navet i projektet
        //--------------------------------------------------------------------//
        public static readonly int isLiveDatabase = 1;
        public static readonly int canLogErrors = 1;
        public static readonly int canEmailErrors = 1;
        public static readonly int canEditDatabase = 1;
        public static readonly int showInvoiceAfterCreation = 1;
        public static readonly int downloadInvoiceAfterCreation = 1;
        //--------------------------------------------------------------------//








        //customers är en sammansättning av customerNumbers och customerNames
        private readonly List<string> customers = new List<string>();
        private readonly List<string> customerNumbers = new List<string>();
        private readonly List<string> customerNames = new List<string>();



        //addresses är en sammansättning av streets, postalCodes och cities
        private readonly List<string> addresses = new List<string>();
        private readonly List<string> streets = new List<string>();
        private readonly List<string> postalCodes = new List<string>();
        private readonly List<string> cities = new List<string>();







        //Datumformat som används för hela appen
        private readonly string dateFormat = "yyyy-MM-dd";



        private readonly string showServicesBtnText = "Se tjänster/varor";
        private readonly string hideServicesBtnText = "Backa till kunder";
        private readonly string showPastInvoicesBtnText ="Se äldre fakturor";
        private readonly string hidePastInvoicesBtnText = "Backa till kunder";
        private readonly string goBackToInvoices = "Backa till fakturor";








        // A property to track the success status across postbacks
        private bool IsDataLoadedSuccessfully
        {
            get { return Session["IsDataLoaded"] != null && (bool)Session["IsDataLoaded"]; }
            set { Session["IsDataLoaded"] = value; }
        }



        // Används till länkar som ska läsas av front-enden
        public static string FrontEndAssets
        {
            get
            {
                string assets = "/wwwroot/";

                // ~ = rot-mappen av denna applikation
                // ResolveUrl slår upp addressen och gör rotmappsadressen till något som kan läsas av en webbläsare,
                // det gör att det fungerar oavsett om appen ligger i en undermapp på domänen
                return HttpContext.Current.Handler is Page page ? page.ResolveUrl('~' + assets) : assets; //Om det är en sida som anropar detta, anropa ResolveUrl, annars anropa bara assets

            }
        }
                

        public static string JQueryFolderPath
        {
            get
            {
                return VirtualPathUtility.Combine(FrontEndAssets, "jquery/");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Resolves the exact page URL cleanly
                string checkInvoiceNumberUrl = ResolveUrl("~/Default.aspx/IsInvoiceNumberTaken");

                // ⏳ Typings triggers: waits 500ms (passes false)
                TbxInvoiceNumber.Attributes.Add("oninput", $"checkInvoiceNumber(this, '{checkInvoiceNumberUrl}', false);");

                // ⚡ Focus loss trigger: runs immediately (passes true)
                TbxInvoiceNumber.Attributes.Add("onblur", $"checkInvoiceNumber(this, '{checkInvoiceNumberUrl}', true);");


                ChbChangeInvoiceNumberManually.Checked = true;

                BtnShowServices.Text = showServicesBtnText;
                BtnShowPastInvoices.Text = showPastInvoicesBtnText;


            }




            // Remove the marker attribute so it never leaves the server
            TbxInvoiceNumber.Attributes.Remove("data-input");
            TbxInvoiceNumber.Attributes.Remove("data-onblur");

            //---------------------------------------------------------------
            // Lägger till CSS- och JavaScript-filer programmatiskt då jag
            // vill ha med en query med filtiden för att undvika cachelagring
            //---------------------------------------------------------------


            //CSS
            //----------------------------------------------------------
            string cssFilePath = FrontEndAssets + "site.css";
            CssLink.Href = cssFilePath + "?" + File.GetLastWriteTime(Server.MapPath(cssFilePath)).ToFileTime();


            //JS
            //-----------------------------------------------------------
            string javaScriptFilePath = FrontEndAssets + "site.js";
            string fullScriptPath = javaScriptFilePath + "?" + File.GetLastWriteTime(Server.MapPath(javaScriptFilePath)).ToFileTime();
            string fullScriptElement = $"<script src='{fullScriptPath}'></script>";

            //Gör så att JavaScript-filen läggs i slutet av filen
            ScriptManager.RegisterStartupScript(this, GetType(), "externalJavaScript", fullScriptElement, false);




            //-------------------------------------------
            //Hämta data och lägg längst ner i HTML-koden
            //-------------------------------------------

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(ConnectionStringProvider.ConnectionString))
                {

                    mySqlConnection.Open();


                    //----------------------------------------
                    //Fakturanummer
                    //----------------------------------------


                    List<int> invoiceNumbers = new List<int>();


                    TbxInvoiceNumber.Enabled = true;


                    using (MySqlCommand mySqlCommand = new MySqlCommand
                    {
                        Connection = mySqlConnection,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "get_invoicenumbers"
                    })
                    {


                        //Hämta alla fakturanummer
                        using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                        {

                            while (mySqlDataReader.Read())
                            {
                                invoiceNumbers.Add(int.Parse(mySqlDataReader.GetString("fldinvoicenumber")));
                            }
                        }
                    }



                    //Sortera fakturanummer
                    invoiceNumbers.Sort();


                    //Fakturanumren har formen av XX01, XX02 osv, där XX är nuvarande år
                    string thisYear = DateTime.Today.ToString("yy");
                    string sequenceNumber = "01";






                    if (!IsPostBack)
                    {
                        ChbChangeInvoiceNumberManually.Checked = false;
                    }

                    //Om fakturanummer ska genereras automatiskt, gör detta
                    if (!ChbChangeInvoiceNumberManually.Checked)
                    {
                        TbxInvoiceNumber.Enabled = false;
                        TbxInvoiceNumber.Text = thisYear + sequenceNumber;




                        //Återställ "Skapa faktura"-knappen om den har blivit disablad tidigare
                        BtnCreateInvoice.Enabled = true;

                        foreach (int invoiceNumber in invoiceNumbers)
                        {
                            //Om numret i textrutan är upptaget
                            if (TbxInvoiceNumber.Text == invoiceNumber.ToString())
                            {
                                //Om fakturanummer ska genereras automatiskt, gör detta
                                //Följande gör att fakturanumret räknar från XX01-XX99 och
                                //blir det fler under samma år blir det XX100, XX101 osv.
                                
                                //Om fakturanummer redan har uppgått till XX99, ändra till XX100
                                if (TbxInvoiceNumber.Text == thisYear + "99")
                                    TbxInvoiceNumber.Text = thisYear + "100";

                                //Annars addera bara 1
                                else
                                    TbxInvoiceNumber.Text = (invoiceNumber + 1).ToString();

                            }
                        }
                    }


                    








                    if (!IsDataLoadedSuccessfully || LtrData.Text == "")
                    {

                        using (MySqlCommand mySqlCommand = new MySqlCommand
                        {
                            Connection = mySqlConnection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "get_personcustomers"
                        })
                        {

                            string personCustomersWithTheirAddresses = "";
                            string businessCustomersWithTheirAddresses = "";

                            for (int i = 0; i < 2; i++)
                            {
                                if (i == 1)
                                {
                                    mySqlCommand.CommandText = "get_businesscustomers";

                                }

                                //Ifall dessa listor har innehåll så töms det så det inte blir dubbel data
                                customerNumbers.Clear();
                                customerNames.Clear();
                                customers.Clear();

                                streets.Clear();
                                postalCodes.Clear();
                                cities.Clear();
                                addresses.Clear();



                                // Execute the command and read the results
                                using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                                {

                                

                                    while (mySqlDataReader.Read())
                                    {
                                        customerNumbers.Add(mySqlDataReader.GetString("fldcustomernumber").Trim());
                                        customerNames.Add(mySqlDataReader.GetString("fldname").Trim());

                                        customers.Add((customerNumbers.Last() + " " + customerNames.Last()).Trim());


                                        streets.Add(mySqlDataReader.GetString("fldstreet").Trim());
                                        postalCodes.Add(mySqlDataReader.GetString("fldpostalcode").Trim());
                                        cities.Add(mySqlDataReader.GetString("fldcity").Trim());


                                        string address = "";

                                        if (streets.Last() != "")
                                            address = streets.Last() + ", ";

                                        if (postalCodes.Last() != "")
                                            address += postalCodes.Last() + " ";

                                        if (cities.Last() != "")
                                            address += cities.Last();

                                        address = address.Trim();

                                        if (address.StartsWith(","))
                                            address = address.Remove(0);

                                        if (address.EndsWith(","))
                                            address = address.Remove(address.Length - 1);


                                        addresses.Add(address.Trim());





                                        //$ = Interpolation, gör att du kan lägga in variabler mellan {}
                                        //JsonSerializer = Escapear tecken som " och \ samt gör så ingen kan lägga in exempelvis "</script>" i databasen
                                        if (i == 0)
                                            personCustomersWithTheirAddresses =
                                                $"const personCustomers = {JsonSerializer.Serialize(customers)};" +
                                                $"const personCustomerNumbers = {JsonSerializer.Serialize(customerNumbers)};" +
                                                $"const personCustomerNames = {JsonSerializer.Serialize(customerNames)};" +
                                                $"const personAddresses = {JsonSerializer.Serialize(addresses)};" +
                                                $"const personStreets = {JsonSerializer.Serialize(streets)};" +
                                                $"const personPostalCodes = {JsonSerializer.Serialize(postalCodes)};" +
                                                $"const personCities = {JsonSerializer.Serialize(cities)};";


                                    

                                        else
                                            businessCustomersWithTheirAddresses =
                                                $"const businessCustomers = {JsonSerializer.Serialize(customers)};" +
                                                $"const businessCustomerNumbers = {JsonSerializer.Serialize(customerNumbers)};" +
                                                $"const businessCustomerNames = {JsonSerializer.Serialize(customerNames)};" +
                                                $"const businessAddresses = {JsonSerializer.Serialize(addresses)};" +
                                                $"const businessStreets = {JsonSerializer.Serialize(streets)};" +
                                                $"const businessPostalCodes = {JsonSerializer.Serialize(postalCodes)};" +
                                                $"const businessCities = {JsonSerializer.Serialize(cities)};";




                                        //Kommer läggas längst ner i html-koden
                                        Session["customersandaddresses"] = personCustomersWithTheirAddresses + businessCustomersWithTheirAddresses;


                                    }
                                }
                            }
                        }
                        
                    






                        // Hämta tjänster från databas och lägg i autocomplete-lista när sidan laddas första gången
                        //-----------------------------------------------------------------------------------------
                        
                        using (MySqlCommand mySqlCommand = new MySqlCommand
                        {
                            Connection = mySqlConnection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "get_servicenames"
                        })
                        {


                            string serviceNames = "const services = [";

                            using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                            {
                                while (mySqlDataReader.Read())
                                    serviceNames += "'" + mySqlDataReader.GetString("fldservicename") + "',";
                            }

                            if (serviceNames.EndsWith(","))
                                serviceNames = serviceNames.Remove(serviceNames.Length - 1);

                            serviceNames += "];";

                            Session["serviceNames"] = serviceNames;

                        }
                    }

                    //Lagra datan från databasen i html-koden så det kan användas av JavaScript
                    LtrData.Text = $"<script>{Session["customersandaddresses"]}{Session["serviceNames"]}</script>";



                    // If we reach this line, mark it as successful
                    IsDataLoadedSuccessfully = true;
                }
            }

            catch (MySqlException ex)
            {
                //Mark as failed so it retries on the next postback
                IsDataLoadedSuccessfully = false;

                ErrorHandler.HandleError(ErrorHandler.ErrorTypes.RetrieveFailed, ex);
            }
        }

        //-----------------------------------------------
        //Page_Load kommer före click-metoder



        //Used when entering invoice number manually
        [WebMethod]
        public static bool IsInvoiceNumberTaken(string invoiceNumber)
        {
            

            string query = "SELECT COUNT(*) FROM tbl_invoice WHERE fldinvoicenumber = @InvoiceNumber";

            // Use "using" statements to ensure connections close immediately and prevent leaks
            using (MySqlConnection conn = new MySqlConnection(ConnectionStringProvider.ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    // Parameterized query strictly prevents SQL Injection attacks
                    cmd.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber.Trim());

                    try
                    {
                        conn.Open();
                        // ExecuteScalar is ideal here since we only expect a single number back
                        long count = Convert.ToInt64(cmd.ExecuteScalar());

                        // Returns true if the number is already taken, false if available
                        return count > 0;
                    }
                    catch (Exception ex)
                    {
                        // Log the exception here depending on your logging framework
                        System.Diagnostics.Debug.WriteLine("Database error: " + ex.Message);

                        // Return true as a safety fallback so you don't allow duplicates if the DB drops
                        return true;
                    }
                }
            }
        }

        protected void BtnShowServices_Click(object sender, EventArgs e)
        {
            if (!DivShowServices.Visible)
            {
                DivShowServices.Visible = true;
                BtnShowServices.Text = hideServicesBtnText;

                DivPastInvoices.Visible = false;
                BtnShowPastInvoices.Text = showPastInvoicesBtnText;

                BindServicesRepeater();

            }

            else {
                DivShowServices.Visible = false;
                BtnShowServices.Text = showServicesBtnText;

                

            }

        }




        private void BindServicesRepeater()
        {

            List<Article> servicesList = new List<Article>();

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(ConnectionStringProvider.ConnectionString))
                {
                    using (MySqlCommand mySqlCommand = new MySqlCommand("get_services_admin_list", mySqlConnection))
                    {
                        mySqlCommand.CommandType = CommandType.StoredProcedure;

                        mySqlConnection.Open();
                        using (MySqlDataReader reader = mySqlCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                servicesList.Add(new Article
                                {
                                    Id = reader.GetInt32("service_id"),
                                    Name = reader.GetString("fldservicename"),
                                    IsVisible = reader.GetBoolean("fldisvisible")
                                });
                            }
                        }
                    }
                }

                // Only bind if the database operation was entirely successful
                rptServices.DataSource = servicesList;
                rptServices.DataBind();
            }
            catch (MySqlException ex)
            {
                // Handle database-specific issues (e.g., connection drop, bad procedure name)
                System.Diagnostics.Debug.WriteLine("MySQL Error: " + ex.Message);
                // Pro-tip: Show a user-friendly error message on the UI here
            }
            catch (Exception ex)
            {
                // Handle general anomalies (e.g., configuration issues, null references)
                System.Diagnostics.Debug.WriteLine("General Error: " + ex.Message);
            }



        }




        [WebMethod]
        public static void UpdateServiceVisibility(int serviceId, bool showInList)
        {
            int mysqlBoolValue = showInList ? 1 : 0;


            using (MySqlConnection mySqlConnection = new MySqlConnection(ConnectionStringProvider.ConnectionString))
            {
                using (MySqlCommand mySqlCommand = new MySqlCommand
                {
                    Connection = mySqlConnection,
                    CommandType = CommandType.Text,
                    CommandText = "UPDATE tbl_service SET fldisvisible = @showValue WHERE service_id = @id;"
                })
                {
                    mySqlCommand.Parameters.AddWithValue("@showValue", mysqlBoolValue);
                    mySqlCommand.Parameters.AddWithValue("@id", serviceId);

                    mySqlConnection.Open();
                    mySqlCommand.ExecuteNonQuery();
                }
            }
        }


        protected void BtnShowPastInvoices_Click(object sender, EventArgs e)
        {
            if (!DivPastInvoices.Visible)
            {
                DivShowServices.Visible = false;
                BtnShowServices.Text = showServicesBtnText;

                DivPastInvoices.Visible = true;

                if (!DivEditInvoice.Visible)
                {
                    GrvInvoices.Visible = true;

                    BtnShowPastInvoices.Text = hidePastInvoicesBtnText;



                    BindGrid();

                }

                else BtnShowPastInvoices.Text=goBackToInvoices;

                
            }

            else
            {
                

                if (!DivEditInvoice.Visible)
                {
                    DivPastInvoices.Visible = false;
                    BtnShowPastInvoices.Text = showPastInvoicesBtnText;

                    
                }

                else
                {
                    DivEditInvoice.Visible = false;
                    GrvInvoices.Visible = true;

                    BtnShowPastInvoices.Text = hidePastInvoicesBtnText;

                    BindGrid();
                }

            }
        }

        protected void GrvInvoices_Sorting(object sender, GridViewSortEventArgs e)
        {
            // Toggle direction string cleanly using ViewState (safer than Session)
            string dir = (ViewState["Dir"]?.ToString() == "ASC") ? "DESC" : "ASC";
            ViewState["Dir"] = dir;

            // Pass the combined sort expression directly to your database loader
            BindGrid(e.SortExpression + " " + dir);
        }


        private void BindGrid(string sortExpression = "")
        {
            try
            {

                using (MySqlConnection conn = new MySqlConnection(ConnectionStringProvider.ConnectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("get_invoices", conn) { CommandType = CommandType.StoredProcedure })
                    {
                        DataTable invoiceListDatatable = new DataTable();
                        invoiceListDatatable.Columns.Add("fldinvoicenumber");
                        invoiceListDatatable.Columns.Add("fldcustomernumber");
                        invoiceListDatatable.Columns.Add("fldname");
                        invoiceListDatatable.Columns.Add("flddate", typeof(DateTime));
                        invoiceListDatatable.Columns.Add("fldnetamount", typeof(int));

                        new MySqlDataAdapter(cmd).Fill(invoiceListDatatable);

                        // Apply sorting if an expression is provided
                        if (!string.IsNullOrEmpty(sortExpression))
                        {
                            invoiceListDatatable.DefaultView.Sort = sortExpression;
                        }

                        GrvInvoices.DataSource = invoiceListDatatable;
                        GrvInvoices.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError(ErrorHandler.ErrorTypes.RetrieveFailed, ex);
            }
        }





        protected void BtnViewInvoice_Click(object sender, EventArgs e)
        {
            GrvInvoices.Visible = false;
            DivEditInvoice.Visible = true;
            BtnShowPastInvoices.Text = goBackToInvoices;
                            

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(ConnectionStringProvider.ConnectionString))
                {

                    mySqlConnection.Open();


                    using (MySqlCommand mySqlCommand = new MySqlCommand
                    {
                        Connection = mySqlConnection,
                        CommandText = "view_invoice",
                        CommandType = CommandType.StoredProcedure
                    })
                    {

                        mySqlCommand.Parameters.AddWithValue("parameter_invoicenumber", HdnInvoiceNumber.Value);
                        mySqlCommand.Parameters["parameter_invoicenumber"].Direction = ParameterDirection.Input;


                        DataTable dataTable = new DataTable();
                        new MySqlDataAdapter(mySqlCommand).Fill(dataTable);
                        RptViewInvoice.DataSource = dataTable;
                        RptViewInvoice.DataBind();






                        //Denna funktion tas bort när funktionen för att ändra fakturor är implementerad
                        void DisableTextBoxesWithin(Control parentControl)
                        {
                            foreach (Control ctrl in parentControl.Controls)
                            {
                                // Check if the control is an ASP.NET TextBox
                                if (ctrl is TextBox textBox)
                                {
                                    textBox.Enabled = false;
                                }

                                // Check nested controls recursively if this control has children
                                if (ctrl.HasControls())
                                {
                                    DisableTextBoxesWithin(ctrl);
                                }
                            }
                        }

                        DisableTextBoxesWithin(RptViewInvoice);
                    }
                }
            }

            catch (MySqlException ex)
            {
                ErrorHandler.HandleError(ErrorHandler.ErrorTypes.RetrieveFailed, ex);
            }

        }


        protected void BtnUpdateInvoice_Click(object sender, EventArgs e)
        {
            //Kommer läggas till i framtiden
        }




        protected void BtnDeleteInvoice_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(ConnectionStringProvider.ConnectionString))
                {
                    mySqlConnection.Open();

                    // Transaction används för att om något går fel vid raderingen av fakturan så rullas det tillbaka som det var innan
                    using (MySqlTransaction mySqlTransaction = mySqlConnection.BeginTransaction())
                    {


                        using (MySqlCommand mySqlCommand = new MySqlCommand
                        {
                            Connection = mySqlConnection,
                            Transaction = mySqlTransaction,
                            CommandText = "delete_invoice",
                            CommandType = CommandType.StoredProcedure
                        })
                        {

                            mySqlCommand.Parameters.AddWithValue("invoicenumber", HdnInvoiceNumber.Value);
                            mySqlCommand.Parameters["invoicenumber"].Direction = ParameterDirection.Input;

                            int rowsAffected = mySqlCommand.ExecuteNonQuery();

                            

                            if (rowsAffected != 1)
                                throw new Exception($"Oväntat antal rader påverkade: {rowsAffected}");

                            // If it reaches here, exactly 1 row was deleted
                            mySqlTransaction.Commit();


                            JsHelpers.Alert("Faktura nummer " + HdnInvoiceNumber.Value + " är raderad!");
                            JsHelpers.DoAny("document.getElementById('" + BtnShowPastInvoices.ID + "').click()");
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                // Note: The transaction object will automatically roll back when disposed by its 'using' block if .Commit() was never called.
                ErrorHandler.HandleError(ErrorHandler.ErrorTypes.DeleteFailed, ex);
            }

        }




        //AJAX
        //Validera användare mot databas när faktura skapas
        [WebMethod]
        public static string ValidateCustomer(string CustomerNumber, string CustomerName)
        {
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(ConnectionStringProvider.ConnectionString))
                {
                    

                    mySqlConnection.Open();



                    using (MySqlCommand mySqlCommand = new MySqlCommand
                    {
                        Connection = mySqlConnection,
                        CommandType = CommandType.Text,
                        CommandText = "CALL check_customer('" + CustomerNumber + "', '" + CustomerName + "');"
                    })
                    {

                        string returnValue = "";





                        //Ska returnera "1" eller "2" eller tom sträng
                        using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                        {
                            while (mySqlDataReader.Read())
                            {
                                returnValue = mySqlDataReader.GetInt32("@prompt").ToString();
                            }
                        }



                        
                        return returnValue;




                    }
                }
            }
            catch (Exception ex)
            {
                return ErrorHandler.GenerateErrorMessage(ErrorHandler.ErrorTypes.InvoiceCreationFailed, ex);
            }

        }










        protected void BtnCreateInvoice_Click(object sender, EventArgs e)
        {

            /*
              // Gather all parameters cleanly into a structured Dictionary map
    var dbParams = new Dictionary<string, object>
    {
        { "@invoiceDate", invoice.CreationDate.ToString(dateFormat) },
        { "@customerNumber", invoice.CustomerNumber },
        { "@invoiceNumber", invoice.InvoiceNumber },
        { "@paraIsBusinessCostumer", ChbIsBusinessCustomer.Checked ? 1 : 0 },
        { "@customerName", invoice.CustomerName },
        // ... Map all remaining parameters here ...
        { "@item5Amount", finalArticles.Count >= 5 ? finalArticles[4].Amount : null },
        { "@item5IsRutDeductible", finalArticles.Count >= 5 ? Convert.ToInt32(!finalArticles[4].IsNotRut) : null }
    };
             
             */




            Invoice invoice = new Invoice
            {
                InvoiceNumber = TbxInvoiceNumber.Text,
                CustomerNumber = TbxCustomerNumber.Text,
                CustomerName = TbxCustomerName.Text,

                BillingAddress = new Address
                {
                    Street = TbxStreet.Text,
                    PostalCode = TbxPostalCode.Text,
                    City = TbxCity.Text
                },

                DeliveryAddress = new Address()
                {
                    Street = TbxDeliveryStreet.Text,
                    PostalCode = TbxDeliveryPostalCode.Text,
                    City = TbxDeliveryCity.Text

                }

            };



            // Extract finalized layout data for database entry
            List<InvoiceArticleItem> finalArticles = new List<InvoiceArticleItem>();





            string[,] articleTextOnInvoice = new string[5, 6];



            for (int i = 0; i < articleTextOnInvoice.GetLength(0); i++)
            {
                for (int j = 0; j < articleTextOnInvoice.GetLength(1); j++)
                {
                    articleTextOnInvoice[i, j] = "";
                }
            }




            // Retrieve the raw form values as comma-separated strings
            string rawArticles = Request.Form["TbxArticle[]"];

            // If the table is empty or wasn't submitted safely
            if (string.IsNullOrEmpty(rawArticles)) return;

            // Split them into clean, index-aligned arrays
            string[] articles = rawArticles.Split(',');

            string[] descriptions = Request.Form["TbxDescription[]"].Split(',');
            string[] deliveryDates = Request.Form["TbxDeliveryDate[]"].Split(',');
            string[] quantities = Request.Form["TbxQuantity[]"].Split(',');
            string[] prices = Request.Form["TbxPricePerUnit[]"].Split(',');
            string[] amounts = Request.Form["TbxRowAmount[]"].Split(',');
            string[] rutFlags = Request.Form["ChbNotRut[]"].Split(',');


            //MessageBox.Show(articles.Length.ToString());
            // Loop through the items safely using the array length
            for (int i = 0; i < articles.Length; i++)
            {
                string article = articles[i].Trim();
                string description = descriptions[i].Trim();
                string deliveryDate = deliveryDates[i].Trim();
                string quantity = quantities[i].Trim().Replace('.',',');
                string price = prices[i].Trim().Replace('.', ',');
                string amount = amounts[i].Trim().Replace('.', ',');
                bool isNotRut = Convert.ToBoolean(rutFlags[i]);


                finalArticles.Add(new InvoiceArticleItem
                {
                    Article = article,
                    Description = description,
                    DeliveryDate = string.IsNullOrWhiteSpace(deliveryDate) ? (DateTime?)null : DateTime.Parse(deliveryDate),
                    Quantity = decimal.Parse(quantity),
                    PricePerUnit = decimal.Parse(price),
                    Amount = decimal.Parse(amount),
                    IsNotRut = isNotRut


                });


                articleTextOnInvoice[i, 0] = finalArticles[i].Article;
                articleTextOnInvoice[i, 1] = finalArticles[i].Description;



                articleTextOnInvoice[i, 2] = finalArticles[i].DeliveryDate?.ToString(dateFormat) ?? "";

                articleTextOnInvoice[i, 3] = finalArticles[i].Quantity.ToString();
                articleTextOnInvoice[i, 4] = finalArticles[i].PricePerUnit.ToString();
                articleTextOnInvoice[i, 5] = finalArticles[i].Amount.ToString();




            }



            invoice.NetAmount = decimal.Parse(TbxNetAmount.Text);
            invoice.ValueAddedTax = decimal.Parse(TbxValueAddedTax.Text);
            invoice.RutDeductionAmount = decimal.Parse(TbxRutDeductionAmount.Text);
            invoice.GrossAmount = decimal.Parse(TbxGrandInvoiceTotal.Text);








            //Skapa PDF med Spire.PDF NuGet-paket
            //-----------------------------------
            

            string pdfDirectoryPath = Server.MapPath("~/wwwroot/pdf/");

            // Create a PdfDocument object
            PdfDocument pdfDocument = new PdfDocument();

            // Load a PDF file
            pdfDocument.LoadFromFile(pdfDirectoryPath + "Fakturamall.pdf");

            // Get page
            PdfPageBase pdfPageBase = pdfDocument.Pages[0];

            // Create a PdfTextReplacer object based on page
            PdfTextReplacer pdfTextReplacer = new PdfTextReplacer(pdfPageBase);

            //Set properties and variables
            invoice.CreationDate = DateTime.Today;
            invoice.DueDate = DateTime.Today.AddDays(15);
            string oldPaymentDays = "15 dagar";
            string newPaymentDays = oldPaymentDays;


            string oldDeliveryAddressSection = "Samma som betalningsadress";
            string newDeliveryAddressSection = oldDeliveryAddressSection;



            string oldRutInfoFirstRow = "RUT-avdrag (50%) görs på allt utom resa och söks";
            string oldRutInfoSecondRow = "hos Skatteverket på personnummer";
            string newRutInfoFirstRow = oldRutInfoFirstRow;
            string newRutInfoSecondRow = oldRutInfoSecondRow;



            if (ChbDeliveryAddress.Checked)
                newDeliveryAddressSection = "";


            if (ChbIsBusinessCustomer.Checked)
            {
                invoice.DueDate = DateTime.Today.AddDays(30);
                newPaymentDays = "30 dagar";
                newRutInfoFirstRow = "";
                newRutInfoSecondRow = "";
            }



            string[] oldText = {
                "[Datum]",
                "[Kundnummer]",
                "[Fakturanummer]",
                "[Namn]",
                "[Gata]",
                "[Postnummer] [Postort]",
                "[Förfallodatum]",
                "[Belopp]",
                "[Personnummer]",
                "[Summa exkl. moms]",
                "[Momsbelopp]",
                "[RUT-avdrag]",
                oldDeliveryAddressSection,
                oldPaymentDays,
                oldRutInfoFirstRow,
                oldRutInfoSecondRow
            };

            /*Där det står "" är ställen där koordinater måste användas
            istället eftersom det behövs speciella typsnitt till dem*/
            string[] newTextOnInvoice =
            {
                invoice.CreationDate.ToString(dateFormat),
                invoice.CustomerNumber,
                invoice.InvoiceNumber,
                "",
                "",
                "",
                "",
                "",
                TbxPersonalIdentityNumber.Text,
                invoice.NetAmount.ToString() + " kr",
                //Convert.ToInt32(invoice.NetAmount).ToString() + " kr",
                invoice.ValueAddedTax.ToString() + " kr",
                //Convert.ToInt32(invoice.ValueAddedTax).ToString() + " kr",
                invoice.RutDeductionAmount.ToString() + " kr",
                //Convert.ToInt32(invoice.RutDeductionAmount).ToString() + " kr",
                newDeliveryAddressSection,
                "",
                newRutInfoFirstRow,
                newRutInfoSecondRow
            };



            string expirationDateAsString = invoice.DueDate.ToString(dateFormat);

            //Konvertera till heltal
            string totalAmountAsString = Convert.ToInt32(invoice.GrossAmount).ToString();

            string totalAmountAsStringWithCurrency = totalAmountAsString + " SEK";












            //Hämta koordinater för specifika textstycken
            //-------------------------------------------

            float recipientInfoXCoordinate = 0f;
            float recipientInfoYCoordinate = 0f;


            float deliveryAddressX = 0f;
            float deliveryAddressY = 0f;

            float amountOfPaymentDaysX = 0f;
            float amountOfPaymentDaysY = 0f;



            //Y-positionerna blir något felaktiga så detta kommer läggas till på Y-axeln
            float yOffset = -3f;





            using (PdfTextFinder pdfTextFinder = new PdfTextFinder(pdfPageBase))
            {
                List<PdfTextFragment> pdfTextFragments = pdfTextFinder.Find("[Namn]");

                foreach (PdfTextFragment pdfTextFragment in pdfTextFragments)
                {
                    //Get the position of a specific instance
                    recipientInfoXCoordinate = pdfTextFragment.Positions[0].X;
                    recipientInfoYCoordinate = pdfTextFragment.Positions[0].Y + yOffset;
                }



                pdfTextFragments = pdfTextFinder.Find(oldDeliveryAddressSection);

                foreach (PdfTextFragment pdfTextFragment in pdfTextFragments)
                {
                    //Get the position of a specific instance
                    deliveryAddressX = pdfTextFragment.Positions[0].X;
                    deliveryAddressY = pdfTextFragment.Positions[0].Y + yOffset;
                }





                //Find text with number of payment days
                pdfTextFragments = pdfTextFinder.Find(oldPaymentDays);

                //Loop through the instances
                foreach (PdfTextFragment pdfTextFragment in pdfTextFragments)
                {
                    //Get the position of a specific instance
                    amountOfPaymentDaysX = pdfTextFragment.Positions[0].X;
                    amountOfPaymentDaysY = pdfTextFragment.Positions[0].Y + yOffset;
                }
            }





            //Ersätt alla förekomster av måltext med ny text
            for (int i = 0; i < oldText.Length; i++)
            {
                pdfTextReplacer.ReplaceAllText(oldText[i], newTextOnInvoice[i]);
            }


            if (ChbIsBusinessCustomer.Checked)
                pdfTextReplacer.ReplaceText("inkl. moms", "exkl. moms");







            string[] itemsPlaceholderText = {
                "[Artikel]", 
                "[Beskrivning]",
                "[Leveransdatum]",
                "[Antal]", 
                "[A-pris]", 
                "[Art.Belopp]", 
            };




            //Lägg in texten för artiklarna på räkningen
            for (int i = 0; i < articleTextOnInvoice.GetLength(0); i++)
            {
                for (int j = 0; j < articleTextOnInvoice.GetLength(1); j++)
                {
                    pdfTextReplacer.ReplaceText(itemsPlaceholderText[j], articleTextOnInvoice[i, j]);


                }

            }











            //Placera text med koordinater
            //------------------------------


            PdfTrueTypeFont recipientFont = PdfTrueTypeFont.FromFontFile(pdfDirectoryPath + "palatino_linotype/palab.ttf", 12.5f);
            PdfTrueTypeFont deliveryAddressFont = PdfTrueTypeFont.FromFontFile(pdfDirectoryPath + "palatino_linotype/pala.ttf", 11.8f);
            PdfTrueTypeFont expirationDateFont = PdfTrueTypeFont.FromFontFile(pdfDirectoryPath + "palatino_linotype/palab.ttf", 11.8f);
            PdfFont firstTotalAmountFont = new PdfFont(PdfFontFamily.Helvetica, 16f, PdfFontStyle.Bold);
            PdfFont secondTotalAmountFont = new PdfFont(PdfFontFamily.Helvetica, 12.5f, PdfFontStyle.Bold);


            PdfTrueTypeFont numberOfPaymentDaysFont = PdfTrueTypeFont.FromFontFile(pdfDirectoryPath + "segoe_ui/segoeuib.ttf", 9.6f);


            string recipientInfo = invoice.CustomerName + "\n" + invoice.BillingAddress.Street + "\n" + (invoice.BillingAddress.PostalCode + " " + invoice.BillingAddress.City).Trim();

            PdfTextWidget pdfTextWidget1 = new PdfTextWidget(invoice.DeliveryAddress.Street + "\n" + (invoice.DeliveryAddress.PostalCode + " " +
                invoice.DeliveryAddress.City).Trim(), deliveryAddressFont, PdfBrushes.Black);

            PdfTextWidget pdfTextWidget2 = new PdfTextWidget(expirationDateAsString, expirationDateFont, PdfBrushes.Black);
            PdfTextWidget pdfTextWidget3 = new PdfTextWidget(totalAmountAsStringWithCurrency, firstTotalAmountFont, PdfBrushes.Black);
            PdfTextWidget pdfTextWidget4 = new PdfTextWidget(totalAmountAsStringWithCurrency, secondTotalAmountFont, PdfBrushes.Black);




            pdfPageBase.Canvas.DrawString(recipientInfo, recipientFont, PdfBrushes.Black, recipientInfoXCoordinate, recipientInfoYCoordinate);
            pdfTextWidget1.Draw(pdfPageBase, deliveryAddressX, deliveryAddressY);
            pdfTextWidget2.Draw(pdfPageBase, new PointF(442.8f, 270f), new PdfTextLayout());
            pdfTextWidget3.Draw(pdfPageBase, new PointF(445.2f, 306.2f), new PdfTextLayout());
            pdfPageBase.Canvas.DrawString(newPaymentDays, numberOfPaymentDaysFont, PdfBrushes.Black, amountOfPaymentDaysX, amountOfPaymentDaysY);
            pdfTextWidget4.Draw(pdfPageBase, new PointF(437f, 727f), new PdfTextLayout());




            //QR code package
            //--------------------//

            string swishUrl = "https://app.swish.nu/1/p/sw/?sw=46706692286&amt=" + totalAmountAsString + "&msg=Faktura " + invoice.InvoiceNumber;
            QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
            QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(swishUrl, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode bitmapByteQRCode = new BitmapByteQRCode(qRCodeData);
            byte[] qrCodeAsBitmapBytes = bitmapByteQRCode.GetGraphic(20);


            //Place QR code
            using (MemoryStream memoryStream = new MemoryStream(qrCodeAsBitmapBytes))
            {
                PdfImage pdfImage = PdfImage.FromStream(memoryStream);

                //Specify the X and Y coordinates to start drawing the image
                float x = 74f;
                float y = 218.9f;

                //Specify the width and height of the image on the page
                float widthHeight = 75f;

                // Draw QR code at specified location
                pdfPageBase.Canvas.DrawImage(pdfImage, x, y, widthHeight, widthHeight);
            }



            string newFileName = "Faktura " + invoice.InvoiceNumber;


            pdfDocument.DocumentInformation.Title = newFileName;
            pdfDocument.DocumentInformation.Author = "Lillemors Allservice";


            // Save new PDF
            pdfDocument.SaveToFile(Server.MapPath("~/wwwroot/Ny_faktura.pdf"));

            // Dispose resources
            pdfDocument.Dispose();











            //Infoga i databas
            /*****************************/


            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(ConnectionStringProvider.ConnectionString))
                {
                    mySqlConnection.Open();


                    int isBusinessCustomer = 0;

                    if (ChbIsBusinessCustomer.Checked)
                        isBusinessCustomer = 1;

                    NumberFormatInfo numberFormatInfo = new NumberFormatInfo
                    {
                        NumberDecimalSeparator = "."
                    };


                    //Kvitto som ska lagras i databas
                    string receipt =
                        $"Datum: {invoice.CreationDate.ToString(dateFormat)},\\n" +
                        $"Kundnummer: {invoice.CustomerNumber},\\n" +
                        $"Fakturanummer: {invoice.InvoiceNumber},\\n" +
                        $"Företagskund (1=Ja, 0=Nej): {isBusinessCustomer},\\n" +
                        $"Namn: {invoice.CustomerName},\\n" +
                        $"Förfallodatum: {invoice.DueDate.ToString(dateFormat)},\\n" +
                        $"Totalsumma netto: {invoice.NetAmount.ToString("0.00", numberFormatInfo)},\\n" +
                        $"Moms: {invoice.ValueAddedTax.ToString("0.00", numberFormatInfo)},\\n" +
                        $"RUT-avdrag: {invoice.RutDeductionAmount.ToString("0.00", numberFormatInfo)},\\n" +
                        $"Totalsumma brutto: {invoice.GrossAmount.ToString("0.00", numberFormatInfo)},\\n" +
                        $"Gata: {invoice.BillingAddress.Street},\\n" +
                        $"Postnummer: {invoice.BillingAddress.PostalCode},\\n" +
                        $"Ort: {invoice.BillingAddress.City},\\n" +
                        $"Gata (Leveransadress): {invoice.DeliveryAddress.Street},\\n" +
                        $"Postnummer (Leveransadress): {invoice.DeliveryAddress.PostalCode},\\n" +
                        $"Ort (Leveransadress): {invoice.DeliveryAddress.City}";

                    foreach (var item in finalArticles)
                    {
                        receipt += ",\\n\\n" +
                        $"Artikel: {item.Article},\\n" +
                        $"Beskrivning: {item.Description},\\n" +
                        $"Leveransdatum: {item.DeliveryDate},\\n" +
                        $"Antal: {item.Quantity.ToString(numberFormatInfo)},\\n" +
                        $"À-pris exkl. moms: {item.PricePerUnit.ToString("0.00", numberFormatInfo)},\\n" +
                        $"Belopp inkl. moms: {item.Amount.ToString("0.00", numberFormatInfo)},\\n" +
                        $"RUT-avdrag (1=Ja, 0=Nej): {!item.IsNotRut}";
                    }

                    


                    List<string> insertIntoDatabase = new List<string> {
                    invoice.CreationDate.ToString(dateFormat),
                    invoice.CustomerNumber,
                    invoice.InvoiceNumber,
                    isBusinessCustomer.ToString(),
                    invoice.CustomerName,
                    invoice.DueDate.ToString(dateFormat),
                    invoice.NetAmount.ToString(numberFormatInfo),
                    invoice.ValueAddedTax.ToString(numberFormatInfo),
                    invoice.RutDeductionAmount.ToString(numberFormatInfo),
                    invoice.GrossAmount.ToString(numberFormatInfo),
                    receipt,
                    invoice.BillingAddress.Street,
                    invoice.BillingAddress.PostalCode,
                    invoice.BillingAddress.City,
                    invoice.DeliveryAddress.Street,
                    invoice.DeliveryAddress.PostalCode,
                    invoice.DeliveryAddress.City,
                    finalArticles.Count.ToString()
                };



                    foreach (var item in finalArticles)
                    {
                        insertIntoDatabase.AddRange(new string[]
                        {
                        item.Article,
                        item.Description,
                        item.DeliveryDate.ToString(),
                        item.Quantity.ToString(numberFormatInfo),
                        item.PricePerUnit.ToString(numberFormatInfo),
                        item.Amount.ToString(numberFormatInfo),
                        Convert.ToInt32(!item.IsNotRut).ToString()
                        });
                    }



                    while (insertIntoDatabase.Count < 53)
                    {
                        insertIntoDatabase.Add("");
                    }



                    for (int i = 0; i < insertIntoDatabase.Count; i++)
                    {
                        if (string.IsNullOrWhiteSpace(insertIntoDatabase[i]) || insertIntoDatabase[i] == "0001-01-01")
                            insertIntoDatabase[i] = "NULL";

                        else
                            insertIntoDatabase[i] = "'" + insertIntoDatabase[i] + "'";

                    }






                    //Anropa MySql-procedur som lägger in datan och lägg
                    //kommatecken mellan varje element så det läggs in korrekt
                    string insertData = "CALL insert_data(" + insertIntoDatabase[0];

                    for (int i = 1; i < insertIntoDatabase.Count; i++)
                        insertData += "," + insertIntoDatabase[i];

                    insertData += ");";


                    //MySqlTransaction behövs eftersom det är flera INSERT-statements i den lagrade proceduren som anropas
                    using (MySqlTransaction mySqlTransaction = mySqlConnection.BeginTransaction())
                    {
                        using (MySqlCommand mySqlCommand = new MySqlCommand
                        {
                            Connection = mySqlConnection,
                            CommandText = insertData,
                            CommandType = CommandType.Text
                        })
                        {

                        



                            //Exekvera procedur
                            if (canEditDatabase == 1)
                            {
                                mySqlCommand.ExecuteNonQuery();

                            
                                // Commit only if everything succeeded
                                mySqlTransaction.Commit();
                            }

                        }
                    }

                    //Töm sessioner eftersom faktura är skapad
                    Session.Remove("addressesRetrieved");
                            Session.Remove("serviceNames");
                            Session.Remove("invoiceNumbers");
                            IsDataLoadedSuccessfully = false;

                    if (showInvoiceAfterCreation == 1)
                    {
                        //False här gör att ThreadAbortException inte körs och därmed att processorn inte tar stryk
                        Response.Redirect("~/WebForm1.aspx?" + newFileName, false);
                                
                        //Sluta köra sidan helt, sparar också processorkraft
                        Context.ApplicationInstance.CompleteRequest();

                        


                    }
                        
                }
            }

            catch (MySqlException ex)
            {
                ErrorHandler.HandleError(ErrorHandler.ErrorTypes.InvoiceCreationFailed, ex);
            }
        }
    }
}