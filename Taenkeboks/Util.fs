namespace Taenkeboks
open PIM
open System

module Json =
  open Newtonsoft.Json
  let serialize obj =
    JsonConvert.SerializeObject obj
  let serializeIndented obj =
    JsonConvert.SerializeObject(obj,Formatting.Indented)
  let deserialize<'a> str =
    try
      JsonConvert.DeserializeObject<'a> str
      |> Result.Ok
    with
      // catch all exceptions and convert to Result
      | ex -> Result.Error ex  

module Seq =
    let all f s = s |> Seq.exists (f>>not) |> not
    let takeMaybe n s = s |> Seq.mapi (fun i v -> (i,v)) |> Seq.takeWhile (fun (i,v) -> i<n) |> Seq.map snd            

module List = 
    let partitionSeq (p:'A -> bool) (s:'A seq) = 
        let mutable t = []
        let mutable f = []
        s |> Seq.iter (fun a -> if p a then t <- a::t else f <-a::f)
        t,f

module Array =
    let r = new Random()
    let choice (a:'A []):'A = a.[r.Next() % a.Length]
    let sampleWithDuplicates (sampleSize:int) (a:'A []):'A[] = 
        Array.init sampleSize (fun i -> a.[r.Next()%a.Length])
    let sampleWithDuplicatesI (sampleSize:int) (populationSize) :int[] = 
        Array.init sampleSize (fun i -> r.Next()%populationSize)
    let indexByArray (ix:int[]) (a:'A []):'A [] = 
        Array.init ix.Length (fun i -> a.[ix.[i]])
    let compare (a:'A [] when 'A:comparison) (b:'A [] when 'A:comparison) = 
        let t = Seq.map2 (fun aa bb -> if aa < bb then -1 elif bb < aa then 1 else 0) a b |> Seq.tryFind ((<>)0)
        if t.IsSome then t.Value else 0
    let firstArgMax (a:'A[]) = 
        if a.Length = 0 then
            failwith "can't argmax empty"
        let mutable iMax = 0
        let mutable max = a.[0]
        for i in 1..a.Length-1 do
            if a.[i] > max then 
                iMax <- i
                max <- a.[i]
        iMax
    let filterI  (f:'A -> bool) (a:'A[]) : (int[]) = a |> Seq.mapi (fun i v -> i, f v) |> Seq.filter snd |> Seq.map fst |> Array.ofSeq
    let randargmax (a:'A[]) = //returns a random maximum value 
        let max = a |> Seq.max
        let options = a |> filterI (fun v -> v = max)
        options |> choice
    let randomOrder (a:'A[]) = //changes order
        seq{
            let mutable l = a.Length
            while l > 0 do
                let rn = r.Next() % l
                let n = a.[rn]
                l <- l - 1  
                a.[rn] <- a.[l]
                a.[l] <- n
                yield n
            yield a.[0]
        }
    let shuffle (p:'A[]) = 
        let a = p |> Array.copy
        let mutable l = a.Length
        while l > 0 do
            let rn = r.Next() % l
            let n = a.[rn]
            l <- l - 1  
            a.[rn] <- a.[l]
            a.[l] <- n
        a
    let sample (potentials:double[]) = 
        let total = potentials |> Array.sum
        match total with
        | Double.PositiveInfinity -> potentials |> Array.findIndex (fun v-> v= infinity)
        | 0.0 -> r.Next() % potentials.Length
        | _ ->
            let v = r.NextDouble() * total
            let mutable sum = potentials.[0]
            let mutable i = 0
            while v > sum do
                i <- i + 1
                sum <- sum + potentials.[i]
            i
    let rank (potentials:double[]) =
        //from 0 highest potential gets higest rank
        potentials
        |> Array.indexed
        |> Array.sortBy snd
        |> Array.mapi (fun vote (index,score) -> vote)
    let argTopN n (potentials:double[]) =
        let sorted = 
            potentials
            |> Array.indexed
            |> shuffle
            |> Array.sortByDescending snd
        sorted
        |> Array.map fst
        |> Array.take n
    let denseRank (potentials:double[]) =
        //from 0 highest potential gets higest rank
        let rank = Array.zeroCreate potentials.Length;
        let mutable r = -1
        let mutable max = Double.NegativeInfinity
        for i,pot in potentials |> Array.indexed |> Array.sortBy snd do            
            if pot > max then
                r <- r + 1
                max <- pot
            rank.[i] <- r
        rank
    let containsSome (values:'A[]) (a:'A[])  = a |> Array.exists (fun v -> values |> Array.contains v)