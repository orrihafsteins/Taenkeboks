function addRoom(name) {
    $.ajax({
        type: 'POST',
        accepts: 'application/json',
        url: '/api/gameroom/',
        contentType: 'application/json',
        data: name,
        error: function (jqXHR, textStatus, errorThrown) {
            alert(xhr.status);
            alert(thrownError);
        },
        success: function (result) {
            //alert("added room:"+result);
        }
    });
}

function addCurrentPlayer(roomID) {
    $.ajax({
        type: 'POST',
        accepts: 'application/json',
        url: '/api/gameroom/' + roomID + "/players",
        contentType: 'application/json',
        //data: roomID,
        error: function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR.status);
            alert(thrownError);
        },
        success: function (result) {
            //alert("said hello");
        }
    });
}

function boolSelect(id,def) {
    var dropdown = $("<select id=\"" + id + "\" />")
    $('<option />', { value: "true", text: 'Yes' }).appendTo(dropdown);
    $('<option />', { value: "false", text: 'No' }).appendTo(dropdown);
    dropdown.val(def)
    return dropdown
}



function intSelect(id, from, to, def,change) {
    var dropdown = $("<select id=\"" + id + "\" />")
    for (i = from; i <= to; i++) {
        var op = $('<option />', { value: i, text: i })
        op.appendTo(dropdown)
    }
    dropdown.val(def)
    if (change)
        dropdown.off().change(change)
    return dropdown
}

function paramRow(name, select) {
    var row = $("<tr/>")
    row.append($('<td />').text(name))
    row.append($('<td />').append(select))
    return row
}


function render(r) {
    $("#dump").empty().append(JSON.stringify(r, undefined, 2))
    var pTable = $("#playerTable").empty()
    addSide(r.players)
    addSide(r.players)
    addSide(r.players)
    addSide(r.players)
    //add parameters
    var changePlayerCount = function () {
        var newPlayerCount = parseInt($("#playerCount").val())
        var pTable = $("#playerTable").empty()
        for (var i = 0; i < newPlayerCount; i++)
            addSide(r.players)
    }
    parametersTable = $("#parametersTable").empty()
    parametersTable.append(paramRow('playerCount', intSelect('playerCount', 2, 10, 4, changePlayerCount)))
    parametersTable.append(paramRow('diceCount', intSelect('diceCount', 1, 10, 4,null)))
    parametersTable.append(paramRow('ofAnyKind', boolSelect('ofAnyKind', "false")))
    parametersTable.append(paramRow('diceCount', intSelect('diceCount', 1, 10, 4)))
    parametersTable.append(paramRow('multiSeries', boolSelect('multiSeries', "false")))
    parametersTable.append(paramRow('oneIsSeries', boolSelect('oneIsSeries', "true")))
    parametersTable.append(paramRow('lastStanding', boolSelect('lastStanding', "true")))
    parametersTable.append(paramRow('extraLives', intSelect('extraLives', 0, 10, 1)))
    setParameters(getParameters("classic"))
    //readParameters()
    //var playerCount = 4
    //var pTable = $("#playerTable").empty()
    //for (i = 0; i < playerCount; i++) {
    //    var row = $('<tr />')
    //    //row.append($('<th />').text("DELETE"))
    //    row.append($('<th />').text(r.players[i].playerName))
    //    pTable.append(row)
    //}
}
function readParameters() {
    var ofAnyKind = $("#ofAnyKind").val()
    var diceCount = $("#diceCount").val()
    var multiSeries = $("#multiSeries").val()
    var oneIsSeries = $("#oneIsSeries").val()
    var lastStanding = $("#lastStanding").val()
    var extraLives = $("#extraLives").val()
    //var header = Array();
    //$("#playerTable tr th").each(function (i, v) {
    //    header[i] = $(this).text();
    //})
    //alert(header);

    var data = Array();
    $("#playerTable tr").each(function (i, v) {
        $(this).children('td').each(function (ii, vv) {
            data[i] = $(this).children('select').val();
        });
    })
    var out = {
        "players": data,
        "specification": {
            "playerCount": data.length,
            "ofAnyKind": ofAnyKind,
            "diceCount": diceCount,
            "multiSeries": multiSeries,
            "oneIsSeries": oneIsSeries,
            "lastStanding": lastStanding,
            "extraLives": extraLives
        }
    }
    //alert(JSON.stringify(out))
    return out
}
function getParameters(gameRules) {
    if (gameRules == "classic")
        return {
            ofAnyKind:"false",
            diceCount:4,
            multiSeries:"false", 
            oneIsSeries:"true",
            lastStanding:"false",
            extraLives:0
        }
    return null;
}

function setParameters(parameters) {
    var ofAnyKind = $("#ofAnyKind").val(parameters.ofAnyKind)
    var diceCount = $("#diceCount").val(parameters.diceCount)
    var multiSeries = $("#multiSeries").val(parameters.multiSeries)
    var oneIsSeries = $("#oneIsSeries").val(parameters.oneIsSeries)
    var lastStanding = $("#lastStanding").val(parameters.lastStanding)
    var extraLives = $("#extraLives").val(parameters.extraLives)
}

function startGame() {
    var gameParameters = readParameters();
    $.ajax({
        type: 'POST',
        accepts: 'application/json',
        url: '/api/gameroom/' + roomID ,
        contentType: 'application/json',
        data: JSON.stringify(gameParameters),
        error: function (jqXHR, textStatus, errorThrown) {
            $('#dump').text("cant start game: "+thrownError);
        },
        success: function (result) {
            window.location.href = "/taenkeboks/"+result;
        }
    });
}
function addSide(players) {
    var playerTable = $("#playerTable")
    var row = $('<tr />')
    var dropdown = $("<select />")
    $('<option />', { value: 'CPU', text: 'CPU' }).appendTo(dropdown);
    for (var val in players) {
        $('<option />', { value: players[val].playerID, text: players[val].playerName }).appendTo(dropdown);
    }
    
    //row.append($('<td />').text("DELETE"))
    row.append($('<td />').append(dropdown))
    playerTable.append(row)   
}



function refreshGameRoom(roomID) {
    $.getJSON("/api/gameroom/" + roomID,
            render
        )
        .fail(
            function (jqXHR, textStatus, err) {
                $('#dump').text('Error: ' + err);
            });
}


function consumeEvents(roomID) {
    console.log("consuming events...")
    $.getJSON("/api/gameroom/" + roomID + "/events",
        function (event) {
            console.log("event retrieved:")
            console.log(JSON.stringify(event, undefined, 2))
            if (event.eventCode==="StartGame") {
                window.location.href = "/taenkeboks/" + event.key;
            }
            else {
                consumeEvents(roomID)
            }
        })
        .fail(
            function (jqXHR, textStatus, err) {
                console.log("Error getting events\ntextStatus: " + textStatus + "\njqXHR: " + jqXHR + "\err: " + err);
                //$("#dump").text(err)
                consumeEvents(gameID)
            });
}

$(document).ready(function () {
    roomID = parseInt($("#roomID").text())
    addCurrentPlayer(roomID);
    refreshGameRoom(roomID);
    consumeEvents(roomID);
    $("#startGame").off().click(startGame);
});

