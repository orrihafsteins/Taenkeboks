namespace Taenkeboks
open PIM
open System

type TaenkeboksGameSpace = GameSpace<TaenkeboksState,TaenkeboksAction,PublicInformation>
module TaenkeboksGameSpace =
    let r = System.Random()
    let initPlayerStates (spec:TaenkeboksGameSpec) livesLeft= 
        livesLeft
        |> Array.mapi(fun i l-> PlayerState.initGame spec l)
    let countValues spec value (hand:Hand)  =
        if TaenkeboksGameSpec.isSeries spec hand then hand.Length + 1
        else
            hand |> Seq.filter(fun v -> v=value || v=1) |> Seq.length
    let countAnything spec (hands:Hand[]) = 
        let i, c = seq{2..6} |> Seq.mapi(fun i v -> i,(hands |> Seq.sumBy (countValues spec v))) |> Seq.maxBy snd
        c,i+2
    let countTotalValuesContributions spec value (hands:Hand[])  =
        if value = 0 then 
            if spec.ofAnyKind |> not then failwith "death"
            let c, v = countAnything spec hands
            let contributions = hands |>  Array.map(countValues spec v)
            c,contributions
        else
            let contributions = hands |>  Array.map(countValues spec value)
            let count = contributions |> Seq.sum
            count,contributions
    let countTotalValues spec value (hands:Hand[])  =
        if value = 0 then 
            if spec.ofAnyKind |> not then failwith "death"
            let c, v = countAnything spec hands
            c
        else
            hands |>  Seq.sumBy (countValues spec value)
    let evaluateBet (state:TaenkeboksState) (bet:Bet)=
        let count, value = bet.count,bet.value
        let totalCount = countTotalValues state.spec value (state.playerStates |> Array.map (fun p -> p.hand))
        totalCount >= count
    let highestStanding (state:TaenkeboksState) (bet:Bet) =
        //Returns the bet with the highest count that has the same value as bet and the contributions to the count from each player
        let count, value = bet.count,bet.value
        let totalCount,contributions = countTotalValuesContributions state.spec value (state.playerStates |> Array.map (fun p -> p.hand))
        {
            count=totalCount
            value=value
        },contributions
    let advanceBet(b:Bet) (state:TaenkeboksState): TaenkeboksState = 
        let nextPlayer =
            seq{1..state.spec.playerCount-1} 
            |> Seq.map(fun i -> (state.currentPlayer + i)%state.spec.playerCount)
            |> Seq.find(fun i -> state.playerStates.[i].diceLeft > 0)
        let playerStates = state.playerStates |> Array.map (fun ps -> ps |> PlayerState.setMessage (sprintf "%s raised %s" (state.playerNames.[state.currentPlayer]) (b|>Bet.print)))
        {state with
            choppingBlock = state.currentPlayer
            currentBet = b
            currentPlayer = nextPlayer
            actionHistory = {
                    time=DateTime.Now
                    side = state.currentPlayer
                    action = {
                            call=true
                            bet=b
                        }
                }::state.actionHistory
            playerStates= playerStates
            status = TaenkeboksStatus.defaultStatus
            roundReport = RoundReport.empty
            gameReport = GameReport.empty
        }
    let resolveBet (state:TaenkeboksState)  : TaenkeboksState = 
        let currentBet = state.currentBet
        let hStanding,contributions = highestStanding state currentBet
        let loser =
            if hStanding >= currentBet then //bet stands, current player loses
                state.currentPlayer
            else //betFails, choppingBlock loses
                state.choppingBlock
        let spec = state.spec
        let diceRemoved = state.playerStates |> Seq.mapi (fun i ps -> if i <> loser && ps.diceLeft > 0 then 1 else 0) |> Seq.sum
        let totalDiceLeft = state.totalDiceLeft - diceRemoved
        let playersRemoved  = state.playerStates |> Seq.mapi (fun i ps -> if i <> loser && ps.diceLeft = 1 then 1 else 0) |> Seq.sum
        let playersLeft = state.playersLeft - playersRemoved
        let roundReport = RoundReport.lostRound state state.playerStates hStanding loser playersLeft contributions
        let playerStates =
            state.playerStates
            |> Array.mapi(fun index p->
                let r =
                    if index <> loser then
                        if p.diceLeft = 0 then
                            p
                        elif p.diceLeft = 1 then
                            p |> PlayerState.surviveGame
                        else
                            p |> PlayerState.surviveRound |> PlayerState.throw
                    else
                        p |> PlayerState.loseRound |> PlayerState.throw
                r
            )
        if playerStates |> Seq.sumBy(fun p -> p.diceLeft) <> totalDiceLeft then
            failwith "death"
        
        
        
        let newState =
            if playersLeft > 1 then
                { state with
                        totalDiceLeft = totalDiceLeft
                        currentBet = Bet.startingBet
                        choppingBlock = -1
                        playersLeft = playersLeft
                        currentPlayer = loser
                        playerStates = playerStates
                        status = TaenkeboksStatus.nextRound
                        roundReport = roundReport
                        gameReport = GameReport.empty
                }
            else
                let playerLives = playerStates |> Array.map (fun p -> p.livesLeft)
                playerLives.[loser] <- playerLives.[loser] - 1 
                let livingPlayers = playerLives |> Seq.indexed |> Seq.filter (fun (i,l) -> l > 0) |> Seq.map fst |> Seq.toArray 
                if (spec.lastStanding && livingPlayers.Length > 1) || (not spec.lastStanding && livingPlayers.Length = spec.playerCount) then
                    //game not over, start a new round
                    { state with
                            totalDiceLeft = livingPlayers.Length * spec.diceCount
                            currentBet = Bet.startingBet
                            choppingBlock = -1
                            playersLeft = livingPlayers.Length
                            currentPlayer = if playerLives.[loser] > 0 then loser else livingPlayers.[r.Next() % livingPlayers.Length]
                            playerStates = initPlayerStates spec playerLives
                            status = TaenkeboksStatus.gameLost loser
                            roundReport = roundReport
                            gameReport = GameReport.empty
                    }
                elif (spec.lastStanding && livingPlayers.Length = 1) then
                    //playing for a winner and one player left
                    { state with
                        totalDiceLeft = 0
                        currentBet = Bet.startingBet
                        choppingBlock = Side.None
                        playersLeft = livingPlayers.Length
                        currentPlayer = Side.None
                        playerStates = initPlayerStates spec playerLives
                        status = TaenkeboksStatus.tournamentWon livingPlayers.[0]
                        roundReport = roundReport
                        gameReport = GameReport.tournamentWon livingPlayers.[0]
                    }
                elif (not spec.lastStanding && livingPlayers.Length < spec.playerCount) then
                    //playing for a loser and a player has lost all lives
                    { state with
                        totalDiceLeft = 0
                        currentBet = Bet.startingBet
                        choppingBlock = Side.None
                        playersLeft = livingPlayers.Length
                        currentPlayer = Side.None
                        playerStates = initPlayerStates spec playerLives    
                        status = TaenkeboksStatus.tournamentLost loser
                        roundReport = roundReport
                        gameReport = GameReport.tournamentLost loser
                    }
                else 
                    failwith "death"
        newState
    let legalActions g s =
        if g.currentPlayer <> s then
              Array.empty
        else
            let currentDice = g.playerStates |> Seq.sumBy(fun p -> p.diceLeft)
            let higherBets =
                Bet.allLarger g.spec g.currentBet currentDice
            let actions =
                if g.currentBet = Bet.startingBet then
                    Array.init(higherBets.Length)(fun i->
                        TaenkeboksAction.raise (Bet.create higherBets.[i])
                    )
                else
                    Array.init(higherBets.Length + 1)(fun i->
                        if i = 0 then
                            TaenkeboksAction.call
                        else
                            TaenkeboksAction.raise (Bet.create higherBets.[i - 1])
                    )
            actions    
    let advance (state:TaenkeboksState) (side:Side) (action:TaenkeboksAction):TaenkeboksState= 
        let setPlayerMessage msg = TaenkeboksState.message msg side    
        if not state.status.inPlay then
            state |> setPlayerMessage "GameOver"
        elif action.call && state.currentBet = Bet.startingBet then
            state |> setPlayerMessage "Can't call initial bet"
        elif state.currentPlayer <> side then
            state |> setPlayerMessage "Not players turn"
        elif (not action.call) && (action.bet<=state.currentBet) then
            state |> setPlayerMessage "Raise must be larger"
        else      
            if action.call then 
                resolveBet state
            else    
                advanceBet action.bet state     
    let create (spec:TaenkeboksGameSpec):TaenkeboksGameSpace= 
        {
            init = (fun playerNames -> TaenkeboksState.init spec playerNames)   //(fun ps -> TaenkeboksState.init spec ps.Length)
            advance = advance
            visible = PublicInformation.create
            gameOver= (fun state -> not state.status.inPlay)
            
            checkTime = (fun g -> g)
            nextPlayer = (fun g-> g.currentPlayer)
        }

