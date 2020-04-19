namespace PIM
open System
open System.Threading
open System.Threading.Channels
open System.Threading.Tasks


type AsyncPlayer<'V,'A> =
    {
        Name:string
        Side:Side
        Last: unit -> 'V
        Next: unit -> ValueTask<'V>
        NextError: unit -> ValueTask<string>
        PerformAction: 'A -> ValueTask
    }
type AsyncGame<'S,'A,'V>(game:Game<'S,'A,'V>,playerNames:string[],ct:CancellationToken) =
    let playerCount = playerNames.Length
    let actionChannel:Channel<struct (int*'A)> = Channel.CreateUnbounded()
    let visibleChannels:Channel<'V>[]= Array.init playerCount (fun i -> Channel.CreateUnbounded())
    let errorChannels:Channel<string>[]= Array.init playerCount (fun i -> Channel.CreateUnbounded())
    let visibleCurrent : 'V[] = Array.init playerCount (fun i -> Unchecked.defaultof<'V>)
    let players = 
        playerNames
        |> Array.mapi (fun i n ->
            {
                Name=n
                Side = i
                Last = (fun () -> visibleCurrent.[i])
                Next = visibleChannels.[i].Reader.ReadAsync
                NextError = errorChannels.[i].Reader.ReadAsync
                PerformAction = (fun (a) -> actionChannel.Writer.WriteAsync(struct (i,a)))
            }
        )
    let updateAllPlayers state = 
        async {
            printfn "Game update all"              
            for player in players do
                let visible = game.visible state player.Side
                visibleCurrent.[player.Side] <- visible
                let w = visibleChannels.[player.Side].Writer
                do! w.WriteAsync(visible,ct).AsTask() |> Async.AwaitWaitHandle
        }
    let updatePlayer state side = 
        printfn "Game update player"
        async {
            let visible = game.visible state side
            visibleCurrent.[side] <- visible
            do! visibleChannels.[side].Writer.WriteAsync(visible,ct).AsTask() |> Async.AwaitTask 
        }        
    let errorPlayer error side = 
        async {
            printfn "Game error player"            
            do! errorChannels.[side].Writer.WriteAsync(error,ct).AsTask() |> Async.AwaitTask 
        }                
    member this.Players = players
    member this.RunGame () =
        async {
            printfn "Game started"
            let mutable state = game.init(playerNames)
            do! updateAllPlayers state
            while game.gameOver state |> not do
                let! side,action = actionChannel.Reader.ReadAsync(ct).AsTask() |> Async.AwaitTask
                match game.advance state side action with
                | Ok s ->
                    state <- s
                    do! updateAllPlayers state
                | GameOver -> do! errorPlayer "GameOver" side
                | WrongTurn -> do! errorPlayer "Not your turn" side
                | InvalidMove e -> do! errorPlayer (sprintf "Invalid Move: %s" e) side
        }
