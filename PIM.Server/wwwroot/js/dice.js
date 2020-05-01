class Die {
    constructor(value) {
        if(value == 0)
            this.value = "any";
        else
            this.value = value;
    }
    render() {
        return `<img class="die" src="/images/Dice${this.value}.gif">`;
    }
}

class Hand {
    constructor(values) {
        this.dice = values.map(function (value) { return new Die(value) });
    }
    render() {
        return this.dice.map(function (die) { return die.render() }).join('\n');
    }
}

