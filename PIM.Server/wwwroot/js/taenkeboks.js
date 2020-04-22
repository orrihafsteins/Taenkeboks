var gameID = "TestGameID"
$(document).ready(function () {
    renderLoop(true)
});


function renderLoop(initial=false) {
    var endpoint = (initial?"/current":"/next")
    $.getJSON("/game/taenkeboks/play/" + gameID + endpoint,
        function (v) {
            var betContinue = function () {
                renderBet(v)
                var millisecondsToWait = 1000;
                setTimeout(renderLoop, millisecondsToWait);//Display the last render for a second before continuing with loop
            }
            var roundContinue = function (continuation) {
                if (v.roundReport.playerMadeBet < 0)//empty round report
                    return continuation;
                else
                    return function () { return renderRoundResult(v, continuation) }
            }
            var gameContinue = function (continuation) {
                if (v.gameReport.playerLost < 0)//empty game report
                    return continuation;
                else
                    return function () { return renderGameResult(v, continuation) }
            }
            var tournamentContinue = function (continuation) {
                if (v.tournamentReport.playerWon >= 0)
                    return function () { renderTournamentWon(v) }//render result and stop
                else if (v.tournamentReport.playerLost >= 0)
                    return function () { renderTournamentLost(v) }//render result and stop
                else
                    return continuation
            }
            roundContinue(gameContinue(tournamentContinue(betContinue)))()
        })
        .fail(
            function (jqXHR, textStatus, err) {
                console.log("Error getting events\ntextStatus: " + textStatus + "\njqXHR: " + jqXHR + "\err: " + err);
                //$("#dump").text(err)
                renderLoop(true)
            });
}


function renderBet(v) {
    renderPlayers(v)
    $("#choppingBlock").html(v.totalDiceLeft + " dice in play")
    $("#currentBet").html(printBet(v.currentBet))
    $("#nextPlayerText").empty()
    hideMoves()
    if (v.nextSide === v.viewingSide) {
        showMoves(v.legalActions, gameID)
    }
    else {
        $("#nextPlayerText").text(v.playerNames[v.nextSide] + " to move")
    }
}

function renderRoundResult(v, continuation) {
    renderPlayers(v)
    //render player hands and contributions
    var report = v.roundReport
    var hands = report.playerDice
    var contributions = report.playerContribution
    var betCalled = report.betCalled
    var highestStanding = report.betHighestStanding
    var betValue = betCalled.value
    var contributionStrings = []
    for (var side in hands) {
        var handElement = $('#hand' + side)
        populateHand(hands[side], handElement)
        var betElement = $('#bet' + side)
        var contribution = {
            "count": contributions[side],
            "value": betValue
        }
        if (contributions[side] > 0)
        {
            contributionStrings.push(printDiceCount(contribution))
            populateBet(contribution, betElement)
        }
        else betElement.html("&nbsp;")
    }
    //render chopping block /player call
    var choppingBlock = $('#choppingBlock');
    var callingPlayer = v.playerNames[report.playerCalledBet]
    choppingBlock.html(callingPlayer + ' called ' + printBet(betCalled))
    //render call 
    var bettingPlayer = v.playerNames[report.playerMadeBet]
    var currentBetElement = $('#currentBet');
    var contributionString = contributionStrings.join(" + ")
    var comp = (report.playerMadeBet == report.playerLost) ? " < ":" >= "
    currentBetElement.html(contributionString+comp+printBet(betCalled))

    //render result and continue button
    var losingPlayer = v.playerNames[report.playerLost]
    $('#nextPlayerText').text(losingPlayer + ' lost')

    hideMoves();
    $("#continueButton").show()
    $("#continueButton").off().click(function () {
        continuation()
    });
}

function renderGameResult(v, continuation) {
    for (var side = 0; side < playerCount; side++) {
        var handElement = $('#hand' + side)
        populateHand([], handElement)
        var betElement = $('#bet' + side)
        betElement.html("&nbsp;")
    }
    //render player hands and contributions
    var report = v.gameReport
    var spec = v.spec
    var playerCount = spec.playerCount

    var choppingBlock = $('#choppingBlock')
    var currentBetElement = $('#currentBet')
    var nextPlayerElement = $('#nextPlayerText')
    choppingBlock.text('round complete')
    //render call 
    var losingPlayer = v.playerNames[report.playerLost]
    var losingPlayerLives = v.livesLeft[report.playerLost]
    if (losingPlayerLives === 0)
        currentBetElement.html(losingPlayer + ' is eliminated')
    else
        currentBetElement.html(losingPlayer + ' lost a life, ' + losingPlayerLives + ' remaining')

    //render result
    var nextPlayer = v.playerNames[v.nextSide]
    nextPlayerElement.text()
    //render continue button
    hideMoves();
    $("#continueButton").show()
    $("#continueButton").off().click(function () {
        continuation()
    });
}

function renderTournamentWon(v) {
    var report = v.tournamentReport
    var choppingBlock = $('#choppingBlock')
    var currentBetElement = $('#currentBet')
    var nextPlayerElement = $('#nextPlayerText')

    choppingBlock.text('game over')
    var winningPlayer = v.playerNames[report.playerWon]
    currentBetElement.html(winningPlayer + ' is the winner')
    nextPlayerElement.text('')
    hideMoves();
    $("#restartButton").show();
    $("#restartButton").off().click(function () { restartGame(board) });
}

function renderTournamentLost(v) {
    var report = v.tournamentReport
    var choppingBlock = $('#choppingBlock')
    var currentBetElement = $('#currentBet')
    var nextPlayerElement = $('#nextPlayerText')

    choppingBlock.text('game over')
    var losingPlayer = v.playerNames[report.playerLost]
    currentBetElement.html(losingPlayer + ' lost')
    //render result
    nextPlayerElement.text('')
    //render restart button
    hideMoves();
    $("#restartButton").show();
    $("#restartButton").off().click(function () { restartGame(board) });
}

function populateBet(bet, betElement) {
    var betString = printBet(bet)
    betElement.html(betString)
}

function hideMoves() {
    $("#callSpan").hide()
    $("#raiseSpan").hide()
    $("#continueButton").hide()
    $("#restartButton").hide()
}

function showMoves(legalMoves) {
    //call button
    if (legalMoves[0].call) {
        $("#callSpan").show()
        $("#callButton").off().click(function () {
            //window.alert("click");
            //alert($('#moves').val())
            $.ajax({
                type: 'POST',
                accepts: 'application/json',
                url: "/game/taenkeboks/play/" + gameID +"/action",
                contentType: 'application/json',
                data: JSON.stringify(legalMoves[0]),
                error: function (jqXHR, textStatus, errorThrown) {
                    alert("error performing move: " + errorThrown);
                },
                success: function (result) {
                    //efreshBoard();
                }
            });
        });
    }
    //numbers
    var raises = legalMoves.filter(a => !a.call)
    var nextRaise = raises[0]
    var counts = Array.from(new Set(raises.map(a => a.bet.count)))
    var fValues = function (count) {
        var countBet = raises.filter(a => a.bet.count == count)
        var countValues = countBet.map(a => a.bet.value)
        return countValues
    }
    //counts
    var countDropdown = $("#raiseCounts")
    countDropdown.empty()
    for (var val in counts) {
        $('<option />', { value: counts[val], text: counts[val] }).appendTo(countDropdown);
    }

    //values
    var valueDropdown = $("#raiseValues")
    var setValues = function (count, selected) {
        var lastValue = valueDropdown.val()
        valueDropdown.empty()
        values = fValues(count)
        for (var val in values) {
            $('<option />', { value: values[val], text: values[val] }).appendTo(valueDropdown);
        }
        if (lastValue)
            if (values.includes(parseInt(lastValue)))
                valueDropdown.val(lastValue)
    }
    setValues(nextRaise.bet.count)
    countDropdown.off().change(function () {
        setValues(countDropdown.val())
    })
    //raise button
    $("#raiseSpan").show()
    $("#raiseButton").off().click(function () {
        //window.alert("click");
        //alert($('#moves').val())
        var bet = {
            "call": false,
            "bet": {
                "count": countDropdown.val(),
                "value": valueDropdown.val()
            }
        }
        $.ajax({
            type: 'POST',
            accepts: 'application/json',
            url: "/game/taenkeboks/play/" + gameID + "/action",
            contentType: 'application/json',
            data: JSON.stringify(bet),
            error: function (jqXHR, textStatus, errorThrown) {
                alert("error performing move: " + errorThrown);
            },
            success: function (result) {
                //refreshBoard();
            }
        });
    });
}

function printBet(b) {
    if (b.count === 0) return "round start"
    return printDiceCount(b)
}

function printDiceCount(b) {
    var valueString = die(b.value)
    var countString = String(b.count)
    return countString + " " + valueString
}

function die(value) {
    if (value > 0)
        return '<img class="die" src="/images/Dice' + value + '.gif"/>'
    else if (value === 0)
        return '<img class="die" src="/images/DiceAny.gif" />'
}

function renderPlayers(v) {
    $("#playerRow").empty()
    for (i = 1; i < v.playerCount; i++) {
        var side = (v.viewingSide + i) % v.playerCount
        $("#playerRow").append(renderPlayerCell(v, side))
    }
    $("#selfRow").empty().append(renderPlayerCell(v, v.viewingSide).attr('colspan', v.playerCount - 1));
}

function clearPlayers(v) {
    $("#playerRow").empty()
    for (i = 1; i < v.playerCount; i++) {
        var side = (v.viewingSide + i) % v.playerCount
        $("#playerRow").append(clearPlayerCell(v, side))
    }
    $("#selfRow").empty().append(clearPlayerCell(v, v.viewingSide).attr('colspan', v.playerCount - 1));
}

function renderPlayerCell(v, side) {
    var diceLeft = v.diceLeft[side]
    var playerName = v.playerNames[side]
    var livesLeft = v.livesLeft[side]
    var lastBet = "&nbsp;"
    for (var i = 0; i < v.actionHistory.length; i++) {
        if (v.actionHistory[i].action.call) break;
        if (v.actionHistory[i].side === side) {
            lastBet = printAction(v.actionHistory[i].action)
            break;
        }
    }
    var playerCell = $('<td id=\"' + side + '\" class="player">')
    if (livesLeft === 0) {
        playerCell = $('<td id=\"' + side + '\" class="player deadPlayer">')
        playerCell.append('<div class="playerName">' + playerName + '</div>')
    }
    else if (livesLeft === 1)
        playerCell.append('<div class="playerName">' + playerName + '</div>')
    else
        playerCell.append('<span class="playerName">' + playerName + '<span class="playerLives">&nbsp;x' + livesLeft + '</span></span>')

    playerCell.append('<div id=\"bet' + side + '\" class="playerBet">' + lastBet + '<div>')
    //var diceTable = $('<table class="dieTable"></table>')
    var hand;
    //diceRow.appendTo(diceTable)

    var handElement = $('<div id=\"hand' + side + '\">')
    if (side === v.viewingSide) {
        hand = v.playerHand
    }
    else {
        hand = Array.apply(null, Array(diceLeft)).map(Number.prototype.valueOf, 0);
    }
    populateHand(hand, handElement)
    playerCell.append(handElement)
    if (v.nextSide === side)
        playerCell.addClass('active')
    return playerCell
}

function clearPlayerCell(v, side) {
    var diceLeft = v.diceLeft[side]
    var playerName = v.playerNames[side]
    var livesLeft = v.livesLeft[side]
    var lastBet = "&nbsp;"
    for (var i = 0; i < v.actionHistory.length; i++) {
        if (v.actionHistory[i].action.call) break;
        if (v.actionHistory[i + 1].side === side) {
            lastBet = printAction(v.actionHistory[i].action)
            break;
        }
    }
    var playerCell = $('<td id=\"' + side + '\" class="player">')
    if (livesLeft === 0) {
        playerCell = $('<td id=\"' + side + '\" class="player deadPlayer">')
        playerCell.append('<div class="playerName">' + playerName + '</div>')
    }
    else if (livesLeft === 1)
        playerCell.append('<div class="playerName">' + playerName + '</div>')
    else
        playerCell.append('<span class="playerName">' + playerName + '<span class="playerLives">&nbsp;x' + livesLeft + '</span></span>')

    playerCell.append('<div id=\"bet' + side + '\" class="playerBet">' + lastBet + '<div>')
    //var diceTable = $('<table class="dieTable"></table>')
    var hand;
    //diceRow.appendTo(diceTable)

    var handElement = $('<div id=\"hand' + side + '\">')
    if (side === v.viewingSide) {
        hand = v.playerHand
    }
    else {
        hand = Array.apply(null, Array(diceLeft)).map(Number.prototype.valueOf, 0);
    }
    populateHand(hand, handElement)
    playerCell.append(handElement)
    if (v.nextSide === side)
        playerCell.addClass('active')
    return playerCell
}

function populateHand(hand, handElement) {
    handElement.empty()
    for (var di = 0; di < hand.length; di++)
        handElement.append($(die(hand[di])))
}

function printAction(a) {
    if (a.call) {
        return " call"
    }
    else {
        return printBet(a.bet)
    }
}