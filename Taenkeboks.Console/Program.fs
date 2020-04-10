namespace Taenkeboks.Console
open System
open Taenkeboks

module Program =
    [<EntryPoint>]
    let main argv =
        printfn "Hello World from F#!"
        let t = Console.In.ReadLine()
        0 // return an integer exit code
