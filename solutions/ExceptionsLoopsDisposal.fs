module ExceptionsLoopsDisposal

open Expecto

type MyTestBuilder(name) =
    member __.Delay(f) = f
    member __.Run(f) = testCase name f
    member __.Zero() = ()

let myTest name = MyTestBuilder(name)

[<Tests>]
let tests =
    testList "my tests" [
        (*
        testCase "A simple test" (fun () ->
            let expected = 4
            Expect.equal expected (2+2) "2+2 = 4"
        )
        *)

        myTest "A simple test" {
            let expected = 4
            Expect.equal expected (2+2) "2+2 = 4"
        }
    ]