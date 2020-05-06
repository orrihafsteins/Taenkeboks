
var newGameForm = {
    schema: {
        game: {
            type: 'string',
            required: true,
            enum: [
                "Taenkeboks",
                "Meier",
                "Hnefatafl",
                "MNK"
            ]
        },
        players: {
            type: 'array',
            required: true,
            items: {
                type: "string",
                enum: [
                    "Bob",
                    "Alice",
                    "Carol",
                    "orrihafsteins@gmail.com"
                ]
            }
        },
        taenkeboks: {
            type: "object",
            properties: {
                diceCount: { type: "integer" },
                extraLives: { type: "integer" },
                ofAnyKind: { type: "boolean"},
                multiSeries: { type: "boolean"},
                oneIsSeries: { type: "boolean" },
                lastStanding: {type: "boolean"}
            }
        }
    },
    form: [
        {
            key: "game",
            title: "Game"
        },
        {
            key: "players",
            title: "Players",
            type: "checkboxes"
        },
        {
            type: "section",
            title: "Taenkeboks Rules",
            items: [
                {
                    key: "taenkeboks.diceCount",
                    title: "Dice count",
                    placeholder: 4
                }, {
                    key: "taenkeboks.extraLives",
                    title: "Extra lives",
                    placeholder: 0
                }, {
                    key: "taenkeboks.ofAnyKind",
                    notitle: true,
                    inlinetitle: "Any of a kind"
                }, {
                    key: "taenkeboks.multiSeries",
                    notitle: true,
                    inlinetitle: "Series starts anywhere"
                }, {
                    key: "taenkeboks.oneIsSeries",
                    notitle: true,
                    inlinetitle: "Single one is series"
                }, {
                    key: "taenkeboks.lastStanding",
                    notitle: true,
                    inlinetitle: "Play for winner"
                }
            ]
        },
        {
            type: "actions",
            items: [
                {
                    "type": "submit",
                    "title": "Start Game"
                }
            ]
        }
    ],
    value: {
        "game": "Tænkeboks",
        "players":
            [
                "Bob",
                "Alice",
                "orrihafsteins@gmail.com"
            ],
        "taenkeboks": {
            "diceCount": 4,
            "oneIsSeries": true,
            "lastStanding": false,
            "extraLives": 0,
            "ofAnyKind": false,
            "multiSeries": false
        }
    },
    onSubmit: function (errors, values) {
        if (errors) {
            alert("error creating game: " + errors);
        }
        else {
            if (values.game === "Taenkeboks")
                values.taenkeboks.playerCount = values.players.length//Todo: find better place for this 
            $.ajax({
                type: 'POST',
                accepts: 'application/json',
                url: "/api/create",
                contentType: 'application/json',
                data: JSON.stringify(values),
                error: function (jqXHR, textStatus, errorThrown) {
                    alert("error creating game: " + errorThrown);
                },
                success: function (newGameID) {
                    window.location.href = "/"+values.game+"/" + newGameID;
                }
            });
        }
    }
};