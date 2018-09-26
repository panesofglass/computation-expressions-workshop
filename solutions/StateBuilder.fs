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

    /// Evaluates the computation, returning the result value.
    let eval (m:State<'a, 's>) (s:'s) = m s |> fst

    /// Executes the computation, returning the final state.
    let exec (m:State<'a, 's>) (s:'s) = m s |> snd

    /// Returns the state as the value.
    let getState (s:'s) = s, s

    /// Ignores the state passed in favor of the provided state value.
    let setState (s:'s) = fun _ -> (), s

type StateBuilder() =
    member __.Return(value) : State<'a, 's> = State.result value
    member __.Bind(m:State<'a, 's>, f:'a -> State<'b, 's>) : State<'b, 's> = State.bind f m
    member __.ReturnFrom(m:State<'a, 's>) = m
    member __.Zero() = State.result ()
    member __.Delay(f) = State.bind f (State.result ())
    member __.Combine(m1:State<unit, 's>, m2:State<'a, 's>) =
        State.bind (fun () -> m2) m1
    member inline __.Combine(m1:State<'a, 's>, m2:State<'a, 's>) =
        fun s ->
            let v1, s1 = m1 s
            let v2, s2 = m2 s
            v1 + v2, s1 + s2

let state = StateBuilder()

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

        test "returns value" {
            let c = state {
                let! (s : string) = State.getState
                return System.String(s.ToCharArray() |> Array.rev)
            }
            let actual = State.eval c "Hello"
            Expect.equal actual "olleH" "Expected \"olleH\" as the value."
        }

        test "returns without changing state" {
            let c = state {
                let! (s : string) = State.getState
                return System.String(s.ToCharArray() |> Array.rev)
            }
            let actual = State.exec c "Hello"
            Expect.equal actual "Hello" "Expected \"Hello\" as the state."
        }

        test "returns unit" {
            let c = state {
                let! (s : string) = State.getState
                let s' = System.String(s.ToCharArray() |> Array.rev)
                do! State.setState s'
            }
            let actual = State.eval c "Hello"
            Expect.equal actual () "Expected return value of unit."
        }

        test "returns changed state" {
            let c = state {
                let! (s : string) = State.getState
                let s' = System.String(s.ToCharArray() |> Array.rev)
                do! State.setState s'
            }
            let actual = State.exec c "Hello"
            Expect.equal actual "olleH" "Expected state of \"elloH\"."
        }

        test "state supports if ... then with no else" {
            let c : State<unit, string> = state {
                if true then
                    printfn "Hello"
            }
            let actual = State.eval c ""
            Expect.equal actual () "Expected the value to be ()."
        }

        test "state supports returning unit and a values" {
            let c : State<string, string> = state {
                return ()
                return "two"
            }
            let actual = State.eval c ""
            Expect.equal actual "two" "Expected \"two\"."
        }

        test "state supports returning multiple values" {
            let c : State<string, string> = state {
                return "one"
                return "two"
            }
            let actual = State.eval c ""
            Expect.equal actual "onetwo" "Expected all returns to be concatenated."
        }
    ]