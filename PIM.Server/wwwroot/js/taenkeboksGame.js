
class TbGame {
    constructor(gameId, eDisplay) {
        this.gameId = gameId
        this.display = (h => eDisplay.html(h))
    }

    render() {
        var gameId = this.gameId
        var displayBet = this.displayBet.bind(this)
        var displayRoundResult = this.displayRoundResult.bind(this)
        var displayGameResult = this.displayGameResult.bind(this)
        var displayTournamentResult = this.displayTournamentResult.bind(this)
        function renderLoop(initial = false) {
            var endpoint = (initial ? "current/" : "next/")
            var betContinue = function (v) {
                displayBet(v)
                var millisecondsToWait = 1000;
                setTimeout(renderLoop, millisecondsToWait);//Display the last render for a second before continuing with loop
            }
            function roundContinue(v) {
                if (v.roundReport.playerMadeBet < 0)//empty round report
                    gameContinue(v);
                else 
                    displayRoundResult(v, gameContinue)
            }
            function gameContinue(v) {
                if (v.gameReport.playerLost < 0)//empty game report
                    tournamentContinue(v);
                else 
                    displayGameResult(v, tournamentContinue)
            }
            function tournamentContinue(v) {
                if (v.tournamentReport.playerLost < 0 && v.tournamentReport.playerWon < 0)//empty tournament report
                    betContinue(v)
                else 
                    displayTournamentResult(v, betContinue)
            }
            $.getJSON(
                "/api/" + endpoint + gameId,
                roundContinue
            ).fail(
                function (jqXHR, textStatus, err) {
                    console.log("Error getting events\ntextStatus: " + textStatus + "\njqXHR: " + jqXHR + "\err: " + err);
                    renderLoop(true)
                });
        }
        renderLoop(true)
    }


    static playerCard(v, side, dice = null, bet = null) {
        var pn = v.playerNames[side]
        var p = new TbPlayerCard(pn)
        if (dice != null) {
            p.dice = new Hand(dice)
        }
        else if (side == v.nextSide && side == v.viewingSide) {
            p.dice = new Hand(v.playerHand)
        }
        else {
            p.dice = new Hand(Array(v.diceLeft[side]).fill(0))
        }
        p.lives = v.livesLeft[side]
        if (v.tournamentReport.playerWon == side)
            p.state = "winner"
        else if (v.tournamentReport.playerLost == side)
            p.state = "loser"
        else if (p.lives == 0)
            p.state = "eliminated"
        else if (v.nextSide == side)
            p.state = "toPlay"    
        else
            p.state = "waiting"
        //Find last bet or show bet form
        if (bet != null)
            p.bet = bet
        else if (side == v.nextSide && side == v.viewingSide) {
            p.bet = new TbAction(v)
        }
        else {
            p.bet = new TbBet({ count: 0, value: 0 })
            for (var i = 0; i < v.actionHistory.length; i++) {
                if (v.actionHistory[i].action.call) break;
                if (v.actionHistory[i].side === side) {
                    p.bet = new TbBet(v.actionHistory[i].action.bet)
                    break;
                }
            }
        }
        return p
    }
    static renderBoard(v, players, mainCard) {
        var viewingPlayer = players[v.viewingSide];
        var otherPlayers = []
        for (var i = 1; i < v.playerCount; i++) {//order players clockwise
            var side = (v.viewingSide + i) % v.playerCount
            otherPlayers.push(players[side])
        }
        function renderOther(p) {
            return `<div class="col">${p.render()}</div>`
        }
        return `
<div class="container m-3">
    <div class="row">
        ${otherPlayers.map(renderOther).join('\n')}
    </div>
    <div class="row">
        <div class="col">
            ${mainCard.render()}
        </div>
    </div>
    <div class="row">
        <div class="col">
            ${viewingPlayer.render()}
        </div>
    </div>
</div>`
    }

    displayBet(v) {
        var players = v.playerNames.map(function (pn, index) {
            return TbGame.playerCard(v, index)
        })
        var mainCard = TbMainCard.Bet(v)

        this.display(TbGame.renderBoard(v, players, mainCard))
        var raise = this.raise.bind(this)
        function onRaise() {
            var bet = {
                count: $('#raiseCounts').val(),
                value: $('#raiseValues').val()
            }
            raise(bet)
        }
        $("#callButton").off().click(this.call.bind(this))
        $("#raiseButton").off().click(onRaise)
    }

    displayRoundResult(v, cnt) {
        var report = v.roundReport
        var betValue = report.betHighestStanding.value
        var contributions = report.playerContribution.map(c => new TbBet({ count: c, value: betValue }))
        var players = v.playerNames.map(function (pn, index) {
            var dice = report.playerDice[index]
            var contribution = contributions[index]
            return TbGame.playerCard(v, index, dice, contribution)
        })
        var mainCard = TbMainCard.Round(v)
        this.display(TbGame.renderBoard(v, players, mainCard))
        $('#continue').off().click(() => cnt(v))
    }

    displayGameResult(v, cnt) {
        var players = v.playerNames.map(function (pn, index) {
            var bet = new TbBet({ count: 0, value: 0 })
            return TbGame.playerCard(v, index, [], bet)
        })
        var mainCard = TbMainCard.Game(v)
        this.display(TbGame.renderBoard(v, players, mainCard))
        $('#continue').off().click(() => cnt(v))
    }

    displayTournamentResult(v) {
        var players = v.playerNames.map(function (pn, index) {
            var bet = new TbBet({ count: 0, value: 0 })
            return TbGame.playerCard(v, index, [], bet)
        })
        var mainCard = TbMainCard.Tournament(v)
        this.display(TbGame.renderBoard(v, players, mainCard))
        $('#restartButton').off().click(this.restartGame.bind(this))
        $('#newGameButton').off().click(this.newGame.bind(this))
    }

    postAction(action) {
        $.ajax({
            type: 'POST',
            accepts: 'application/json',
            url: "/api/action/" + this.gameId,
            contentType: 'application/json',
            data: JSON.stringify(action),
            error: function (jqXHR, textStatus, errorThrown) {
                alert("error performing move: " + errorThrown);
            },
            success: function (result) {
                //refreshBoard();
            }
        });
    }

    call() {
        var action = {
            "call": true,
            "bet": {
                "count": -1,
                "value": -1
            }
        }
        this.postAction(action)
    }

    raise(bet) {
        var action = {
            "call": false,
            "bet": bet
        }
        this.postAction(action)
    }

    restartGame() {
        var data = { GameId: this.gameId }
        $.ajax({
            type: 'POST',
            accepts: 'application/json',
            url: "/api/duplicate",
            contentType: 'application/json',
            data: JSON.stringify(data),
            error: function (jqXHR, textStatus, errorThrown) {
                alert("error duplicating game: " + errorThrown);
            },
            success: function (newGameID) {
                window.location.href = "/taenkeboks/" + newGameID;
            }
        });
    }

    newGame() {
        window.location.href = "/newgame/"
    }
}

