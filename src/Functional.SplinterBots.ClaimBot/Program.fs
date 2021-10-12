open Functional.SplinterBots
open Config 

let private logToConsole user message context = 
    async {
        printfn "[%s]: %s - %s" (System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) user message
    }

//let private runIfConfigAllows shouldRun actions =
//    match shouldRun with 
//    | true -> actions
//    | _ -> [||]

let transferResourceFromOneAccount page transferCards claimWeekly claimSeason transferDetail= 
    let log = logToConsole transferDetail.username
    try 
        [|
            Splinterland.loadSplinterlands()
            Splinterland.login log transferDetail
            Splinterland.closePopUp()
            Splinterland.transferDec log transferDetail
            Splinterland.transferSPS log transferDetail
            //runIfConfigAllows transferCards (Splinterland.transferCards)
            //runIfConfigAllows claimWeekly (Splinterland.claimWeeklyRewards)
            //runIfConfigAllows claimSeason (Splinterland.claimSeasonRewards)
            Splinterland.logout()
        |] |> Seq.concat |> Splinterland.runActions page
    with 
    | :? System.Exception as exp -> printfn $"{exp.Message}"

let trasferUserResources config = 
    let page = Splinterland.getPage config.browser.headless |> Async.RunSynchronously
    let transferResources = 
        transferResourceFromOneAccount page config.sentCards config.claimWeeklyReward config.claimSeasonReward
    
    config.transferDetails
    |> Array.iter transferResources
    
    Splinterland.close page.Browser |> Async.RunSynchronously

[<EntryPoint>]
let main(args) =
    let config = Config.getConfiguration args
    trasferUserResources config
    printfn "Finished running SplinterBots"
    0
