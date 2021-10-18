namespace Functional.SplinterBots

module Splinterland =

    open Browser
    open Config
    open Types

    let getPage headless =
        getBrowser headless
        |> getNewPage

    let loadSplinterlands () = 
        [|
            setViewPortSize 1200 1500
            goTo "https://splinterlands.com/"
        |]

    let close () = 
        let closeBorwser (context: Context) = context.Browser |> closeBrowser
        [|
            closeBorwser
        |]

    let login log (config: TransferDetails) = 
        [|
            log $"Trying to log in ..."
            click "#log_in_button > button"
            ``type`` "#email" config.username
            ``type`` "#password" config.postingKey
            pressKey Keys.Enter 
            log $"User logged"            
        |]

    let logout () =
        [|
            evaluate "SM.Logout();"
            waitFor5Seconds
        |]
        
    let closePopUp () =
        [| 
            pressKey Keys.Escape 
        |]
