namespace Taenkeboks.Console
open System
open Taenkeboks

module ConsolePlayer =
    let create (space:TaenkeboksGameSpace) name : TaenkeboksPlayer = 
        let rec getMove (state:PublicInformation) =
            printf "Enter move %s: " name
            let move = Console.ReadLine();
            match Json.deserialize<TaenkeboksAction>(move) with
            | Error err -> 
                printfn "Could not parse move: %A" err.Message
                getMove (state)  
            |  Ok oMove ->
                let t = space.validateAction state state.currentPlayer oMove  
                match t with
                | OK -> oMove
                | GameOver -> failwith "death"
                | InvalidAction message ->
                    printfn "Could not parse move: %A" message
                    getMove (state) 
        {    
            playerName = name
            policy = (fun state ->
                getMove state            
            )
            updatePlayer = (fun v -> 
                v |> Json.serialize |> printfn "%s" 
            )
            think = (fun v until-> ())
        } 
