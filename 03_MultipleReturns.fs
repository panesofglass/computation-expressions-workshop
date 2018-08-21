module MultipleReturns

open System.Text
open Expecto

type Trace<'T> = Trace of 'T * string list

type TraceBuilder() =
    member __.Yield(value) = Trace(value, [])
    (*
    member __.Bind(Trace(v, sb), f) =
        let (Trace(newVal, sb')) = f v
        Trace(newVal, sb @ [string v] @ sb')
    *)
    member __.Combine(Trace(v, sb), f: unit -> Trace<_>) =
        // TODO: how might we make this tail recursive?
        let (Trace(newVal, sb')) = f()
        Trace(newVal, sb @ [string v] @ sb')
    member __.Delay(f) = f
    member __.Run(f) = f()

let trace = TraceBuilder()

[<Tests>]
let tests =
    testList "multiple returns" [
        yield! testParam (trace { yield 1 }) [
            "new trace can return a value", fun (Trace(actual,_)) () ->
                Expect.equal actual 1 "Trace should return 1"

            "new trace returns a trace as a StringBuilder", fun (Trace(_,log)) () ->
                Expect.equal (log.GetType()) typeof<string list> "Trace should return a string builder as state."

            "new trace returns a trace as a StringBuilder with an empty string", fun (Trace(_,log)) () ->
                Expect.equal log [] "Trace should return an empty string builder."
        ] |> List.ofSeq

        yield test "TraceBuilder can return multiple times" {
            let (Trace(actual, log)) = trace {
                yield 1
                yield 2
                yield 3
            }
            Expect.equal actual 3 "Actual should match expected + expected"
            Expect.equal log ["1";"2"] "log should include previous list of values"
        }
    ]
