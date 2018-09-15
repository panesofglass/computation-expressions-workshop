module FizzBuzz

type FizzBuzzState =
    | Pass
    | Fail of failedOn : int
    | Next of int list

let bind f m =
    match m with
    | Pass -> Fail -1
    | Fail x -> Fail x
    | Next x -> f x

type Checker() =
    member __.For(source:seq<int>, f) =
        bind f (Next(List.ofSeq source))
    member __.Yield(n) =
        Next n
    member __.Delay(f) = f
    member __.Run(f) =
        match f() with
        | Pass -> printfn "passed!"
        | Fail -1 -> eprintfn "exceeded range!"
        | Fail x -> eprintfn "failed on %i" x
        | Next x -> eprintfn "incomplete: %A" x
    
    member __.FilterBind(m, f) =
        bind (fun xs ->
            match xs with
            | [] -> Fail -1
            | [x] when f x -> Pass
            | x::xs when f x -> Next xs
            | x::_ -> Fail x) m

    [<CustomOperation("num")>]
    member __.Number(m, n) =
        __.FilterBind(m, fun x -> n = x && x % 3 <> 0 && x % 5 <> 0)

    [<CustomOperation("fizz")>]
    member __.Fizz(m) =
        __.FilterBind(m, fun x -> x % 3 = 0 && x % 5 <> 0)

    [<CustomOperation("buzz")>]
    member __.Buzz(m) =
        __.FilterBind(m, fun x -> x % 3 <> 0 && x % 5 = 0)

    [<CustomOperation("fizzbuzz")>]
    member __.FizzBuzz(m) =
        __.FilterBind(m, fun x -> x % 3 = 0 && x % 5 = 0)

let check = Checker()

let pass =
    check {
        for i in 1..15 do
        num 1
        num 2
        fizz
        num 4
        buzz
        fizz
        num 7
        num 8
        fizz
        buzz
        num 11
        fizz
        num 13
        num 14
        fizzbuzz
    }

let fail =
    check {
        for i in 1..15 do
        num 1
        num 2
        fizz
        num 4
        buzz
        fizz
        num 7
        num 8
        fizz
        buzz
        num 11
        num 12
        num 13
        num 14
        fizzbuzz
    }

let notEnough =
    check {
        for i in 1..15 do
        num 1
        num 2
        fizz
    }

let tooMany =
    check {
        for i in 1..15 do
        num 1
        num 2
        fizz
        num 4
        buzz
        fizz
        num 7
        num 8
        fizz
        buzz
        num 11
        fizz
        num 13
        num 14
        fizzbuzz
        num 16
        num 17
        fizz
        num 19
        buzz
        fizz
    }
