using Ical.Net;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TinnyMagicMiror.Code
{
    public static class WebRequestsHandel
    {
        public static string GetCalanderEvents(string calurl, string delimiter)
        {
            WebClient client = new WebClient();
            string reply = client.DownloadString(calurl);
            if (!string.IsNullOrEmpty(reply))
            {
                var calendar = Calendar.Load(reply);
                var caleves = calendar.GetOccurrences(DateTime.Now, DateTime.Now.AddDays(15));
                var eventsJson = new StringBuilder();

                if (caleves.Count > 0)
                {
                    foreach (var item in caleves)
                    {
                        eventsJson.Append(delimiter);
                        eventsJson.Append($"{{'EventTitle':'{((Ical.Net.CalendarEvent)item.Source).Summary}','StartTime':'{item.Period.StartTime.AsSystemLocal}'}}");
                        delimiter = ",";


                    }
                    return eventsJson.ToString();
                }

            }
            return ("No upcoming events found.");

        }

        public static string GetWeatherdata(string calurl, string delimiter)
        {
            WebClient client = new WebClient();
            string reply = client.DownloadString(calurl);
            if (!string.IsNullOrEmpty(reply))
            {
                return reply;
            }
            return "";
        }
    }
}