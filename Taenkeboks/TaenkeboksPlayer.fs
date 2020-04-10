namespace Taenkeboks
open PIM
module Simulator = 
    let r = new System.Random()
    let sampleBet spec (hand:Hand) (otherDice:int[]) (n:int) (bet:Bet) =
        let count,value = bet.count,bet.value
        let mutable wins = 0
        if value = 0 then
            if spec.ofAnyKind |> not then failwith "death"
            for i in 1..n do
                let hands = Array.init (otherDice.Length+1) (fun i -> if i = otherDice.Length then hand else Hand.throwN (otherDice.[i]))
                let sampleCount = TaenkeboksGameSpace.countTotalValues spec 0 hands
                if sampleCount >= count then wins <- wins + 1
        else
            let countValues = TaenkeboksGameSpace.countValues spec value
            let myCount = countValues hand
            for i in 1..n do
                let otherCount = 
                    otherDice 
                    |> Seq.map (fun d -> Hand.throwN d |> countValues)
                    |> Seq.sum
                let sampleCount = otherCount + myCount
                if sampleCount >= count then wins <- wins + 1
        (float wins) / (float n)
    let sampleBetWithBias spec (hand:Hand) (otherDice:int[]) (bias:int[]) (n:int) (bet:Bet) =
        let count,value = bet.count,bet.value
        let mutable wins = 0
        if value = 0 then
            if spec.ofAnyKind |> not then failwith "death"
            for i in 1..n do
                let hands = Array.init (otherDice.Length+1) (fun i -> if i = otherDice.Length then hand else Hand.throwN (otherDice.[i]))
                let sampleCount = TaenkeboksGameSpace.countTotalValues spec 0 hands
                if sampleCount >= count then wins <- wins + 1
        else
            let countValues = TaenkeboksGameSpace.countValues spec value
            let myCount = countValues hand
            for i in 1..n do
                let otherCount = 
                    Seq.map2 (fun d b-> 
                        let hand = Hand.throwN d
                        if b > 0 then hand.[r.Next() % d]<-b
                        hand |> countValues
                    ) otherDice bias
                    |> Seq.sum
                let sampleCount = otherCount + myCount
                if sampleCount >= count then wins <- wins + 1
        (float wins) / (float n)
    let sampleCurrentBet (g:PublicInformation) n =     
        let otherDice = g.diceLeft |> Array.mapi (fun i x -> (i<>g.nextPlayer,x)) |> Array.filter fst |> Array.map snd
        sampleBet g.spec g.playerHand otherDice n g.currentBet
module AI = 
    
    // if prob < 1 / players then call else minimum raise
    let minIncrementStrategy spec simLast :Policy<PublicInformation,TaenkeboksAction> = 
        let bets = Bet.all spec
        (fun info -> 
            let p = Simulator.sampleCurrentBet info simLast
            let target = 1.0 / (float info.playersLeft)
            if p < target then
                TaenkeboksAction.call info.currentBet
            else
                let bi = bets |> Array.findIndex (fun b -> b = info.currentBet)
                TaenkeboksAction.raise bets.[bi + 1]
        )
    let panicStrategy:Policy<PublicInformation,TaenkeboksAction> = 
        (fun info -> 
            failwith "panic!"
        )
    let minIncrementStrategy2 spec simLast :Policy<PublicInformation,TaenkeboksAction> = 
        let bets = Bet.all spec
        (fun info -> 
            let p = Simulator.sampleCurrentBet info simLast
            let target = 0.25
            if p < target then
                TaenkeboksAction.call info.currentBet
            else
                let bi = bets |> Array.findIndex (fun b -> b = info.currentBet)
                TaenkeboksAction.raise bets.[bi + 1]
        )
    let bestLocalIncrementStrategy spec simLast simNext:Policy<PublicInformation,TaenkeboksAction> = 
        let bets = Bet.all spec
        (fun info -> 
            let p = Simulator.sampleCurrentBet info simLast
            let target = 1.0 / (float info.playersLeft)
            if p < target then
                TaenkeboksAction.call info.currentBet
            else
                let bi = bets |> Array.findIndex (fun b -> b = info.currentBet)
                let possible = bets.[bi+1..bi+1+6]
                let otherDice = info.diceLeft |> Array.mapi (fun i x -> (i<>info.nextPlayer,x)) |> Array.filter fst |> Array.map snd
                let probs = possible |> Array.map (Simulator.sampleBet spec info.playerHand otherDice simNext)
                let bestI = probs |> Array.firstArgMax
                TaenkeboksAction.raise possible.[bestI]
        )
    let bestLocalWithPrior spec simLast simNext:Policy<PublicInformation,TaenkeboksAction> = 
        let bets = Bet.all spec
        (fun info -> 
            let predictedHands = Array.zeroCreate info.playerCount 
            let moveHistory = info.actionHistory |> Seq.map (fun a->a.action)
            let hotseatHistory = info.actionHistory |> Seq.map (fun a->a.side)
            let thisRoundsBets = 
                Seq.zip moveHistory (hotseatHistory |> Seq.skip 1)
                |> Seq.takeMaybe (info.playersLeft - 1)
                |> Seq.takeWhile (fun (b,p) -> not b.call)
                |> Seq.iter (fun (b,p) -> 
                    let count,value = b.bet.count,b.bet.value
                    predictedHands.[p] <- value
                )
            let p = Simulator.sampleCurrentBet info simLast
            let target = 1.0 / (float info.playersLeft)
            if p < target then
                TaenkeboksAction.call info.currentBet
            else
                let bi = bets |> Array.findIndex (fun b -> b = info.currentBet)
                let possible = bets.[bi+1..bi+1+6]
                let otherDice = info.diceLeft |> Array.mapi (fun i x -> (i<>info.nextPlayer,x)) |> Array.filter fst |> Array.map snd
                let otherBias = predictedHands |> Array.mapi (fun i x -> (i<>info.nextPlayer,x)) |> Array.filter fst |> Array.map snd
                let probs = possible |> Array.map (Simulator.sampleBetWithBias spec info.playerHand otherDice otherBias simNext)
                let bestI = probs |> Array.firstArgMax
                TaenkeboksAction.raise possible.[bestI]
        )

    let aggressiveStrategy spec simLast simNext maxOutragiousBetProb minSafeBetProb minPlausibleBetProb bluffProb:Policy<PublicInformation,TaenkeboksAction> = 
        let bets = Bet.all spec
        let r = new System.Random();
        (fun info -> 
            let p = Simulator.sampleCurrentBet info simLast
//            let maxOutragiousBet = 0.1
            if p < maxOutragiousBetProb then
                TaenkeboksAction.call info.currentBet
            else
                //let raiseTarget = 1.5 / (float info.playersLeft)
//                let minGoodBet = 0.8
//                let minPlausibleBet = 0.5         

                let bi =    
                    if info.currentBet = Bet.startingBet then 5
                    else bets |> Array.findIndex (fun b -> b = info.currentBet)
                let bestValue = 
                    let notOnes = 
                        info.playerHand
                        |> Array.filter (fun v -> v<>1)
                    if notOnes.Length = 0 then
                        6
                    else//find the highest value with the highest count
                        notOnes
                        |> Seq.groupBy (id)
                        |> Seq.map (fun (v,vs) -> (vs|>Seq.length),v) 
                        |> Seq.sortDescending
                        |> Seq.head
                        |> snd
                let bluffValue = 
                    if spec.ofAnyKind then
                        0
                    else    
                        info.currentBet.value
                
                //let maxBet = min (bets.Length - 1) (bi+1+48)
                //let possible = bets.[bi+1..maxBet]
                let otherDice = info.diceLeft |> Array.mapi (fun i x -> (i<>info.nextPlayer,x)) |> Array.filter fst |> Array.map snd
                
                let maxSampled = 6;
                let bestBets = bets.[bi+1..] |> Array.filter (fun b -> b.value = bestValue)
                let bluffBets = bets.[bi+1..] |> Array.filter (fun b -> b.value = bluffValue && b.value <> bestValue) 
                let localBets = bets.[bi+1..] |> Array.filter (fun b -> b.count = info.currentBet.count && b.value <> bluffValue && b.value <> bestValue) 
                let sampleSize = simNext / maxSampled
                
                let safeBluff: Bet option =
                    let bluffSample = bluffBets |> Array.take (min maxSampled bluffBets.Length)
                    let bluffProbs = bluffSample |> Array.map (Simulator.sampleBet spec info.playerHand otherDice sampleSize)
                    let iSafeBluffs = bluffProbs |> Array.filterI (fun p -> p > minSafeBetProb)
                    if iSafeBluffs.Length > 0 then
                        let safeBluffProbs = bluffProbs |> Array.indexByArray iSafeBluffs
                        let iMostAggressiveSafeBluff = safeBluffProbs |> Array.map (fun p -> -p) |> Array.firstArgMax
                        Some bluffSample.[iMostAggressiveSafeBluff]
                    else
                        None

                let possible = 
                    bestBets 
                    |> Array.take (min maxSampled bestBets.Length)
                    |> Array.append localBets
                
                let probs = possible |> Array.map (Simulator.sampleBet spec info.playerHand otherDice sampleSize)
                let bestProbI = probs |> Array.firstArgMax
                let bestProb = probs.[bestProbI]
                let probsI = probs |> Array.mapi (fun i p -> i,p) 

                let bestI = 
                    if bestProb > minSafeBetProb then
                        probsI
                        |> Seq.filter (fun (i,p) -> p > minSafeBetProb)
                        |> Seq.minBy snd
                        |> fst
                    elif info.currentBet = Bet.startingBet || bestProb > minPlausibleBetProb then
                        probsI
                        |> Seq.maxBy snd
                        |> fst
                    else -1
                if bestI >= 0 then
                    TaenkeboksAction.raise possible.[bestI]
                else
                    TaenkeboksAction.call info.currentBet
        )

    let createAggressivePlayer spec simLast simNext maxOutragiousBet minGoodBet minPlausibleBet bluff name:Player<PublicInformation,TaenkeboksAction> =
        {
            playerName = name
            policy=aggressiveStrategy spec simLast simNext maxOutragiousBet minGoodBet minPlausibleBet bluff
            updatePlayer = (fun visible -> ())
            think = (fun player visible until -> ())
        }
    
    let createMinIncrementPlayer spec simLast name:Player<PublicInformation,TaenkeboksAction> =
        {   
            playerName = name
            policy=minIncrementStrategy spec simLast
            updatePlayer = (fun visible -> ())
            think = (fun player visible until -> ())
        }
    

    let createLocIncrPlayer spec simLast simNext name:Player<PublicInformation,TaenkeboksAction> =
        {
            playerName = name
            policy=bestLocalIncrementStrategy spec simLast simNext
            updatePlayer = (fun visible -> ())
            think = (fun player visible until -> ())
        }
    let betProbs (visible:PublicInformation) maxCount =
        let n = 100
        let othersDice = PublicInformation.othersDice visible
        let hand = visible.playerHand
        let valueFreqs = Array.zeroCreate 7
        if TaenkeboksGameSpec.isSeries visible.spec hand then
            for i in 1..6 do valueFreqs.[i] <- hand.Length + 1
            else 
                hand 
                |> Seq.iter (fun v -> 
                    if v = 1 then 
                        for i in 1..6 do valueFreqs.[i] <- valueFreqs.[i] + 1
                    else 
                        valueFreqs.[v] <- valueFreqs.[v] + 1
                )
        let diceFreqSample = valueFreqs |> Array.mapi (fun  v c ->(v,c)) |> Array.filter (fun (v,c) -> v > 1) |> Array.distinctBy ( fun (v,c) -> c)
        let diceFreqI:int[] = Array.zeroCreate 7
        diceFreqSample |> Array.iteri ( fun i (v,c) -> diceFreqI.[c] <- i)
        let counts = [|0..maxCount|]
        let freqProbs = Array2D.init diceFreqSample.Length counts.Length (fun freq count ->
            let (v,f) = diceFreqSample.[freq]
            if count > 0 then
                Simulator.sampleBet visible.spec hand othersDice n ({count=count;value=v})         
            else 
                0.0
        )
        let bets = Bet.all visible.spec
        let t = 
            bets
            |> Array.map (fun b -> 
                let c,v = b.count,b.value
                let p = 
                    if c > maxCount then 0.0
                    else freqProbs.[diceFreqI.[valueFreqs.[v]],c]
                b,p
            )
        t

        
    let createCachedPlayer gameSpec name simLast simNext:Player<PublicInformation,TaenkeboksAction> = 
        let mutable t = Array.empty //betProbs (visible:PublicInformation) maxCount          
        
        {
            playerName=name
            policy = (fun info -> 
                //let bets = t |> Array.map fst
                let bi = t |> Array.findIndex (fun (b,p) -> b = info.currentBet)
                let b,p = t.[bi]
                //let p = Simulator.sampleCurrentBet info simLast
                let target = 1.0 / (float info.playersLeft)
                if bi > 0 && p < target then
                    TaenkeboksAction.call info.currentBet
                else
                    //let bi = bets |> Array.findIndex (fun b -> b = info.currentBet)
                    let posB,posP = t.[bi+1..bi+1+6] |> Array.rev |> Array.unzip
                    let bestI = posP |> Array.firstArgMax
                    TaenkeboksAction.raise posB.[bestI]
            )
            updatePlayer = (fun info -> 
                if info.choppingBlock = -1 then  
                    let maxCount = min (max info.totalDiceLeft 6) 12
                    t <- betProbs info maxCount
            )
            think = (fun player visible until -> ())
        }


