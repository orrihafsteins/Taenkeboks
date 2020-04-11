namespace Taenkeboks.Console
open System
open Taenkeboks
type PlayerRoundReportView = {
    name:string
    dice:int[]
    contribution:int
}
type RoundReportView = {
    playerMadeBet:string
    playerCalledBet:string
    playerLost:string
    players:PlayerRoundReportView[]
    betCalled:string
    betHighestStanding:string
}

module RoundReportView = 
    let create (v:PublicInformation) :RoundReportView =
        let report = v.roundReport
        {
            playerMadeBet=v.playerNames.[report.playerMadeBet]
            players = Array.init v.playerCount (fun i -> {name=v.playerNames.[i];dice=report.playerDice.[i];contribution=report.playerContribution.[i]})
            playerCalledBet=v.playerNames.[report.playerCalledBet]
            playerLost=v.playerNames.[report.playerLost]
            betCalled=report.betCalled |> Bet.print
            betHighestStanding= report.betHighestStanding |> Bet.print
        }

type GameReportView = {
    playerLost:string
}
module GameReportView = 
    let create (v:PublicInformation) =
        let report = v.gameReport
        {
            playerLost = if report.playerLost >= 0 then v.playerNames.[report.playerLost] else ""
        }        

type TournamentReportView = {
    playerWon:string
    playerLost:string
}
module TournamentReportView = 
    let create (v:PublicInformation) =
        let report = v.tournamentReport
        {
            playerLost = if report.playerLost >= 0 then v.playerNames.[report.playerLost] else ""
            playerWon = if report.playerWon >= 0 then v.playerNames.[report.playerWon] else ""
        }        

type PlayerStateView ={
    name:string
    diceLeft: int
    livesLeft: int
}    
type StateView = {
        nextPlayer:string
        players:PlayerStateView[]
        totalDiceLeft:int
        choppingBlock:string
        currentBet:string
        playerMessage:string
        playerHand:Hand
}
module StateView = 
    let create (info:PublicInformation) =
        let choppingBlock = if info.choppingBlock >= 0 then info.playerNames.[info.choppingBlock] else ""
        {
            nextPlayer = info.playerNames.[info.nextPlayer]
            players = Array.init info.playerCount (fun i -> {name=info.playerNames.[i];diceLeft=info.diceLeft.[i];livesLeft=info.livesLeft.[i]})
            totalDiceLeft = info.totalDiceLeft
            choppingBlock = choppingBlock
            currentBet = info.currentBet |> Bet.print
            playerMessage = info.playerMessage
            playerHand = info.playerHand
        }

module ConsolePlayer =
    let create name : TaenkeboksPlayer = 
        let parseMove s:Result<TaenkeboksAction,Exception> =
            if s = "c" then
                Ok TaenkeboksAction.call 
            else 
                try
                    let count = Int32.Parse(string s.[0])
                    let value = Int32.Parse(string s.[2])
                    Ok (TaenkeboksAction.raise {count = count;value = value})
                with 
                | e -> Error e

        let rec getMove () =
            printf "Enter move %s: " name
            let sMove = Console.ReadLine();
            match parseMove sMove with
            | Error err -> 
                printfn "Could not parse move: %A" err.Message
                getMove ()  
            |  Ok move ->
                move
        {    
            playerName = name
            policy = (fun v ->
                printfn "----------------------------- POLICY ---------------------------------"
                v |> StateView.create |> Json.serializeIndented |> printfn "%s" 
                getMove ()
            )
            updatePlayer = (fun v -> 
                printfn "----------------------------- UPDATE ---------------------------------"
                if v.roundReport <> RoundReport.empty then
                    v |> RoundReportView.create |> Json.serializeIndented |> printfn "%s" 
                if v.gameReport <> GameReport.empty then
                    v |> GameReportView.create |> Json.serializeIndented |> printfn "%s"
                if v.tournamentReport <> TournamentReport.empty then
                    v |> TournamentReportView.create |> Json.serializeIndented |> printfn "%s"
                printfn "MESSAGE: %s" v.playerMessage
            )
        } 
