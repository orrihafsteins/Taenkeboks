var wildcard = new Card("Wild Card");
var hand = new Hand([1, 3, 5, 6])
wildcard.items = {
    dice: hand.render()
}
var wildcardPlayer = new TbPlayer("Wildcard Player");
var display = wildcardPlayer.render();
