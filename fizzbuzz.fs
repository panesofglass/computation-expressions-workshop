module FizzBuzz

open Expecto

type FizzBuzzState =
    | Pass of next : int
    | Fail of failedOn : int

let bind f m =
    match m with
    | Pass x -> f x
    | Fail x -> Fail x

type Checker() =
    member __.Bind(m, f) =
        printfn "Binding %A" m
        bind f m
    member __.Yield(n) =
        printfn "Yielding %i" n
        Pass n
    member __.Combine(m, f) =
        printfn "Combining"
        match m with
        | Pass x ->
            printfn "Calling f with %i" (x+1)
            f(x+1)
        | Fail x -> Fail x
    member __.Delay(f) = f

    member __.Run(f) =
        match f() with
        | Pass _ -> printfn "passed!"
        | Fail x -> eprintfn "failed on %i" x

    [<CustomOperation("num", MaintainsVariableSpace=true)>]
    member __.Number(m, n) =
        bind (fun x -> printfn "%i" x
                       if n = x && x % 3 <> 0 && x % 5 <> 0
                       then Pass(x+1)
                       else Fail x) m

    [<CustomOperation("fizz", MaintainsVariableSpace=true)>]
    member __.Fizz(m) =
        bind (fun x -> printfn "fizz %i" x
                       if x % 3 = 0 && x % 5 <> 0
                       then Pass(x+1)
                       else Fail x) m

    [<CustomOperation("buzz", MaintainsVariableSpace=true)>]
    member __.Buzz(m) =
        bind (fun x -> printfn "buzz %i" x
                       if x % 3 <> 0 && x % 5 = 0
                       then Pass(x+1)
                       else Fail x) m

    [<CustomOperation("fizzbuzz", MaintainsVariableSpace=true)>]
    member __.FizzBuzz(m) =
        bind (fun x -> printfn "fizzbuzz %i" x
                       if x % 3 = 0 && x % 5 = 0
                       then Pass(x+1)
                       else Fail x) m

let check = Checker()

let pass =
    check {
        let! i = Pass 1
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
        let! i = Pass 1
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