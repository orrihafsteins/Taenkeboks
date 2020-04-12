namespace Taenkeboks.Console
open System
open Taenkeboks

module Program =
    [<EntryPoint>]
    let main argv =
        printfn "--------------------------- TAENKEBOKS -------------------------------"

        let playerCount = 2
        let spec = TbGameSpec.initClassicRules playerCount
        let spec = {spec with playerCount = playerCount;extraLives = 0;lastStanding=false;diceCount=1}
        let space = TbGameSpace.create spec
        let players = 
            [|
                TbPlayer.createPlayer spec "local" "local"
                //TaenkeboksPlayer.createPlayer spec "min" "min"
                ConsolePlayer.create "Orri"
            |]
        if players.Length <> playerCount then failwith "death"        
        let game = PIM.Game.play space players
        
        printfn "------------------------- TAENKEBOKS END -----------------------------"
        let t = Console.In.Read()
        0 // return an integer exit code
