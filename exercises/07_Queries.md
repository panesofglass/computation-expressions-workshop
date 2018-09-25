# Query Expressions

In this section we will look at [Query Expressions](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/query-expressions). Query Expressions were introduced in F# 3.0, alongside [Type Providers](https://docs.microsoft.com/en-us/dotnet/fsharp/tutorials/type-providers/).

In this exercise, we'll look at the `QueryBuilder` provided by `FSharp.Core`, implement a few more extensions to support `'a option` results, and explore the range of `CustomOperation`s by implementing several members of the `rxquery` expression for the Reactive Extensions found in [FSharp.Control.Reactive](https://github.com/fsprojects/FSharp.Control.Reactive). 

1. Create a new file, `Queries.fs`.
2. Add the file to your `.fsproj` with `<Compile Include="Queries.fs" />` just below `Extensions.fs`.
3. Add the following lines to the `Extensions.fs` file:
``` fsharp
module Queries

open Expecto

[<Tests>]
let tests =
    testList "queries" [
    ]
```

## `QueryBuilder`


## Extending `QueryBuilder` to support `'a option`


## `CustomOperation`s in `rxquery`


## Understanding Restrictions

There are a handful of rules about `CustomOperation`s that are not well defined except by error messages. You can find those restrictions in the [`FSComp.txt`](https://github.com/Microsoft/visualfsharp/blob/81894434220bb19e2985946afd15fbe4d91df9b4/src/fsharp/FSComp.txt#L1197-L1215) file in the [visualfsharp](https://github.com/Microsoft/visualfsharp) repository (or by trying and failing to build).

One such restriction states, "The implementations of custom operations may not be overloaded." Another states, "A custom operation may not be used in conjunction with 'use', 'try/with', 'try/finally', 'if/then/else' or 'match' operators within this computation expression." These are fairly standard use cases elsewhere.

You can sometimes get around these restrictions by using nested computations, e.g.

``` fsharp
let q =
    query {
        for x in source do
        where x < 1
        select x
    }
seq {
    for x in q do
        try
            yield 1 / x
        with e ->
            yield x
}
```

## Review

This section completes our exploration into the features provided by computation and query expressions. In this section, we reviewed the following:

* `QueryBuilder`
* Extending Query Expressions
* `CustomOperation` parameter uses

You have now seen all the tools available for creating useful, reusable abstractions to simplify your programs. However, we can take this even farther by leveraging `CustomOperation`s to embed domain specific languages within F# and provide a even more expressive abstractions for writing simple programs to solve complex problems.
