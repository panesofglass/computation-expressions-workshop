module States

open System.Text
open Expecto

type State<'a, 's> = 's -> 'a * 's

module State =
    // Explicit
    //let result x : State<'a, 's> = fun s -> x, s
    // Less explicit but works better with other, existing functions:
    let result x s = x, s

    let bind (f:'a -> State<'b, 's>) (m:State<'a, 's>) : State<'b, 's> =
        // return a function that takes the state
        fun s ->
            // Get the value and next state from the m parameter
            let a, s' = m s
            // Get the next state computation by passing a to the f parameter
            let m' = f a
            // Apply the next state to the next computation
            m' s'

type StateBuilder() =
    member __.Return(value) : State<'a, 's> = State.result value
    member __.Bind(m:State<'a, 's>, f:'a -> State<'b, 's>) : State<'b, 's> = State.bind f m
    member __.ReturnFrom(m:State<'a, 's>) = m

[<Tests>]
let tests =
    testList "states" [
        test "StringBuilder as state" {
            let printA (sb:StringBuilder) = sb.Append("A")
            let printB (sb:StringBuilder) = sb.Append("B")
            let printC (sb:StringBuilder) = sb.Append("C")
            let run (sb:StringBuilder) = sb.ToString()
            let sb = StringBuilder()
            let actual = sb |> printA |> printB |> printC |> run
            Expect.equal actual "ABC" "Expected ABC."
        }
    ]