

function getGameRooms() {
    $.getJSON("/api/gameroom/",
        function (r) {
            //$("#roomNames").append(" <th>" + playerNames[i] + "</th>")
            roomCount = r.length
            for (i = 0; i < roomCount; i++) {
                $("#roomTable").append(" <tr><td><a href=\"/gameroom/" + r[i].gameRoomID + "\">" + r[i].name +"</a></td></tr>")
                //$("#roomPlayers").append(" <th>" + r[i].players.length + "</th>")
                //$("#roomLinks").append(" <th>/gameroom/" + r[i].gameroomid + "</th>")
            }
            //$("#dump").empty().append(JSON.stringify(r, undefined, 2))
        })
        .fail(
            function (jqXHR, textStatus, err) {
                $('#items').text('Error: ' + err);
            });
}


$(document).ready(function () {
    getGameRooms()
});
