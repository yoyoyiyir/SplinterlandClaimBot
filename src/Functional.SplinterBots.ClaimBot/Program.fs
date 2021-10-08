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

let trasferUserResources transferDetails = 
    let page = Splinterland.getPage() |> Async.RunSynchronously

    [|
        Splinterland.loadSplinterlands()
        Splinterland.login transferDetails
        Splinterland.closePopUp()
        Splinterland.transferDec transferDetails
        Splinterland.transferSPS transferDetails
        //Splinterland.transferCards
    |] |> Seq.concat |> Splinterland.runActions page
    
    Splinterland.close page

[<EntryPoint>]
let main(args) =
    let test =
        getConfiguration args
        |> Array.item 0
    trasferUserResources test 
    printfn "Finished running SplinterBots"
    0
