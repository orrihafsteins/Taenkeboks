namespace Taenkeboks.Console
open System
open System.Threading
open System.Threading.Tasks
open Taenkeboks
open PIM

type PlayerType =
    | Cpu
    | Console    
type PlayerSpec = {
    name:string
    playerType:PlayerType
}
module Program =
    let playerTask asyncPlayer spec pSpec = 
        match pSpec.playerType with
        | Cpu -> 
            TbAiPlayerAsync.play asyncPlayer (TbAiPlayer.createPolicy spec pSpec.name)
        | Console -> 
            TbConsolePlayer.asyncPlay asyncPlayer
 

 
    [<EntryPoint>]
    let main argv =
        printfn "--------------------------- TAENKEBOKS -------------------------------"

        let playerCount = 3
        let spec = TbGameSpec.initClassicRules playerCount
        let spec = {spec with playerCount = playerCount;extraLives = 1;lastStanding=true;diceCount=4}
        let game = TbGame.create spec
        let playerSpecs = 
            [|
                {playerType = Cpu; name = "Bob"}
                {playerType = Cpu; name = "Alice"}
                {playerType = Console; name = "Orri"}
            |]
        if playerSpecs.Length <> playerCount then failwith "death"        
        let playerNames = playerSpecs |> Array.map (fun ps -> ps.name)
        let cts = new CancellationTokenSource();
        let asyncGame : AsyncGame<TbState,TbAction,TbVisible>= AsyncGame<TbState,TbAction,TbVisible>(game,playerNames,cts.Token)
        
        let playerTask = 
            playerSpecs 
            |> Array.mapi (fun i ps -> playerTask (asyncGame.Players.[i]) spec ps)
            |> Array.iter Async.Start
        let gameTask = asyncGame.RunGame() |> Async.RunSynchronously
        //let game = PIM.Game.play game players
    
        printfn "------------------------- TAENKEBOKS END -----------------------------"
        let t = Console.In.Read()
        0 // return an integer exit code[<EntryPoint>]
    //let mainAsync argv =
    //    printfn "--------------------------- TAENKEBOKS -------------------------------"

    //    let playerCount = 3
    //    let spec = TbGameSpec.initClassicRules playerCount
    //    let spec = {spec with playerCount = playerCount;extraLives = 1;lastStanding=true;diceCount=4}
    //    let game = TbGame.create spec
    //    let asyncGame = PIM.AsyncGame(game,playerNames:string[],ct:CancellationToken)
    //    let players = 
    //        [|
    //            TbAiPlayer.createPlayer spec "Bob"
    //            TbAiPlayer.createPlayer spec "Alice"
    //            //TaenkeboksPlayer.createPlayer spec "min" "min"
    //            TbConsolePlayer.create "Orri"
    //        |]
    //    if players.Length <> playerCount then failwith "death"        
    //    let game = PIM.Game.play game players
    
    //    printfn "------------------------- TAENKEBOKS END -----------------------------"
    //    let t = Console.In.Read()
    //    0 // return an integer exit code

