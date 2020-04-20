namespace PIM
open System
open System.Threading
open System.Threading.Tasks
open System.Threading.Tasks.Dataflow


type AsyncPlayer<'V,'A> =
    {
        Name:string
        Side:Side
        Last: unit -> 'V
        Next: unit -> Task<'V>
        NextError: unit -> Task<string>
        PerformAction: 'A -> Task<bool>
    }


type AsyncGame<'S,'A,'V>(game:Game<'S,'A,'V>,playerNames:string[],ct:CancellationToken) =
    let playerCount = playerNames.Length
    let actionChannel:BufferBlock<struct (int*'A)> = BufferBlock()
    let visibleChannels:BufferBlock<'V>[]= Array.init playerCount (fun i -> BufferBlock())
    let errorChannels:BufferBlock<string>[]= Array.init playerCount (fun i -> BufferBlock())
    let visibleCurrent : 'V[] = Array.init playerCount (fun i -> Unchecked.defaultof<'V>)
    let players = 
        playerNames
        |> Array.mapi (fun i n ->
            {
                Name=n
                Side = i
                Last = (fun () -> visibleCurrent.[i])
                Next = visibleChannels.[i].ReceiveAsync
                NextError = errorChannels.[i].ReceiveAsync
                PerformAction = (fun (a) -> actionChannel.SendAsync(struct (i,a)))
            }
        )
    let updateAllPlayers state = 
        async {
            printfn "Game update all"              
            for player in players do
                let visible = game.visible state player.Side
                visibleCurrent.[player.Side] <- visible
                let w = visibleChannels.[player.Side]
                let! sent = w.SendAsync(visible) |> Async.AwaitTask
                if not sent then failwith "Could not send"
        }
    let updatePlayer state side = 
        printfn "Game update player"
        async {
            let visible = game.visible state side
            visibleCurrent.[side] <- visible
            let w = visibleChannels.[side]
            let! sent = w.SendAsync(visible) |> Async.AwaitTask
            if not sent then failwith "Could not send"
        }        
    let errorPlayer error side = 
        async {
            printfn "Game error player"            
            let w = errorChannels.[side]
            let! sent = w.SendAsync(error) |> Async.AwaitTask
            if not sent then failwith "Could not send"
        }                
    member this.Players = players
    member this.RunGame () =
        async {
            printfn "Game started"
            let mutable state = game.init(playerNames)
            do! updateAllPlayers state
            while game.gameOver state |> not do
                let! side,action = actionChannel.ReceiveAsync() |> Async.AwaitTask
                match game.advance state side action with
                | Ok s ->
                    state <- s
                    do! updateAllPlayers state
                | GameOver -> do! errorPlayer "GameOver" side
                | WrongTurn -> do! errorPlayer "Not your turn" side
                | InvalidMove e -> do! errorPlayer (sprintf "Invalid Move: %s" e) side
        }
