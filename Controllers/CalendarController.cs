using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using TinnyMagicMiror.Code;

namespace TinnyMagicMiror.Controllers
{
    public class CalendarController : Controller
    {
        public IConfiguration _configuration { get; }
        public CalendarController(IConfiguration Configuration)
        {
            _configuration = Configuration;
        }
        public ActionResult Getcalendar()
        {
            try
            {
                var calsetting = _configuration.GetSection("calander").GetChildren().ToArray();
                if (calsetting != null)
                {
                    var _header = calsetting.Where(w => w.Key == "Header").First().Value;
                    var cals = calsetting.Where(w => w.Key == "calendars").ToArray()[0].GetChildren();
                    var calobject = new StringBuilder();
                    calobject.Append($"{{'Header':'{_header }', 'calendars':[");

                    var calsets = (calsetting.Where(w => w.Key == "calendars").ToArray()[0]).GetChildren();
                    string delimiter = "";
                    foreach (var calset in calsets)
                    {
                        var ValuesSym = calset["symbol"];
                        var ValuesUrl = calset["url"];

                        var cal = WebRequestsHandel.GetCalanderEvents(ValuesUrl, delimiter);
                        delimiter = ",";
                        calobject.Append(cal);

                    }
                    calobject.Append("]}");
                    return Json(calobject.ToString().Replace("'", "\""));
                }
                else
                {
                    throw new System.Exception("no calader configured");
                }

            }
            catch (System.Exception ex)
            {
                throw new System.Exception("Error getting Calander");
            }
        }
    }
}
