# Return

In this first tutorial, we'll create our first computation expression builder.
Most users of F# stick to many of the functional aspects of the language.
Computation expressions, however, will lead you into at least a little of the
object-oriented side of the language by virtue that a computation expression is
merely a class with members matching certain names.

## Create Return.fs

1. Remove `Sample.fs`.
2. Create a new file, `Return.fs`.
3. Add the file to your `.fsproj` with `<Compile Include="Return.fs" />` just above a similar line for `Sample.fs`.
4. Remove `Sample.fs` from your `fsproj`.
5. Add a module declaration to your file, e.g. `module Return`.
6. `open Expecto` so that we can add tests.
7. Declare a `type TraceBuilder() = class end`.

NOTE: If `TraceBuilder` sounds familiar, it's likely you have seen it before in one of
many computation expression tutorials. While not very exciting, it is worthwhile to use
when getting started as it will show you how you can identify what's being called when.
Many people get tripped up when implementing computation expressions because the compiler
expands the calls at runtime differently from what they imagine. Learning how to print out
the expansions is a great way to fully understand how computation expressions work.
We'll get to more exciting computation expressions later.

## Empty builder

1. Create an instance of the builder: `let trace = TraceBuilder()`
2. Add a test to validate the trace builder exists. We'll assert that it should support `return`, which we'll implement next, and that it should return the value provided.

```
[<Tests>]
let tests =
    testList "traces" [
        test "TraceBuilder returns value" {
            let expected = 1
            let actual = trace { return expected }
            Expect.equal actual expected "Actual should match expected"
        }
    ]
```

Build and run the program with `dotnet test`. Your program should fail to compile with the following error:

```
/Users/ryan/Code/computation-expressions-workshop/Return.fs(20,34): error FS0708: This control construct may only be used if the computation expression builder defines a 'Return' method [/Users/ryan/Code/computation-expressions-workshop/computation-expressions-workshop.fsproj]
```

The compiler informs us that in order to use the `return` keyword, we must implement the `Return` method on our builder.

NOTE: if you run into problems, notably an error regarding `EntryPointAttribute`, try the following:
Rename `Main.fs` to `Program.fs` and make the same change in the `fsproj`. You may also remove
the `module` declaration in the `Program.fs` file, but that should not have any material impact.

## Return a Value

1. Remove the `class end` from the `TraceBuilder` declaration.
2. Add `member __.Return(value) = value` to the body of the `TraceBuilder`.

Build and run the program with `dotnet test`.

Your test should pass. Congratulations! You have written your first computation expression!

Next we'll look at `let` bindings with side-effects, or `let!`.
