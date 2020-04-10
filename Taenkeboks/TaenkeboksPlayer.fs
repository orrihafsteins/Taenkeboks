namespace Taenkeboks
open PIM
type TaenkeboksPlayer = Player<PublicInformation,TaenkeboksAction>
module TaenkeboksPlayer =
    let createPlayer spec playerType name: TaenkeboksPlayer =
        let policy = 
            match playerType with
            | "local" -> AI.bestLocalIncrementStrategy spec 100 100
            | "prior" -> AI.bestLocalWithPrior spec 100 100
            | "min" -> AI.minIncrementStrategy spec 100
            | "api" -> AI.panicStrategy
            | "aggro" -> 
                let maxOutragiousBet = 0.01
                let minGoodBet = 0.8
                let minPlausibleBet = 0.5 
                let bluff = 0.3 
                AI.aggressiveStrategy spec 100 100 maxOutragiousBet minGoodBet minPlausibleBet bluff
            | _ -> failwith "death"
        {
            playerName = name
            policy = policy
            updatePlayer = (fun v -> ())
            think = (fun v until-> ())
        }   
