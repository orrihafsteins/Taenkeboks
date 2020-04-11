namespace Taenkeboks.Console
open System
open Taenkeboks

module Program =
    [<EntryPoint>]
    let main argv =
        printfn "--------------Taenkeboks---------------"

        let playerCount = 3
        let spec = TaenkeboksGameSpec.initClassicRules playerCount
        let space = TaenkeboksGameSpace.create spec
        let players = 
            [|
                TaenkeboksPlayer.createPlayer spec "local" "local"
                TaenkeboksPlayer.createPlayer spec "min" "min"
                ConsolePlayer.create "Orri"
            |]
        if players.Length <> playerCount then failwith "death"        
        let game = PIM.Game.play space players
        
        printfn "--------------Taenkeboks Done---------------"
        let t = Console.In.Read()
        0 // return an integer exit code
