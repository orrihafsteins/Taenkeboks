namespace Taenkeboks.Console
open System
open Taenkeboks

//module ConsolePlayer =
//    let create name : TaenkeboksPlayer = {
//            playerName = name
//            policy = (fun state ->
//                printf "Enter move: "
//                let move = Console.ReadLine();
//                TaenkeboksAction.Parse
//            )
//            updatePlayer = (fun v -> ())
//            think = (fun v until-> ())
//        } 

module TestGame =
    let playerCount = 2
    let spec = TaenkeboksGameSpec.initClassicRules playerCount
    let space = TaenkeboksGameSpace.create spec
    let players = 
        [|
            TaenkeboksPlayer.createPlayer spec "local" "local"
            TaenkeboksPlayer.createPlayer spec "min" "min"
        |]
    
    let game = PIM.Game.play
    