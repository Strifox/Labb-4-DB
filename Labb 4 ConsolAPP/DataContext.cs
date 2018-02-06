﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Labb_4_ConsolAPP
{
    public class DataContext
    {
        static CosmoDB cosmoDB = new CosmoDB();
        public static void Menu()
        {
            Console.WriteLine("Welcome to Azure-Cosmos Labb!");
            Thread.Sleep(1000);

            ConsoleKey key;
            do
            {

                Console.WriteLine("1.) Add User\n2.) Show all users\n3.) Show pending pictures");
                key = Console.ReadKey(true).Key;
                switch(key)
                {
                    case ConsoleKey.D1:
                        // Fråga efter email och photo och lägg sedan till user!
                        cosmoDB.CreateDocuments();
                        break;
                    case ConsoleKey.D2:
                        cosmoDB.ExecuteSimpleQuery();
                        break;
                    case ConsoleKey.D3:
                        cosmoDB.ExecuteSimpleQuery();
                        break;
                    default:
                        Console.WriteLine("Wrong input. Try again!");
                        Console.ReadKey();
                        break;
            
                }

            } while (key != ConsoleKey.D1 && key != ConsoleKey.D2 && key != ConsoleKey.D3);
        }


        private static bool IsValid(string input)
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
    }
}
