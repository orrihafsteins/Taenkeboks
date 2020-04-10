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
type Policy<'V,'A> = 'V -> 'A
type ValidateActionResult = 
    | OK
    | GameOver
    | InvalidAction of String
    member this.message : String =
        match this with
        | OK -> "OK"
        | GameOver -> "GameOver"
        | InvalidAction(message) -> sprintf "Invalid Action: %s" message
type Player<'V,'A> =
    {
        playerName:String
        policy:Policy<'V,'A>
        updatePlayer: 'V -> unit
        think:'V -> CancellationToken-> unit
    }
type GameSpace<'S,'A,'V> = 
    {
        init: unit -> 'S
        advance:'S-> Side -> 'A -> 'S
        legalActions: 'S -> Side -> 'A[]
        validateAction:'S -> Side -> 'A -> ValidateActionResult
        visible: 'S-> Side -> 'V
        checkTime: 'S -> 'S*bool // returns true if state update
        gameOver: 'S -> bool
    }
    
type Game<'S,'A,'V>(space:GameSpace<'S,'A,'V>,players:Player<'V,'A>[]) =
    let mutable gameState = space.init()
    
    member private this.UpdatePlayers () =
        Array.iteri (fun i p -> 
            let visible= space.visible gameState i
            p.updatePlayer visible
        ) players
    member this.LegalActions side = space.legalActions gameState side
    member this.TryAction (side:Side) (action:'A) : String =
        if gameState|> space.gameOver then
            ValidateActionResult.GameOver.message;
        else     
            match space.validateAction gameState side action with
            | OK -> 
                gameState <- space.advance gameState side action
                null
            | r -> r.message
    member this.GameOver = gameState |> space.gameOver
    member this.Visible side = space.visible gameState side
    member this.CheckTime() = 
        let newGame,doUpdate = space.checkTime gameState
        gameState<-newGame
        if doUpdate then
            this.UpdatePlayers()
        doUpdate
    member this.PlayerNames = players |> Array.map (fun ps-> ps.playerName)
    member this.State = gameState
    