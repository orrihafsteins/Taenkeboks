namespace Taenkeboks.Console
open System
open Taenkeboks

module ConsolePlayer =
    let create name : TaenkeboksPlayer = 
        let rec getMove (state:PublicInformation) =
            printf "Enter move %s: " name
            let move = Console.ReadLine();
            match Json.deserialize<TaenkeboksAction>(move) with
            | Error err -> 
                printfn "Could not parse move: %A" err.Message
                getMove (state)  
            |  Ok oMove ->
                oMove
        {    
            playerName = name
            policy = (fun state ->
                getMove state            
            )
            updatePlayer = (fun v -> 
                v |> Json.serialize |> printfn "%s" 
            )
        } 
