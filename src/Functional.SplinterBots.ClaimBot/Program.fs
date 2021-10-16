open Functional.SplinterBots
open Config 

let private logToConsole user message context = 
    async {
        printfn "[%s]: %s - %s" (System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) user message
    }

let runIfConfigAllows check action =
    match check with 
    | true -> action
    | _ -> [||]

let transferResourceFromOneAccount config transferDetail= 
    let log = logToConsole transferDetail.username
    let page = Splinterland.getPage config.browser.headless |> Async.RunSynchronously
    try 
        try 
            [|
                Splinterland.loadSplinterlands()
                Splinterland.login log transferDetail
                Splinterland.closePopUp()
                Splinterland.transferDec log transferDetail
                Splinterland.transferSPS log transferDetail
                runIfConfigAllows 
                    config.transferCards 
                    (Splinterland.transferCards log transferDetail) 
                //runIfConfigAllows claimWeekly (Splinterland.claimWeeklyRewards)
                //runIfConfigAllows claimSeason (Splinterland.claimSeasonRewards)
                Splinterland.logout()
            
            |] |> Seq.concat |> Splinterland.runActions page
        with 
            | :? System.Exception as exp -> printfn $"{exp.Message}"
    finally
        Splinterland.close() |> Splinterland.runActions page

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
