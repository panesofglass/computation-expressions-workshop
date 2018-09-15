module Containers

open System
open Expecto

type Trace<'T> = Trace of 'T * string list

type TraceBuilder() =
    member __.Return(value) = Trace(value, [])
    (*
    member __.Run(Trace(v,log)) =
        for l in log do printfn "%s" l
        v
    *)

let trace = TraceBuilder()

[<Tests>]
let tests =
    testList "containers" (
        testParam (trace { return 1 }) [
            "new trace can return a value", fun (Trace(actual,_)) () ->
                Expect.equal actual 1 "Trace should return 1"

            "new trace returns a trace as a StringBuilder", fun (Trace(_,log)) () ->
                Expect.equal (log.GetType()) typeof<string list> "Trace should return a string list as state."

            "new trace returns a trace as a StringBuilder with an empty string", fun (Trace(_,log)) () ->
                Expect.equal log [] "Trace should return an empty string list."
        ] |> List.ofSeq
    )
