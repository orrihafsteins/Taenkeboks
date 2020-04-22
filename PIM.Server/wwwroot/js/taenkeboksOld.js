lastRefreshed = -1
function refreshBoard(gameID) {
    $.getJSON("/api/taenkeboks/" + gameID,
        function (b) {
            currentMove = b.visible.moveHistory.length
            if (currentMove > lastRefreshed) {
                lastRefreshed = currentMove
                if (!b.status.inPlay) {
                    renderBet(b, gameID)
                    renderLifeResult(b.status,b,gameID,null)
                }
                else
                    renderBet(b, gameID)
            }
        })
        .fail(
            function (jqXHR, textStatus, err) {
                $("#dump").text(err)
            });
}


function restartGame(board) {
    var spec = board.visible.spec
    var players = board.playerIDs
    var gameParameters = {
        "players": players,
        "specification": {
            "playerCount": players.length,
            "ofAnyKind": spec.ofAnyKind,
            "diceCount": spec.diceCount,
            "multiSeries": spec.multiSeries,
            "oneIsSeries": spec.oneIsSeries,
            "lastStanding": spec.lastStanding,
            "extraLives": spec.extraLives
        }
    }
    $.ajax({
        type: 'POST',
        accepts: 'application/json',
        url: '/api/gameroom/0',
        contentType: 'application/json',
        data: JSON.stringify(gameParameters),
        error: function (jqXHR, textStatus, errorThrown) {
            $('#dump').text("cant start game: " + thrownError);
        },
        success: function (result) {
            window.location.href = "/taenkeboks/" + result;
        }
    });
}
function renderBetResults(s, nextBoard, gameID, continuation) {
    //render player hands and contributions
    var report = nextBoard.status.roundReport
    var hands = report.playerDice
    var contributions = report.playerContribution
    var betCalled = report.betCalled
    var highestStanding = report.betHighestStanding
    var betValue = betCalled.value
    for (var side in hands) {
        var handElement = $('#hand' + side)
        populateHand(hands[side], handElement)
        var betElement = $('#bet' + side)
        var contribution = {
            "count": contributions[side],
            "value": betValue
        }
        if (contributions[side] > 0) populateBet(contribution, betElement)
        else betElement.html("&nbsp;")
    }
    //render chopping block /player call
    var choppingBlock = $('#choppingBlock');
    var callingPlayer = nextBoard.playerNames[report.playerCalledBet]
    choppingBlock.text(callingPlayer + ' called')
    //render call 
    var bettingPlayer = nextBoard.playerNames[report.playerMadeBet]
    var currentBetElement = $('#currentBet');
    currentBetElement.html(bettingPlayer + ' bet ' + printBet(betCalled) + ', actually ' + printDiceCount(highestStanding))

    //render result and continue button
    var losingPlayer = nextBoard.playerNames[report.playerLost]
    $('#nextPlayerText').text(losingPlayer + ' lost')

    hideMoves();
    $("#continueButton").show()
    $("#continueButton").off().click(function () {
        continuation()
    });
}

function renderLifeResult(s, nextBoard, gameID, continuation) {
    //render player hands and contributions
    var report = nextBoard.status.gameReport
    var board = nextBoard
    var spec = board.visible.spec
    var playerCount = spec.playerCount
    for (var side = 0; side < playerCount; side++) {
        var handElement = $('#hand' + side)
        populateHand([], handElement)   
    }

    var choppingBlock = $('#choppingBlock')
    var currentBetElement = $('#currentBet')
    var nextPlayerElement = $('#nextPlayerText')
    if (!report.gameComplete) {
        //we have another round
        //render chopping block /player call
        choppingBlock.text('round complete')
        //render call 
        var losingPlayer = board.playerNames[report.playerLostLife]
        var losingPlayerLives = board.visible.livesLeft[report.playerLostLife] 
        if(losingPlayerLives === 0)
            currentBetElement.html(losingPlayer + ' is eliminated')
        else
            currentBetElement.html(losingPlayer + ' lost a life, ' + losingPlayerLives + ' remaining')

        //render result
        var nextPlayer = board.playerNames[board.visible.nextPlayer]
        nextPlayerElement.text(nextPlayer + ' starts next round')
        //render continue button
        hideMoves();
        $("#continueButton").show()
        $("#continueButton").off().click(function () {
            continuation()
        });
    }
    else if (report.gameComplete && report.playerWinner >= 0) {
        //we have a winner
        //render chopping block /player call
        choppingBlock.text('game over')
        //render call 
        var winningPlayer = board.playerNames[report.playerWinner]
        currentBetElement.html(winningPlayer + ' is the winner')
        //render result 

        nextPlayerElement.text('')
        //render restart button
        hideMoves();
        $("#restartButton").show();
        $("#restartButton").off().click(function () { restartGame(board) });
        renderPlayers(board)
    }
    else if (report.gameComplete && report.playerLostLife >= 0) {
        //we have a loser
        //render chopping block /player call
        choppingBlock.text('game over')
        //render call 
        var losingPlayer = board.playerNames[report.playerLostLife]
        currentBetElement.html(losingPlayer + ' lost and owes a round')
        //render result
        nextPlayerElement.text('')
        //render restart button
        hideMoves();
        $("#restartButton").show();
        $("#restartButton").off().click(function () { restartGame(board) });
        renderPlayers(board)
    }
}

function consumeEvents(gameID) {
    $.getJSON("/api/taenkeboks/" + gameID + "/events",
        function (event) {
            var roundEnd = event.eventCode == 'PerformedAction' && event.playerAction.call
            var board = event.board
            
            if (!roundEnd) {
                if (board) {
                    renderBet(board, gameID)
                    consumeEvents(gameID)
                } else
                    consumeEvents(gameID)
            }
            else
            {
                var gameReport = event.report.gameReport
                
                var renderAndConsume = function () {
                    renderBet(board, gameID)
                    consumeEvents(gameID)
                }
                var continuation;
                if (gameReport.playerLostLife >= 0 || gameReport.playerWinner >= 0) {
                    continuation = function () {
                        renderLifeResult(event.report, board, gameID, renderAndConsume)
                    }
                }
                else continuation = renderAndConsume;
                renderBetResults(event.report, board, gameID, continuation)
            }
        })
        .fail(
        function (jqXHR, textStatus, err) {
            console.log("Error getting events\ntextStatus: " + textStatus + "\njqXHR: " + jqXHR + "\err: " + err );
            //$("#dump").text(err)
            consumeEvents(gameID)
        });
}


$(document).ready(function () {
    gameID = parseInt($("#gameID").text())
    refreshBoard(gameID);
    consumeEvents(gameID);

});

function die(value) {
    if (value > 0)
        return '<img class="die" src="/images/Dice' + value + '.gif"/>'
    else if (value === 0)
        return '<img class="die" src="/images/DiceAny.gif" />'
}


function printDiceCount(b) {
    var valueString = die(b.value)
    var countString = String(b.count)
    return countString + " " + valueString
}

function printBet(b) {
    if (b.count === 0) return "round start"
    return printDiceCount(b)
}


function printAction(a) {
    if (a.call) {
        return " call"
    }
    else {
        return printBet(a.bet)
    }
}

function populateHand(hand, handElement) {
    handElement.empty()
    for (var di = 0; di < hand.length; di++)
        handElement.append($(die(hand[di])))
}

function populateBet(bet, betElement) {
    var betString = printBet(bet)
    betElement.html(betString)
}

function renderPlayers(b) {
    var playerCount = b.visible.playerCount
    var perspective = b.viewingSide
    $("#playerRow").empty()
    for (i = 1; i < playerCount; i++) {
        var side = (perspective + i) % playerCount
        $("#playerRow").append(playerCell(b, side))
    }
    $("#selfRow").empty().append(playerCell(b, perspective).attr('colspan', playerCount - 1));
}

function playerCell(b, side) {
    var diceLeft = b.visible.diceLeft[side]
    var playerName = b.playerNames[side]
    var livesLeft = b.visible.livesLeft[side]
    var lastBet = '&nbsp;'
    for (var i = 0; i < b.visible.moveHistory.length; i++) {
        if (b.visible.moveHistory[i].call) break;
        if (b.visible.hotseatHistory[i + 1] === side) {
            lastBet = printAction(b.visible.moveHistory[i])
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
        playerCell.append('<span class="playerName">' + playerName + '<span class="playerLives">&nbsp;x' + livesLeft +'</span></span>')

    playerCell.append('<div id=\"bet' + side + '\" class="playerBet">' + lastBet + '<div>')
    //var diceTable = $('<table class="dieTable"></table>')
    var hand;
    //diceRow.appendTo(diceTable)

    var handElement = $('<div id=\"hand' + side + '\">')
    if (side === b.viewingSide) {
        hand = b.hidden.hand
    }
    else {
        hand = Array.apply(null, Array(diceLeft)).map(Number.prototype.valueOf, 0);
    }
    populateHand(hand,handElement)
    playerCell.append(handElement)
    if (b.visible.nextPlayer === side)
        playerCell.addClass('active')
    return playerCell
}
function showMoves(legalMoves, gameID) {
    //call button
    if (legalMoves[0].call) {
        $("#callSpan").show()
        $("#callButton").off().click(function () {
            //window.alert("click");
            //alert($('#moves').val())
            $.ajax({
                type: 'POST',
                accepts: 'application/json',
                url: '/api/taenkeboks/' + gameID,
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
    var setValues = function (count,selected) {
        var lastValue = valueDropdown.val()
        valueDropdown.empty()
        values = fValues(count)
        for (var val in values) {
            $('<option />', { value: values[val], text: values[val] }).appendTo(valueDropdown);
        }
        if (lastValue)
        if(values.includes(parseInt(lastValue)))
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
            url: '/api/taenkeboks/' + gameID,
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


function hideMoves() {
    $("#callSpan").hide()
    $("#raiseSpan").hide()
    $("#continueButton").hide()
    $("#restartButton").hide()
}
function renderBet(b, gameID) {
    //$("#dump").empty().append(JSON.stringify(b, undefined, 2))
    var perspective = b.viewingSide
    var visible = b.visible
    var hidden = b.hidden
    var playerCount = visible.playerCount
    var nextPlayer = visible.nextPlayer
    var choppingBlock = visible.choppingBlock
    var playerNames = b.playerNames
    var nextPlayerName = playerNames[visible.nextPlayer]

    renderPlayers(b)
    //render players
    
    
    //action: chopping block
    //if(b.visible.choppingBlock < 0)
    //    $("#choppingBlock").html("&nbsp;")
    //else
    //    $("#choppingBlock").text(playerNames[choppingBlock] + " bet")
    $("#choppingBlock").html(visible.totalDiceLeft+ " dice in play")

    //result: current bet
    $("#currentBet").html(printBet(visible.currentBet))

    //reaction: next player / player prompt
    $("#nextPlayerText").empty()
    hideMoves()
    if (nextPlayer === perspective) {
        showMoves(b.legalMoves, gameID)
    }
    else {
        $("#nextPlayerText").text(playerNames[nextPlayer] + " to move")
    }
}


