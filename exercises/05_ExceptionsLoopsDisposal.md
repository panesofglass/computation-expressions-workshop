# Exceptions, Loops, Disposals, and Quotations

We have covered most, but not all, of the members that make up computation expressions. Fortunately, these are not defined as part of any interface, with rigid type signatures, as we would otherwise have already stumbled into several issues with our previous exercises. on the other hand, these next members are all typically implementable with the same, basic signature, regardless of computation expression.

While these members have not been core to our previous exercises, they can be used almost entirely on their own to solve some very useful problems. [Expecto](https://github.com/haf/expecto)'s [`test`](https://github.com/haf/expecto/blob/master/Expecto/Expecto.fs#L1479-L1505) CE -- which we have been using in all our tests thus far -- is almost entirely composed of only these members.

In this exercise, we'll build our own `MyTestBuilder` to demonstrate how to write similar helpers. Feel free to also go back to the previous builders and add these members if you like.

1. Create a new file, `ExceptionsLoopsDispoal.fs`.
2. Add the file to your `.fsproj` with `<Compile Include="ExceptionsLoopsDisposal.fs" />` just below `Sequences.fs`.
3. Add the following lines to the `ExceptionsLoopsDisposal.fs` file:
``` fsharp
module ExceptionsLoopsDisposal

open Expecto

type MyTestBuilder() = class end

let myTest = MyTestBuilder()

[<Tests>]
let tests =
    testList "my tests" [
        testCase "A simple test" (fun () ->
            let expected = 4
            Expect.equal expected (2+2) "2+2 = 4"
        )
    ]
```

## Delaying Execution

We want to achieve the following syntax:

``` fsharp
        myTest "A simple test" {
            let expected = 4
            Expect.equal expected (2+2) "2+2 = 4"
        }
```

Change your current test to reflect our desired syntax with `myTest`.

This will work, but the test will evaluate immediately, which we don't want. Based on the standard `testCase` from Expecto above, we want to delay execution until we are ready to run the test. We know how to do that already:

``` fsharp
type MyTestBuilder() =
    member __.Delay(f) = f
    member __.Run(f) = testCase name f
```

This of course won't compile, as we don't have a `name` value available, and we don't want to hard-code it. Fortunately, we know how to create class types, and class constructors can take parameters for the class. We also see from our desired syntax that our CE needs to accept a string, i.e. the `name` parameter:

``` fsharp
type MyTestBuilder(name) =
    member __.Delay(f) = f
    member __.Run(f) = testCase name f

let myTest name = MyTestBuilder(name)
```

Close but not quite, as the compiler is still unhappy:

```
/Users/ryan/Code/computation-expressions-workshop/solutions/ExceptionsLoopsDisposal.fs(21,13): error FS0708: This control construct may only be used if the computation expression builder definesa 'Zero' method
```

`Zero` is required as we never return a value, and the computation needs some sort of return value. Tests always return `unit`, so this implementation is very simple:

``` fsharp
type MyTestBuilder(name) =
    member __.Delay(f) = f
    member __.Run(f) = testCase name f
    member __.Zero() = ()
```

Run `dotnet test`, and you should see all tests pass.

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

Our syntax should
