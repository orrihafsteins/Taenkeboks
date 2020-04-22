namespace Taenkeboks
open PIM
open System

module TbRules =
    let r = System.Random()
    let countValues spec value hand =
        if TbGameSpec.isSeries spec hand then hand.Length + 1
        else
            hand |> Seq.filter(fun v -> v=value || v=1) |> Seq.length
    let countAnything spec (hands:TbHand[]) = 
        let i, c = seq{2..6} |> Seq.mapi(fun i v -> i,(hands |> Seq.sumBy (countValues spec v))) |> Seq.maxBy snd
        c,i+2
    let countTotalValuesContributions spec value (hands:TbHand[])  =
        if value = 0 then 
            if spec.ofAnyKind |> not then failwith "death"
            let c, v = countAnything spec hands
            let contributions = hands |>  Array.map(countValues spec v)
            c,contributions
        else
            let contributions = hands |>  Array.map(countValues spec value)
            let count = contributions |> Seq.sum
            count,contributions
    let countTotalValues spec value (hands:TbHand[])  =
        if value = 0 then 
            if spec.ofAnyKind |> not then failwith "death"
            let c, v = countAnything spec hands
            c
        else
            hands |>  Seq.sumBy (countValues spec value)
    let evaluateBet (state:TbState) (bet:TbBet)=
        let count, value = bet.count,bet.value
        let totalCount = countTotalValues state.spec value (state.playerStates |> Array.map (fun p -> p.hand))
        totalCount >= count
    let highestStanding (state:TbState) (bet:TbBet) =
        //Returns the bet with the highest count that has the same value as bet and the contributions to the count from each player
        let count, value = bet.count,bet.value
        let totalCount,contributions = countTotalValuesContributions state.spec value (state.playerStates |> Array.map (fun p -> p.hand))
        {
            count=totalCount
            value=value
        },contributions
    let advanceBet(b:TbBet) (state:TbState): TbState = 
        let nextPlayer =
            seq{1..state.spec.playerCount-1} 
            |> Seq.map(fun i -> (state.nextSide + i)%state.spec.playerCount)
            |> Seq.find(fun i -> state.playerStates.[i].diceLeft > 0)
        let playerStates = state.playerStates |> Array.map (fun ps -> ps |> TbPlayerState.setMessage (sprintf "%s raised %s" (state.playerNames.[state.nextSide]) (b|>TbBet.print)))
        {state with
            madeBetSide = state.nextSide
            currentBet = b
            nextSide = nextPlayer
            actionHistory = {
                    time=DateTime.Now
                    side = state.nextSide
                    action = {
                            call=false
                            bet=b
                        }
                }::state.actionHistory
            playerStates= playerStates
            status = TbStatus.inPlay
            roundReport = TbRoundReport.empty
            gameReport = TbGameReport.empty
        }
    let resolveBet (state:TbState)  : TbState = 
        let currentBet = state.currentBet
        let hStanding,contributions = highestStanding state currentBet
        let loser =
            if hStanding >= currentBet then //bet stands, current player loses
                state.nextSide
            else //betFails, choppingBlock loses
                state.madeBetSide
        let spec = state.spec
        let playerStates =
            state.playerStates
            |> Array.mapi(fun index p->
                let r =
                    if index <> loser then
                        if p.diceLeft = 0 then
                            let raising = state.playerNames.[state.madeBetSide]
                            let calling = state.playerNames.[state.nextSide]
                            let losing = state.playerNames.[loser]
                            p |> TbPlayerState.setMessage (sprintf "%s called %s's bet of %s. %s lost. " calling raising (currentBet |> TbBet.print) losing)
                        else
                            p |> TbPlayerState.surviveRound
                    else
                        p |> TbPlayerState.loseRound
                r
            )
        let totalDiceLeft = playerStates |> Seq.sumBy (fun ps -> ps.diceLeft)
        let playersLeft = playerStates |> Seq.mapi (fun i ps -> if ps.diceLeft > 0 then 1 else 0) |> Seq.sum
        let roundReport = TbRoundReport.lostRound state state.playerStates hStanding loser playersLeft contributions
        if playerStates |> Seq.sumBy(fun p -> p.diceLeft) <> totalDiceLeft then
            failwith "death"
        let newState =
            if playersLeft > 1 then
                { state with
                        totalDiceLeft = totalDiceLeft
                        currentBet = TbBet.startingBet
                        madeBetSide = -1
                        playersLeft = playersLeft
                        nextSide = loser
                        actionHistory = {
                                time=DateTime.Now
                                side = state.nextSide
                                action = TbAction.call
                            }::state.actionHistory
                        playerStates = playerStates |> Array.map (fun ps -> if ps.livesLeft > 0 then ps |> TbPlayerState.throw else ps)
                        status = TbStatus.inPlay
                        roundReport = roundReport
                        gameReport = TbGameReport.empty
                }
            else
                let playerStates = 
                    playerStates 
                    |> Array.mapi (fun i ps -> if i=loser then ps |> TbPlayerState.loseGame else ps)
                let playerLives = playerStates |> Array.map (fun p -> p.livesLeft)
                let livingPlayers = playerLives |> Seq.indexed |> Seq.filter (fun (i,l) -> l > 0) |> Seq.map fst |> Seq.toArray 
                if (spec.lastStanding && livingPlayers.Length > 1) || (not spec.lastStanding && livingPlayers.Length = spec.playerCount) then
                    //tournament not over, start a new game
                    let playerStates = playerStates |> Array.map (TbPlayerState.initGame spec)
                    { state with
                        totalDiceLeft = livingPlayers.Length * spec.diceCount
                        currentBet = TbBet.startingBet
                        madeBetSide = -1
                        playersLeft = livingPlayers.Length
                        nextSide = if playerLives.[loser] > 0 then loser else livingPlayers.[r.Next() % livingPlayers.Length]
                        actionHistory = {
                                time=DateTime.Now
                                side = state.nextSide
                                action = TbAction.call
                            }::state.actionHistory
                        playerStates = playerStates
                        status = TbStatus.inPlay
                        roundReport = roundReport
                        gameReport = TbGameReport.gameLost loser
                    }
                elif (spec.lastStanding && livingPlayers.Length = 1) then
                    //tournament playing for a winner and one player left
                    { state with
                        totalDiceLeft = 0
                        currentBet = TbBet.startingBet
                        madeBetSide = Side.None
                        playersLeft = livingPlayers.Length
                        nextSide = Side.None
                        actionHistory = {
                                time=DateTime.Now
                                side = state.nextSide
                                action = TbAction.call
                            }::state.actionHistory
                        playerStates = playerStates
                        status = TbStatus.tournamentWon livingPlayers.[0]
                        roundReport = roundReport
                        gameReport = TbGameReport.gameLost loser
                        tournamentReport = TbTournamentReport.tournamentWon livingPlayers.[0]
                    }
                elif (not spec.lastStanding && livingPlayers.Length < spec.playerCount) then
                    //tournament playing for a loser and a player has lost all lives
                    { state with
                        totalDiceLeft = 0
                        currentBet = TbBet.startingBet
                        madeBetSide = Side.None
                        playersLeft = livingPlayers.Length
                        nextSide = Side.None
                        actionHistory = {
                                time=DateTime.Now
                                side = state.nextSide
                                action = TbAction.call
                            }::state.actionHistory
                        playerStates = playerStates
                        status = TbStatus.tournamentLost loser
                        roundReport = roundReport
                        gameReport = TbGameReport.gameLost loser
                        tournamentReport = TbTournamentReport.tournamentLost loser
                    }
                else 
                    failwith "death"
        newState
    let legalActions (state:TbState) s =
        if state.nextSide <> s then
              Array.empty
        else
            let currentDice = state.playerStates |> Seq.sumBy(fun p -> p.diceLeft)
            let higherBets =
                TbBet.allLarger state.spec state.currentBet currentDice
            let actions =
                if state.currentBet = TbBet.startingBet then
                    Array.init(higherBets.Length)(fun i->
                        TbAction.raise (TbBet.create higherBets.[i])
                    )
                else
                    Array.init(higherBets.Length + 1)(fun i->
                        if i = 0 then
                            TbAction.call
                        else
                            TbAction.raise (TbBet.create higherBets.[i - 1])
                    )
            actions    
    let advance (state:TbState) (side:Side) (action:TbAction):AdvanceResult<TbState>= 
        if not state.status.inPlay then
            AdvanceResult.GameOver
        elif side <> state.nextSide then
            AdvanceResult.WrongTurn
        elif action.call && state.currentBet = TbBet.startingBet then
            AdvanceResult.InvalidMove "Can't call initial bet"
        elif state.nextSide <> side then
            AdvanceResult.InvalidMove "Not players turn"
        elif (not action.call) && (action.bet<=state.currentBet) then
            AdvanceResult.InvalidMove "Raise must be larger"
        else      
            if action.call then 
                AdvanceResult.Ok (resolveBet state)
            else    
                AdvanceResult.Ok (advanceBet action.bet state)