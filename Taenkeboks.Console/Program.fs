namespace Taenkeboks.Console
open System
open Taenkeboks

module Program =
    [<EntryPoint>]
    let main argv =
        printfn "--------------------------- TAENKEBOKS -------------------------------"

        let playerCount = 3
        let spec = TbGameSpec.initClassicRules playerCount
        let spec = {spec with playerCount = playerCount;extraLives = 1;lastStanding=true;diceCount=4}
        let game = TbGame.create spec
        let players = 
            [|
                TbAiPlayer.createPlayer spec "Bob"
                TbAiPlayer.createPlayer spec "Alice"
                //TaenkeboksPlayer.createPlayer spec "min" "min"
                ConsolePlayer.create "Orri"
            |]
        if players.Length <> playerCount then failwith "death"        
        let game = PIM.Game.play game players
        
        printfn "------------------------- TAENKEBOKS END -----------------------------"
        let t = Console.In.Read()
        0 // return an integer exit code
