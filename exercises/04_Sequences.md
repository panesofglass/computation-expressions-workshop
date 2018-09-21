# Sequences

This exercise will take us on a slightly different path of building computation expressions where the the goal is to produce and consume sequences. We've looked at multiple `return` statements and `Combine` in previous exercises, but we have not yet covered anything like what you find with `seq { }` expressions.

1. Create a new file, `Sequences.fs`.
2. Add the file to your `.fsproj` with `<Compile Include="Sequences.fs" />` just below `StateBuilder.fs`.
3. Add the following lines to the `Sequences.fs` file:
``` fsharp
module Sequences

open Expecto

type Stack<'a> =
    | Empty
    | Cons of top:'a * rest:Stack<'a>

module Stack =
    let push v s = Cons(v, s)
    let pop s =
        match s with
        | Cons(v, c) -> v, c
        | _ -> failwith "Nothing to pop!"
    let lift v = push v Empty
    let toList s =
        let rec loop s cont =
            match s with
            | Cons(head, tail) ->
                loop tail (fun rest -> head::rest)
            | Empty -> cont []
        loop s id


type StackBuilder() = class end

let stack = StackBuilder()
```


