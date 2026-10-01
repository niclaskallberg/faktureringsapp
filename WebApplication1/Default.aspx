<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication1.Default" MaintainScrollPositionOnPostback="true" %>



<!DOCTYPE html>

<%-- lang har jag själv lagt till --%>
<html xmlns="http://www.w3.org/1999/xhtml" lang="sv-SE">

<head runat="server">
    <link rel="preload" fetchpriority="high" as="image" href="wwwroot/img/logo.png" type="image/png" />

    <title>Faktureringsprogram</title>


    <%-- jQuery UI till skräddarsydda autocomplete-listor till textrutor --%>
    <%-- PlaceHolder-taggen är till för att kunna köra back-end-kod innanför taggen --%>
    <asp:PlaceHolder runat="server">

        <script src="<%= VirtualPathUtility.Combine(JQueryFolderPath, "jquery.js") %>"></script>
        <script src="<%= VirtualPathUtility.Combine(JQueryFolderPath, "jquery-ui.min.js") %>"></script>
        <link rel="stylesheet" href="<%= VirtualPathUtility.Combine(JQueryFolderPath, "jquery-ui.min.css") %>" />
        <link rel="stylesheet" href="<%= VirtualPathUtility.Combine(JQueryFolderPath, "jquery-ui.structure.min.css") %>" />
        <link rel="stylesheet" href="<%= VirtualPathUtility.Combine(JQueryFolderPath, "jquery-ui.theme.min.css") %>" />

    </asp:PlaceHolder>



    <%-- Href-attributet på detta element läggs till från code-behind --%>
    <link runat="server" id="CssLink" rel="stylesheet" />



    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>

</head>
    

<body>
    


    <form id="form1" runat="server">
        <main>
            <a id="LogoLink" href='<%= Page.ResolveUrl("~/") %>'>
                <asp:Image ID="LogoImage" runat="server" ImageUrl="~/wwwroot/img/logo.png" AlternateText="Logotyp" />
            </a>


            <h1 class="screen-reader-only"><%= this.Page.Header.Title%></h1>


            <%-- CSS-behållare till första raden av element --%>
            <div id="CssContainer">


                <%-- Mottagaruppgifter --%>
                <div runat="server" id="DivInvoiceRecipient">

                    <%-- data-has-blur="true" --%>
                    <asp:Label ID="LblInvoiceNumber" runat="server" Text="Fakturanummer" AssociatedControlID="TbxInvoiceNumber"></asp:Label>
                    <asp:TextBox ID="TbxInvoiceNumber" runat="server" data-oninput="attached-from-code-behind" data-onblur="attached-from-code-behind" ></asp:TextBox>
                    <span id="invoiceNumberFeedback"></span>


                    <asp:CheckBox ID="ChbChangeInvoiceNumberManually" runat="server" Text="Välj fakturanummer själv" AutoPostBack="true" onmousedown="resetCheckInvoiceNumber()" />

                    <br />


                    <asp:Label ID="LblCustomerNumber" runat="server" Text="Kundnummer" AssociatedControlID="TbxCustomerNumber"></asp:Label>
                    <asp:TextBox ID="TbxCustomerNumber" runat="server" oninput="this.classList.remove('auto-filled-background-color'); filterAndSelect(); storeListboxSelections();" ></asp:TextBox>
                
                    <asp:CheckBox ID="ChbIsBusinessCustomer" runat="server" onchange="toggleBusinessCustomer()" Text="Företagskund" />

                    <br />

                    <asp:Label ID="LblCustomerName" runat="server" Text="Namn" AssociatedControlID="TbxCustomerName"></asp:Label>
                    <asp:TextBox ID="TbxCustomerName" runat="server" oninput="this.classList.remove('auto-filled-background-color'); filterAndSelect(); storeListboxSelections(); capitalizeFirstLetter(this);" ></asp:TextBox>

                    <br />

                    <asp:Label ID="LblStreet" runat="server" Text="Gata" AssociatedControlID="TbxStreet"></asp:Label>
                    <asp:TextBox ID="TbxStreet" runat="server" oninput="this.classList.remove('auto-filled-background-color'); filterAndSelect(); storeListboxSelections(); capitalizeFirstLetter(this);" ></asp:TextBox>
                
                    <br />
                
                    <asp:Label ID="LblPostalCode" runat="server" Text="Postnummer" AssociatedControlID="TbxPostalCode"></asp:Label>
                    <asp:TextBox ID="TbxPostalCode" runat="server" oninput="this.classList.remove('auto-filled-background-color'); filterAndSelect(); storeListboxSelections();" ></asp:TextBox>

                    <br />

                    <asp:Label ID="LblCity" runat="server" Text="Ort" AssociatedControlID="TbxCity"></asp:Label>
                    <asp:TextBox ID="TbxCity" runat="server" oninput="this.classList.remove('auto-filled-background-color'); filterAndSelect();  storeListboxSelections(); capitalizeFirstLetter(this);" ></asp:TextBox>


                    <br />

                    <asp:Label ID="LblDeliveryAddress" runat="server" Text="Annan leveransadress" AssociatedControlID="ChbDeliveryAddress"></asp:Label>
                    <asp:CheckBox ID="ChbDeliveryAddress" runat="server" onclick="toggleDeliveryAddress()"/>

                    <br />

                    <div id="DivDeliveryAddress" runat="server" class="display-none">

                        <asp:Label ID="LblDeliveryStreet" runat="server" Text="Gata" AssociatedControlID="TbxDeliveryStreet"></asp:Label>
                        <asp:TextBox ID="TbxDeliveryStreet" runat="server" oninput="capitalizeFirstLetter(this)" ></asp:TextBox>
                
                        <br />

                        <asp:Label ID="LblDeliveryPostalCode" runat="server" Text="Postnummer" AssociatedControlID="TbxDeliveryPostalCode"></asp:Label>
                        <asp:TextBox ID="TbxDeliveryPostalCode" runat="server" ></asp:TextBox>

                        <br />

                        <asp:Label ID="LblDeliveryCity" runat="server" Text="Ort" AssociatedControlID="TbxDeliveryCity"></asp:Label>
                        <asp:TextBox ID="TbxDeliveryCity" runat="server" oninput="capitalizeFirstLetter(this)" ></asp:TextBox>

                        <br />
                    </div>

                    <asp:Label ID="LblPersonalIdentityNumber" runat="server" Text="Personnummer" AssociatedControlID="TbxPersonalIdentityNumber"></asp:Label>
                    <asp:TextBox ID="TbxPersonalIdentityNumber" runat="server" ></asp:TextBox>

                
                

                    <%-- Rensa mottagare --%>
                     <asp:Button ID="BtnClearInvoiceRecipient" runat="server" OnClientClick="clearInvoiceRecipient()" Text="Rensa textrutor" /> 




                </div>




                <%-- Kundregister --%>
                <div id="DivDatabase" runat="server">

                    <h2>Hämta uppgifter</h2>
                    

                    <div id="DivDatabaseButtons">
                        <asp:Button ID="BtnShowServices" runat="server" OnClick="BtnShowServices_Click" />
                        <asp:Button ID="BtnShowPastInvoices" runat="server" OnClientClick="getInvoices()" OnClick="BtnShowPastInvoices_Click" UseSubmitBehavior="False" />
                    </div>

                    <div id="DivDatabaseContainer" runat="server">

                        <div id="DivAutofill" runat="server">
                            <h3 class="screen-reader-only">Autofill</h3>
                    
                            <div id="DivSelectCustomer" runat="server">

                                <h4>Kundnummer och namn</h4>
                                <select size="4" name="LbxCustomers" id="LbxCustomers" onchange="toggleAutofillButtons(); storeListboxSelections();"></select> 
                                <asp:Button ID="BtnAutofillCustomer" runat="server" Text="Använd kund" OnClientClick="autofillCustomer(); filterAndSelect(); storeListboxSelections(); return false;" />
                
                            </div>

                            <div id="DivSelectAddress" runat="server">

                                <h4>Adresser</h4>
                                <select size="4" name="LbxPostalAddresses" id="LbxPostalAddresses" onchange="toggleAutofillButtons(); storeListboxSelections();"></select> 
                                <asp:Button ID="BtnAutofillAddress" runat="server" Text="Använd adress" OnClientClick="autofillAddress(); filterAndSelect(); storeListboxSelections(); return false;" />
                
                            </div>

                            <asp:Button ID="BtnAutofillAll" runat="server" Text="Använd båda" OnClientClick="autofillCustomer(); autofillAddress(); filterAndSelect(); storeListboxSelections(); document.activeElement.blur(); return false;" />

                        </div>

                        <div id="DivShowServices" runat="server" visible="false">

                            <asp:Repeater ID="rptServices" runat="server">
                                <HeaderTemplate>
                                    <table class="services-table">
                                        <tr>
                                            <th>Show in List</th>
                                            <th>Service Name</th>
                                        </tr>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr>
                                        <!-- Binds safely to the clean C# property names -->
                                        <td>
                                            <input type="checkbox" 
                                                   class="visibility-toggle" 
                                                   data-id='<%# Eval("Id") %>' 
                                                   <%# (bool)Eval("IsVisible") ? "checked" : "" %> />
                                        </td>
                                        <td><%# Eval("Name") %></td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </table>
                                </FooterTemplate>
                            </asp:Repeater>


                        </div>



                        <div id="DivPastInvoices" runat="server" visible="false" >

                            <asp:GridView ID="GrvInvoices" runat="server" CellPadding="4" ForeColor="#333333" GridLines="None" AllowSorting="True" AutoGenerateColumns="False" OnSorting="GrvInvoices_Sorting" >
                                <AlternatingRowStyle BackColor="White" />
                                <Columns>
                                    <asp:TemplateField ShowHeader="False">
                                        <ItemTemplate>
                                            <asp:Button ID="BtnViewInvoice" runat="server" Text="Välj" OnClientClick="openInvoice(this)" OnClick="BtnViewInvoice_Click" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="fldinvoicenumber" HeaderText="Fakturanr" SortExpression="fldinvoicenumber" />
                                    <asp:BoundField DataField="fldcustomernumber" HeaderText="Kundnr" SortExpression="fldcustomernumber" />
                                    <asp:BoundField DataField="fldname" HeaderText="Namn" SortExpression="fldname" />
                                    <asp:BoundField DataField="flddate" HeaderText="Datum" SortExpression="flddate" DataFormatString="{0:yyyy-MM-dd}"  NullDisplayText="Ingen datum" />
                                    <asp:BoundField DataField="fldnetamount" HeaderText="Belopp" SortExpression="fldnetamount" />
                                </Columns>
                                <EditRowStyle BackColor="#7C6F57" />
                                <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                                <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                                <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                                <RowStyle BackColor="#E3EAEB" />
                                <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                                <SortedAscendingCellStyle BackColor="#F8FAFA" />
                                <SortedAscendingHeaderStyle BackColor="#246B61" />
                                <SortedDescendingCellStyle BackColor="#D4DFE1" />
                                <SortedDescendingHeaderStyle BackColor="#15524A" />
                            </asp:GridView>








                            <div runat="server" id="DivEditInvoice" Visible="false">

                        
                                <h3>Faktura</h3>

                                <asp:Repeater ID="RptViewInvoice" runat="server">
                                    <HeaderTemplate>
                                        <div class="header-block">

                                            
                                            <table class="invoice-header">

                                                    <caption class="screen-reader-only">Fakturadetaljer</caption>
                                                <tbody>


                                                    <tr>
                                                        <th scope="row"><asp:Label ID="LblViewInvoiceInvoiceNumber" runat="server" Text="Fakturanummer" AssociatedControlID="TbxViewInvoiceInvoiceNumber"></asp:Label></th>

                                                       <td><asp:TextBox ID="TbxViewInvoiceInvoiceNumber" runat="server" Text='<%# RptViewInvoice.DataSource is System.Data.DataTable dt && dt.Rows.Count > 0 ? dt.Rows[0]["fldinvoicenumber"] : "" %>'></asp:TextBox></td>
                                                
                                                    </tr>
                                                    <tr>

                                                        <th scope="row"><asp:Label ID="LblViewInvoiceInvoiceDate" runat="server" Text="Datum" AssociatedControlID="TbxViewInvoiceInvoiceDate"></asp:Label></th>

                                                        <td>


                                                            <asp:TextBox ID="TbxViewInvoiceInvoiceDate" runat="server" TextMode="Date"

                                                                Text='<%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["flddate"] == DBNull.Value 
                                                                ? "No Date" 
                                                                : String.Format("{0:yyyy-MM-dd}", (DateTime)((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["flddate"]) %>'>

                                                            </asp:TextBox>
                                                        
                                                        </td>


                                                    </tr>




                                                    <tr>
                                                        <th scope="row">
                                                            <asp:Label ID="LblViewInvoiceExpirationDate" runat="server" Text="Förfallodatum" AssociatedControlID="TbxViewInvoiceExpirationDate"></asp:Label></th>

                                                        <td>
                                                            <asp:TextBox ID="TbxViewInvoiceExpirationDate" runat="server" TextMode="Date" 

                                                                Text='<%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldexpirationdate"] == DBNull.Value 
                                                                ? "No Date" 
                                                                : String.Format("{0:yyyy-MM-dd}", (DateTime)((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldexpirationdate"]) %>' />
                                                        </td>


                                                    </tr>



                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label1" runat="server" Text="Kundnummer" AssociatedControlID="TbxViewInvoiceCustomerNumber"></asp:Label></th>
                                                        <td><asp:TextBox ID="TbxViewInvoiceCustomerNumber" runat="server" Text='<%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldcustomernumber"] %>'></asp:TextBox></td>

                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label2" runat="server" Text="Namn" AssociatedControlID="TbxViewInvoiceCustomerName"></asp:Label></th>
                                                        <td><asp:TextBox ID="TbxViewInvoiceCustomerName" runat="server" Text='<%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldname"] %>'></asp:TextBox></td>

                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label3" runat="server" Text="Gata" AssociatedControlID="TbxViewInvoiceStreet"></asp:Label></th>
                                                        <td><asp:TextBox ID="TbxViewInvoiceStreet" runat="server" Text='<%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldstreet"] %>'></asp:TextBox></td>

                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label4" runat="server" Text="Postnummer" AssociatedControlID="TbxViewInvoicePostalCode"></asp:Label></th>
                                                        <td><asp:TextBox ID="TbxViewInvoicePostalCode" runat="server" Text='<%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldpostalcode"] %>'></asp:TextBox></td>

                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label5" runat="server" Text="Ort" AssociatedControlID="TbxViewInvoiceCity"></asp:Label></th>
                                                        <td><asp:TextBox ID="TbxViewInvoiceCity" runat="server" Text='<%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldcity"] %>'></asp:TextBox></td>

                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label6" runat="server" Text="Leveransgata" AssociatedControlID="TbxViewInvoiceDeliveryStreet"></asp:Label></th>
                                                        <td><asp:TextBox ID="TbxViewInvoiceDeliveryStreet" runat="server" Text='<%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["deliverystreet"] %>'></asp:TextBox></td>

                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label7" runat="server" Text="Leveransort" AssociatedControlID="TbxViewInvoiceDeliveryPostalCode"></asp:Label></th>
                                                        <td><asp:TextBox ID="TbxViewInvoiceDeliveryPostalCode" runat="server" Text='<%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["deliverypostalcode"] %>'></asp:TextBox></td>

                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label8" runat="server" Text="Leveransstad" AssociatedControlID="TbxViewInvoiceDeliveryCity"></asp:Label></th>
                                                        <td><asp:TextBox ID="TbxViewInvoiceDeliveryCity" runat="server" Text='<%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["deliverycity"] %>'></asp:TextBox></td>

                                                    </tr>

                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label9" runat="server" Text="Nettosumma" AssociatedControlID="TbxViewInvoiceNetAmount"></asp:Label></th>
                                                        <td><asp:TextBox ID="TbxViewInvoiceNetAmount" runat="server" Text='<%# Convert.ToDecimal(((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldgrossamount"], System.Globalization.CultureInfo.InvariantCulture).ToString("N2") %>'></asp:TextBox></td>

                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label10" runat="server" Text="Momssumma" AssociatedControlID="TbxViewInvoiceValueAddedTax"></asp:Label></th>
                                                        <td><asp:TextBox ID="TbxViewInvoiceValueAddedTax" runat="server" Text='<%# Convert.ToDecimal(((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldvalueaddedtax"], System.Globalization.CultureInfo.InvariantCulture).ToString("N2") %>'></asp:TextBox></td>

                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label11" runat="server" Text="RUT-avdrag" AssociatedControlID="TbxViewInvoiceRutDeductionAmountAmount"></asp:Label></th>
                                                        <td><asp:TextBox ID="TbxViewInvoiceRutDeductionAmountAmount" runat="server" Text='<%# Convert.ToDecimal(((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldrutdeduction"], System.Globalization.CultureInfo.InvariantCulture).ToString("N2") %>'></asp:TextBox></td>

                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label12" runat="server" Text="Bruttosumma" AssociatedControlID="TbxViewInvoiceGrossAmount"></asp:Label></th>
                                                        <td><asp:TextBox ID="TbxViewInvoiceGrossAmount" runat="server" Text='<%# Convert.ToDecimal(((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldnetamount"], System.Globalization.CultureInfo.InvariantCulture).ToString("N2") %>'></asp:TextBox></td>

                                                    </tr>
                                                </tbody>

                                            </table>
                                        </div>
                                        <hr />
                                    </HeaderTemplate> 


                                    <ItemTemplate>
                                        <div class="view-invoice-article-block" >
                                            <table class="my-details-style">
                                                <caption>Artikel <%# Container.ItemIndex + 1 %></caption>


                                                <tbody>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label11" runat="server" Text="Tjänst" AssociatedControlID="txtService"></asp:Label></th>
                                                        <td><asp:TextBox ID="txtService" runat="server" Text='<%# Eval("fldservicename") %>' /></td>
                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label13" runat="server" Text="Beskrivning" AssociatedControlID="txtDesc"></asp:Label></th>
                                                        <td><asp:TextBox ID="txtDesc" runat="server" Text='<%# Eval("flddescription") %>' /></td>
                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label14" runat="server" Text="Leveransdatum" AssociatedControlID="txtDate"></asp:Label></th>
                                                        <td><asp:TextBox ID="txtDate" runat="server" Text='<%# Eval("flddeliverydate", "{0:yyyy-MM-dd}") %>' /></td>
                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label15" runat="server" Text="Antal" AssociatedControlID="txtQ"></asp:Label></th>
                                                        <td><asp:TextBox ID="txtQ" runat="server" Text='<%# Eval("fldquantity") %>' /></td>
                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label16" runat="server" Text="À-pris" AssociatedControlID="txtPpu"></asp:Label></th>
                                                        <td><asp:TextBox ID="txtPpu" runat="server" Text='<%# Eval("fldpriceperunit") %>' /></td>
                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label17" runat="server" Text="Total" AssociatedControlID="txtAmount"></asp:Label></th>
                                                        <td><asp:TextBox ID="txtAmount" runat="server" Text='<%# Eval("fldamount") %>' /></td>
                                                    </tr>
                                                    <tr>
                                                        <th scope="row"><asp:Label ID="Label18" runat="server" Text="RUT" AssociatedControlID="txtIsRutDeductible"></asp:Label></th>
                                                        <td><asp:TextBox ID="txtIsRutDeductible" runat="server" Text='<%# Eval("fldisrutdeductible") %>' /></td>
                                                    </tr>

                                                </tbody>
                                            </table>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                        
                        




                                <%--<asp:Button ID="BtnUpdateInvoice" runat="server" Text="Spara ändringar" OnClick="BtnUpdateInvoice_Click"/>--%>
                                <asp:Button ID="BtnDeleteInvoice" runat="server" Text="Radera faktura" OnClick="BtnDeleteInvoice_Click" OnClientClick="deleteInvoice()" />

                            </div>
                        </div>
                    </div>
                </div>
            </div>


        

            <%-- Artiklar --%>
            <div runat="server" id="DivArticleList">
            
                <h2>Artiklar</h2>



                    <table id="TblArticleList">
                        <thead>

                            <tr>
                                <th>Tjänst/vara</th>
                                <th>Mer information</th>
                                <th>Leveransdatum</th>
                                <th>Antal</th>
                                <th>À-pris inkl. moms</th>
                                <th>Totalt</th>
                                <th></th>
                                <th></th>
                            </tr>
                        </thead>

                        
        


                        <tbody>
                            
                            <!-- Row 1 acts as your base template and first row -->
                            <tr class="data-row">
                                <td><input type="text" name="TbxArticle[]" oninput="capitalizeFirstLetter(this)" /></td>
                                <td><input type="text" name="TbxDescription[]" oninput="capitalizeFirstLetter(this)" /></td>


                                <td class="delivery-date-cell"><input type="date" name="TbxDeliveryDate[]" class="empty" onfocus="this.classList.remove('empty');" />

                                </td>


                                <td><input type="number" name="TbxQuantity[]" value="1" step="0.5" min="0.5" class="tbxquantity" /></td>
                                <td><input type="text" name="TbxPricePerUnit[]" value="500" class="tbxpriceperunit" /></td>
                                <td><input type="text" name="TbxRowAmount[]" value="500" class="tbx-article-total" /></td> 
                                <td>
                                    <input type="checkbox" name="ChbNotRut_Placeholder" onchange="updateCheckboxValue(this)" />
                                    <input type="hidden" name="ChbNotRut[]" value="false" />
                                    Inget RUT-avdrag
                                </td>
                                <td>
                                    <button type="button" class="btn-delete" onclick="removeRow(this)" style="display:none;">Ta bort</button>
                                </td>
                            </tr>





                        </tbody>

                    </table>
                    <button id="BtnAddArticle" type="button" onclick="addArticle()">Lägg till rad</button>


                    <fieldset class="invoice-summary">

                        <!-- Visible to screen readers, invisible to sighted users -->
                        <legend class="screen-reader-only">Invoice Amount Breakdown</legend>


                        <asp:Label ID="LblNetAmount" runat="server" Text="Summa exkl. moms" AssociatedControlID="TbxNetAmount"></asp:Label><asp:TextBox ID="TbxNetAmount" runat="server" value="400,00"></asp:TextBox>
                        <asp:Label ID="LblValueAddedTax" runat="server" Text="Momsbelopp (25 %)" AssociatedControlID="TbxValueAddedTax"></asp:Label><asp:TextBox ID="TbxValueAddedTax" runat="server" value="100,00"></asp:TextBox>
                        <asp:Label ID="LblRutDeductionAmount" runat="server" Text="RUT-avdrag" AssociatedControlID="TbxRutDeductionAmount"></asp:Label><asp:TextBox ID="TbxRutDeductionAmount" runat="server" value="250,00"></asp:TextBox>
                        <asp:Label ID="LblGrandInvoiceTotal" runat="server" Text="Att betala" AssociatedControlID="TbxGrandInvoiceTotal"></asp:Label><asp:TextBox ID="TbxGrandInvoiceTotal" runat="server" value="250,00" ></asp:TextBox>
        
                    </fieldset>

                </div>

            
                

            <asp:Label ID="LblError" runat="server" ></asp:Label>

            <%-- Validera först hos klient, sedan skicka till server --%>
            <asp:Button ID="BtnCreateInvoice" runat="server" Text="Skapa faktura" OnClientClick="return validate();" OnClick="BtnCreateInvoice_Click" />


            <%--<div class="tooltip-container">
                Hover over me
                <span class="tooltip-text">This is the tooltip!</span>
            </div>


            <style>

                /* Container for the text and the tooltip */
                .tooltip-container {
                  position: relative;
                  display: inline-block;
                  cursor: pointer;
                  border-bottom: 1px dashed #333; /* Visual cue for the user */
                }

                /* Style and hide the tooltip text by default */
                .tooltip-text {
                  visibility: hidden;
                  background-color: #333;
                  color: #fff;
                  text-align: center;
                  padding: 5px 10px;
                  border-radius: 4px;
                  font-size: 14px;
  
                  /* Position the tooltip above the text */
                  position: absolute;
                  bottom: 125%; 
                  left: 50%;
                  transform: translateX(-50%);
                  white-space: nowrap;
                  z-index: 1;
                }

                /* Show the tooltip when hovering */
                .tooltip-container:hover .tooltip-text {
                  visibility: visible;
                }

            </style>--%>


            
        

        <%-- Bool-värden --%>
        <asp:HiddenField ID="HdnOpenDatabaseButtonPressed" runat="server" />
        <asp:HiddenField ID="HdnInvoiceNumber" runat="server" />
        <asp:HiddenField ID="HdnConfirmDeletion" runat="server" />
        <asp:HiddenField ID="HdnConfirmCreation" runat="server" />
        <asp:Literal ID="LtrData" runat="server"></asp:Literal>
            </main>
    </form>
</body>
</html>