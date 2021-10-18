namespace Functional.SplinterBots

module ActionRunner =

    open Types
    open Pholly
    open Retry

    let private executeAction (context: Context) (action: Context -> Async<'a>) =
        action context |> Async.RunSynchronously |> ignore

    let runActions context (actions: seq<Context -> Async<'a>>)  =
        let execute = executeAction context
        actions |> Seq.iter execute
