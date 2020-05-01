class TbBet {
    constructor(count,die) {
        this.count = count
        this.die = die;
    }
    render() {
        return `${this.count} ${this.die.render()}`;
    }
}

class TbPlayerCard {
    constructor(name) {
        this.name = name
        this.card = new Card(name);
        this.lives = 1;
        this.bet = new TbBet(3, new Die(6));
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

class TbGame {
    constructor(gameId) {
        this.gameId = gameId
        this.viewingPlayer = new TbPlayerCard("orrihafsteinz@gmail.com")
        this.mainCard = new TbMainCard()
        this.otherPlayers = [
            new TbPlayerCard("Alice"),
            new TbPlayerCard("Bob"),
            new TbPlayerCard("Carol")
        ]
    }

    render() {
        
    }
}

