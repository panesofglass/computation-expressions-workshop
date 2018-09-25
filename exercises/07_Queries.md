# Query Expressions

In this section we will look at [Query Expressions](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/query-expressions). Query Expressions were introduced in F# 3.0, alongside [Type Providers](https://docs.microsoft.com/en-us/dotnet/fsharp/tutorials/type-providers/).






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
