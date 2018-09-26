module FizzBuzz

type FizzBuzzState =
    | Pass
    | Fail of failedOn : int
    | Next of int list

type Checker() = class end
    // TODO: implement the builder

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
