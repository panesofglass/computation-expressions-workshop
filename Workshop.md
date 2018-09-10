# Workshop

## Computation Expressions

### What?

1. [Language Reference](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions)
2. [Embedded DSL](http://www.readcopyupdate.com/blog/2014/10/10/edsls-using-custom-operations.html)

### Why?

1. Reduce arrowhead pattern
2. Familiar syntax, e.g. `let` and `do` with extensions: `let!`, `do!`, `return`, etc.
3. `async`
4. `seq`
5. `test` (from Expecto)


### `OptionBuilder`

1. Without a computation expression
2. `Return`
3. `Bind`
4. Side effects and `Delay`
5. `do!` with a `unit` result using `Combine`
6. `if ... then` without an `else`
7. Exercise: `ChoiceBuilder`, e.g. `choice { return! None; return 1 }`

### Continuation Passing Style

1. Without a computation expression
2. Side effects passed into a computation
3. Differences between "container" and "computation"
4. Exercise: `StateBuilder`

### Generating Sequences

1. `Yield` = `Return`
2. `For` = `Bind`
3. Exercise: `EventBuilder`

### Error Handling, Disposal, and More

1. `TryWith`
2. `TryFinally`
3. `Using`
4. `While`
5. `Quote`

## Extending Computation Expressions

### Method Overloads

1. `AsyncBuilder.Bind` for `Task<'T>`
2. `AsyncBuilder.Bind` for `Task`

### Adding Custom Extensions

1. Brief introduction to `CustomOperationAttribute`
2. Emulating applicatives with `for ... and! ...`
3. Overloading `CustomOperation` parameters
    1. Short answer: not allowed
    2. Long answer: use SRTP
4. Exercise: overload applicative for `Async<'T>` and `Task<'T>`

## Query Expressions

1. `query`
2. [`cil`](https://github.com/rspeele/LicenseToCIL)
3. Exercise: are you smarter than a FizzBuzz? (`FizzBuzzChecker`)

## Mixing it up

1. `AsyncSeq`
2. `AsyncState`
3. `Freya` and `FreyaRouter`
4. `Saturn`

# Project

Suggestions:
* Extend existing computation expressions in some way
* [probs.fsx](https://gist.github.com/mavnn/8ef06cb12ebc9a1807799bc01667e32a)
* CE using `Expr` to generate code
* Generate CE from Type Provider
* DSL for some business process
