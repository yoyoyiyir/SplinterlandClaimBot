namespace Functional.SplinterBots

module ActionRunner =

    open Types
    open Pholly
    open Retry
    open Commons

    let private executeAction context handleMessages action: unit =
        let result = action context |> Async.RunSynchronously 
        handleMessages result

    let runActions context handleMessages (actions: seq<Context -> Async<'a>>)  =
        let execute = executeAction context handleMessages 
        actions |> Seq.iter execute
