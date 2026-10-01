<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WebApplication1.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

    <script>
       


window.onload = function () {
    // 1. Get the iframe element
    var iframe = document.getElementById('DummyFrame');

    if (iframe) {
        // 2. Set the iframe source to trigger the download (using the current page's query string)
        iframe.src = "WebForm2.aspx" + window.location.search;
    }

    // 3. Wait a tiny fraction of a second for the browser to register the download stream, 
    // then immediately display the PDF on screen.
    setTimeout(function () {
        window.location.replace('/wwwroot/Ny_faktura.pdf');
            }, 250);
        };


    </script>

</head>
<body>
    <form id="form1" runat="server">
        <div>
            <iframe id="DummyFrame" runat="server" hidden="hidden"></iframe>
        </div>
    </form>
</body>
</html>
