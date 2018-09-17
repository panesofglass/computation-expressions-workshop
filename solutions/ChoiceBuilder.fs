module Choose

open Expecto
open Options

type ChoiceBuilder() =
    inherit OptionBuilder()
    
    (*
    // First attempt
    member __.Combine(m:'a option, f:unit -> 'a option) =
        printfn "choose.Combine(%A, %A)" m f
        match m with
        | Some _ -> m
        | None -> f()
    member __.Delay(f:unit -> _ option) =
        printfn "choose.Delay(%A)" f
        f()
    *)

    (*
    // Second attempt
    member __.Combine(m1:'a option, m2:'a option) =
        printfn "choose.Combine(%A, %A)" m1 m2
        match m1 with
        | Some _ -> m1
        | None -> m2
    member __.Delay(f:unit -> _ option) =
        printfn "choose.Delay(%A)" f
        f()
    // NOTE: results in printing statements after return! due to delay not blocking further evaluation.
    *)

    (*
    // Third attempt
    member __.Combine(m1:'a option, m2:'a option) =
        printfn "choose.Combine(%A, %A)" m1 m2
        match m1 with
        | Some _ -> m1
        | None -> m2
    member __.Delay(f:unit -> 'a option) =
        printfn "choose.Delay(%A)" f
        f
    *)

    // Fourth attempt
    member __.Combine(m:'a option, f:unit -> 'a option) =
        printfn "choose.Combine(%A, %A)" m f
        match m with
        | Some _ -> m
        | None -> f()
    member __.Delay(f:unit -> 'a option) =
        printfn "choose.Delay(%A)" f
        f
    member __.Run(f:unit -> _ option) =
        printfn "choose.Run(%A)" f
        f()

let choose = ChoiceBuilder()

[<Tests>]
let tests =
    testList "choices" [
        test "ChoiceBuilder returns value" {
            let expected = 1
            let actual = choose { return expected }
            Expect.equal actual (Some expected) "Expected Some 1"
        }

        test "ChoiceBuilder can bind option values" {
            let actual = choose {
                let! w = opt1
                let! x = opt2
                let! y = opt3
                let! z = opt4
                let result = sum4 w x y z
                printfn "Result: %d" result // print if a result was computed.
                return result
            }
            Expect.equal actual nested "Actual should sum to the same value as nested."
        }

        test "ChoiceBuilder instance can be used directly" {
            let actual =
                choose.Bind(opt1, fun w ->
                    choose.Bind(opt2, fun x ->
                        choose.Bind(opt3, fun y ->
                            choose.Bind(opt4, fun z ->
                                let result = sum4 w x y z
                                printfn "Result: %d" result
                                choose.Return(result)
                            )
                        )
                    )
                )
            Expect.equal actual composed "Actual should sum to the same value as nested."
        }

        test "ChoiceBuilder can exit without returning a value" {
            let dirExists path =
                let fileInfo = System.IO.FileInfo(path)
                let fileName = fileInfo.Name
                let pathDir = fileInfo.Directory.FullName.TrimEnd('~')
                if System.IO.Directory.Exists(pathDir) then
                    Some (System.IO.Path.Combine(pathDir, fileName))
                else None

            let choosePath = Some "~/test.txt"

            let actual =
                choose {
                    let! path = choosePath
                    let! fullPath = dirExists path
                    System.IO.File.WriteAllText(fullPath, "Test succeeded")
                }

            Expect.equal actual None "Actual should be None"
        }

        test "ChoiceBuilder supports if then without an else" {
            let choosePath = Some "~/test.txt"

            let actual =
                choose {
                    let! path = choosePath
                    let pathDir = System.IO.Path.GetDirectoryName(path)
                    if not(System.IO.Directory.Exists(pathDir)) then
                        return "Select a valid path."
                }

            Expect.equal actual (Some "Select a valid path.") "Actual should return Some(\"Select a valid path.\")"
        }

        test "ChoiceBuilder allows for early escape with return!" {
            let actual =
                choose {
                    if true then
                        return! None
                    else
                        let! w = opt1
                        let! x = opt2
                        let! y = opt3
                        let! z = opt4
                        let result = sum4 w x y z
                        printfn "Result: %d" result // print if a result was computed.
                        return result
                }
            Expect.equal actual None "Should return None immediately"
        }

        test "choose returns first value if it is Some" {
            let actual = choose {
                return! Some 1
                printfn "returning first value?"
                return! Some 2
            }
            Expect.equal actual (Some 1) "Expected the first value to be returned."
        }

        (*
        test "expanding choose for the second attempt runs the same way" {
            let actual =
                choose.Delay(fun () ->
                    choose.ReturnFrom(Some 1)
                    |> fun v1 ->
                        choose.Delay(fun () ->
                            printfn "returning first value?"
                            choose.ReturnFrom(Some 2)
                            |> fun v2 ->
                                choose.Combine(v1, v2)
                        )
                )
            Expect.equal actual (Some 1) "Expected the first value to be returned."
        }
        *)

        test "expanding choose for the fourth attempt runs the same way" {
            let actual =
                choose.Run(
                    choose.Delay(fun () ->
                        choose.Combine(
                            choose.ReturnFrom(Some 1), 
                            choose.Delay(fun () ->
                                printfn "returning first value?"
                                choose.ReturnFrom(Some 2)
                            )
                        )
                    )
                )
            Expect.equal actual (Some 1) "Expected the first value to be returned."
        }

        test "choose returns second value if first is None" {
            let actual = choose {
                return! None
                printfn "returning second value?"
                return! Some 2
            }
            Expect.equal actual (Some 2) "Expected the second value to be returned."
        }

        test "choose returns the last value if all previous are None" {
            let actual = choose {
                return! None
                return! None
                return! None
                return! None
                return! None
                return! None
                return! Some 7
            }
            Expect.equal actual (Some 7) "Expected the seventh value to be returned."
        }

        test "ChoiceBuilder can chain a computation onto another returning None, where None indicates success" {
            let dirExists path =
                let fileInfo = System.IO.FileInfo(path)
                let fileName = fileInfo.Name
                let pathDir = fileInfo.Directory.FullName.TrimEnd('~')
                if System.IO.Directory.Exists(pathDir) then
                    Some (System.IO.Path.Combine(pathDir, fileName))
                else None

            let choosePath = Some "~/test.txt"

            let writeFile =
                choose {
                    let! path = choosePath
                    let! fullPath = dirExists path
                    System.IO.File.WriteAllText(fullPath, "Test succeeded")
                }
            let actual =
                choose {
                    return! writeFile
                    return "Successfully wrote file"
                }

            Expect.equal actual (Some "Successfully wrote file") "Actual should indicate success"
        }
    ]