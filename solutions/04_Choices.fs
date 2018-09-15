module Choose

open Expecto

type ChoiceBuilder() =
    member __.ReturnFrom(m:_ option) = m
    
    (*
    // First attempt
    member __.Combine(m1, m2) =
        match m1 with
        | Some _ -> m1
        | None -> m2
    member __.Delay(f:unit -> _ option) = f()
    // NOTE: results in printing statements after return! due to delay not blocking further evaluation.
    *)

    // Second attempt
    member __.Combine(m1, m2) =
        match m1 with
        | Some _ -> m1
        | None -> m2()
    member __.Delay(f:unit -> _ option) = f
    member __.Run(f:unit -> _ option) = f()

let choose = ChoiceBuilder()

[<Tests>]
let tests =
    testList "choices" [
        test "choose returns first value if it is Some" {
            let actual = choose {
                return! Some 1
                printfn "returning first value?"
                return! Some 2
            }
            Expect.equal actual (Some 1) "Expected the first value to be returned."
        }

        test "choose returns second value if first is None" {
            let actual = choose {
                return! None
                printfn "returning second value?"
                return! Some 2
            }
            Expect.equal actual (Some 2) "Expected the second value to be returned."
        }

        test "choose returns the last value if all previous are None" {
            let actual = choose {
                return! None
                return! None
                return! None
                return! None
                return! None
                return! None
                return! Some 7
            }
            Expect.equal actual (Some 7) "Expected the seventh value to be returned."
        }
    ]