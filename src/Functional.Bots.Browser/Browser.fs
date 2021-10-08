module Browser

open Support
open PuppeteerSharp
open System.Threading.Tasks

let DefaulWwaitTime = 100  * 0

type Keys =
    | Enter
    | Escape
    | Tab

type Context = Page
    
let handleTaskOf (handle: Context -> Task<'a>)  (context: Context) =
    async {
        let! _ = handle context |> Async.AwaitTask
        return ()
    }

let handleTask (handle: Context -> Task) (context: Context) =
    async {
        let! _ = handle context |> Async.AwaitTask
        return ()
    }

let updateContextAsync handle (context: Context)=
    async {
        let! result = handle context |> Async.AwaitTask

        return ()
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

let closeBrowser (context: Context) =
    async {
        let! _ =  context.Browser.CloseAsync() |> Async.AwaitTask
        return ()
    }

let goTo url (context: Context) = 
    async { 
        let! _ = context.GoToAsync(url) |> Async.AwaitTask
        return ()
    }
let setViewPortSize width height (context: Context) = 
    async {
        let viewportOptions = new ViewPortOptions (Width = width, Height = height)
        let! _ = context.SetViewportAsync(viewportOptions) |> Async.AwaitTask
        return ()
    }

let waitForPageToLoad (context: Context) =
    async {
        let options = new NavigationOptions (
            WaitUntil = [| WaitUntilNavigation.Networkidle2 |])
        let! _ = context.WaitForNavigationAsync(options) |> Async.AwaitTask
        return ()
    }

let clickBySelector selector (context: Context) =
    context |> handleTask (fun ctx -> ctx.ClickAsync(selector))

let selectOptionBySelector selector optionName (context: Context) = 
    context |> handleTaskOf (fun ctx -> ctx.SelectAsync(selector, [| String.toString optionName |]))

let typeBySelector selector text (context: Context) =
    context |> handleTask (fun ctx -> ctx.TypeAsync(selector, text))

let readValueFromSelector selector propertyKey (context: Context) = 
    async {
        let! value = context.WaitForSelectorAsync(selector) |> Async.AwaitTask
        let! valueInJson = value.EvaluateFunctionAsync("el => el.textContent") |> Async.AwaitTask
        let value = valueInJson.ToString()

        return value
    }

let pressKey (key: Keys) (context: Context) =
    context |> handleTask (fun ctx -> ctx.Keyboard.PressAsync(key.ToString()))

let evaluate javascript (context: Context) =
    context |> handleTaskOf (fun ctx -> ctx.EvaluateExpressionAsync(javascript))

let closeConfirmationDialogWhenAppear () =
    evaluate "window.confirm = () => true" 
    //context |> handleTaskOf (fun ctx -> )
