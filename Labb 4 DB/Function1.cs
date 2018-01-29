using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                         select kv.Value).FirstOrDefault();//gjgjghg

             // Get request body
             dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            email = email ?? data?.email;
            photo = photo ?? data?.photo;

            if (email != null && photo != null)
            {
                // extension-method
                if (true)
                {
                    // returnera en sträng med information om vilka bilder som finns att granska!
                    CosmosDB c = new CosmosDB(email, photo);
                    return req.CreateResponse(HttpStatusCode.OK, "User added!");
                }
                //else
                    
                    //return req.CreateResponse(HttpStatusCode.BadRequest, "Invalid Email, try again!");
            }

            else
                return req.CreateResponse(HttpStatusCode.BadRequest, "Enter both email and photo!");
        }   

        //private static void ReviewPhotos()
        //{
        //    CosmosDB c = new CosmosDB();
        //}

        private static bool IsValid(this string input)
        {
            var pattern = @"^[A-Za-z0-9]+\.?[A-Za-z0-9]+@{1}(gmail|yahoo|hotmail|outlook){1}\.{1}[A-Za-z]+$";
            var match = Regex.Match(input, pattern);

            if (match.Success)
            {
                return true;
            }
            else
                return false;
        }
    }
}
