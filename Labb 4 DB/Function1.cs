using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Labb_4_DB

                                                                     //LABB-BESKRIVNING!

//Appen ska vara en console-applikation, som kan ansluta till en Cosmos databas i Azure-molnet.Man ska kunna lägga till användare som har e-postadress och profilbild.Men för att hindra att vilka bilder som helst läggs upp så ska de granskas av en administratör innan de godkänns.Det ska också gå att se vilka användare som finns inlagda och vilka bilder som finns i kö för att godkännas.

//Skapa tabeller/collections för:
//användare(e-postadress)
//bilder som inte granskats
//bilder som granskats och är godkända
//                                                                           API

//Skapa en Azure Function med en HTTP trigger.När man kör funktionen så kan den anropas genom att man skriver in en speciell URL i webbläsarens adressfält. Lokalt: localhost:portnr/sökväg? key = value.Det som står efter frågetecknet kallas querystring. Vi kan skicka "parametrar" till funktionen genom att lägga till dem till querystring.Om jag till exempel vill skicka en parameter bad med värdet "boll" så skriver man? bad = boll sist i URL:en.Lägg till flera parametrar till querystring genom att sätta ett &-tecken emellan.

//Man ska kunna anropa funktionen på olika sätt:

//mode= viewReviewQueue - ska returnera en sträng som talar om vilka bilder som finns att granska
//mode= approve & id =[id] - ska godkänna bilden med[id], dvs flytta över dokumentet från gransknings-collection till godkända-bilder-collection; sedan returnera en sträng som talar om ifall den lyckades eller inte
//mode=reject&id=[id] - ska ta bort bilden med[id] från gransknings-collection och returnera en sträng som talar om ifall den lyckades eller inte


{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            var email = (from kv in req.GetQueryNameValuePairs()
                        where kv.Key == "email"
                        select kv.Value).FirstOrDefault();

            var photo = (from kv in req.GetQueryNameValuePairs()
                         where kv.Key == "photo"
                         select kv.Value).FirstOrDefault();

            //mode=viewReviewQueue - ska returnera en sträng som talar om vilka bilder som finns att granska
            var mode = (from kv in req.GetQueryNameValuePairs()
                        where kv.Key == "mode"
                        select kv.Value).FirstOrDefault();
            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            email = email ?? data?.email;
            photo = photo ?? data?.photo;

            if (email != null && photo != null)
            {
                // extension-method
                if (email.IsValid())
                {
                    try
                    {
                        // skapa instans
                        CosmosDB cosmos = new CosmosDB();

                        // skapa db / collections..
                        await cosmos.CreateDBIfNotExists();
                        await cosmos.CreateCollectionsIfNotExists();

                        // skapa document

                        await cosmos.CreateDocuments(email, photo);

                        // Lägg till användare om inte redan existerande

                        return await cosmos.InsertUserIfNotExists(MessageToUser, req);
                        
                    }
                    catch (DocumentClientException de)
                    {
                        Exception baseException = de.GetBaseException();
                        var message = $"{de.StatusCode} error occurred: {de.Message}, Message: {baseException.Message}";

                        return MessageToUser(message, false, req);
                    }
                    catch (Exception e)
                    {
                        Exception baseException = e.GetBaseException();
                        var message = $"Error: {e.Message}, message: {baseException.Message}";

                        return MessageToUser(message, false, req);
                        throw;
                    }
                    finally
                    {

                    }
                }

                else

                    return MessageToUser("Invalid email, try again!", false, req);
            }

            else if (mode == "viewReviewQueue")
            {
                // skriv ut alla bilder som finns att granska!
                CosmosDB cosmos = new CosmosDB();
                return cosmos.ExecuteSimpleQuery(MessageToUser, req);
                
            }
            else
                return MessageToUser("Invalid Input", false, req);
        }   



        private static bool IsValid(this string input)
        {
            var pattern = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*"
                        + "@"
                        + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";
            var match = Regex.Match(input, pattern);

            if (match.Success)
            {
                return true;
            }
            else
                return false;
        }
        
        // Ger användaren status efter query
        private static HttpResponseMessage MessageToUser(string message, bool valid, HttpRequestMessage req)
        {
            if (valid)
            {
                return req.CreateResponse(HttpStatusCode.OK, message);
            }
            
            else
                return req.CreateResponse(HttpStatusCode.BadRequest, message);

        }
    }
}
