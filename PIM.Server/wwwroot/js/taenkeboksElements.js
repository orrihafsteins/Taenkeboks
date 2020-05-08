class TbBet {
    constructor(bet) {
        this.bet = bet
    }
    render() {
        if (this.bet.count == 0)
            return ""
        var die = new Die(this.bet.value)
        return `${this.bet.count} ${die.render()}`;
    }
}

class TbPlayerCard {
    constructor(name) {
        this.name = name
        this.card = new Card(name);
        this.lives = 1;
        this.bet = new TbBet({ count: 0, value: 6 });
        this.dice = new Hand([]);
        this.stateClasses = {
            eliminated: "dark",
            toPlay: "primary",
            waiting: "secondary",
            safe: "dark",
            winner: "success",  
            loser: "danger"
        }
        this.state = "waiting"
    }
    render() {
        var livesClass = (this.lives > 0) ? "success" : "danger";
        this.card.items['Lives'] = `<span class="badge badge-${livesClass} badge-pill">x${this.lives}</span>`
        this.card.items['Dice'] = this.dice.render()
        this.card.items['Bet'] = this.bet.render()
        this.card.cardClass = this.stateClasses[this.state]
        return this.card.render();
    }
}

class TbMainCard {
    static Bet(v) {
        var card = new Card("Current Bet");
        card.items['Total Dice'] = v.totalDiceLeft
        card.items['Player'] = (v.madeBetSide >= 0) ? v.playerNames[v.madeBetSide] : ""
        card.items['Bet'] = (new TbBet(v.currentBet)).render()
        return card
    }

    static Round(v) {
        var card = new Card("Call Report");
        var report = v.roundReport
        var betValue = report.betHighestStanding.value //highest standing has the best value in case of "any" valued bet
        var betCalled = new TbBet(report.betCalled)
        var contributions = report.playerContribution.filter(c => c > 0).map(c => new TbBet({ count: c, value: betValue })).map(b => b.render())
        var callingPlayer = v.playerNames[report.playerCalledBet]
        var bettingPlayer = v.playerNames[report.playerMadeBet]
        var losingPlayer = v.playerNames[report.playerLost]
        var contributionString = contributions.join(" + ")
        var comp = (report.playerMadeBet == report.playerLost) ? " < " : " >= "
        var evaluation = contributionString + comp + betCalled.render()
        var cnt = '<button class="btn btn-success" id="continue">Continue</button>'
        card.items['Made Bet'] = bettingPlayer
        card.items['Called Bet'] = callingPlayer
        card.items['Evaluation'] = evaluation
        card.items['Loser'] = losingPlayer
        card.items['Continue'] = cnt
        return card
    }

    static Game(v) {
        var card = new Card("Round Report");
        var report = v.gameReport
        var losingPlayer = v.playerNames[report.playerLost]
        var losingPlayerLives = v.livesLeft[report.playerLost]
        var roundResult;
        if (losingPlayerLives === 0)
            roundResult = losingPlayer + ' is eliminated'
        else
            roundResult = losingPlayer + ' lost a life, ' + losingPlayerLives + ' remaining'
        var cnt = '<button class="btn btn-success" id="continue">Continue</button>'
        card.items['Round Result'] = roundResult
        card.items['Continue'] = cnt
        return card
    }

    static Tournament(v) {
        var card = new Card("Game Report");
        var report = v.tournamentReport
        if (report.playerLost >= 0)
            card.items['Loser'] = v.playerNames[report.playerLost]
        if (report.playerWon >= 0)
            card.items['Winner'] = v.playerNames[report.playerWon]
        card.items['Restart'] = '<button class="btn btn-success" id="restartButton">Restart</button>'
        card.items['New Game'] = '<button class="btn btn-success" id="newGameButton">New Game</button>'
        return card
    }
}


class TbUtil {
    static renderOption(value, description, selectedValue) {
        var selected = '';
        if (value == selectedValue)
            selected = 'selected="selected" '
        return `<option ${selected}value="${value}">${description}</option>`
    }

    static renderDropdown(id, values, fDescription, selectedValue, onchange = null) {
        onchange = (onchange == null) ? "" : 'onchange = "' + onchange + '"'
        var items = values.map(value => this.renderOption(value, fDescription(value), selectedValue, selectedValue)).join('\n')
        return `<select class="form-control" id="${id}" ${onchange}>${items}<select>`
    }

    static updateDropdown(id, values, fDescription, selectedValue) {
        var element = $('#' + id)
        var selectedValue = element.val()
        function renderOption(value, description) {
            var selected = '';
            if (value == selectedValue)
                selected = 'selected="selected" '
            return `<option ${selected}value="${value}">${description}</option>`
        }
        var items = values.map(value => renderOption(value, fDescription(value))).join('\n')
        return element.html(items)
    }
}

class TbAction {
    constructor(v) {
        this.v=v
    }
    render() {
        var v = this.v
        var raises = v.legalActions.filter(a => !a.call)
        var nextRaise = raises[0]
        var counts = Array.from(new Set(raises.map(a => a.bet.count)))
        var fValues = function (count) {
            var countBet = raises.filter(a => a.bet.count == count)
            var countValues = countBet.map(a => a.bet.value)
            return countValues
        }
        var callId = "callButton"
        var raiseId = "raiseButton"
        var countsId = "raiseCounts"
        var valuesId = "raiseValues"
        var onCountChange = `(function(selectedIndex,options){
                                var raises = ${JSON.stringify(raises).replace(/"/g, "'")};
                                var fValues = function (count) {
                                    var countBet = raises.filter(a => a.bet.count == count)
                                    var countValues = countBet.map(a => a.bet.value)
                                    return countValues
                                }
                                var count = options[selectedIndex].value
                                TbUtil.updateDropdown('${valuesId}', fValues(count), v => (v==0)?'Any':v, null)                                

                            })(this.selectedIndex,this.options)`
        var sCountDropdown = TbUtil.renderDropdown(countsId, counts, c => c, nextRaise.bet.count, onCountChange)
        var sValueDropdown = TbUtil.renderDropdown(valuesId, fValues(nextRaise.bet.count), v => (v == 0) ?'Any':v, nextRaise.bet.value)
        var sCall = ""
        if (v.madeBetSide >= 0)
            sCall = `
                <button class="btn btn-success" id="${callId}">Call</button>
                &nbsp;/&nbsp;
            `
        return `    
<div>
    <form class="form-inline m-1" onsubmit="return false">
        ${sCall}
        <button class="btn btn-success" id="${raiseId}">Raise</button>
        &nbsp;
        <div>${sCountDropdown}
        &nbsp;of&nbsp;
        ${sValueDropdown}
    </form>
</div>`
    }
}
