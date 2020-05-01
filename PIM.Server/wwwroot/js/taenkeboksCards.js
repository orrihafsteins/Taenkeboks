class TbBet {
    constructor(count,die) {
        this.count = count
        this.die = die;
    }
    render() {
        return `${this.count} ${this.die.render()}`;
    }
}

class TbPlayer {
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
        this.card.items.lives = `<span class="badge badge-${livesClass} badge-pill">x${this.lives}</span>`
        this.card.items.dice = this.dice.render()
        this.card.items.bet = this.bet.render()
        this.card.cardClass = this.stateClasses[this.state]
        return this.card.render();
    }
}
