namespace Taenkeboks
open System

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
    let create (v:TbVisible) :RoundReportView =
        let report = v.roundReport
        {
            playerMadeBet=v.playerNames.[report.playerMadeBet]
            players = Array.init v.playerCount (fun i -> {name=v.playerNames.[i];dice=report.playerDice.[i];contribution=report.playerContribution.[i]})
            playerCalledBet=v.playerNames.[report.playerCalledBet]
            playerLost=v.playerNames.[report.playerLost]
            betCalled=report.betCalled |> TbBet.print
            betHighestStanding= report.betHighestStanding |> TbBet.print
        }

type GameReportView = {
    playerLost:string
}

module GameReportView = 
    let create (v:TbVisible) =
        let report = v.gameReport
        {
            playerLost = if report.playerLost >= 0 then v.playerNames.[report.playerLost] else ""
        }        

type TournamentReportView = {
    playerWon:string
    playerLost:string
}

module TournamentReportView = 
    let create (v:TbVisible) =
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
        playerHand:TbHand
}

module StateView = 
    let create (info:TbVisible) =
        {
            nextPlayer = info.playerNames.[info.nextPlayer]
            players = Array.init info.playerCount (fun i -> {name=info.playerNames.[i];diceLeft=info.diceLeft.[i];livesLeft=info.livesLeft.[i]})
            totalDiceLeft = info.totalDiceLeft
            choppingBlock = if info.choppingBlock >= 0 then info.playerNames.[info.choppingBlock] else ""
            currentBet = info.currentBet |> TbBet.print
            playerMessage = info.playerMessage
            playerHand = info.playerHand
        }

module TbConsole =
    let parseMove s:Result<TbAction,Exception> =
        if s = "c" then
            Result.Ok TbAction.call 
        else 
            try
                let tokens = s.Split "d"
                let count = Int32.Parse(tokens.[0])
                let value = Int32.Parse(tokens.[1])
                Result.Ok (TbAction.raise {count = count;value = value})
            with 
            | e -> Error e
    let promptPlayer name v = 
        printfn "----------------------------- GET MOVE -------------------------------"
        v |> StateView.create |> Json.serializeIndented |> printfn "%s" 
    let getPlayerMove name v = 
        let rec getMove () =
            printf "Enter move %s: " name
            let sMove = Console.ReadLine();
            match parseMove sMove with
            | Result.Error err -> 
                printfn "Could not parse move: %A" err.Message
                getMove ()  
            |  Result.Ok move ->
                move
        promptPlayer name v
        getMove ()
    let updatePlayer name v =
        printfn "----------------------------- UPDATE ---------------------------------"
        if v.roundReport <> TbRoundReport.empty then
            v |> RoundReportView.create |> Json.serializeIndented |> printfn "%s" 
        if v.gameReport <> TbGameReport.empty then
            v |> GameReportView.create |> Json.serializeIndented |> printfn "%s"
        if v.tournamentReport <> TbTournamentReport.empty then
            v |> TournamentReportView.create |> Json.serializeIndented |> printfn "%s"
        printfn "MESSAGE: %s" v.playerMessage
