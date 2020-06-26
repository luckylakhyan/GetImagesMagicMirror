function Getweather() {
    $.ajax({
        type: "GET",
        contentType: 'application/json; charset=utf-8',
        url: "/Weather/GetWeather",
        datatype: 'json',
        success: function (data, status) {
            var result = JSON.parse(data);
            

            var weatherfeelslike = document.getElementById("weatherfeelslike");
            weatherfeelslike.innerHTML = result.main.feels_like.toFixed(0) + "°";

            var weathertemprature = document.getElementById("weathertemprature");
            weathertemprature.innerHTML = result.main.temp.toFixed(1) + "°";

            var weatherwindIcon = document.getElementById("weatherwindIcon");
            $(weatherwindIcon).addClass("from-" + result.wind.deg + "-deg");

            var weatherwindspeed = document.getElementById("weatherwindspeed");
            weatherwindspeed.innerHTML = result.wind.speed;

            var weatherwindDir = document.getElementById("weatherwindDir");
            var winddirecti = WeatherWindDirection(result.wind.deg);
            weatherwindDir.innerHTML = winddirecti;

            var weatherweathericon = document.getElementById("weatherweathericon");
            var weatherweathericonclass = WeatherIconClass(result.weather[0].icon);
            $(weatherweathericon).addClass(weatherweathericonclass);

            SumTimes(result.sys.sunset, result.sys.sunrise);

            setTimeout('Getweather()', 600000);
        }, error: function (data) {
            console.log("weather script error data", data);
        }
    });
}
function GetweatherForecast() {
    $.ajax({
        type: "GET",
        contentType: 'application/json; charset=utf-8',
        url: "/Weather/GetWeatherForcast",
        datatype: 'json',
        success: function (data, status) {
            var wdata = JSON.parse(data);
            
           
            var headerhtml = "Weather Forcast for " + wdata.city.name + ", " + wdata.city.country;
            var weatherforecastheadere = document.getElementById("weatherforecastheadere");
            weatherforecastheadere.innerHTML = headerhtml;

            var wftable = document.getElementById("weatherforecasttable");
            wftable.innerHTML ="";
            weeklyforcat=processWeather(wdata);
            var opecityvar = 2;
            for (var i = 0; i < weeklyforcat.length; i++) {
                var weatherdata = weeklyforcat[i];
                 var htmltext = '<tr style="opacity: ' + opecityvar / i + ';">' +
                    '<td class="day">' + weatherdata.day + '</td>' +
                    '<td class="bright weather-icon"><span class="wi weathericon ' + WeatherIconClass(weatherdata.icon) +'"></span></td>' +
                    '<td class="align-right bright max-temp">' + weatherdata.maxTemp.toFixed(0) + '°</td>' +
                    '<td class="align-right min-temp">' + weatherdata.minTemp.toFixed(0) + '°</td></tr>';
                wftable.innerHTML += htmltext;
            }

            setTimeout('GetweatherForecast()', 600000);
        }, error: function (data) {
            console.log("weather script error data", data);
        }
    });
}

function processWeather(data) {
    var fetchedLocationName = data.city.name + ", " + data.city.country;

    var forecastarray = [];
    var lastDay = null;
    var forecastData = {};

    for (var i = 0, count = data.list.length; i < count; i++) {

        var forecast = data.list[i];
        parserDataWeather(forecast); // hack issue #1017

        var day;
        var hour;
        if (!!forecast.dt_txt) {
            day = moment(forecast.dt_txt, "YYYY-MM-DD hh:mm:ss").format("ddd");
            hour = moment(forecast.dt_txt, "YYYY-MM-DD hh:mm:ss").format("H");
        } else {
            day = moment(forecast.dt, "X").format("ddd");
            hour = moment(forecast.dt, "X").format("H");
        }

        if (day !== lastDay) {
            var forecastData = {
                day: day,
                icon: forecast.weather[0].icon,
                maxTemp: forecast.temp.max,
                minTemp:forecast.temp.min,
                rain: this.processRain(forecast, data.list)
            };

            forecastarray.push(forecastData);
            lastDay = day;

            // Stop processing when maxNumberOfDays is reached
            if (forecastarray.length === 7) {//.config.maxNumberOfDays) {
                break;
            }
        } else {
            //Log.log("Compare max: ", forecast.temp.max, parseFloat(forecastData.maxTemp));
            forecastData.maxTemp = forecast.temp.max > parseFloat(forecastData.maxTemp) ? forecast.temp.max : forecastData.maxTemp;
            //Log.log("Compare min: ", forecast.temp.min, parseFloat(forecastData.minTemp));
            forecastData.minTemp = forecast.temp.min < parseFloat(forecastData.minTemp) ? forecast.temp.min : forecastData.minTemp;

            // Since we don't want an icon from the start of the day (in the middle of the night)
            // we update the icon as long as it's somewhere during the day.
            if (hour >= 8 && hour <= 17) {
                forecastData.icon = forecast.weather[0].icon;
            }
        }
    }
    return forecastarray;
}

function parserDataWeather(data) {
    if (data.hasOwnProperty("main")) {
        data["temp"] = { "min": data.main.temp_min, "max": data.main.temp_max };
    }
    return data;
}
function processRain(forecast, allForecasts) {
    //If the amount of rain actually is a number, return it
    if (!isNaN(forecast.rain)) {
        return forecast.rain;
    }

    //Find all forecasts that is for the same day
    var checkDateTime = (!!forecast.dt_txt) ? moment(forecast.dt_txt, "YYYY-MM-DD hh:mm:ss") : moment(forecast.dt, "X");
    var daysForecasts = allForecasts.filter(function (item) {
        var itemDateTime = (!!item.dt_txt) ? moment(item.dt_txt, "YYYY-MM-DD hh:mm:ss") : moment(item.dt, "X");
        return itemDateTime.isSame(checkDateTime, "day") && item.rain instanceof Object;
    });

    //If no rain this day return undefined so it wont be displayed for this day
    if (daysForecasts.length == 0) {
        return undefined;
    }

    //Summarize all the rain from the matching days
    return daysForecasts.map(function (item) {
        return Object.values(item.rain)[0];
    }).reduce(function (a, b) {
        return a + b;
    }, 0);
}

function SumTimes(sunsettime, sunriseTime) {
    var chour = moment().isBetween(sunriseTime,sunsettime);
    var weatherSunicon = document.getElementById("weatherSunicon");
    var weatherSunTime = document.getElementById("weatherSunTime");

    $(weatherSunicon).removeClass("wi-sunset");
    $(weatherSunicon).removeClass("wi-sunrise");
    var timtouse = sunsettime;
    var classtouse = "wi-sunset";
    if (chour) {
        timtouse = sunriseTime
        classtouse = "wi-sunrise";
    }

    $(weatherSunicon).addClass(classtouse);

    weatherSunTime.innerHTML = moment(timtouse, "X").format('hh:mm a');
}
function WeatherWindDirection(degrees) {
    {
        if (degrees > 11.25 && degrees <= 33.75) {
            return "NNE";
        } else if (degrees > 33.75 && degrees <= 56.25) {
            return "NE";
        } else if (degrees > 56.25 && degrees <= 78.75) {
            return "ENE";
        } else if (degrees > 78.75 && degrees <= 101.25) {
            return "E";
        } else if (degrees > 101.25 && degrees <= 123.75) {
            return "ESE";
        } else if (degrees > 123.75 && degrees <= 146.25) {
            return "SE";
        } else if (degrees > 146.25 && degrees <= 168.75) {
            return "SSE";
        } else if (degrees > 168.75 && degrees <= 191.25) {
            return "S";
        } else if (degrees > 191.25 && degrees <= 213.75) {
            return "SSW";
        } else if (degrees > 213.75 && degrees <= 236.25) {
            return "SW";
        } else if (degrees > 236.25 && degrees <= 258.75) {
            return "WSW";
        } else if (degrees > 258.75 && degrees <= 281.25) {
            return "W";
        } else if (degrees > 281.25 && degrees <= 303.75) {
            return "WNW";
        } else if (degrees > 303.75 && degrees <= 326.25) {
            return "NW";
        } else if (degrees > 326.25 && degrees <= 348.75) {
            return "NNW";
        } else {
            return "N";
        }
    }
}
function WeatherIconClass(weathericon) {
    var returnClass = ""
    switch (weathericon) {

        case "01d": returnClass = "wi-day-sunny"; break;
        case "02d": returnClass = "wi-day-cloudy"; break;
        case "03d": returnClass = "wi-cloudy"; break;
        case "04d": returnClass = "wi-cloudy-windy"; break;
        case "09d": returnClass = "wi-showers"; break;
        case "10d": returnClass = "wi-rain"; break;
        case "11d": returnClass = "wi-thunderstorm"; break;
        case "13d": returnClass = "wi-snow"; break;
        case "50d": returnClass = "wi-fog"; break;
        case "01n": returnClass = "wi-night-clear"; break;
        case "02n": returnClass = "wi-night-cloudy"; break;
        case "03n": returnClass = "wi-night-cloudy"; break;
        case "04n": returnClass = "wi-night-cloudy"; break;
        case "09n": returnClass = "wi-night-showers"; break;
        case "10n": returnClass = "wi-night-rain"; break;
        case "11n": returnClass = "wi-night-thunderstorm"; break;
        case "13n": returnClass = "wi-night-snow"; break;
        case "50n": returnClass = "wi-night-alt-cloudy-windy"; break;
    }

    return returnClass;
}
