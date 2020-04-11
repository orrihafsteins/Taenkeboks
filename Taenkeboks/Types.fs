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
        message:string
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
        playerMadeBet:Side
        playerCalledBet:Side
        playerLost:Side
        playerDice:int[][]
        playerContribution:int[]
        betCalled:Bet
        betHighestStanding:Bet
    }

type GameReport =
    {
        playerWon:Side
        playerLost:Side
        gameComplete:bool
    }


type TaenkeboksStatus =
    {
        inPlay:bool
        loser:Side
        winner:Side
   }

type TaenkeboksActionInstance = {
    time:DateTime
    side:Side
    action: TaenkeboksAction
}

type TaenkeboksState =
    {
        playerNames:String[]
        totalDiceLeft:int
        spec:TaenkeboksGameSpec
        currentBet:Bet
        choppingBlock:Side
        playersLeft:int
        currentPlayer:Side
        playerStates:PlayerState[]
        status:TaenkeboksStatus
        actionHistory: TaenkeboksActionInstance List
        roundReport:RoundReport
        gameReport:GameReport
    }

type PublicInformation =
    {
        playerNames:string[]
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
        playerMessage:string
        roundReport:RoundReport
        gameReport:GameReport
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
            lastStanding = true
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
        elif spec.multiSeries && hand.Length > 1 && hand = ([| hand.[0]..(hand.[0] + hand.Length) |]) then
            true
        else (spec.multiSeries |> not) && hand.Length > 1 && hand = ([| 1..hand.Length |])
            

module Bet =
    let startingBet = {
        count=0
        value=6//force update count
    }
    let call = {
        count = -1
        value = -1
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
        
        sprintf "%dd%d" bet.count bet.value
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
            playerWon=Side.None
            playerLost=Side.None
            gameComplete= false
        }
    let gameLost loser = 
        {
            playerWon=Side.None
            playerLost=loser
            gameComplete= false
        }
    let tournamentWon winner = 
        {
            playerWon=winner
            playerLost=Side.None
            gameComplete= true
        }
    let tournamentLost loser = 
        {
            playerWon=Side.None
            playerLost= loser
            gameComplete= true
        }

module TaenkeboksStatus =
    let defaultStatus: TaenkeboksStatus =
        {
            inPlay=true
            loser=Side.None
            winner=Side.None
        }
    let nextRound: TaenkeboksStatus =
        {
            inPlay=true
            loser=Side.None
            winner=Side.None
        }       
    let gameLost loser= 
        {
            inPlay=true
            loser=Side.None
            winner=Side.None
        }
    let tournamentLost loser = 
        {
            inPlay=false
            loser=loser
            winner=Side.None
        }
    let tournamentWon winner = 
        {
            inPlay=false
            loser=Side.None
            winner = winner
        }

module Hand =
    let r = System.Random()
    let throw() = r.Next()%6 + 1
    let throwN n = Array.init n (fun i-> throw()) |> Array.sort
    let print(h:Hand) = h |> Seq.map(fun i -> string i) |> String.concat ","

module PlayerState =
    let empty =
        {
            diceLeft = 0
            hand = [||]
            livesLeft = 0
            message = ""
        }
    let initTournament spec =
        let livesLeft = spec.extraLives + 1
        {
            diceLeft = spec.diceCount
            hand = Hand.throwN spec.diceCount
            livesLeft = livesLeft
            message = sprintf "Game start %d lives left" livesLeft
        }
    let initGame spec lives=
        {
            diceLeft = if lives > 0 then spec.diceCount else 0
            hand = if lives > 0 then Hand.throwN spec.diceCount else [||]
            livesLeft = lives
            message = sprintf "Game start %d lives left" lives
        }
    let throw playerState:PlayerState = {playerState with hand = Hand.throwN playerState.diceLeft}
    let surviveGame playerState:PlayerState = {playerState with hand = Array.empty; diceLeft = 0; message = "Survived game"}
    let surviveRound playerState:PlayerState = {playerState with hand = Array.empty; diceLeft = playerState.diceLeft - 1;message = "Survived round"}
    let loseRound playerState:PlayerState = {playerState with message = "Lost round"}
    let setMessage message playerState:PlayerState = {playerState with message = message}

module TaenkeboksAction =
    let call=
        {
            call=true
            bet=Bet.startingBet
        }
    let raise bet=
        {
            call=false
            bet=bet
        }
    let print (a:TaenkeboksAction) = 
        if a.call then "Call"
        else Bet.print a.bet
module TaenkeboksState =
    let r = System.Random()
    let init(spec:TaenkeboksGameSpec) (playerNames:string[])= 
        if spec.playerCount < 2 then failwith "Player count < 2"
        let startingPlayer = r.Next() % spec.playerCount
        {
            playerNames=playerNames
            spec=spec
            totalDiceLeft = spec.playerCount * spec.diceCount
            currentBet = Bet.startingBet
            choppingBlock = -1
            playersLeft = spec.playerCount
            currentPlayer = startingPlayer
            playerStates = Array.init spec.playerCount (fun i -> PlayerState.initTournament spec)
            status = TaenkeboksStatus.defaultStatus
            actionHistory = []
            roundReport = RoundReport.empty
            gameReport = GameReport.empty
        }
    let message msg side state =
        // Add a message to a particular player
        let playerStates = state.playerStates |> Array.mapi (fun i ps->
            if i = side then
                {ps with message = msg}
            else ps            
        )
        {state with playerStates = playerStates}

module PublicInformation =
    let create (s:TaenkeboksState) player = 
        {
            playerNames = s.playerNames 
            spec = s.spec
            nextPlayer = s.currentPlayer
            diceLeft = s.playerStates |> Array.map(fun p -> p.diceLeft)
            playerCount = s.playerStates.Length
            totalDiceLeft = s.totalDiceLeft
            choppingBlock = s.choppingBlock
            currentBet = s.currentBet
            playersLeft = s.playersLeft
            actionHistory = s.actionHistory
            livesLeft = s.playerStates |> Array.map(fun p-> p.livesLeft)
            viewingPlayer = player
            status = s.status
            playerMessage = s.playerStates.[player].message
            playerHand = s.playerStates.[player].hand
            roundReport = s.roundReport
            gameReport = s.gameReport
        }
    let othersDice(v:PublicInformation) = 
        v.diceLeft |> Array.mapi(fun i x -> (i<> v.viewingPlayer,x)) |> Array.filter fst |> Array.map snd

