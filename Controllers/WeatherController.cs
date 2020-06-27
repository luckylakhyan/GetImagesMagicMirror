using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TinnyMagicMiror.Code;

namespace TinnyMagicMiror.Controllers
{
    public class WeatherController : Controller
    {
        public IConfiguration _configuration { get; }

        string weatherApiBase = "https://api.openweathermap.org/data/2.5/weather?id={0}&units={1}&lang={2}&APPID={3}";
        string weatherforcastapi = "https://api.openweathermap.org/data/2.5/forecast/daily?id={0}&units={1}&lang={2}&APPID={3}";
        string apiId = "";
        string locationId = "";
        string units = "metric";
        string language = "en";
        public WeatherController (IConfiguration Configuration)
        {

            _configuration = Configuration;
            var weathesetting = _configuration.GetSection("weather");
            apiId       = weathesetting["apiId"];
            locationId  = weathesetting["locationId"];
            units       = weathesetting["units"];
            language    = weathesetting["language"];
            weatherApiBase = string.Format("https://api.openweathermap.org/data/2.5/weather?id={0}&units={1}&lang={2}&APPID={3}", locationId, units, language, apiId);
            weatherforcastapi = string.Format("https://api.openweathermap.org/data/2.5/forecast?id={0}&units={1}&lang={2}&APPID={3}", locationId, units, language, apiId);
                                            
        }
        public IActionResult GetWeather()
        {
            var result=WebRequestsHandel.GetWeatherdata(weatherApiBase, ",");
            return Json(result);
        }
        public IActionResult GetWeatherForcast()
        {
            var result = WebRequestsHandel.GetWeatherdata(weatherforcastapi, ",");
            return Json(result);
        }
    }
}
