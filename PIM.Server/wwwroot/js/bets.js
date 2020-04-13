var sCounts =
    [
        "INITIAL BET",
        "one",
        "two",
        "three",
        "four",
        "five",
        "six",
        "seven",
        "eight",
        "nine",
        "ten",
        "eleven",
        "twelve",
        "thirteen",
        "fourteen",
        "fifteen",
        "sixteen",
        "seventeen",
        "eighteen",
        "nineteen"
    ]
var sValuesPlural =
    [
        "of a kind",
        "ones",
        "twos",
        "threes",
        "fours",
        "fives",
        "sixes"
    ]
var sValuesSingular =
    [
        "of a kind",
        "one",
        "two",
        "three",
        "four",
        "five",
        "six"
    ]


function printBet(b) {
    var valueString = "of a kind"
    if (b.count === 1) {
        valueString = sValuesSingular[b.value]
    }
    else if (b.count > 1)
        valueString = sValuesPlural[b.value]
    var countString = String(b.count)
    if (b.count < 20)
        countString = sCounts[b.count]
    return countString + " " + valueString
}

function printAction(a) {
    if (a.call) {
        return "Call"
    }
    else {
        return printBet(a.bet)
    }
}
