# `OptionBuilder`

In this first tutorial, we'll create a computation expression to remove the arrowhead pattern from working with `option` types.

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

## Objective

We will build an `OptionBuilder` to flatten the code we wrote above in `nested` and `composed` into the following:

``` fsharp
let actual = maybe {
    let! w = opt1
    let! x = opt2
    let! y = opt3
    let! z = opt4
    return sum4 w x y z
}
```

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

Replace:
``` fsharp
type OptionBuilder() = class end
```
with
```fsharp
type OptionBuilder() =
    member __.Return(value) = Some value
```

Build and run the program with `dotnet test`. Your tests should pass.

## Composing `'a option` Values

`let` bindings binds a value to a name. Computation expressions provide a `let!` binding that can bind a value according to rules specified by the computation expression. This facilitates several possibilities:
* making a decision as to whether to continue or halt a computation
* side effects, e.g. printing to the screen or making a network call
* transform a result into another form

To find out how to implement `let!`, add a test:

``` fsharp
        test "OptionBuilder can bind option values" {
            let actual = maybe {
                let! w = opt1
                let! x = opt2
                let! y = opt3
                let! z = opt4
                return sum4 w x y z
            }
            Expect.equal actual nested "Actual should sum to the same value as nested."
        }
```

Build and run the program with `dotnet test`. Your program should fail to compile with the following error:

```
/Users/ryan/Code/computation-expressions-workshop/solutions/OptionBuilder.fs(61,17): error FS0708: This control construct may only be used if the computation expression builder defines a 'Bind' method
```

The compiler informs us that in order to use the `let!` keyword, we must implement the `Bind` method on our builder.

The F# Language Specification indicates that the `Bind` member should have the following signature (specialized for our immediate use case):
``` fsharp
member Bind : 'a option * ('a -> 'b option) -> 'b option
```

As you either know or expect, this matches very closely with the signature of `Option.bind`:
``` fsharp
module Option =
    val bind : ('a -> 'b option) -> 'a option -> 'b option
```

We can therefore implement `Bind` as follows:
``` fsharp
    member Bind(m, f) = Option.bind f m // notice the parameter orientation
```

We could also implement `Bind` like this:
``` fsharp
    member Bind(m:'a option, f:'a -> 'b option) =
        match m with
        | Some x -> f x
        | None -> None
```

Build and run the program with `dotnet test`. Your tests should pass.

## Delaying Execution

When running this computation, the results are eagerly evaluated, including any `printfn` statements. To observe this behavior, change your code to match the following:

``` fsharp
let opt1 = Some 1
let opt2 = Some 2
let opt3 = None // Return None at this point
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
                | Some z ->
                    let result = sum4 w x y z
                    // Print the result if successful
                    printfn "%d" result
                    Some result
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
                    let result = sum4 w x y z
                    // Print the result if successful
                    printfn "%d" result
                    result
                )
            )
        )
    )

// ... then in your test:

        test "OptionBuilder can bind option values" {
            let actual = maybe {
                let! w = opt1
                let! x = opt2
                let! y = opt3
                let! z = opt4
                let result = sum4 w x y z
                printfn "%d" result // print if a result was computed.
                return result
            }
            Expect.equal actual nested "Actual should sum to the same value as nested."
        }

```