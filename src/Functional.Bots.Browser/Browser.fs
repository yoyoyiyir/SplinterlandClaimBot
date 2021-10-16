module Browser

open Support
open Types
open PuppeteerSharp
open System

let getBrowser headless = 
    async {
        let browserFetcher = new BrowserFetcher(new BrowserFetcherOptions())
        browserFetcher.DownloadAsync() 
            |> Async.AwaitTask 
            |> Async.RunSynchronously
            |> ignore
        let options = new LaunchOptions (Headless = headless)
        return! (Puppeteer.LaunchAsync(options) |> Async.AwaitTask)
    }

let getNewPage (browser: Async<Browser>) = 
    async {
        let! browser = browser
        let! page = browser.NewPageAsync() |> Async.AwaitTask
        return page
    }

let closeBrowser (context: Browser) =
    async {
        do!  context.CloseAsync() |> Async.AwaitTask
    }

let closePage (context: Context) =
    context |> handleTask (fun ctx -> ctx.CloseAsync())

let goTo url (context: Context) = 
    context |> handleTaskOf (fun ctx -> ctx.GoToAsync(url))

let setViewPortSize width height (context: Context) = 
    let options = new ViewPortOptions (Width = width, Height = height)
    context |> handleTask (fun ctx -> ctx.SetViewportAsync(options))

let clickBySelector selector (context: Context) =
    context |> handleTaskWithSelector selector (fun ctx -> ctx.ClickAsync())

let selectOptionBySelector selector optionName (context: Context) = 
    let values = [| String.toString optionName |]
    context |> handleTaskOfWithSelector selector (fun ctx -> ctx.SelectAsync(values))

let typeBySelector selector text (context: Context) =
    context |> handleTaskWithSelector selector (fun ctx -> ctx.TypeAsync(text))

let readValueBySelector selector (context: Context) = 
    async {
        let! value = context.WaitForSelectorAsync(selector) |> Async.AwaitTask
        let! valueInJson = value.EvaluateFunctionAsync("el => el.textContent") |> Async.AwaitTask
        let result = valueInJson.ToString()

        return result
    }

let readMultiplePropertyValueBySelector selector property (context: Context) = 
    async {
        let! elements = context.QuerySelectorAllAsync selector |> Async.AwaitTask
        let results = 
            elements 
            |> Seq.map (fun ctx -> ctx.GetPropertyAsync property |> Async.AwaitTask |> Async.RunSynchronously)
            |> Seq.map (fun prop -> prop.RemoteObject.Value.ToString())
            |> Array.ofSeq
        return results
    }

let pressKey (key: Keys) (context: Context) =
    context |> handleTask (fun ctx -> ctx.Keyboard.PressAsync(key.ToString()))

let evaluate javascript (context: Context) =
    context |> handleTaskOf (fun ctx -> ctx.EvaluateExpressionAsync(javascript))

let closeConfirmationDialogWhenAppear (context: Context) =
    evaluate "window.confirm = () => true" context

let waitForXSeconds timeout (context: Context) =
    let timeoutInMiliseconds = int(TimeSpan.FromSeconds(timeout).TotalMilliseconds)
    context |> handleTask ( fun ctx -> ctx.WaitForTimeoutAsync(timeoutInMiliseconds))
let waitFor5Seconds (context: Context) =
    waitForXSeconds 5.0 context
let waitForASecond (context: Context) =
    waitForXSeconds 1.0 context
