module Taenkeboks.TestGame
open PIM
open NUnit.Framework

[<SetUp>]
let Setup () =
    ()


    
[<TestCase>]
let TaenkeboksGame =
    let spec = TaenkeboksGameSpec.initClassicRules(2)
    let game = TaenkeboksGame.create(spec)
    printfn "%A" game.LegalActions
    Assert.Fail(sprintf "%A" (game.Visible Side.X))
    
