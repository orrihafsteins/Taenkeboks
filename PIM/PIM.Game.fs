namespace PIM
open System
open System.Threading

//'S: Board state type (full game information)
//'A: Action type
//'V: Visible information

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

type IPlayer<'V,'A> =
    abstract member playerName:String
    abstract member policy: 'V -> 'A
    abstract member update: 'V -> unit

type PlayerName = string

type IGame<'S,'A,'V> = 
    abstract member init: PlayerName[] -> 'S
    abstract member advance:'S-> Side -> 'A -> 'S // also handles invalid actions
    abstract member visible: 'S-> Side -> 'V
    abstract member checkTime: 'S -> 'S
    abstract member gameOver: 'S -> bool
    abstract member nextPlayer: 'S -> Side

module IGame =
    let play (game:IGame<'S,'A,'V>) (players:IPlayer<'V,'A>[]) =
        let updatePlayers state =
            Array.iteri (fun i (p:IPlayer<'V,'A>) -> 
                let visible= game.visible state i
                p.update visible
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
        