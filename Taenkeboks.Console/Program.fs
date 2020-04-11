namespace Taenkeboks.Console
open System
open Taenkeboks

module Program =
    [<EntryPoint>]
    let main argv =
        printfn "--------------------------- TAENKEBOKS -------------------------------"

        let playerCount = 2
        let spec = TaenkeboksGameSpec.initClassicRules playerCount
        let spec = {spec with playerCount = playerCount;extraLives = 0;lastStanding=false}
        let space = TaenkeboksGameSpace.create spec
        let players = 
            [|
                TaenkeboksPlayer.createPlayer spec "local" "local"
                //TaenkeboksPlayer.createPlayer spec "min" "min"
                ConsolePlayer.create "Orri"
            |]
        if players.Length <> playerCount then failwith "death"        
        let game = PIM.Game.play space players
        
        printfn "------------------------- TAENKEBOKS END -----------------------------"
        let t = Console.In.Read()
        0 // return an integer exit code
