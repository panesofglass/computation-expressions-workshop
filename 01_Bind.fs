module Bind

open Expecto

type TraceBuilder() =
    member __.Return(value) = value
    member __.Bind(m, f) =
        printfn "%A %A" m f
        f m

let trace = TraceBuilder()

[<Tests>]
let tests =
    testList "binds" [
        test "TraceBuilder returns value" {
            let expected = 1
            let actual = trace { return expected }
            Expect.equal actual expected "Actual should match expected"
        }

        test "TraceBuilder performs a side effect" {
            let expected = 1
            let actual = trace {
                let! result = expected + expected
                return result
            }
            Expect.equal actual (expected + expected) "Actual should match expected + expected"
        }
    ]
