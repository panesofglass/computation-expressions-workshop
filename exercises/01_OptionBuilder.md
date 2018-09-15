# `OptionBuilder`

In this first tutorial, we'll create our first computation expression builder.

In order to understand what we want to accomplish, we should also be familiar with how to do the task without a computation expression.

## Without a Computation Expression

1. Remove `Sample.fs`.
2. Create a new file, `OptionBuilder.fs`.
3. Add the file to your `.fsproj` with `<Compile Include="OptionBuilder.fs" />` above `Program.fs`.
4. Add the following lines of code to `OptionBuilder.fs`:
``` fsharp
module Options

open Expecto

let opt1 = Some 1
let opt2 = Some 2
let opt3 = Some 3
let opt4 = Some 4
let sum4 w x y z = w + x + y + z

let nested =
    match opt1 with
    | Some w ->
        match opt2 with
        | Some x ->
            match opt3 with
            | Some y ->
                match opt4 with
                | Some z -> Some(sum4 w x y z)
                | None -> None
            | None -> None
        | None -> None
    | None -> None

let composed =
    opt1
    |> Option.bind (fun w ->
        opt2
        |> Option.bind (fun x ->
            opt3
            |> Option.bind (fun y ->
                opt4
                |> Option.map (fun z ->
                    sum4 w x y z
                )
            )
        )
    )

[<Tests>]
let tests =
    testList "OptionBuilder" [
        test "nested = composed" {
            Expect.equal nested composed "Expected nested to equal composed"
        }
    ]
```
5. Build and run the program with `dotnet test`.

> NOTE: if you run into problems, notably an error regarding `EntryPointAttribute`, try the following:
Rename `Main.fs` to `Program.fs` and make the same change in the `fsproj`. You may also remove
the `module` declaration in the `Program.fs` file, but that should not have any material impact.

## Empty builder

1. Add a new type, `type OptionBuilder() = class end`
2. Create an instance of the builder: `let maybe = OptionBuilder()`
3. Add a test to validate the maybe builder exists. We'll assert that it should support `return`, which we'll implement next, and that it should return the value provided.
``` fsharp
type OptionBuilder() = class end

let maybe = OptionBuilder()

[<Tests>]
let tests =
    testList "OptionBuilder" [
        // ...
        // previous tests
        // ...

        test "OptionBuilder returns value" {
            let expected = 1
            let actual = maybe { return expected }
            Expect.equal actual (Some expected) "Expected Some 1"
        }
    ]
```
4. Build and run the program with `dotnet test`. Your program should fail to compile with the following error:
```
/Users/ryan/Code/computation-expressions-workshop/solution/OptionBuilder.fs(53,34): error FS0708: This control construct may only be used if the computation expression builder defines a 'Return' method
```

## `Return` a Value

The compiler informs us that in order to use the `return` keyword, we must implement the `Return` method on our builder.

1. Replace:
``` fsharp
type OptionBuilder() = class end
```
```fsharp
type OptionBuilder() =
    member __.Return(value) = Some value
```
2. Build and run the program with `dotnet test`. Your test should pass.

Next we'll look at `let` bindings with side-effects, or `let!`.
