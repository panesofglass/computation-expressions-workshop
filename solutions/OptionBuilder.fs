module Options

open Expecto

let opt1 = Some 1
let opt2 = Some 2
let opt3 = Some 3
let opt4 = Some 4
let sum4 w x y z = w + x + y + z

let nested =
    match opt1 with
    | Some w ->
        match opt2 with
        | Some x ->
            match opt3 with
            | Some y ->
                match opt4 with
                | Some z -> Some(sum4 w x y z)
                | None -> None
            | None -> None
        | None -> None
    | None -> None

let composed =
    opt1
    |> Option.bind (fun w ->
        opt2
        |> Option.bind (fun x ->
            opt3
            |> Option.bind (fun y ->
                opt4
                |> Option.map (fun z ->
                    sum4 w x y z
                )
            )
        )
    )

//type OptionBuilder() = class end
type OptionBuilder() =
    member __.Return(value) = Some value

let maybe = OptionBuilder()

[<Tests>]
let tests =
    testList "OptionBuilder" [
        test "nested = composed" {
            Expect.equal nested composed "Expected nested to equal composed"
        }

        test "OptionBuilder returns value" {
            let expected = 1
            let actual = maybe { return expected }
            Expect.equal actual (Some expected) "Expected Some 1"
        }
    ]
