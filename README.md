# Computation Expressions Workshop

The Computations Expressions Workshop collects and presents the content
of several papers and presents the material as a set of tutorials.
This content is intended to be used in a workshop setting.

## Setting up

The workshop uses the `dotnet` CLI as a base case. However, attendees may
elect to use an editor of their choice, though the steps to use those tools
will be omitted from the tutorials.

### Install `dotnet`

* Install the `dotnet` CLI through the [.NET Core SDK](https://www.microsoft.com/net/download)

NOTE: Some IDEs will also install the `dotnet` CLI.

### `git clone` Workshop Repository

Clone the workshop repository so you have the materials available locally.
Each tutorial is also tagged so that you can reference the finished result if you get stuck.
`git clone https://github.com/panesofglass/computation-expressions-workshop`

### Create a New Expecto Project

Now that you have the basics, you can get started by creating a new project with [Expecto](https://github.com/haf/expecto).

* Install the [Expecto](https://github.com/haf/expecto) template with `dotnet new -i Expecto.Template`
* Create a new project with `dotnet new expecto -lang fsharp`

# Outline

## Computation Expressions

### Introduction (15 minutes)

#### What?

1. [Language Reference](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions)
2. [Embedded DSL](http://www.readcopyupdate.com/blog/2014/10/10/edsls-using-custom-operations.html)

#### Why?

1. Reduce arrowhead pattern
2. Familiar syntax, e.g. `let` and `do` with extensions: `let!`, `do!`, `return`, etc.
3. `async`
4. `seq`
5. `test` (from Expecto) & `BuildTask`

### `OptionBuilder` (45 minutes)

1. Without a computation expression
2. `Return`
3. `Bind`
4. Side effects and `Delay`
5. `do!` with a `unit` result using `Combine`
6. `if ... then` without an `else`
7. Exercise: `ChoiceBuilder`, e.g. `choice { return! None; return 1 }`

### Computation Expressions for Computations (45 minutes)

1. Without a computation expression
2. Side effects passed into a computation
3. Differences between "container" and "computation"
4. Exercise: `StateBuilder`

### Generating Sequences (45 minutes)

1. `Yield` = `Return`
2. `For` = `Bind`
3. Exercise: `EventBuilder`

### Error Handling, Disposal, and More (30 minutes)

1. `TryWith`
2. `TryFinally`
3. `Using`
4. `While`
5. `Quote`

## Extending Computation Expressions

### Method Overloads (30 minutes)

1. `AsyncBuilder.Bind` for `Task<'T>`
2. `AsyncBuilder.Bind` for `Task`

### Adding Custom Extensions (30 minutes)

1. Brief introduction to `CustomOperationAttribute`
2. Emulating applicatives with `for ... and! ...`
3. Overloading `CustomOperation` parameters
    1. Short answer: not allowed
    2. Long answer: use SRTP
4. Exercise: overload applicative for `Async<'T>` and `Task<'T>`

## Lunch

## Query Expressions (1 hour)

1. `query`
2. [`cil`](https://github.com/rspeele/LicenseToCIL)
3. Exercise: are you smarter than a FizzBuzz? (`FizzBuzzChecker`)

## Mixing it up (1 hour)

1. `AsyncSeq`
2. `AsyncState`
3. `FreyaMachine` and `FreyaRouter`
4. `Saturn`

# Project (2 hours)

Suggestions:
* Extend existing computation expressions in some way
* [probs.fsx](https://gist.github.com/mavnn/8ef06cb12ebc9a1807799bc01667e32a)
* CE using `Expr` to generate code
* Generate CE from Type Provider
* DSL for some business process
