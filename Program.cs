using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LogAnalyze
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> logs = File.ReadLines("D:\\log.txt", Encoding.UTF8).ToList<string>();

            APIcall POST_add_ticket = new APIcall { method = "POST", api = "/api/online/platforms/facebook_canvas/users/*/add_ticket", calls = 0 };
            APIcall POST_users = new APIcall { method = "POST", api = "/api/users/*", calls = 0 };
            APIcall GET_users = new APIcall { method = "GET", api = "/api/users/*", calls = 0 };


            foreach (string log in logs)
            {
                string[] logEntries = log.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                DateTime entryDate = Convert.ToDateTime(logEntries[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                string entryMethod = logEntries[3].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                string entryAPI = logEntries[4].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                string entryHost = logEntries[5].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                string entryIP = logEntries[6].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1].TrimStart(new char[] { '"' }).TrimEnd(new char[] { '"' });
                string entryDyno = logEntries[7].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

                //bool match = WildCardCompare("/api/online/platforms/facebook_canvas/users/*/add_ticket", entryAPI);

                if (WildCardCompare(POST_add_ticket.api, entryAPI) && entryMethod == POST_add_ticket.method)
                {
                    POST_add_ticket.calls++;
                }
                else if (WildCardCompare(GET_users.api, entryAPI) && entryMethod == GET_users.method)
                {
                    GET_users.calls++;
                }
                else if (WildCardCompare(POST_users.api, entryAPI) && entryMethod == POST_users.method)
                {
                    POST_users.calls++;
                }
            }
        }

        public static bool WildCardCompare(string pattern, string text, bool caseSensitive = false)
        {
            pattern = pattern.Replace(".", @"\.");
            pattern = pattern.Replace("?", ".");
            pattern = pattern.Replace("*", ".*?");
            pattern = pattern.Replace(@"\", @"\\");
            pattern = pattern.Replace(" ", @"\s");
            return new Regex(pattern, caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase).IsMatch(text);
        }
    }

    public struct APIcall
    {
        public string method;
        public string api;
        public int calls;
    }
}
