module Extensions

open Expecto
open Options

//type Microsoft.FSharp.Control.AsyncBuilder with
    //member this.Foo(bar) = sprintf "foo%sbaz" bar

//let result = async.Foo("bar")

type Microsoft.FSharp.Control.AsyncBuilder with
    member this.Bind(task, f) =
        this.Bind(Async.AwaitTask task, f)
    member this.Bind(task:System.Threading.Tasks.Task, f) =
        this.Bind(Async.AwaitTask task, f)
    //member this.Bind(task:System.Threading.Tasks.Task, f) =
    //    this.Bind(Async.AwaitTask(task.ContinueWith(System.Func<_,_>(fun _ -> ()))), f)

type OptionBuilder with
    member this.Bind(value, f) =
        this.Bind(Option.ofNullable value, f)
    member this.Bind(value, f) =
        this.Bind(Option.ofObj value, f)

[<Tests>]
let tests =
    testList "extensions" [
        test "async can bind Task<'T>" {
            let expected = 0
            let task = System.Threading.Tasks.Task.FromResult expected
            let actual =
                async {
                    let! res = task
                    return res
                }
                |> Async.RunSynchronously
            Expect.equal actual expected "Expected async to bind Task<'T>"
        }

        test "async can bind Task" {
            let task =
                System.Threading.Tasks.Task.FromResult ()
                :> System.Threading.Tasks.Task
            let actual =
                async {
                    do! task
                }
                |> Async.RunSynchronously
            Expect.equal actual () "Expected async to bind Task"
        }

        test "maybe can bind Nullable<'T>" {
            let expected = 1
            let nullable = System.Nullable(1)
            let actual =
                maybe {
                    let! x = nullable
                    return x
                }
            Expect.equal actual (Some expected) "Expected a Nullable 1 to return Some 1"
        }

        test "maybe can bind a null object" {
            let expected : string = null
            let actual =
                maybe {
                    let! x = expected
                    return x
                }
            Expect.equal actual None "Expected a null object to return None"
        }
    ]