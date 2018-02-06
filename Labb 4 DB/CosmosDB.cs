using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
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

        private static string databaseName = "info";
        private static string[] collections = new string[] { "Emails", "NonExaminedPhotos", "ExaminedPhotos" };
        private UserEmail emailDoc;
        private UserPhoto photoDoc;

        // constructor 
        public CosmosDB()
        {
            client = new DocumentClient(new Uri(EndpointUrl), PKey);
        }

        public async Task CreateDBIfNotExists()
        {
            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName });
        }

        public async Task CreateCollectionsIfNotExists()
        {

            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseName), new DocumentCollection { Id = collections[0] });

            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseName), new DocumentCollection { Id = collections[1] });

            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseName), new DocumentCollection { Id = collections[2] });
        }

        // Create documents with class - instances.
        public async Task CreateDocuments(string emailAdress, string photoUrl)
        {
           emailDoc = new UserEmail(emailAdress);
            photoDoc = new UserPhoto(photoUrl, emailAdress);
        }

        //private void WriteToConsole(string format, params object[] args)   // Debug syfte
        //{
        //    Debug.WriteLine(format, args);
        //    Debug.WriteLine("Press any key to continue ...");
        //    Console.ReadKey();
        //}

        private class UserEmail
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
            public string EmailAdress { get; set; }

            public UserEmail(string emailAdress)
            {
                this.Id = emailAdress;
                this.EmailAdress = emailAdress;
            }
        }

        private class UserPhoto
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
            public string PhotoUrl { get; set; }

            public UserPhoto(string photoUrl, string emailAdress)
            {
                this.Id = emailAdress;
                this.PhotoUrl = photoUrl;
            }
        }

        // CREATES USER IF NOT EXISTS!
        public async Task<HttpResponseMessage> InsertUserIfNotExists(Func<string, bool, HttpRequestMessage, HttpResponseMessage> messageCallback, HttpRequestMessage req) /*where T : //?*/
        {
            try
            {
                await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collections.First(), this.emailDoc.Id));

                return messageCallback($"User {this.emailDoc.EmailAdress} is already in DB!", false, req);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collections.First()), this.emailDoc);

                    await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collections.Last()), this.photoDoc);

                    return messageCallback("Added user " + this.emailDoc.EmailAdress, true, req);

                }
                else
                {
                    throw; // ??
                }
            }
        }

        public HttpResponseMessage ExecuteSimpleQuery(Func<string, bool, HttpRequestMessage, HttpResponseMessage> messageCallback, HttpRequestMessage req)
        {
            // Set some common query options
            //FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            string nonExaminedPhotosAsString = "NON-EXAMINED PHOTOS\n\n";

            IQueryable<string> nonExaminedPhotos = this.client.CreateDocumentQuery<UserPhoto>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collections[1]), null)
                    .Select(p => p.PhotoUrl);

            foreach (var photo in nonExaminedPhotos)
            {
                // callback Invoke!
                nonExaminedPhotosAsString += photo + "\n";
            }
            return messageCallback(nonExaminedPhotosAsString, true, req);

            // The query is executed synchronously here, but can also be executed asynchronously via the IDocumentQuery<T> interface
            //Console.WriteLine("Running LINQ query...");
            //foreach (Family family in familyQuery)
            //{
            //    Console.WriteLine("\tRead {0}", family);
            //}
        }
        public void ExecuteSimpleQueryApprovedPhotos(Func<string, bool, HttpRequestMessage, HttpResponseMessage> messageCallback, HttpRequestMessage req)
        {
            // Set some common query options
            //FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            var tempPhoto = (from pic in collections[1]
                             where collections[1] == photoDoc.PhotoUrl
                             select pic).ToString();

            var read = client.ReadDocumentAsync(tempPhoto);


            foreach (var picture in read.ToString())
            {
                req.CreateResponse(HttpStatusCode.OK, "Removed " + read);
                client.DeleteDocumentAsync(picture.ToString());
            }

            client.CreateDocumentQuery<UserPhoto>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collections[2]), null)
                .Select(p => p.PhotoUrl);

        }
    }
}
