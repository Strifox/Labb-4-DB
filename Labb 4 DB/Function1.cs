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

//Appen ska vara en console-applikation, som kan ansluta till en Cosmos databas i Azure-molnet.Man ska kunna l�gga till anv�ndare som har e-postadress och profilbild.Men f�r att hindra att vilka bilder som helst l�ggs upp s� ska de granskas av en administrat�r innan de godk�nns.Det ska ocks� g� att se vilka anv�ndare som finns inlagda och vilka bilder som finns i k� f�r att godk�nnas.

//Skapa tabeller/collections f�r:
//anv�ndare(e-postadress)
//bilder som inte granskats
//bilder som granskats och �r godk�nda
//                                                                           API

//Skapa en Azure Function med en HTTP trigger.N�r man k�r funktionen s� kan den anropas genom att man skriver in en speciell URL i webbl�sarens adressf�lt. Lokalt: localhost:portnr/s�kv�g? key = value.Det som st�r efter fr�getecknet kallas querystring. Vi kan skicka "parametrar" till funktionen genom att l�gga till dem till querystring.Om jag till exempel vill skicka en parameter bad med v�rdet "boll" s� skriver man? bad = boll sist i URL:en.L�gg till flera parametrar till querystring genom att s�tta ett &-tecken emellan.

//Man ska kunna anropa funktionen p� olika s�tt:

//mode= viewReviewQueue - ska returnera en str�ng som talar om vilka bilder som finns att granska
//mode= approve & id =[id] - ska godk�nna bilden med[id], dvs flytta �ver dokumentet fr�n gransknings-collection till godk�nda-bilder-collection; sedan returnera en str�ng som talar om ifall den lyckades eller inte
//mode=reject&id=[id] - ska ta bort bilden med[id] fr�n gransknings-collection och returnera en str�ng som talar om ifall den lyckades eller inte


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
                    // returnera en str�ng med information om vilka bilder som finns att granska!
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
