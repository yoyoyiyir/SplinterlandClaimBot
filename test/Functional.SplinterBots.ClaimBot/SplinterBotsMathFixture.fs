module Tests

open Xunit
open FsUnit.Xunit

let roundValues : obj[] seq = 
    seq { 
        yield [| 100M; 100M |] 
        yield [| 100.123; 100.123 |]
        yield [| 100.12356789; 100.123 |]
        yield [| 0.01001; 0.010 |]
        yield [| 0.99099; 0.990 |]
    }

[<Theory>]
[<MemberData("roundValues")>]
let ``Given decimal amount When round Then number is always nmo smaller than 3 numbers after dot `` (number: decimal) (expectedResult: decimal) = 
    number  
    |> SplinterBotsMath.round
    |> should equal expectedResult
