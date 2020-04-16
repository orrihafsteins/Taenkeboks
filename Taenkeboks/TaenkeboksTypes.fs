namespace Taenkeboks
open PIM
open System
//'S: Game
//'A: Bet Option
//'V: Visible information (for player)

type TbGameSpec =
    {
        playerCount:int
        ofAnyKind:bool
        diceCount:int
        multiSeries:bool
        oneIsSeries:bool
        extraLives:int
        lastStanding:bool
    }

type TbHand = int[]

type TbPlayerState = 
    {
        hand:TbHand
        diceLeft:int
        livesLeft:int
        message:string
    }

type TbBet = {
    count:int
    value:int
}

type TbStatus =
    {
        inPlay:bool
        loser:Side
        winner:Side
   }
   
type TbAction = {
    call:bool
    bet:TbBet
}
       
type TbActionInstance = {
    time:DateTime
    side:Side
    action: TbAction
}

type TbRoundReport =
    {
        playerMadeBet:Side
        playerCalledBet:Side
        playerLost:Side
        playerDice:int[][]
        playerContribution:int[]
        betCalled:TbBet
        betHighestStanding:TbBet
    }

type TbGameReport =
    {
        playerLost:Side
    }

type TbTournamentReport =
    {
        playerWon:Side
        playerLost:Side
    }

type TbState =
    {
        playerNames:String[]
        totalDiceLeft:int
        spec:TbGameSpec
        currentBet:TbBet
        choppingBlock:Side
        playersLeft:int
        currentPlayer:Side
        playerStates:TbPlayerState[]
        status:TbStatus
        actionHistory: TbActionInstance List
        roundReport:TbRoundReport
        gameReport:TbGameReport
        tournamentReport:TbTournamentReport
    }

type TbVisible =
    {
        playerNames:string[]
        spec:TbGameSpec
        nextPlayer:int
        diceLeft:int[]
        livesLeft:int[]
        playerCount:int
        totalDiceLeft:int
        choppingBlock:int
        currentBet:TbBet
        playersLeft:int
        viewingPlayer:int
        actionHistory: TbActionInstance List
        status:TbStatus
        playerHand:TbHand
        playerMessage:string
        roundReport:TbRoundReport
        gameReport:TbGameReport
        tournamentReport:TbTournamentReport
    }

module TbGameSpec =
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
    let isSeries(spec:TbGameSpec) (hand:TbHand) = 
        if spec.oneIsSeries && hand = [|1|] then 
            true
        elif spec.multiSeries && hand.Length > 1 && hand = ([| hand.[0]..(hand.[0] + hand.Length) |]) then
            true
        else (spec.multiSeries |> not) && hand.Length > 1 && hand = ([| 1..hand.Length |])

module TbBet =
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
    let allLarger (spec:TbGameSpec) (bet:TbBet) (diceCount:int) = 
        let bCount,bValue = bet.count,bet.value
        let playerCount = spec.playerCount
        [|
            for value in (bValue + 1)..6 do yield bCount, value
            for count in (bCount + 1)..(diceCount + playerCount) do 
                if spec.ofAnyKind then yield count, 0
                for value in 2..6 do yield count, value
        |]
    let print (bet:TbBet) = 
        sprintf "%dd%d" bet.count bet.value
    let create (count,value) =
        {count=count;value=value}

module TbRoundReport = 
    let empty:TbRoundReport = 
        {
            playerMadeBet=Side.None
            playerCalledBet=Side.None
            playerLost=Side.None
            playerDice=Array.empty
            playerContribution = Array.empty
            betCalled=TbBet.startingBet
            betHighestStanding=TbBet.startingBet
        }
    let lostRound (state:TbState) (playerStates:TbPlayerState[]) (highestStanding:TbBet) loser playersLeft playerContributions  = 
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

module TbTournamentReport =
    let empty =
        {
            playerWon=Side.None
            playerLost=Side.None
        }
    let tournamentWon winner = 
        {
            playerWon=winner
            playerLost=Side.None
        }
    let tournamentLost loser = 
        {
            playerWon=Side.None
            playerLost= loser
        }

module TbGameReport = 
    let empty:TbGameReport= 
        {
            playerLost=Side.None
        }
    let gameLost loser = 
        {
            playerLost=loser
        }

module TbStatus =
    let inPlay: TbStatus =
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

module TbHand =
    let r = System.Random()
    let throw() = r.Next()%6 + 1
    let throwN n = Array.init n (fun i-> throw()) |> Array.sort
    let print(h:TbHand) = h |> Seq.map(fun i -> string i) |> String.concat ","

module TbPlayerState =
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
            hand = TbHand.throwN spec.diceCount
            livesLeft = livesLeft
            message = sprintf "Game start %d lives left" livesLeft
        }
    let initGame spec playerState:TbPlayerState =
        {playerState with
            diceLeft = if playerState.livesLeft > 0 then spec.diceCount else 0
            hand = if playerState.livesLeft > 0 then TbHand.throwN spec.diceCount else [||]
            message = sprintf "Game start %d lives left" playerState.livesLeft
        }
    let throw playerState:TbPlayerState = {playerState with hand = TbHand.throwN playerState.diceLeft}
    let loseGame playerState:TbPlayerState = {playerState with livesLeft = playerState.livesLeft - 1; message = "Lost game"}
    let surviveRound playerState:TbPlayerState = {playerState with diceLeft = playerState.diceLeft - 1;message = "Survived round"}
    let loseRound playerState:TbPlayerState = {playerState with message = "Lost round"}
    let setMessage message playerState:TbPlayerState = {playerState with message = message}

module TbAction =
    let call=
        {
            call=true
            bet=TbBet.startingBet
        }
    let raise bet=
        {
            call=false
            bet=bet
        }
    let print (a:TbAction) = 
        if a.call then "Call"
        else TbBet.print a.bet

module TbTaenkeboksState =
    let r = System.Random()
    let create(spec:TbGameSpec) (playerNames:string[])= 
        if spec.playerCount < 2 then failwith "Player count < 2"
        let startingPlayer = r.Next() % spec.playerCount
        {
            playerNames=playerNames
            spec=spec
            totalDiceLeft = spec.playerCount * spec.diceCount
            currentBet = TbBet.startingBet
            choppingBlock = -1
            playersLeft = spec.playerCount
            currentPlayer = startingPlayer
            playerStates = Array.init spec.playerCount (fun i -> TbPlayerState.initTournament spec)
            status = TbStatus.inPlay
            actionHistory = []
            roundReport = TbRoundReport.empty
            gameReport = TbGameReport.empty
            tournamentReport = TbTournamentReport.empty
        }
    // let messagePlayer msg side state =
    //     // Add a message to a particular player
    //     let playerStates = state.playerStates |> Array.mapi (fun i ps->
    //         if i = side then
    //             {ps with message = msg}
    //         else ps            
    //     )
    //     {state with playerStates = playerStates}

module TbVisible =
    let create (s:TbState) player = 
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
            tournamentReport = s.tournamentReport
        }
    let othersDice(v:TbVisible) = 
        v.diceLeft |> Array.mapi(fun i x -> (i<> v.viewingPlayer,x)) |> Array.filter fst |> Array.map snd

