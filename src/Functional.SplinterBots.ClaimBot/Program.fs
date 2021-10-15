open Functional.SplinterBots
open Config 

let private logToConsole user message context = 
    async {
        printfn "[%s]: %s - %s" (System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) user message
    }

let transferResourceFromOneAccount config transferDetail= 
    let log = logToConsole transferDetail.username
    try 
        let page = Splinterland.getPage config.browser.headless |> Async.RunSynchronously
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
            Splinterland.close()
        |] |> Seq.concat |> Splinterland.runActions page
    with 
    | :? System.Exception as exp -> printfn $"{exp.Message}"

let trasferUserResources config = 
    let transferResources = 
        transferResourceFromOneAccount config
    
    config.transferDetails
    |> Array.iter transferResources

[<EntryPoint>]
let main(args) =
    let config = Config.getConfiguration args
    trasferUserResources config
    printfn "Finished running SplinterBots"
    0
