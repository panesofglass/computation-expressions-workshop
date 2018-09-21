module Sequences

open Expecto

type Stack<'a> =
    | Empty
    | Cons of head:'a * tail:Stack<'a>

module Stack =
    let push v s = Cons(v, s)
    let pop s =
        match s with
        | Cons(v, c) -> v, c
        | _ -> failwith "Nothing to pop!"
    let lift v = push v Empty
    let toList s =
        let rec loop s cont =
            match s with
            | Cons(head, tail) ->
                loop tail (fun rest -> head::rest)
            | Empty -> cont []
        loop s id

type StackBuilder() =
    member __.Yield(value) = Stack.lift value

let stack = StackBuilder()

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
