using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Labb_4_ConsolAPP
{
    public class CosmoDB
    {
        private const string EndpointUrl = "https://adventuredb.documents.azure.com:443/";
        private const string PKey = "flO2gE3q4Evebl52oLD1WYLVn5DG95E1DYpEYzPTWZnSP4NZpir7FhAo49W9gBYTGAZFCtJLRMZtt2RSWOElfA==";
        private DocumentClient client;

        private static string databaseName = "info";
        private static string[] collections = { "Emails", "NonExaminedPhotos", "ExaminedPhotos" };
        private UserEmail emailDoc;
        private UserPhoto photoDoc;

        // constructor 
        public CosmoDB()
        {
            client = new DocumentClient(new Uri(EndpointUrl), PKey);
        }

        public void CreateDBIfNotExists()
        {
            client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName });
        }

        public  void CreateCollectionsIfNotExists()
        {

           client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseName), new DocumentCollection { Id = collections[0] });

           client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseName), new DocumentCollection { Id = collections[1] });

           client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseName), new DocumentCollection { Id = collections[2] });
        }

        // Create documents with class - instances.
        public async Task CreateDocuments()
        {
            Console.Clear();
            Console.WriteLine("Add Email");
            string email = Console.ReadLine();
            Console.WriteLine("Add a photo url");
            string photoUrl = Console.ReadLine();
            if (DataContext.IsValid(email))
            {
                emailDoc = new UserEmail(email);
                photoDoc = new UserPhoto(photoUrl, email);
                InsertUserIfNotExists();
            }
            else
            {
                Console.WriteLine("Invalid Input");
            }
            Console.Clear();
        }

        public class UserEmail
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

        public class UserPhoto
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
        public async void InsertUserIfNotExists() /*where T : //?*/
        {
            try
            {
                await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collections.First(), this.emailDoc.Id));
                Console.WriteLine($"User {emailDoc.EmailAdress} is already in DB!");
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                   await  client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collections[0]), this.emailDoc);

                    await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collections[1]), this.photoDoc);

                    Console.WriteLine("User added " + emailDoc.EmailAdress);

                }
                else
                {
                    Console.WriteLine("User was not added " + emailDoc.EmailAdress);
                }
            }
        }

        public void GetUsers()
        {
            Console.Clear();
            IQueryable<string> showPlayerQueryable = this.client.CreateDocumentQuery<UserEmail>(
                        UriFactory.CreateDocumentCollectionUri(databaseName, collections[0]), null)
                    .Select(p => p.EmailAdress);

            string users = null;
            foreach (var user in showPlayerQueryable)
            {
                users += user + "\n";
            }
            Console.WriteLine($"Users\n\n" + users);

        }
        public void GetPendingPhotos()
        {
            Console.Clear();
            IQueryable<string> nonExaminedPhotos = this.client.CreateDocumentQuery<UserPhoto>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collections[1]), null)
                    .Select(p => p.PhotoUrl);

            string pendingPhoto = null;
            foreach (var photo in nonExaminedPhotos)
            {
                pendingPhoto += photo + "\n";
            }
            Console.WriteLine($"Pending photos for approval\n\n" + pendingPhoto);
        }


    }

}
