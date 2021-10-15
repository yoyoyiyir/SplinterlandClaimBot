module Support

open Types
open PuppeteerSharp
open System.Threading.Tasks

let waitForPageActions (page: Page) = 
    let defaultWaitTime = 10 * 100
    page.WaitForTimeoutAsync(defaultWaitTime) 
    |> Async.AwaitTask 
    |> Async.RunSynchronously

module String =
    let toString item =
        item.ToString()

let handleTaskOf handle context =
   async {
       let! _ = handle context |> Async.AwaitTask
       waitForPageActions context
   }
let handleTask (handle: Context -> Task) context =
   async {
       let! _ = handle context |> Async.AwaitTask
       waitForPageActions context
   }
let handleTaskOfWithSelector selector handle (context: Context) =
   async {
       let! element = context.WaitForSelectorAsync(selector) |> Async.AwaitTask
       let! _ = handle element |> Async.AwaitTask
       waitForPageActions context
   }
let handleTaskWithSelector selector (handle: ElementHandle -> Task) (context: Context) =
   async {
       let! element = context.WaitForSelectorAsync(selector) |> Async.AwaitTask
       do! handle element |> Async.AwaitTask
       waitForPageActions context
   }
