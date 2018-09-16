# `ChoiceBuilder`

In this next exercise, we'll look at how we can chain `'a option` computations together by returning multiple values. This is similar to allow the computation to make a choice, e.g.

``` fsharp
let actual : string option =
    choice {
        return! None
        return "Use this instead."
    }
```

Our implemetation will allow the computation to _choose_ a path based on the values returned. Following our example from the previous exercise, successfully writing the file and returning `None` could then be a way of signaling to continue, whereas a value of `Some "some error message"` would indicate the computation should halt.

1. Create a new file, `ChoiceBuilder.fs`.
2. Add the file to your `.fsproj` with `<Compile Include="ChoiceBuilder.fs" />` just below `OptionBuilder.fs`.
3. Add the following lines to the `ChoiceBuilder.fs` file:
``` fsharp
module Choose

open Expecto

type ChoiceBuilder() =
    member __.ReturnFrom(m:'a option) =
        printfn "choose.ReturnFrom(%A)" m
        m

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
    ]
```

## Combining Computations

Run `dotnet test`. The project will once again fail to compile with a helpful error message:

```
/Users/ryan/Code/computation-expressions-workshop/solutions/ChoiceBuilder.fs(19,17): error FS0708: This control construct may only be used if the computation expression builder defines a 'Combine' method
```




## Delaying Execution

When running this computation, the results are eagerly evaluated, including any `printfn` statements. To observe this behavior, change your code to match the following:

``` fsharp
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
                | Some z ->
                    let result = sum4 w x y z
                    // Print the result if successful
                    printfn "Nested: %d" result
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
                    printfn "Composed: %d" result
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
                printfn "Result: %d" result // print if a result was computed.
                return result
            }
            Expect.equal actual nested "Actual should sum to the same value as nested."
        }
// ...
```

Running these changes with `dotnet test` should succeed, and you should see printed console output for `Nested: 10`, `Computed: 10`, and `Result: 10`.

It's possible to have the computation expression delay execution until you are ready by implementing the `Delay` member:

``` fsharp
    member __.Delay(f:unit -> 'a option) = f
```

`Delay` takes a function and returns it without doing anything with it. What does the compiler do with this?

Running `dotnet test` fails the test and produces the following result:

```
val actual : (unit -> int option)
```

Our `actual` value is now a function that must be called. We can do several things with this now:

* Let the user call this when necessary
* Add a `runMaybe` function that accepts a `unit -> 'a option` and runs it
* Wrap this as a `Lazy<_>` value to avoid repeat executions

We could fix this by just calling `f()` in our `Delay` member defintion, but that would only produce the same behavior as we currently have. What we need to add is another member, `Run`:

``` fsharp
    member __.Run(f:unit -> 'a option) = f()
```

Aside from reading the specification, it can be useful to add traces or `printfn` statements into your CE builder definition while developing it. Let's do that now:

``` fsharp
type OptionBuilder() =
    member __.Return(value) =
        printfn "maybe.Return(%A)" value
        Some value
    member __.Bind(m, f) =
        printfn "maybe.Bind(%A, %A)" m f
        Option.bind f m
    member __.Delay(f: 'a option) =
        printfn "maybe.Delay(%A)" f
        f
```
