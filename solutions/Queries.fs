module Queries

open System
open System.Linq
open System.Reactive.Linq
open System.Reactive.Concurrency
open Microsoft.FSharp.Linq
open Expecto

type TestRec = { Value : int }

// Extensions for `QueryBuilder`
type Microsoft.FSharp.Linq.QueryBuilder with

    [<CustomOperation("headOrNone")>] 
    member __.HeadOrNone(source:QuerySource<'T,'Q>) =
        Seq.tryHead source.Source

    [<CustomOperation("exactlyOneOrNone")>] 
    member __.ExactlyOneOrNone(source:QuerySource<'T,'Q>) =
        if Seq.length source.Source = 1 then
            Enumerable.Single(source.Source) |> Some
        else None

type RxQueryBuilder() =
    member __.For (s:IObservable<_>, body : _ -> IObservable<_>) = s.SelectMany(body)
    member __.Yield (value) = Observable.Return(value)
    member __.Zero () = Observable.Empty(Scheduler.CurrentThread :> IScheduler)

    [<CustomOperation("select")>]
    member __.Select(s:IObservable<_>, [<ProjectionParameter>] selector: _ -> _) =
        s.Select(selector)

    [<CustomOperation("head")>]
    member __.Head (s:IObservable<_>) = s.FirstAsync()
    [<CustomOperation("exactlyOne")>]
    member __.ExactlyOne (s:IObservable<_>) = s.SingleAsync()

    [<CustomOperation("where", MaintainsVariableSpace=true)>]
    member __.Where (s:IObservable<_>, [<ProjectionParameter>] predicate : _ -> bool) =
        s.Where(predicate)

    [<CustomOperation("groupBy", AllowIntoPattern=true)>]
    member __.GroupBy (s:IObservable<_>,[<ProjectionParameter>] keySelector : _ -> _) =
        s.GroupBy(new Func<_,_>(keySelector))

    [<CustomOperation("join", IsLikeJoin=true, JoinConditionWord="on")>]
    member __.Join (s1:IObservable<_>, s2:IObservable<_>,
        [<ProjectionParameter>] s1KeySelector : _ -> _,
        [<ProjectionParameter>] s2KeySelector : _ -> _,
        [<ProjectionParameter>] resultSelector : _ -> _) =
        s1.Join(s2,
            new Func<_,_>(s1KeySelector),
            new Func<_,_>(s2KeySelector),
            new Func<_,_,_>(resultSelector))

(*
    [<CustomOperation("takeWhile", MaintainsVariableSpace=true, AllowIntoPattern=true)>]
    member __.TakeWhile (s:IObservable<_>, [<ProjectionParameter>] predicate : _ -> bool ) = s.TakeWhile(predicate)
    [<CustomOperation("take", MaintainsVariableSpace=true, AllowIntoPattern=true)>]
    member __.Take (s:IObservable<_>, count: int) = s.Take(count)
    [<CustomOperation("skipWhile", MaintainsVariableSpace=true, AllowIntoPattern=true)>]
    member __.SkipWhile (s:IObservable<_>, [<ProjectionParameter>] predicate : _ -> bool ) = s.SkipWhile(predicate)
    [<CustomOperation("skip", MaintainsVariableSpace=true, AllowIntoPattern=true)>]
    member __.Skip (s:IObservable<_>, count: int) = s.Skip(count)
    [<CustomOperation("count")>]
    member __.Count (s:IObservable<_>) = Observable.Count(s)
    [<CustomOperation("all")>]
    member __.All (s:IObservable<_>, [<ProjectionParameter>] predicate : _ -> bool ) = s.All(new Func<_,bool>(predicate))
    [<CustomOperation("contains")>]
    member __.Contains (s:IObservable<_>, key) = s.Contains(key)
    [<CustomOperation("distinct", MaintainsVariableSpace=true, AllowIntoPattern=true)>]
    member __.Distinct (s:IObservable<_>) = s.Distinct()
    [<CustomOperation("exactlyOne")>]
    member __.ExactlyOne (s:IObservable<_>) = s.SingleAsync()
    [<CustomOperation("exactlyOneOrDefault")>]
    member __.ExactlyOneOrDefault (s:IObservable<_>) = s.SingleOrDefaultAsync()
    [<CustomOperation("find")>]
    member __.Find (s:IObservable<_>, [<ProjectionParameter>] predicate : _ -> bool) = s.FirstAsync(new Func<_,bool>(predicate))
    [<CustomOperation("head")>]
    member __.Head (s:IObservable<_>) = s.FirstAsync()
    [<CustomOperation("headOrDefault")>]
    member __.HeadOrDefault (s:IObservable<_>) = s.FirstOrDefaultAsync()
    [<CustomOperation("last")>]
    member __.Last (s:IObservable<_>) = s.LastAsync()
    [<CustomOperation("lastOrDefault")>]
    member __.LastOrDefault (s:IObservable<_>) = s.LastOrDefaultAsync()
    [<CustomOperation("maxBy")>]
    member __.MaxBy (s:IObservable<'a>,  [<ProjectionParameter>] valueSelector : 'a -> 'b) = s.MaxBy(new Func<'a,'b>(valueSelector))
    [<CustomOperation("minBy")>]
    member __.MinBy (s:IObservable<'a>,  [<ProjectionParameter>] valueSelector : 'a -> 'b) = s.MinBy(new Func<'a,'b>(valueSelector))
    [<CustomOperation("nth")>]
    member __.Nth (s:IObservable<'a>,  index ) = s.ElementAt(index)
    [<CustomOperation("sumBy")>]
    member inline __.SumBy (s:IObservable<_>,[<ProjectionParameter>] valueSelector : _ -> _) = s.Select(valueSelector).Aggregate(Unchecked.defaultof<_>, new Func<_,_,_>( fun a b -> a + b)) 
    [<CustomOperation("groupBy", AllowIntoPattern=true)>]
    member __.GroupBy (s:IObservable<_>,[<ProjectionParameter>] keySelector : _ -> _) = s.GroupBy(new Func<_,_>(keySelector))
    [<CustomOperation("groupValBy", AllowIntoPattern=true)>]
    member __.GroupValBy (s:IObservable<_>,[<ProjectionParameter>] resultSelector : _ -> _,[<ProjectionParameter>] keySelector : _ -> _) = s.GroupBy(new Func<_,_>(keySelector),new Func<_,_>(resultSelector))
    [<CustomOperation("join", IsLikeJoin=true)>]
    member __.Join (s1:IObservable<_>,s2:IObservable<_>, [<ProjectionParameter>] s1KeySelector : _ -> _,[<ProjectionParameter>] s2KeySelector : _ -> _,[<ProjectionParameter>] resultSelector : _ -> _) = s1.Join(s2,new Func<_,_>(s1KeySelector),new Func<_,_>(s2KeySelector),new Func<_,_,_>(resultSelector))
    [<CustomOperation("groupJoin", AllowIntoPattern=true)>]
    member __.GroupJoin (s1:IObservable<_>,s2:IObservable<_>, [<ProjectionParameter>] s1KeySelector : _ -> _,[<ProjectionParameter>] s2KeySelector : _ -> _,[<ProjectionParameter>] resultSelector : _ -> _) = s1.GroupJoin(s2,new Func<_,_>(s1KeySelector),new Func<_,_>(s2KeySelector),new Func<_,_,_>(resultSelector))
    [<CustomOperation("zip", IsLikeZip=true)>]
    member __.Zip (s1:IObservable<_>,s2:IObservable<_>,[<ProjectionParameter>] resultSelector : _ -> _) = s1.Zip(s2,new Func<_,_,_>(resultSelector))
    //[<CustomOperation("forkJoin", IsLikeZip=true)>]
    //member __.ForkJoin (s1:IObservable<_>,s2:IObservable<_>,[<ProjectionParameter>] resultSelector : _ -> _) = s1.ForkJoin(s2,new Func<_,_,_>(resultSelector))
    [<CustomOperation("iter")>]
    member __.Iter(s:IObservable<_>, [<ProjectionParameter>] selector : _ -> _) = s.Do(selector)
*)

let rxquery = RxQueryBuilder()

[<Tests>]
let tests =
    testList "queries" [
        test "query supports F# types with headOrDefault" {
            let actual =
                query {
                    for x in Seq.empty<TestRec> do
                    headOrDefault
                }
            Expect.equal actual (Unchecked.defaultof<TestRec>) "Expected default value of TestRec"
        }

        test "query supports F# types with headOrNone" {
            let actual =
                query {
                    for x in Seq.empty<TestRec> do
                    headOrNone
                }
            Expect.equal actual None "Expected None"
        }

        test "query exactlyOneOrNone returns the single value for a seq with one element" {
            let source = seq { yield { Value = 1 } }
            let actual =
                query {
                    for x in source do
                    exactlyOneOrNone
                }
            Expect.equal actual (Seq.tryHead source) "Expected { Value = 1 }"
        }

        test "query exactlyOneOrNone returns None for an empty seq" {
            let source = Seq.empty<TestRec>
            let actual =
                query {
                    for x in source do
                    exactlyOneOrNone
                }
            Expect.equal actual None "Expected None"
        }

        test "query exactlyOneOrNone returns None for a seq with more than one element" {
            let source = seq { yield { Value = 1 }; yield { Value = 2 } }
            let actual =
                query {
                    for x in source do
                    exactlyOneOrNone
                }
            Expect.equal actual None "Expected None"
        }

        test "rxquery can select values from a source" {
            let expected = [|1..10|]
            let actual = Array.zeroCreate<int> 10
            let source = Observable.Range(1, 10)
            use disp =
                rxquery {
                    for x in source do
                    select x
                }
                |> Observable.subscribe (fun i -> actual.[i - 1] <- i)
            Expect.equal actual expected "Expected observable to populate empty array with selected values"
        }

        test "rxquery can return the first value from a source" {
            let mutable actual : int = -1
            let source = Observable.Range(1, 10)
            use disp =
                rxquery {
                    for x in source do
                    head
                }
                |> Observable.subscribe (fun i -> actual <- i)
            Expect.equal actual 1 "Expected head to return 1"
        }

        test "rxquery can return the single value from a source of one element" {
            let mutable actual : int = -1
            let source = Observable.Return(1)
            use disp =
                rxquery {
                    for x in source do
                    head
                }
                |> Observable.subscribe (fun i -> actual <- i)
            Expect.equal actual 1 "Expected exactlyOne to return 1"
        }

        test "rxquery can filter an observable with where" {
            let expected = [|1..5|]
            let actual = Array.zeroCreate<int> 5
            let source = Observable.Range(1, 10)
            use disp =
                rxquery {
                    for x in source do
                    where (x <= 5)
                    select x
                }
                |> Observable.subscribe (fun i -> actual.[i - 1] <- i)
            Expect.equal actual expected "Expected where to filter input"
        }

        test "rxquery can group an observable" {
            let expected = [|"a";"b"|]
            let actual = ResizeArray<string>()
            let source =
                Observable.Generate(1,
                    (fun x -> x < 10),
                    (fun x -> x + 1),
                    (fun x ->
                        if x < 2 then "a", x
                        else "b", x))
            use disp =
                rxquery {
                    for (k, _) in source do
                    groupBy k into g
                    select g.Key
                }
                |> Observable.subscribe actual.Add
            Expect.equal (actual.ToArray()) expected "Expected where to filter input"
        }

        test "rxquery can join two observables" {
            let expected = [|3;4;5|]
            let actual = ResizeArray<int>()
            let source1 = Observable.Range(1, 5)
            let source2 = Observable.Range(3, 5)
            use disp =
                rxquery {
                    for x in source1 do
                    join y in source2 on (x = y)
                    select x
                }
                |> Observable.subscribe actual.Add
            Expect.equal (actual.ToArray()) expected "Expected join to produce [|3;4;5|]"
        }
    ]
