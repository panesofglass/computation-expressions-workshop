# Exceptions, Loops, Disposals, and Quotations

We have covered most, but not all, of the members that make up computation expressions. Fortunately, these are not defined as part of any interface, with rigid type signatures, as we would otherwise have already stumbled into several issues with our previous exercises. on the other hand, these next members are all typically implementable with the same, basic signature, regardless of computation expression.

While these members have not been core to our previous exercises, they can be used almost entirely on their own to solve some very useful problems. [Expecto](https://github.com/haf/expecto)'s [`test`](https://github.com/haf/expecto/blob/master/Expecto/Expecto.fs#L1479-L1505) CE -- which we have been using in all our tests thus far -- is almost entirely composed of these members.

1. Create a new file, `ExceptionsLoopsDispoal.fs`.
2. Add the file to your `.fsproj` with `<Compile Include="ExceptionsLoopsDisposal.fs" />` just below `Sequences.fs`.
3. Add the following lines to the `ExceptionsLoopsDisposal.fs` file:
``` fsharp
module ExceptionsLoopsDisposal

open Expecto

type Stack<'a> =
    | Empty
    | Cons of top:'a * rest:Stack<'a>

module Stack =
    /// Pushes a new value on top of the stack
    let push v s = Cons(v, s)

    /// Pops the top value off the stack,
    /// returning both the value and remaining stack.
    /// Throws an error if there are no remaining values.
    let pop s =
        match s with
        | Cons(v, c) -> v, c
        | _ -> failwith "Nothing to pop!"

    /// Converts the Stack<'a> to an 'a list.
    let toList s =
        let rec loop s cont =
            match s with
            | Cons(head, tail) ->
                loop tail (fun rest -> cont(head::rest))
            | Empty -> cont []
        loop s id

    /// Pushes a value onto a new stack.
    let lift v = push v Empty

type StackBuilder() = class end

let stack = StackBuilder()

[<Tests>]
let tests =
    testList "sequences" [
        test "Stack.toList generates a matching list" {
            let actual = Cons(1, Cons(2, Cons(3, Empty))) |> Stack.toList
            Expect.equal actual [1;2;3] "Expected list containing [1;2;3]"
        }
    ]
```
## Exceptions

Much like F# exception handling `try ... with` and `try ... finally`, computation expressions provide two members for dealing with exception handling: `TryWith` and `TryFinally`.

### `TryWith`

The `TryWith` member exposes the `try ... with` syntax within the CE. It's signature is:

``` fsharp
M<'T> * (exn -> M<'T>) -> M<'T>
```

> **NOTE:** I've never seen a variation from this signature, though I suspect there are ways to change the `M`, just as with `Bind`, `Combine`, etc.

The basic implementation strategy is something along the lines of:

``` fsharp
    member __.TryWith(tryBlock, withBlock) =
        // May need to return a lambda taking a parameter, e.g. `State`
        try tryBlock // may need to pass a parameter, e.g. `State` or `Expecto`
        with e -> withBlock e
```

Other strategies may be required based on the wrapper or computation type, but this general strategy typically serves the purpose.

> **NOTE:** You can find some other variations in the sample CE in the [Computation Expressions](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions#creating-a-new-type-of-computation-expression) docs and the [FSharpx.Extras `Continuation` module](https://github.com/fsprojects/FSharpx.Extras/blob/master/src/FSharpx.Extras/ComputationExpressions/Continuation.fs#L34-L40).


