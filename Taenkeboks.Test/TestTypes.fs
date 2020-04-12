module Taenkeboks.TestTypes

open NUnit.Framework
open PIM

[<SetUp>]
let Setup () =
    ()
    
let BET_2D6 = {count=2;value=6}
let BET_3D5 = {count=3;value=5}
let BET_3D6 = {count=3;value=6}
let BET_4D3 = {count=4;value=3}

let Bet_Comparison_LeftSmaller_Cases =
    [
        BET_2D6, BET_3D5
        BET_3D5, BET_3D6
        BET_3D6, BET_4D3
    ]  |> List.map (fun (s,l) -> TestCaseData(s,l))
[<TestCaseSource("Bet_Comparison_LeftSmaller_Cases")>]
let Bet_Comparison_LeftSmaller (smaller:TbBet) (larger:TbBet) =
    Assert.IsTrue(smaller<larger)
    


    
    