module Return

open Expecto

type TraceBuilder() =
    member __.Return(value) = value

let trace = TraceBuilder()

[<Tests>]
let tests =
    testList "returns" [
        test "TraceBuilder returns value" {
            let expected = 1
            let actual = trace { return expected }
            Expect.equal actual expected "Actual should match expected"
        }
    ]
