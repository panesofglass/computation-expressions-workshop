module Probs
let rec gcd left right =
    match left, right with
    | l, r when left = right ->
        left
    | v, z
    | z, v when z = 0 ->
        v
    | l, r when l % 2 = 0 && r % 2 = 0 ->
        2 * gcd (l / 2) (r / 2)
    | o, e
    | e, o when o % 2 = 1 && e % 2 = 0 ->
        gcd (e / 2) o
    | l, r ->
        gcd (abs (l - r)) (min l r)

type Frac =
    { num : int
      denom : int }
    static member Simplify { num = n; denom = d } =
        { num = n / gcd n d
          denom = d / gcd n d }
    static member (+) (left, right) =
        { num = left.num * right.denom + right.num * left.denom
          denom = left.denom * right.denom }
        |> Frac.Simplify
    static member (*) (left, right) =
        { num = left.num * right.num
          denom = left.denom * right.denom }
        |> Frac.Simplify
    static member Zero =
        { num = 0
          denom = 1 }
    override x.ToString() =
        sprintf "%d/%d" x.num x.denom

type Spread<'t> = ('t * Frac) list

module Spread =
    let ofList xs =
        let denom = List.length xs
        xs
        |> List.map (fun x -> x, { num = 1; denom = denom })

    let certain v =
        [v, { num = 1; denom = 1 }]

    let ofWeightedList xs =
        let denom = List.sumBy snd xs
        xs
        |> List.map (fun (x, w) -> x, { num = w; denom = denom })

let diceRoll faces : Spread<int> =
    [1..faces]
    |> Spread.ofList

type Face =
    | Heads
    | Tails

let fairCoin : Spread<Face> =
    Spread.ofList [Heads;Tails]

let unfairCoin heads tails =
    [(Heads, heads);(Tails, tails)]
    |> Spread.ofWeightedList

let private applyParent prob spread =
    spread
    |> List.map (fun (v, w) -> v, w * prob)

let totalByResultFold (spread : Spread<'a>) =
    let folder map (result, frac) =
        match Map.tryFind result map with
        | Some v -> Map.add result (frac + v) map
        | None -> Map.add result frac map
    List.fold folder (Map.empty) spread
    |> Map.toList

let totalByResultFold' (spread : Spread<'a>) : Spread<'a> =
    spread
    |> List.groupBy fst
    |> List.map (fun (key, probs) ->
                     let totalProb =
                        probs
                        |> List.fold
                              (fun totalSoFar prob -> (snd prob) + totalSoFar)
                              // (fun totalSoFar (_, prob) -> prob + totalSoFar)
                              { num = 0; denom = 1 }
                     key, totalProb)

let totalByResultSum (spread : Spread<'a>) : Spread<'a> =
    spread
    |> List.groupBy fst
    |> List.map (fun (key, probs) ->
                     let totalProb =
                        probs
                        |> List.sumBy snd
                     key, totalProb)

let (>>=) (m : Spread<'a>) (f : 'a -> Spread<'b>) : Spread<'b> =
    m
    |> List.collect (fun (value, prob) -> f value |> applyParent prob)
    |> totalByResultSum

let return_ x =
    Spread.certain x

// *** Examples ***
let r =
    diceRoll 20
    >>= (fun i -> if i < 4 then return_ "hit" else return_ "miss")

let thing =
    fairCoin
    >>= (function Heads -> r | Tails -> return_ "miss")

let thing2 =
    unfairCoin 15 5
    >>= (function Heads -> thing | Tails -> return_ "wat?")

// *** Readable output ***
open System
open System.Text

let displaySpread (spread : Spread<'a>) : string =
    let withStringCats =
        spread
        |> List.map (fun (c, w) -> sprintf "%A" c, w.ToString())
    let longestCat =
        withStringCats
        |> List.map (fst >> String.length)
        |> List.max
    let longestNumber =
        withStringCats
        |> List.map (snd >> String.length)
        |> List.max
    let sb = StringBuilder()
    let append (str : string) =
        sb.Append str |> ignore
    let writeLine (cat : string, weight : string) =
        append "| "
        append (cat.PadRight(longestCat))
        append " | "
        append (weight.PadLeft(longestNumber))
        append " |"
        sb.AppendLine() |> ignore
    List.iter writeLine withStringCats
    sb.ToString()

displaySpread thing2

// *** Example set 2 ***
type RainOrDry =
    | Rain
    | Dry

let rainingNow =
    [Rain, 1; Dry, 5]
    |> Spread.ofWeightedList

let bringUmbrella currentRain =
    match currentRain with
    | Rain ->
        [true, 10; false, 1]
        |> Spread.ofWeightedList
    | Dry ->
        [true, 1; false, 5]
        |> Spread.ofWeightedList

let rainingLater currentRain =
    match currentRain with
    | Rain ->
        [Rain, 2; Dry, 1]
        |> Spread.ofWeightedList
    | Dry ->
        [Dry, 2; Rain, 1]
        |> Spread.ofWeightedList


type ISpreadBuilder =
    abstract Bind<'a, 'b when 'a : equality and 'b : equality> :
        Spread<'a> * ('a -> Spread<'b>) -> Spread<'b>
    abstract Return : 'a -> Spread<'a>

type ProbBuilder() =
    member __.Bind(m, f) =
        m >>= f
    member __.Return x =
        return_ x
    member __.ReturnFrom x = x
    member this.Zero() =
        this.Return()
    interface ISpreadBuilder with
        member x.Bind(m : Spread<'a>, f) : Spread<'b> =
            x.Bind(m, f)
        member x.Return a =
            x.Return a

let prob = ProbBuilder()

let dryLater (chance : ISpreadBuilder) =
    chance {
        let! isRaining =
            [Rain, 1; Dry, 5]
            |> Spread.ofWeightedList
        let! umbrellaGrabbed =
            bringUmbrella isRaining
        let! willRainLater =
            rainingLater isRaining
        match umbrellaGrabbed, willRainLater with
        | true, Rain ->
            return "little damp"
        | true, Dry ->
            return "left umbrella at work"
        | false, Rain ->
            return "very wet"
        | false, Dry ->
            return "nice walk"
    }

displaySpread <| dryLater prob
