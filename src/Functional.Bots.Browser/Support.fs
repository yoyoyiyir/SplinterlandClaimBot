module Support

open PuppeteerSharp

let inline notNull value = not (obj.ReferenceEquals(value, null))
let defaultWaitTime = 10 * 100

let (|NotNull|_|) value = 
  if obj.ReferenceEquals(value, null) then None 
  else Some()

let whenElementExists selector (existHandler: ElementHandle -> Async<unit>) (page: Page) =
    async {
        let! element = page.QuerySelectorAsync(selector) |> Async.AwaitTask

        match element with
        | NotNull -> 
            let! _ = existHandler element
            ()
        | _ -> ()
    }

let waitForPageActions (page: Page) = 
    page.WaitForTimeoutAsync(defaultWaitTime) 
    |> Async.AwaitTask 
    |> Async.RunSynchronously

module String =
    let toString item =
        item.ToString()