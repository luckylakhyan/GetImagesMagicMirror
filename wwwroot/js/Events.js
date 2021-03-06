﻿function compareValues(key, order = 'asc') {
    return function innerSort(a, b) {
        if (!a.hasOwnProperty(key) || !b.hasOwnProperty(key)) {
            // property doesn't exist on either object
            return 0;
        }

        const varA = (typeof a[key] === 'string')
            ? a[key].toUpperCase() : a[key];
        const varB = (typeof b[key] === 'string')
            ? b[key].toUpperCase() : b[key];

        let comparison = 0;
        if (varA > varB) {
            comparison = 1;
        } else if (varA < varB) {
            comparison = -1;
        }
        return (
            (order === 'desc') ? (comparison * -1) : comparison
        );
    };
}
function GetEvents() {
    $.ajax({
        type: "GET",
        contentType: 'application/json; charset=utf-8',
        url: "/calendar/Getcalendar",
        datatype: 'json',
        success: function (data, status) {
            result = JSON.parse(data.replace("No upcoming events found.", ""));
            var Calenderheader = document.getElementById("Calenderheader");
            var Calendertable = document.getElementById("Calendertable");
            var events = result.calendars.sort(compareValues('StartTime'));
            Calenderheader.innerHTML = result.Header;
            Calendertable.innerHTML = "";
            var displayCount = 10;
            if (events.length < 10) {
                displayCount = events.length;
            }
            for (var i = 0; i < displayCount; i++) {
                if (events[i].ErrorMessage == "") {
                    var eventcompTime = events[i].StartTime.split(' ');
                    var eventDate = eventcompTime[0];
                    var eventTime = eventcompTime.length > 1 ? eventcompTime[1].split(':') : '  :  :  :'.split(':');
                    var eventPeriod = eventcompTime.length > 1 ? eventcompTime[2] : ' ';
                    if (eventTime[0].length < 2) {
                        eventTime[0] = '0' + eventTime[0];
                    }
                    var hoursandmin = eventTime[0] + ":" + eventTime[1];
                    if (hoursandmin.trim() == ":" || hoursandmin.trim() == "12:00") {
                        hoursandmin = "{All Day}";
                    }
                    var dispTime = moment(eventDate).format("MMM DD") + " " + hoursandmin;
                    if (events[i].EventTitle.length > 20) {
                        events[i].EventTitle = events[i].EventTitle.substr(0, 20) + ".."
                    }
                    Calendertable.innerHTML += '<tr class="normal" style="color: rgb(255, 255, 255);"><td class="symbol align-right "><span class="fa fa-fw fa-calendar-check"></span></td><td class="title ">' + events[i].EventTitle + '</td><td class="time light ">' + dispTime + '</td></tr>'
                } else {
                    console.log("calendars error",events[i].ErrorMessage);
                }
            }
            setTimeout("GetEvents()", 300000);
        },
        Error: function (data) {
            console.log(data);
            setTimeout("GetEvents()", 1000);
        }
    });
}
