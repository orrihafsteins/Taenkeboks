namespace Taenkeboks
open PIM
open System

module TbVisibleModule =
    let create (s:TbState) player = 
        {
            playerNames = s.playerNames 
            spec = s.spec
            nextSide = s.nextSide
            diceLeft = s.playerStates |> Array.map(fun p -> p.diceLeft)
            playerCount = s.playerStates.Length
            totalDiceLeft = s.totalDiceLeft
            madeBetSide = s.madeBetSide
            currentBet = s.currentBet
            playersLeft = s.playersLeft
            actionHistory = s.actionHistory
            livesLeft = s.playerStates |> Array.map(fun p-> p.livesLeft)
            viewingSide = player
            status = s.status
            legalActions = TbRules.legalActions s s.nextSide
            playerMessage = s.playerStates.[player].message
            playerHand = s.playerStates.[player].hand
            roundReport = s.roundReport
            gameReport = s.gameReport
            tournamentReport = s.tournamentReport
        }
    let othersDice(v:TbVisible) = 
        v.diceLeft |> Array.mapi(fun i x -> (i<> v.viewingSide,x)) |> Array.filter fst |> Array.map snd


type TbGame = Game<TbState,TbAction,TbVisible>
module TbGame =
    open TbRules
    let create (spec:TbGameSpec):TbGame= 
        if not spec.lastStanding && spec.extraLives > 0 then failwith "Extra lives only when last standing"
        {
            init = (fun playerNames -> TbTaenkeboksState.create spec playerNames)   //(fun ps -> TaenkeboksState.init spec ps.Length)
            advance = advance
            visible = TbVisibleModule.create
            gameOver= (fun state -> not state.status.inPlay)
            nextSide = (fun state -> state.nextSide)
            checkTime = (fun g -> NoChange)
        }

