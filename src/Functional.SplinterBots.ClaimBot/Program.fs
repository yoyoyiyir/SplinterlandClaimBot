open Microsoft.Extensions.Configuration
open Functional.SplinterBots
open Functional.SplinterBots.Splinterland

let getConfiguration (args: string array)  =
    new ConfigurationBuilder()
        |> fun config -> JsonConfigurationExtensions.AddJsonFile(config, "config.json")
        |> fun config -> CommandLineConfigurationExtensions.AddCommandLine(config, args)
        |> fun config -> config.Build()
        |> fun config -> 
            let sentTo = config.GetValue<string>("sentTo")
            config.GetSection("accounts").Get<UserConfig array>()
                |> Array.map (fun userInfo -> TransferDetails.bind sentTo userInfo)

let private logToConsole user message context = 
    async {
        printfn "[%s]: %s - %s" (System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")) user message
    }

let transferResourceFromOneAccount page transferDetail = 
    let log = logToConsole transferDetail.username
    try 
        [|
            Splinterland.loadSplinterlands()
            Splinterland.login log transferDetail
            Splinterland.closePopUp()
            Splinterland.transferDec transferDetail
            Splinterland.transferSPS transferDetail
            //Splinterland.transferCards
            Splinterland.logout()
        |] |> Seq.concat |> Splinterland.runActions page
    with 
    | :? System.Exception as exp -> printfn $"{exp.Message}"

let trasferUserResources transferDetails = 
    let page = Splinterland.getPage() |> Async.RunSynchronously
    
    transferDetails
    |> Array.iter (transferResourceFromOneAccount page)
    
    Splinterland.close page |> Async.RunSynchronously

[<EntryPoint>]
let main(args) =
    do getConfiguration args |> trasferUserResources
    printfn "Finished running SplinterBots"
    0
