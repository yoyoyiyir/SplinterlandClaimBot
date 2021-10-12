module Browser

open Support
open PuppeteerSharp
open System.Threading.Tasks
open System

let DefaulWwaitTime = 500  * 0

type Keys =
    | Enter
    | Escape
    | Tab

type Context = Page
    
let private handleTaskOf handle context =
    async {
        let! _ = handle context |> Async.AwaitTask
        Support.waitForPageActions context
    }
let private handleTask (handle: Context -> Task) context =
    async {
        let! _ = handle context |> Async.AwaitTask
        Support.waitForPageActions context
    }
let private handleTaskOfWithSelector selector handle (context: Context) =
    async {
        let! element = context.WaitForSelectorAsync(selector) |> Async.AwaitTask
        let! _ = handle element |> Async.AwaitTask
        Support.waitForPageActions context
    }
let private handleTaskWithSelector selector (handle: ElementHandle -> Task) (context: Context) =
    async {
        let! element = context.WaitForSelectorAsync(selector) |> Async.AwaitTask
        do! handle element |> Async.AwaitTask
        Support.waitForPageActions context
    }

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

let readValueFromSelector selector propertyKey (context: Context) = 
    async {
        let! value = context.WaitForSelectorAsync(selector) |> Async.AwaitTask
        let! valueInJson = value.EvaluateFunctionAsync("el => el.textContent") |> Async.AwaitTask
        let result = valueInJson.ToString()

        return result
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
