# `StateBuilder`

This exercise works through the `StateBuilder`, in which we want to manage some state of a computation. The `StateBuilder` is a useful example as it is the generalized version of several of the popular computation expressions you'll find in F# libraries, e.g. [Freya](https://freya.io/) and [Saturn](https://saturnframework.org/).

1. Create a new file, `StateBuilder.fs`.
2. Add the file to your `.fsproj` with `<Compile Include="StateBuilder.fs" />` just below `ChoiceBuilder.fs`.
3. Add the following lines to the `StateBuilder.fs` file:
``` fsharp
module States

open Expecto

type StateBuilder() = class end
```

## `State`

Let's first define the `State` type that will define our interactions. In order to understand how this type should be defined, let's look at an example of how we might use some state without a computation expression:

``` fsharp
open System.Text

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
```

We've defined four functions, each taking a `StringBuilder` and returning a `StringBuilder`, or in the case of `run`, a `string`. This allows us to nicely chain these functions together and then produce a result at the end. Your primary **observation** here should be that there's a pattern of passing a specific instance and getting it back in the result.

However, this is overly simplified, as we may not _always_ want to `Append` to the `StringBuilder`, and we may want to compute a value, which may require retrieving the current value from the `StringBuilder`. With this in mind, we can define our type as:

``` fsharp
type State<'a, 's> = 's -> 'a * 's
```

or

``` fsharp
type State<'a, 's> = State('s -> 'a * 's)
```

depending on whether you prefer the single-case union style. The latter makes the type much more explicit, but it may also prevent you from using existing functions without first wrapping and unwrapping them. For this exercise, we'll use the first version as a means of demonstrating that approach.

> **NOTE:** you may not always need to produce a value to follow this pattern. A great example is the `HttpHandler` from Suave and Giraffe: `type HttpHandler = HttpContext -> Async<HttpContext option>`. This type mixes several concepts together but is ultimately similar to what we are building here, only without a value produced aside from the state.

## `State` module

Many of our computation expression member implementations will be very similar. However, we were able to leverage existing module functions for the `OptionBuilder`, and having created a new type, we have no functions with which to work. Let's define those now.

> **NOTE:** creating a module of functions allows you to work without a computation expression, as well. This can be quite useful for debugging or for writing simple computations where the CE may not be quite as useful. You may also find it easier to think of each function separately rather than trying to define the behavior within a class member.

### `Return`

In order to return a value, we'll need to wrap it in our `'s -> 'a * 's` function.

``` fsharp
module State =
    // Explicit
    //let result x : State<'a, 's> = fun s -> x, s
    // Less explicit but works better with other, existing functions:
    let result x s = x, s
```

The `result` definition has been expressed in two ways. Interstingly, the first form with the `State<'a, 's>` does not actually force F# to use that type definition, so the latter may be preferred as it is shorter and gives you a good idea of the form of functions that could be used without modification. Were we to use the single-case union, you could enforce the type signature to be `State<'a,'s>` and not the type it aliases.

### `Bind`

Next we want to look at how we compose two `State<'a, 's>` types. Here's the signature we have to work with

``` fsharp
val bind : ('a -> State<'b, 's>) -> State<'a, 's> -> State<'b, 's>

// expanded:
val bind : ('a -> ('s -> 'b * 's)) -> ('s -> 'a * 's) -> ('s -> 'b * 's)
```

The expanded signature shows us that we need a state value to retrieve the `'a` value needed to pass to the function:

``` fsharp
    let bind (f:'a -> State<'b, 's>) (m:State<'a, 's>) : State<'b, 's> =
        // return a function that takes the state
        fun s ->
            // Get the value and next state from the m parameter
            let a, s' = m s
            // Get the next state computation by passing a to the f parameter
            let m' = f a
            // Apply the next state to the next computation
            m' s'
```

In this case, we used the explicit lambda as it allows us to specify the types in the `bind` definition to help us guide our implementation.

We now have enough to begin implementing our `StateBuilder`:

``` fsharp
type StateBuilder() =
    member __.Return(value) : State<'a, 's> = State.result value
    member __.Bind(m:State<'a, 's>, f:'a -> State<'b, 's>) : State<'b, 's> = State.bind f m
    member __.ReturnFrom(m:State<'a, 's>) = m
```

> **NOTE:** the type declarations are not required, but they can prove helpful when verifying the types are what you want.


