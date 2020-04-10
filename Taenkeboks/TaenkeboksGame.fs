namespace Taenkeboks
open PIM

type TaenkeboksGame = Game<TaenkeboksState,TaenkeboksAction,PublicInformation>
module TaenkeboksGame =
    let create (spec:TaenkeboksGameSpec) : TaenkeboksGame=
        let space = TaenkeboksGameSpace.create spec
        let players = 
            [
                "local","local"
                "min", "min"
            ] |> Seq.map (fun (pType,pName) -> TaenkeboksPlayer.createPlayer spec pType pName) |> Seq.toArray
        new TaenkeboksGame(space,players) 