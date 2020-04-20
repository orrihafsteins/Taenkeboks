namespace Taenkeboks.Console
open System
open System.IO
open System.Threading
open System.Threading.Tasks
open PIM
open Taenkeboks

    let asyncPlay (asyncPlayer:AsyncPlayer<TbVisible,TbAction>):Async<unit> = 
        async {
            let rec getMove ():Async<TbAction> =
                async {
                    printf "Enter move %s: " asyncPlayer.Name
                    let! sMove = Console.In.ReadLineAsync() |> Async.AwaitTask
                    match parseMove sMove with
                    | Result.Error err -> 
                        printfn "Could not parse move: %A" err.Message
                        return! getMove ()  
                    |  Result.Ok move ->
                        return move
                }
            printfn "Started player %s" asyncPlayer.Name
            while asyncPlayer.Next().IsCompleted |> not do
                let! v = asyncPlayer.Next() |> Async.AwaitTask
                printfn "----------------------------- UPDATE ---------------------------------"
                if v.roundReport <> TbRoundReport.empty then
                    v |> RoundReportView.create |> Json.serializeIndented |> printfn "%s" 
                if v.gameReport <> TbGameReport.empty then
                    v |> GameReportView.create |> Json.serializeIndented |> printfn "%s"
                if v.tournamentReport <> TbTournamentReport.empty then
                    v |> TournamentReportView.create |> Json.serializeIndented |> printfn "%s"
                printfn "MESSAGE: %s" v.playerMessage
                if v.nextPlayer = asyncPlayer.Side then
                    printfn "----------------------------- GET MOVE -------------------------------"
                    v |> StateView.create |> Json.serializeIndented |> printfn "%s" 
                    let! action = getMove ()
                    let! performed =  asyncPlayer.PerformAction(action) |> Async.AwaitTask
                    if not performed then failwith "Could not perform"
        }
