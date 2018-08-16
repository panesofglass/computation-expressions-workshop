module Containers

open System.Text
open Expecto

type Trace<'T> = Trace of 'T * StringBuilder

type TraceBuilder() =
    member __.Return(value) = Trace(value, StringBuilder())
    (*
    member __.Run(Trace(v,sb)) =
        printfn "%s" (sb.ToString())
        v
    *)

let trace = TraceBuilder()

[<Tests>]
let tests =
    testList "containers" (
        testParam (trace { return 1 }) [
            "new trace can return a value", fun (Trace(actual,_)) () ->
                Expect.equal actual 1 "Trace should return 1"

            "new trace returns a trace as a StringBuilder", fun (Trace(_,sb)) () ->
                Expect.equal (sb.GetType()) typeof<StringBuilder> "Trace should return a string builder as state."

            "new trace returns a trace as a StringBuilder with an empty string", fun (Trace(_,sb)) () ->
                Expect.equal (sb.ToString()) "" "Trace should return an empty string builder."
        ] |> List.ofSeq
    )
