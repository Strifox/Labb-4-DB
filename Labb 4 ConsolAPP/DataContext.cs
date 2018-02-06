using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Labb_4_ConsolAPP
{
    public static class DataContext
    {
        private static void MessageToUser(string message)
        {
            Console.WriteLine(message);
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
