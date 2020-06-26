
function GetNewsFeed() {
    $.ajax({
        type: "GET",
        contentType: 'application/json; charset=utf-8',
        url: "/Newsfeed/GetNewsDisplay",
        datatype: 'json',
        success: function (data, status) {
            var newsfeedHeader = document.getElementById("newsfeedHeader");
            var newsfeednews   = document.getElementById("newsfeednews");
            newsfeedHeader.innerHTML = data.header;
            newsfeednews.innerHTML = data.title;
            setTimeout("GetNewsFeed()", 30000);
        },
        Error: function (data) {
            console.log(data);
        }
    });
}