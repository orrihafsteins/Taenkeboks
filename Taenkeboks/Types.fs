namespace Taenkeboks
open PIM
open System

//'S: Game
//'A: Bet Option
//'V: Visible information (for player)

type TaenkeboksGameSpec =
    {
        playerCount:int
        ofAnyKind:bool
        diceCount:int
        multiSeries:bool
        oneIsSeries:bool
        extraLives:int
        lastStanding:bool
    }

type Hand = int[]

type PlayerState = 
    {
        hand:Hand
        diceLeft:int
        livesLeft:int
    }

type Bet = {
    count:int
    value:int
}

type TaenkeboksAction = {
    call:bool
    bet:Bet
}
    
type RoundReport =
    {
        playerMadeBet:int
        playerCalledBet:int
        playerLost:int
        playerDice:int[][]
        playerContribution:int[]
        betCalled:Bet
        betHighestStanding:Bet
    }

type GameReport =
    {
        playerWinner:int
        playerLostLife:int
        gameComplete:bool
    }

type TaenkeboksStatus =
    {
        inPlay:bool
        loser:Side
        winner:Side
        roundReport:RoundReport
        gameReport:GameReport
   }

type TaenkeboksActionInstance = {
    time:DateTime
    side:Side
    action: TaenkeboksAction
}

type TaenkeboksState =
    {
        totalDiceLeft:int
        spec:TaenkeboksGameSpec
        currentBet:Bet
        choppingBlock:Side
        playersLeft:int
        message:string
        currentPlayer:Side
        playerStates:PlayerState[]
        status:TaenkeboksStatus
        actionHistory: TaenkeboksActionInstance List
    }

type PublicInformation =
    {
        spec:TaenkeboksGameSpec
        nextPlayer:int
        diceLeft:int[]
        livesLeft:int[]
        playerCount:int
        totalDiceLeft:int
        choppingBlock:int
        currentBet:Bet
        playersLeft:int
        viewingPlayer:int
        actionHistory: TaenkeboksActionInstance List
        status:TaenkeboksStatus
        playerHand:Hand
    }

module TaenkeboksGameSpec =
    let initClassicRules playerCount =
        {
            playerCount = playerCount
            ofAnyKind=false
            diceCount=4
            multiSeries=false
            oneIsSeries=true
            extraLives = 1
            lastStanding = false
        }
    let initTestRules playerCount= 
        {
            playerCount = playerCount
            ofAnyKind = false
            diceCount = 1
            multiSeries = false
            oneIsSeries = true
            extraLives = 0
            lastStanding = true
        }   
    let initGoldenLionRules playerCount= 
        {
            playerCount=playerCount
            ofAnyKind=true
            diceCount=4
            multiSeries=true
            oneIsSeries=true
            extraLives = 1
            lastStanding = true
        }   
    let isSeries(spec:TaenkeboksGameSpec) (hand:Hand) = 
        if spec.oneIsSeries && hand = [|1|] then 
            true
        elif spec.multiSeries && hand.Length > 1 && hand = ([| hand.[0]..hand.[0] + hand.Length - 1 |]) then
            true
        elif (spec.multiSeries |> not) && hand.Length > 1 && hand = ([| 1..hand.Length |]) then
            true
        else 
            false

module Bet =
    let startingBet = {
        count=0
        value=6//must update count
    }
    let all spec = 
        let diceCount = spec.diceCount
        let playerCount = spec.playerCount
        [|
            yield startingBet
            for count in 1..(diceCount * playerCount + playerCount) do 
                if spec.ofAnyKind then yield {count=count; value=0}
                for value in 2..6 do yield {count=count; value=value}
        |]
    let allLarger (spec:TaenkeboksGameSpec) (bet:Bet) (diceCount:int) = 
        let bCount,bValue = bet.count,bet.value
        let playerCount = spec.playerCount
        [|
            for value in (bValue + 1)..6 do yield bCount, value
            for count in (bCount + 1)..(diceCount + playerCount) do 
                if spec.ofAnyKind then yield count, 0
                for value in 2..6 do yield count, value
        |]
    let print (bet:Bet) = 
        let v = if bet.value = 0 then "anything" else (sprintf "%ds" bet.value)
        sprintf "%d of %s" bet.count v
    let create (count,value) =
        {count=count;value=value}

module RoundReport = 
    let empty:RoundReport = 
        {
            playerMadeBet=Side.None
            playerCalledBet=Side.None
            playerLost=Side.None
            playerDice=Array.empty
            playerContribution = Array.empty
            betCalled=Bet.startingBet
            betHighestStanding=Bet.startingBet
        }
    let lostRound (state:TaenkeboksState) (playerStates:PlayerState[]) (highestStanding:Bet) loser playersLeft playerContributions  = 
        
        let playerDice = playerStates |> Array.map (fun p->p.hand)
        let roundReport = 
             {
                playerMadeBet=state.choppingBlock
                playerCalledBet=state.currentPlayer
                playerLost=loser
                playerDice=playerDice
                playerContribution = playerContributions
                betCalled=state.currentBet
                betHighestStanding=highestStanding
            }
        roundReport

module GameReport = 
    let empty:GameReport= 
        {
            playerWinner=Side.None
            playerLostLife=Side.None
            gameComplete= false
        }
    let playerLostLife loser = 
        {
            playerWinner=Side.None
            playerLostLife=loser
            gameComplete= false
        }
    let gameWon winner = 
        {
            playerWinner=winner
            playerLostLife=Side.None
            gameComplete= true
        }
    let gameLost loser = 
        {
            playerWinner=Side.None
            playerLostLife= loser
            gameComplete= true
        }

module TaenkeboksStatus =
    let initialStatus: TaenkeboksStatus =
        {
            inPlay=true
            loser=Side.None
            winner=Side.None
            roundReport = RoundReport.empty
            gameReport = GameReport.empty
        }
    let nextRound roundReport: TaenkeboksStatus =
        {
            inPlay=true
            loser=Side.None
            winner=Side.None
            roundReport = roundReport
            gameReport = GameReport.empty
        }       
    let lostLifeStatus roundReport loser= 
        {
            inPlay=true
            loser=Side.None
            winner=Side.None
            roundReport = roundReport
            gameReport = GameReport.playerLostLife loser
        }
    let lostGameStatus roundReport loser = 
        {
            inPlay=false
            loser=loser
            winner=Side.None
            roundReport=roundReport
            gameReport = GameReport.gameLost loser
        }
    let wonGameStatus roundReport winner = 
        {
            inPlay=false
            loser=Side.None
            winner = winner
            roundReport=roundReport
            gameReport = GameReport.gameWon winner
        }

module Hand =
    let r = new System.Random()
    let throw() = r.Next()%6 + 1
    let throwN n = Array.init n (fun i-> throw()) |> Array.sort
    let print(h:Hand) = h |> Seq.map(fun i -> string i) |> String.concat ","

module PlayerState =
    let empty =
        {
            diceLeft = 0
            hand = [||]
            livesLeft = 0
        }
    let init spec =
        {
            diceLeft = spec.diceCount
            hand = Hand.throwN spec.diceCount
            livesLeft = spec.extraLives + 1
        }
    let initRound spec lives=
        {
            diceLeft = if lives > 0 then spec.diceCount else 0
            hand = if lives > 0 then Hand.throwN spec.diceCount else [||]
            livesLeft = lives
        }
    let throw playerState:PlayerState = {playerState with hand = Hand.throwN playerState.diceLeft}
    let clear playerState:PlayerState = {playerState with hand = Array.empty}
    let win playerState:PlayerState = {playerState with hand = Array.empty; diceLeft = playerState.diceLeft - 1}
    let loseRound playerState:PlayerState = {playerState with livesLeft = playerState.livesLeft - 1}

module TaenkeboksAction =
    let print (a:TaenkeboksAction) = 
        if a.call then "Call"
        else Bet.print a.bet
    let call bet=
        {
            call=true
            bet=bet
        }
    let raise bet=
        {
            call=false
            bet=bet
        }

module TaenkeboksState =
    let init(spec:TaenkeboksGameSpec) playerCount currentPlayer:TaenkeboksState = 
        {
            spec=spec
            totalDiceLeft = playerCount * spec.diceCount
            currentBet = Bet.startingBet
            choppingBlock = -1
            playersLeft = playerCount
            message = ""
            currentPlayer = currentPlayer
            playerStates = Array.init playerCount (fun i -> PlayerState.init spec)
            status = TaenkeboksStatus.initialStatus
            actionHistory = []
        }

module PublicInformation =
    let create (s:TaenkeboksState) player = 
        {
            spec=s.spec
            nextPlayer = s.currentPlayer
            diceLeft = s.playerStates |> Array.map(fun p -> p.diceLeft)
            playerCount= s.playerStates.Length
            totalDiceLeft = s.totalDiceLeft
            choppingBlock = s.choppingBlock
            currentBet=s.currentBet
            playersLeft = s.playersLeft
            actionHistory = s.actionHistory
            livesLeft = s.playerStates |> Array.map(fun p-> p.livesLeft)
            viewingPlayer = player
            status=s.status
            playerHand = s.playerStates.[player].hand
        }
    let othersDice(v:PublicInformation) = 
        v.diceLeft |> Array.mapi(fun i x -> (i<> v.viewingPlayer,x)) |> Array.filter fst |> Array.map snd

