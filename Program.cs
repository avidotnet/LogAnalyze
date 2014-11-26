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

            APIcall GET_count_pending_messages = new APIcall { api = "/api/users/*/count_pending_messages" };
            APIcall GET_get_messages = new APIcall { api = "/api/users/*/get_messages" };
            APIcall GET_get_friends_progress = new APIcall { api = "/api/users/*/get_friends_progress" };
            APIcall GET_get_friends_score = new APIcall { method = "GET", api = "/api/users/*/get_friends_score" };
            APIcall GET_users = new APIcall { api = "/api/users/*" };
            APIcall POST_users = new APIcall { method = "POST", api = "/api/users/*" };


            foreach (string log in logs)
            {
                string[] logEntries = log.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                DateTime entryDate = Convert.ToDateTime(logEntries[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                string entryMethod = logEntries[3].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                string entryAPI = logEntries[4].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                string entryHost = logEntries[5].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                string entryIP = logEntries[6].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim().TrimStart(new char[] { '"' }).TrimEnd(new char[] { '"' });
                string entryDyno = logEntries[7].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                string entryConnect = logEntries[8].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim().TrimEnd(new char[] { 'm', 's' });
                string entryService = logEntries[9].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim().TrimEnd(new char[] { 'm', 's' });
                int entryResponse = Convert.ToInt32(entryConnect) + Convert.ToInt32(entryService);

                //bool match = WildCardCompare("/api/online/platforms/facebook_canvas/users/*/add_ticket", entryAPI);

                if (WildCardCompare(GET_count_pending_messages.api, entryAPI) && entryMethod == GET_count_pending_messages.method)
                {
                    GET_count_pending_messages.calls++;
                    GET_count_pending_messages.responseTimes.Add(entryResponse);
                }
                else if (WildCardCompare(GET_get_messages.api, entryAPI) && entryMethod == GET_get_messages.method)
                {
                    GET_get_messages.calls++;
                    GET_get_messages.responseTimes.Add(entryResponse);
                }
                else if (WildCardCompare(GET_get_friends_progress.api, entryAPI) && entryMethod == GET_get_friends_progress.method)
                {
                    GET_get_friends_progress.calls++;
                    GET_get_friends_progress.responseTimes.Add(entryResponse);
                }
                else if (WildCardCompare(GET_get_friends_score.api, entryAPI) && entryMethod == GET_get_friends_score.method)
                {
                    GET_get_friends_score.calls++;
                    GET_get_friends_score.responseTimes.Add(entryResponse);
                }
                else if (WildCardCompare(GET_users.api, entryAPI) && entryMethod == GET_users.method)
                {
                    GET_users.calls++;
                    GET_users.responseTimes.Add(entryResponse);
                }
                else if (WildCardCompare(POST_users.api, entryAPI) && entryMethod == POST_users.method)
                {
                    POST_users.calls++;
                    POST_users.responseTimes.Add(entryResponse);
                }
            }

            GET_count_pending_messages.Calculate();
            GET_get_messages.Calculate();
            GET_get_friends_progress.Calculate();
            GET_get_friends_score.Calculate();
            GET_users.Calculate();
            POST_users.Calculate();

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

    public class APIcall
    {
        public string method = "GET";
        public string api;
        public int calls = 0;
        public List<int> responseTimes = new List<int>();
        public float responseMean = 0;
        public float responseMedian = 0;
        public int responseMode;

        public void Calculate()
        {
            int[] times = BubbleSort(this.responseTimes.ToArray());
            this.responseMean = CalculateMean(times);
            this.responseMedian = CalculateMedian(times);
            this.responseMode = CalculateMode(times);
        }

        public static int[] BubbleSort(int[] arr)
        {
            int i, j, temp, n = arr.Length;
            for (i = 0; i < n; i++)
            {
                for (j = n - 1; j > i; j--)
                {
                    if (arr[j] < arr[j - 1])
                    {
                        temp = arr[j];
                        arr[j] = arr[j - 1];
                        arr[j - 1] = temp;
                    }

                }
            }

            return arr;
        }

        public static float CalculateMean(int[] arr)
        {
            float mean = 0;
            int sum = 0, j = 0, n = arr.Length;
            while (j < n)
            {
                sum = sum + arr[j];
                j++;
            }
            mean = (float)sum / n;

            return mean;
        }

        public static float CalculateMedian(int[] arr)
        {
            float median = 0;
            int n = arr.Length;
            if (n % 2 != 0) median = arr[n / 2];
            else median = (arr[(n / 2) - 1] + arr[n / 2]) / (float)2;

            return median;
        }

        public static int CalculateMode(int[] arr)
        {
            int i, j, n = arr.Length;
            int[,] mode = new int[n, 2];
            //initialize 2D array storing numbers of occurences, and values
            for (i = 0; i < 2; i++)
                for (j = 0; j < n; j++) mode[j, i] = 0;
            mode[0, 0] = 1;

            for (i = 0; i < n; i++)
                for (j = 0; j < n - 1; j++)
                    if (arr[i] == arr[j + 1]) { ++mode[i, 0]; mode[i, 1] = arr[i]; }

            int max;
            int k = 0;
            max = mode[0, 0];
            for (j = 0; j < n; j++)
            {
                if (max < mode[j, 0]) { max = mode[j, 0]; k = j; }
            }

            return mode[k, 1];
        }
    }
}
