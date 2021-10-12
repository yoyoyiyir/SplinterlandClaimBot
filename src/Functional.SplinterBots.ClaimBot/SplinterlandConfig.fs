namespace Functional.SplinterBots

open Microsoft.Extensions.Configuration

module Config = 

    type DestinationAccountName = string
    type Username = string
    type Password = string
    
    type UserConfig () = 
        member val username = "" with get,set
        member val masterPassword = "" with get,set

    type TransferDetails = 
       {
           destinationAccount: DestinationAccountName 
           username: Username 
           password: Password
       }
    module TransferDetails = 
        let bind destinationAccount (loginDetails: UserConfig) = 
           {
               destinationAccount = destinationAccount
               username = loginDetails.username
               password = loginDetails.masterPassword
           }

    type Browser () = 
        member val headless = true with get,set

    type SplinterBotsCofig = 
        {
            browser: Browser
            sentCards: bool
            claimWeeklyReward: bool
            claimSeasonReward: bool            
            transferDetails: TransferDetails array
        }

    let getConfiguration (args: string array)  =
        new ConfigurationBuilder()
            |> fun config -> JsonConfigurationExtensions.AddJsonFile(config, "config.json")
            |> fun config -> CommandLineConfigurationExtensions.AddCommandLine(config, args)
            |> fun config -> config.Build()
            |> fun config -> 
                let browserSettings = config.GetSection("browser").Get<Browser>()
                let sentCards = config.GetValue<bool>("sentCards")
                let claimWeeklyReward = config.GetValue<bool>("claimWeeklyReward")
                let claimSeasonReward = config.GetValue<bool>("claimSeasonReward")                
                let transferSettigs = 
                    let sentTo = config.GetValue<string>("sentTo")
                    config.GetSection("accounts").Get<UserConfig array>()
                    |> Array.map (fun userInfo -> TransferDetails.bind sentTo userInfo)
                {
                    browser = browserSettings
                    sentCards = sentCards
                    claimWeeklyReward = claimWeeklyReward
                    claimSeasonReward = claimSeasonReward
                    transferDetails = transferSettigs
                }
