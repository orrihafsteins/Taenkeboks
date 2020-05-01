class TbBet {
    constructor(bet) {
        this.bet = bet
    }
    render() {
        var die = new Die(this.bet.value)
        return `${this.bet.count} ${die.render()}`;
    }
}

class TbPlayerCard {
    constructor(name) {
        this.name = name
        this.card = new Card(name);
        this.lives = 1;
        this.bet = new TbBet({ count: 6, value: 6 });
        this.dice = new Hand([3, 3, 3]);
        this.stateClasses = {
            eliminated: "dark",
            toPlay: "primary",
            waiting: "secondary"
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
    constructor() {
        this.card = new Card("Current Bet");
        this.card.cardClass = "info"
        this.totalDice = 666
        this.madeBet = "Satan"
        this.bet = new TbBet(13,new Die(6))
        
    }
    render() {
        this.card.items['Total Dice'] = this.totalDice
        this.card.items['Player'] = this.madeBet
        this.card.items['Bet'] = this.bet.render()
        return this.card.render();
    }
}

var v = { "playerNames": ["orrihafsteins@gmail.com", "Alice", "Bob", "Carol"], "spec": { "playerCount": 4, "ofAnyKind": false, "diceCount": 4, "multiSeries": false, "oneIsSeries": true, "extraLives": 0, "lastStanding": false }, "nextSide": 0, "diceLeft": [4, 4, 4, 4], "livesLeft": [1, 1, 1, 1], "playerCount": 4, "totalDiceLeft": 16, "madeBetSide": 3, "currentBet": { "count": 6, "value": 2 }, "playersLeft": 4, "viewingSide": 0, "legalActions": [{ "call": true, "bet": { "count": 0, "value": 6 } }, { "call": false, "bet": { "count": 6, "value": 3 } }, { "call": false, "bet": { "count": 6, "value": 4 } }, { "call": false, "bet": { "count": 6, "value": 5 } }, { "call": false, "bet": { "count": 6, "value": 6 } }, { "call": false, "bet": { "count": 7, "value": 2 } }, { "call": false, "bet": { "count": 7, "value": 3 } }, { "call": false, "bet": { "count": 7, "value": 4 } }, { "call": false, "bet": { "count": 7, "value": 5 } }, { "call": false, "bet": { "count": 7, "value": 6 } }, { "call": false, "bet": { "count": 8, "value": 2 } }, { "call": false, "bet": { "count": 8, "value": 3 } }, { "call": false, "bet": { "count": 8, "value": 4 } }, { "call": false, "bet": { "count": 8, "value": 5 } }, { "call": false, "bet": { "count": 8, "value": 6 } }, { "call": false, "bet": { "count": 9, "value": 2 } }, { "call": false, "bet": { "count": 9, "value": 3 } }, { "call": false, "bet": { "count": 9, "value": 4 } }, { "call": false, "bet": { "count": 9, "value": 5 } }, { "call": false, "bet": { "count": 9, "value": 6 } }, { "call": false, "bet": { "count": 10, "value": 2 } }, { "call": false, "bet": { "count": 10, "value": 3 } }, { "call": false, "bet": { "count": 10, "value": 4 } }, { "call": false, "bet": { "count": 10, "value": 5 } }, { "call": false, "bet": { "count": 10, "value": 6 } }, { "call": false, "bet": { "count": 11, "value": 2 } }, { "call": false, "bet": { "count": 11, "value": 3 } }, { "call": false, "bet": { "count": 11, "value": 4 } }, { "call": false, "bet": { "count": 11, "value": 5 } }, { "call": false, "bet": { "count": 11, "value": 6 } }, { "call": false, "bet": { "count": 12, "value": 2 } }, { "call": false, "bet": { "count": 12, "value": 3 } }, { "call": false, "bet": { "count": 12, "value": 4 } }, { "call": false, "bet": { "count": 12, "value": 5 } }, { "call": false, "bet": { "count": 12, "value": 6 } }, { "call": false, "bet": { "count": 13, "value": 2 } }, { "call": false, "bet": { "count": 13, "value": 3 } }, { "call": false, "bet": { "count": 13, "value": 4 } }, { "call": false, "bet": { "count": 13, "value": 5 } }, { "call": false, "bet": { "count": 13, "value": 6 } }, { "call": false, "bet": { "count": 14, "value": 2 } }, { "call": false, "bet": { "count": 14, "value": 3 } }, { "call": false, "bet": { "count": 14, "value": 4 } }, { "call": false, "bet": { "count": 14, "value": 5 } }, { "call": false, "bet": { "count": 14, "value": 6 } }, { "call": false, "bet": { "count": 15, "value": 2 } }, { "call": false, "bet": { "count": 15, "value": 3 } }, { "call": false, "bet": { "count": 15, "value": 4 } }, { "call": false, "bet": { "count": 15, "value": 5 } }, { "call": false, "bet": { "count": 15, "value": 6 } }, { "call": false, "bet": { "count": 16, "value": 2 } }, { "call": false, "bet": { "count": 16, "value": 3 } }, { "call": false, "bet": { "count": 16, "value": 4 } }, { "call": false, "bet": { "count": 16, "value": 5 } }, { "call": false, "bet": { "count": 16, "value": 6 } }, { "call": false, "bet": { "count": 17, "value": 2 } }, { "call": false, "bet": { "count": 17, "value": 3 } }, { "call": false, "bet": { "count": 17, "value": 4 } }, { "call": false, "bet": { "count": 17, "value": 5 } }, { "call": false, "bet": { "count": 17, "value": 6 } }, { "call": false, "bet": { "count": 18, "value": 2 } }, { "call": false, "bet": { "count": 18, "value": 3 } }, { "call": false, "bet": { "count": 18, "value": 4 } }, { "call": false, "bet": { "count": 18, "value": 5 } }, { "call": false, "bet": { "count": 18, "value": 6 } }, { "call": false, "bet": { "count": 19, "value": 2 } }, { "call": false, "bet": { "count": 19, "value": 3 } }, { "call": false, "bet": { "count": 19, "value": 4 } }, { "call": false, "bet": { "count": 19, "value": 5 } }, { "call": false, "bet": { "count": 19, "value": 6 } }, { "call": false, "bet": { "count": 20, "value": 2 } }, { "call": false, "bet": { "count": 20, "value": 3 } }, { "call": false, "bet": { "count": 20, "value": 4 } }, { "call": false, "bet": { "count": 20, "value": 5 } }, { "call": false, "bet": { "count": 20, "value": 6 } }], "actionHistory": [{ "time": "2020-05-02T00:57:20.2083615+02:00", "side": 3, "action": { "call": false, "bet": { "count": 6, "value": 2 } } }, { "time": "2020-05-02T00:57:20.2056162+02:00", "side": 2, "action": { "call": false, "bet": { "count": 5, "value": 6 } } }, { "time": "2020-05-02T00:57:20.1929396+02:00", "side": 1, "action": { "call": false, "bet": { "count": 4, "value": 6 } } }, { "time": "2020-05-02T00:57:20.0967772+02:00", "side": 0, "action": { "call": false, "bet": { "count": 1, "value": 2 } } }], "status": { "inPlay": true, "loser": -1, "winner": -1 }, "playerHand": [1, 3, 3, 3], "playerMessage": "Carol raised 6d2", "roundReport": { "playerMadeBet": -1, "playerCalledBet": -1, "playerLost": -1, "playerDice": [], "playerContribution": [], "betCalled": { "count": 0, "value": 6 }, "betHighestStanding": { "count": 0, "value": 6 } }, "gameReport": { "playerLost": -1 }, "tournamentReport": { "playerWon": -1, "playerLost": -1 } }
class TbGame {
    constructor(gameId) {
        this.gameId = gameId
    }

    render() {
        var players = v.playerNames.map(function (pn, index) {
            var p = new TbPlayerCard(pn)
            if (index == v.nextSide) {
                p.dice = new Hand(v.playerHand)
            }
            else {
                p.dice = new Hand(Array(v.diceLeft[index]).fill(0))
            }
            p.lives = v.livesLeft[index]
            if (p.lives == 0)
                p.state = "eliminated"
            else if (v.nextSide == index)
                p.state = "toPlay"
            else
                p.state = "waiting"
            //Find last bet
            p.bet = new TbBet({ count:1, value:0 })
            for (var i = 0; i < v.actionHistory.length; i++) {
                if (v.actionHistory[i].action.call) break;
                if (v.actionHistory[i].side === index) {
                    p.bet = new TbBet(v.actionHistory[i].action.bet)
                    break;
                }
            }
            return p
        })
        var viewingPlayer = players[v.viewingSide];
        var otherPlayers = players.filter(function (p, index) { return v.viewingSide!=index})
        function renderOther(p) {
            return `<div class="col">${p.render()}</div>`
        }

        var mainCard = new TbMainCard()
        mainCard.bet = new TbBet(v.currentBet);
        mainCard.madeBet = v.playerNames[v.madeBetSide]
        mainCard.totalDice = v.totalDiceLeft
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
}

