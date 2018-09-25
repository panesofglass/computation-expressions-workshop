module Queries

open System
open System.Linq
open System.Reactive.Linq
open System.Reactive.Concurrency
open Microsoft.FSharp.Linq
open Sequences
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

/// An Observable computation builder.
type ObservableBuilder() =
    member __.Bind(m: IObservable<_>, f: _ -> IObservable<_>) = m.SelectMany(f)
    member __.Combine(comp1, comp2) = Observable.Concat(comp1, comp2)
    member __.Delay(f: _ -> IObservable<_>) = Observable.Defer(fun _ -> f())
    member __.Zero() = Observable.Empty(Scheduler.CurrentThread :> IScheduler)
    member __.For(sequence, body) = Observable.For(sequence, Func<_,_> body)
    member __.TryWith(m: IObservable<_>, h: #exn -> IObservable<_>) = Observable.Catch(m, h)
    member __.TryFinally(m, compensation) = Observable.Finally(m, Action compensation)
    member __.Using(res: #IDisposable, body) = Observable.Using((fun () -> res), Func<_,_> body)
    member __.While(guard, m: IObservable<_>) = Observable.While(Func<_> guard, m)
    member __.Yield(x) = Observable.Return(x, Scheduler.CurrentThread)
    member __.YieldFrom m : IObservable<_> = m

let observe = ObservableBuilder()

/// A reactive query builder.
/// See http://mnajder.blogspot.com/2011/09/when-reactive-framework-meets-f-30.html
type RxQueryBuilder() =
    member __.For (s:IObservable<_>, body : _ -> IObservable<_>) = s.SelectMany(body)
    [<CustomOperation("select", AllowIntoPattern=true)>]
    member __.Select (s:IObservable<_>, [<ProjectionParameter>] selector : _ -> _) = s.Select(selector)
    [<CustomOperation("where", MaintainsVariableSpace=true, AllowIntoPattern=true)>]
    member __.Where (s:IObservable<_>, [<ProjectionParameter>] predicate : _ -> bool ) = s.Where(predicate)
    [<CustomOperation("takeWhile", MaintainsVariableSpace=true, AllowIntoPattern=true)>]
    member __.TakeWhile (s:IObservable<_>, [<ProjectionParameter>] predicate : _ -> bool ) = s.TakeWhile(predicate)
    [<CustomOperation("take", MaintainsVariableSpace=true, AllowIntoPattern=true)>]
    member __.Take (s:IObservable<_>, count: int) = s.Take(count)
    [<CustomOperation("skipWhile", MaintainsVariableSpace=true, AllowIntoPattern=true)>]
    member __.SkipWhile (s:IObservable<_>, [<ProjectionParameter>] predicate : _ -> bool ) = s.SkipWhile(predicate)
    [<CustomOperation("skip", MaintainsVariableSpace=true, AllowIntoPattern=true)>]
    member __.Skip (s:IObservable<_>, count: int) = s.Skip(count)
    member __.Zero () = Observable.Empty(Scheduler.CurrentThread :> IScheduler)
    member __.Yield (value) = Observable.Return(value)
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
    ]
