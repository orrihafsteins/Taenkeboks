namespace Taenkeboks
open PIM


type TaenkeboksGame = Game<TaenkeboksState,TaenkeboksAction,PublicInformation>
//module TaenkeboksGame =
    
//    let create (spec:TaenkeboksGameSpec) (playerNames:string[]) : TaenkeboksGame=
//        let space = TaenkeboksGameSpace.create spec
//        let aiFunc =
//            let local = AI.bestLocalIncrementStrategy spec 100 100
//            let prior = AI.bestLocalWithPrior spec 100 100
//            let min = AI.minIncrementStrategy spec 100 
            
//            let maxOutragiousBet = 0.01
//            let minGoodBet = 0.8
//            let minPlausibleBet = 0.5 
//            let bluff = 0.3 
//            let aggro = AI.aggressiveStrategy spec 100 100 maxOutragiousBet minGoodBet minPlausibleBet bluff
//            let panic = AI.panicStrategy
//            (fun playerType v h -> 
//                let f=
                    
//                    match playerType with
//                    | "local" -> local
//                    | "prior" -> prior
//                    | "min" -> min
//                    | "api" -> panic
//                    | "aggro" -> aggro
//                f v h
//            )
            
//        new TaenkeboksGame(space,playerNames,aiFunc) 
//    //let createClassicTaenkeboks (playerCount:int): TaenkeboksInterface=
//    //    let spec = TaenkeboksGameSpec.initClassicRules playerCount
//    //    let space = FastGame.createFast spec
//    //    new GameInterface<TaenkeboksState, Action, PublicInformation, HiddenPlayerState,TaenkeboksStatus>(space,spec.playerCount) :> TaenkeboksInterface
    
