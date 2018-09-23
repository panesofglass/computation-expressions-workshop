# Extensions

An easy way to put what we've learned thus far to use is in extending existing computation expressions. We one way do to this in the second exercise where we created the `ChoiceBuilder` by inheriting from `OptionBuilder`. Inheritance, however, is not always an option. In this exercise, we'll look at two more ways to extend existing builders, both leveraging [type extensions](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/type-extensions):

1. [Method overloads](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/members/methods#overloaded-methods)
2. [`CustomOperation`s](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions#custom-operations)

Fortunately, CEs can pick up any type extensions to builder types, so you can define these in other modules, libraries, etc. This means you could define different extensions for different use cases and only open the extensions you want in a given context.

In this exercise, we'll extend both the `Microsoft.FSharp.Control.AsyncBuilder` and `OptionBuilder` types with new functionality.

1. Create a new file, `Extensions.fs`.
2. Add the file to your `.fsproj` with `<Compile Include="Extensions.fs" />` just below `ExceptionsLoopsDisposal.fs`.
3. Add the following lines to the `Extensions.fs` file:
``` fsharp
module Extensions

open Expecto
open Options

[<Tests>]
let tests =
    testList "extensions" [
    ]
```

## Type Extensions (Optional)

This section serves as a primer for those unfamiliar with type extensions.

### Intrinsic Type Extensions

Intrinsic type extensions are defined next to the type within the same code file, namespace, etc. Here's an example:

``` fsharp
type USAddress =
    {
        Recipient : string
        Street1 : string
        Street2 : string
        City : string
        State : string
        Zip : string
    }

type USAddress with
    member this.Format() =
        let sb = System.Text.StringBuilder()
        sb.AppendLine(this.Recipient)
          .AppendLine(this.Street1)
          .AppendLine(this.Street2)
          .Append(this.City)
          .Append(", ")
          .Append(this.State)
          .Append(" ")
          .AppendLine(this.Zip)
```

Even though the `Format` method is defined in a type extension, this method will be included in the core definition of the `USAddress` type and may exist alongside in either a `namespace` or `module`. This type of extension is useful when you want to separate fields and behaviors or when you may want to define helper functions that will be referenced in the methods but that should not be part of the `type` definition itself.

### Optional Type Extensions

Optional type extensions may be defined outside the original `type` definition and may be applied to types from other libraries. These _are not_ made a part of the type and _must_ be declared within a `module`. Here's an example:

``` fsharp
module Extensions

open Expecto

type Microsoft.FSharp.Control.AsyncBuilder with
    member this.Foo(bar) = sprintf "foo%sbaz" bar

let result = async.Foo("bar")
```

The `async` CE builder now has a `Foo` member you can call. You did not modify the `AsyncBuilder` type directly, but the compiler will respect the member as an optional part of the type.

## Method Overloading

A common complaint when working with C# code is having to translate between `Task<'T>` (and `Task`) and `Async<'T>` with `Async.AwaitTask`. Wouldn't it be nice if the `async { let! res = task }` could just bind to a `Task<'T>` or `Task`?

Despite the specified signature of `Bind` from the docs, `M<'T> * ('T -> M<'U>) -> M<'U>`, we can achieve this feature using method overloading. It turns out that `M` is not required to remain the same type all over.

Add the following test case:

``` fsharp
        test "async can bind Task<'T>" {
            let expected = 0
            let task = System.Threading.Tasks.Task.FromResult expected
            let actual =
                async {
                    let! res = task
                    return res
                }
                |> Async.RunSynchronously
            Expect.equal actual expected "Expected async to bind Task<'T>"
        }
```

We need to teach `AsyncBuilder` how to handle `Task<'T>`, which is easily implemented with `Async.AwaitTask`. Add the following type extension above your tests:

``` fsharp
type Microsoft.FSharp.Control.AsyncBuilder with
    member this.Bind(task, f) =
        this.Bind(Async.AwaitTask task, f)
```

Once you add this implementation, you should be able to confirm that the F# compiler correctly infers the type of your `Bind` member as:

``` fsharp
System.Threading.Tasks.Task<'a> * ('a -> Async<'b>) -> Async<'b>
```

In addition, the red squiggle in your test case indicating a compiler error should have disappeared.

> **Observation:** this once again highlights the flexibility of the CE mechanism to adapt beyond the rigid type constraints found in the documentation.

Run your tests with `dotnet test` to see your tests pass.

Let's add another test case:

``` fsharp
        test "async can bind Task" {
            let expected = 0
            let task =
                System.Threading.Tasks.Task.FromResult ()
                :> System.Threading.Tasks.Task
            let actual =
                async {
                    do! task
                }
                |> Async.RunSynchronously
            Expect.equal actual expected "Expected async to bind Task"
        }
```

Unfortunately, our overload does not handle the non-generic `Task`. That's okay, we can create another overload to handle that case:

``` fsharp
    member this.Bind(task:System.Threading.Tasks.Task, f) =
        this.Bind(Async.AwaitTask task, f)
```

More recent versions of F# have an `Async.AwaitTask` that can await non-generic tasks. However, if you are using an older version of FSharp.Core, you may need a more involved implementation:

``` fsharp
    member this.Bind(task:System.Threading.Tasks.Task, f) =
        // One of many possible implementations ...
        this.Bind(Async.AwaitTask(task.ContinueWith(System.Func<_,_>(fun _ -> ()))), f)
```

Similar extensions may be made to our `OptionBuilder` from the first exercise. Add the following tests:

``` fsharp
        test "maybe can bind Nullable<'T>" {
            let expected = 1
            let nullable = System.Nullable(1)
            let actual =
                maybe {
                    let! x = nullable
                    return x
                }
            Expect.equal actual (Some expected) "Expected a Nullable 1 to return Some 1"
        }

        test "maybe can bind a null object" {
            let expected : string = null
            let actual =
                maybe {
                    let! x = expected
                    return x
                }
            Expect.equal actual None "Expected a null object to return None"
        }
```

Add overloads of `Bind` in our `Extensions` module to satisfy the tests above. Once complete, you should be able to run `dotnet test` and see all tests pass successfully.

## `CustomOperation`s


