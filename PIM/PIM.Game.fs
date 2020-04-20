namespace PIM
open System
open System.Threading

//'S: Board state type (full game information)
//'A: Action type
//'V: Visible information
//'R: Action report
//'Status: Game status, (in play, game over, x won, y lost, etc.)


type Side = int
module Side = 
    let None = -1
    let X = 0
    let O = 1
    let other (s:int) = 1-s
type GameStatus =
    | InPlay
    | Winner of Side
    | Loser of Side 
type Player<'V,'A> =
    {
        playerName:String
        policy:'V -> 'A
        updatePlayer: 'V -> unit
    }

type PlayerName = string

type AdvanceResult<'S> = 
    | Ok of 'S //advance ok
    | GameOver //game already over
    | WrongTurn //not acting players turn
    | InvalidMove of string 
    member this.TryOk(state:'S byref) =
        match this with
        | Ok s -> 
            state <- s
            true
        | _ -> false
    member this.Message:string =
        match this with
        | Ok s -> "Ok"
        | GameOver -> "Game Over"
        | WrongTurn -> "Not players turn"
        | InvalidMove  e -> (sprintf "Invalid move: %s" e)

type CheckTimeResult<'S> = 
    | NoChange
    | StateChange of 'S

type Game<'S,'A,'V> = 
    {
        init: PlayerName[] -> 'S
        advance:'S-> Side -> 'A -> AdvanceResult<'S> // also handles invalid actions
        visible: 'S-> Side -> 'V
        checkTime: 'S -> CheckTimeResult<'S>
        gameOver: 'S -> bool
        nextSide: 'S -> Side
    }
 module Game =
     let play (game:Game<'S,'A,'V>) (players:Player<'V,'A>[]) =
         let updatePlayers state =
             Array.iteri (fun i p -> 
                 let visible= game.visible state i
                 p.updatePlayer visible
             ) players
         let rec resolve state:'S =
             updatePlayers state
             if game.gameOver state then state
             else
                 let nextSide = game.nextSide state
                 let nextPlayer = players.[nextSide] 
                 let nextPlayerView = game.visible state nextSide
                 let nextPlayerMove = nextPlayer.policy nextPlayerView 
                 let nextState =  
                     match game.advance state nextSide nextPlayerMove with
                     | Ok s -> s //advance ok
                     | GameOver -> state
                     | WrongTurn -> state
                     | InvalidMove s -> state
                 resolve nextState
         let initial = game.init (players |> Array.map (fun p -> p.playerName))
         resolve initial
        