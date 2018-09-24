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

let example =
    async {
        match! Async.Parallel [|async.Return(1); async.Return(2); async.Return(3)|] with
        | [|first; second; third|] ->
            return first + second + third
        | _ ->
            failwith "Impossible scenario"
            return -1
    }

type Microsoft.FSharp.Control.AsyncBuilder with
    [<CustomOperation("and!", IsLikeZip = true)>]
    member this.Merge(x, y, [<ProjectionParameter>] resultSelector) =
        async {
            let! [|x';y'|] = Async.Parallel [|x;y|]
            return resultSelector x' y'
        }
    member this.For(m, f) = this.Bind(m, f)

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

        test "concurrent async execution" {
            let expected = 3
            let parallel =
                async {
                    for a in async.Return(1) do
                    ``and!`` b in async.Return(2)
                    return a + b
                }
            let actual = Async.RunSynchronously parallel
            Expect.equal actual expected "Expected actual to equal 3"
        }

        test "concurrent runs concurrently, not sequentially" {
            let task1 =
                async {
                    do! Async.Sleep(1000)
                    return 1
                }
            let task2 =
                async {
                    do! Async.Sleep(1000)
                    return 2
                }
            let sequentialStopwatch = System.Diagnostics.Stopwatch()
            let sequential =
                async {
                    sequentialStopwatch.Start()
                    let! a = task1
                    let! b = task2
                    sequentialStopwatch.Stop()
                    return a + b
                }
                |> Async.RunSynchronously
            let parallelStopwatch = System.Diagnostics.Stopwatch()
            let parallel =
                async {
                    parallelStopwatch.Start()
                    for a in task1 do
                    ``and!`` b in task2
                    parallelStopwatch.Stop()
                    return a + b
                }
                |> Async.RunSynchronously
            printfn "Sequential: %d, Concurrent: %d"
                sequentialStopwatch.ElapsedMilliseconds
                parallelStopwatch.ElapsedMilliseconds
            Expect.equal parallel sequential
                "Expected parallel result to equal sequential result"
            Expect.isLessThan
                parallelStopwatch.ElapsedMilliseconds
                sequentialStopwatch.ElapsedMilliseconds
                "Expected parallel to be less than sequential"
        }
    ]