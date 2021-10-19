namespace Functional.SplinterBots

module Splinterland =

    open Browser
    open Commons
    open Config
    open Types

    let getPage headless =
        getBrowser headless
        |> getNewPage

    let loadSplinterlands () = 
        [|
            message BrowserInitialisation
            setViewPortSize 1200 1500 >+ SetUpBrowser
            goTo "https://splinterlands.com/" >+ SetUpBrowser
        |]

    let close () = 
        let closeBrowser (context: Context) = context.Browser |> closeBrowser
        [|
            closeBrowser >+ CloseBrowser
        |]

    let login (config: TransferDetails) = 
        [|
            message LoginStarting
            click "#log_in_button > button" >+ LoginInProgress config.username 
            ``type`` "#email" config.username >+ LoginInProgress config.username 
            ``type`` "#password" config.postingKey >+ LoginInProgress config.username 
            pressKey Keys.Enter >+ LoginInProgress config.username
            message (UserIsLoggedIn config.username)
        |]

    let logout () =
        [|
            evaluate "SM.Logout();" >+ Logout
            waitFor5Seconds >+ Ignore
        |]
        
    let closePopUp () =
        [| 
            pressKey Keys.Escape >+ ClosePopup
        |]
