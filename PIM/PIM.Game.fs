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
type Game<'S,'A,'V> = 
    {
        init: PlayerName[] -> 'S
        advance:'S-> Side -> 'A -> 'S // also handles invalid actions
        visible: 'S-> Side -> 'V
        checkTime: 'S -> 'S
        gameOver: 'S -> bool
        nextPlayer: 'S -> Side
    }
    static member StateType = typeof<'S>
    static member VisibleType = typeof<'V>
    static member ActionType = typeof<'A>
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
                let nextSide = game.nextPlayer state
                let nextPlayer = players.[nextSide] 
                let nextPlayerView = game.visible state nextSide
                let nextPlayerMove = nextPlayer.policy nextPlayerView 
                let nextState =  game.advance state nextSide nextPlayerMove 
                resolve nextState
        let initial = game.init (players |> Array.map (fun p -> p.playerName))
        resolve initial
        