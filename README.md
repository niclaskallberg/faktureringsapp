<p align="center">
  <img width="1400" alt="Logo" src="https://github.com/user-attachments/assets/47b7e640-16fc-400f-bed6-23db005fe43c" />
</p>

Det här är en webbapp utvecklad och skräddarsydd från grunden åt ett företag där man har ett register med kunder och kan skriva fakturor till dessa.

Detta projekt utvecklas kontinuerligt.

### 🚀 Signifikanta funktioner

* **Error-mejl**: Ett mejl med information skickas till min inkorg när fel uppstår så jag får veta det direkt och snabbt kan börja felsöka, samt att det loggas i en fil på servern (Koden för detta finns i WebApplication/Utilities/ErrorHandler).
* **QR-kod för Swish**: Genererar fakturan som PDF-fil komplett med unik QR-kod för Swish där det automatiskt fylls i vilket nummer det ska betalas till, en låst summa samt ett låst meddelande bestående av fakturanumret.
* **Kryptering**: Kryptering till connection-stringar m.m. (WebApplication/Encryption.aspx) som en extra säkerhetsåtgärd, krypteringsfilen kan efteråt raderas från servern.
* **Skalbar app + databas**: Kan utvidgas till att ha fler funktioner i framtiden, som att ta emot ordrar eller automatiskt mejla fakturan till kund m.m.

### 🛠️ Arkitektur & tech-stack

* **Frontend**: ASP.NET Web Forms, Vanilla JavaScript, jQuery, CSS3, HTML5
* **Backend**: C#/.NET Framework
* **Databas**: MySQL
