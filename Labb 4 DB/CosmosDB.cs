using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Labb_4_DB
{
    public class CosmosDB
    {
        private const string EndpointUrl = "https://labb4db.documents.azure.com:443/";
        private const string PKey = "WDNmnN1ZP4MwffILst3A4qQZ4pbtIsYVcfrwYgHZNwEO7gEZjgVnTu7Q00yjO0B94c3xD4cM2PEzR5uEcKOnPQ ==";
        private DocumentClient client;

        //private static int id = 1;

        public CosmosDB(string emailAdress, string photoUrl)
        {
            try
            {
                // Öppna
                CreateThings(emailAdress, photoUrl).Wait();
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, message: {1}", e.Message, baseException.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("Operation(s) completed!");
                Console.ReadKey();
            }
        }

        private async Task CreateThings(string emailAdress, string photoUrl)
        {
            client = new DocumentClient(new Uri(EndpointUrl), PKey);

            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = "info" });

            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("info"), new DocumentCollection { Id = "NonExaminedPhotos" });

            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("info"), new DocumentCollection { Id = "ExaminedPhotos" });

            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("info"), new DocumentCollection { Id = "Emails" });


            // Create documents with class - instances.

            UserEmail email = new UserEmail(emailAdress);
            UserPhoto photo = new UserPhoto(photoUrl, emailAdress);

            await CreateDocumentIfNotExists("info", "Emails", email);
            await CreateDocumentIfNotExists("info", "NonExaminedPhotos", photo);
            
        }

        private void WriteToConsole(string format, params object[] args)   // Debug syfte
        {
            Debug.WriteLine(format, args);
            Debug.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }

        public class UserEmail
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
            public string EmailAdress { get; set; }

            public UserEmail (string emailAdress)
            {
                this.Id = emailAdress;
                this.EmailAdress = emailAdress;
            }
        }

        public class UserPhoto
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
            public string PhotoUrl { get; set; }

            public UserPhoto (string photoUrl, string emailAdress)
            {
                this.Id = emailAdress;
                this.PhotoUrl = photoUrl;
            }
        }

        // public class 

        // CREATES DOCUMENTS IF NOT EXISTS!
        private async Task CreateDocumentIfNotExists(string databaseName, string collectionName, UserEmail document) /*where T : //??*/
        {
            try
            {
                await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, document.Id.ToString()));
                //WriteToConsole("Found {0}", document.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), document);
                    //this.WriteToConsole("Created {0}", document.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateDocumentIfNotExists(string databaseName, string collectionName, UserPhoto document) /*where T : //??*/
        {
            try
            {
                await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, document.Id.ToString()));
                //WriteToConsole("Found {0}", document.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), document);
                    //this.WriteToConsole("Created {0}", document.Id);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
