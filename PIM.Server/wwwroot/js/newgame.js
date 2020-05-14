
function update(evt) {
    $('#newGameForm').submit()
}

function render(display) {
    function renderLoop(lastVersion) {
        $.getJSON(
            "/api/lobby/next/" + lastVersion,
            lobbyState => {
                display.empty()
                display.jsonForm(newGameForm(lobbyState))
                $("input").change(update)
                $("select").change(update)
                $("button").removeClass("btn-default").addClass("btn-primary")
                renderLoop(lobbyState.Version)
            }
        ).fail(
            function (jqXHR, textStatus, err) {
                console.log("Error getting events\ntextStatus: " + textStatus + "\njqXHR: " + jqXHR + "\err: " + err);
                renderLoop(-1)
            });
    }
    renderLoop(-1)
}

function addPlayer() {
    $.ajax({
        type: 'POST',
        accepts: 'application/json',
        url: "/api/lobby/addPlayer/",
        contentType: 'application/json',
        data: "{}",
        error: function (jqXHR, textStatus, errorThrown) {
            alert("error adding player: " + errorThrown);
        },
        success: function (result) {
            //refreshBoard();
        }
    });
}

function removePlayer() {
    $.ajax({
        type: 'POST',
        accepts: 'application/json',
        url: "/api/lobby/removePlayer/",
        contentType: 'application/json',
        data: "{}",
        error: function (jqXHR, textStatus, errorThrown) {
            alert("error removing player: " + errorThrown);
        },
        success: function (result) {
            //refreshBoard();
        }
    });
}

function newGameForm(lobbyState) {
    return {
        schema: {
            Game: {
                type: 'string',
                required: true,
                enum: [
                    "Taenkeboks",
                    "Meier",
                    "Hnefatafl",
                    "MNK"
                ]
            },
            Players: {
                type: 'array',
                required: true,
                items: {
                    type: "string",
                    enum: lobbyState.Players
                }
            },
            Taenkeboks: {
                type: "object",
                properties: {
                    diceCount: { type: "integer" },
                    extraLives: { type: "integer" },
                    ofAnyKind: { type: "boolean" },
                    multiSeries: { type: "boolean" },
                    oneIsSeries: { type: "boolean" },
                    lastStanding: { type: "boolean" }
                }
            }
        },
        form: [
            {
                key: "Game",
                title: "Game"
            },
            {

                key: "Players",
                title: "Players",
                type: "checkboxes"
            },
            {
                type: "fieldset",
                title: "Taenkeboks Rules",
                items: [
                    {
                        key: "Taenkeboks.diceCount",
                        title: "Dice count",
                        placeholder: 4
                    }, {
                        key: "Taenkeboks.extraLives",
                        title: "Extra lives",
                        placeholder: 0
                    }, {
                        key: "Taenkeboks.ofAnyKind",
                        notitle: true,
                        inlinetitle: "Any of a kind"
                    }, {
                        key: "Taenkeboks.multiSeries",
                        notitle: true,
                        inlinetitle: "Series starts anywhere"
                    }, {
                        key: "Taenkeboks.oneIsSeries",
                        notitle: true,
                        inlinetitle: "Single one is series"
                    }, {
                        key: "Taenkeboks.lastStanding",
                        notitle: true,
                        inlinetitle: "Play for winner"
                    }
                ]
            },
            {
                type: "actions",
                items: [
                    {
                        "type": "button",
                        "title": "Ready",
                        "onClick": function (evt) {
                            evt.preventDefault();
                            alert('Thank you!');
                        },
                        "fieldHtmlClass": "yeah",
                        "htmlClass": "yeah2"
                    }
                ]
            }
        ],
        value: lobbyState.Spec,
        params: {
            "fieldHtmlClass": "input-small"
        },
        //{
        //    "game": "Tænkeboks",
        //    "players":
        //        [
        //            "Bob",
        //            "Alice",
        //            "orrihafsteins@gmail.com"
        //        ],
        //    "taenkeboks": {
        //        "diceCount": 4,
        //        "oneIsSeries": true,
        //        "lastStanding": false,
        //        "extraLives": 0,
        //        "ofAnyKind": false,
        //        "multiSeries": false
        //    }
        //},
        onSubmit: function (errors, values) {
            if (errors) {
                alert("error creating game: " + errors);
            }
            else {
                $.ajax({
                    type: 'POST',
                    accepts: 'application/json',
                    url: "/api/lobby/update",
                    contentType: 'application/json',
                    data: JSON.stringify(values),
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert("error creating game: " + errorThrown);
                    },
                    success: function (newGameID) {
                        //window.location.href = "/" + values.Game + "/" + newGameID;
                    }
                });
            }
        }
    }
}