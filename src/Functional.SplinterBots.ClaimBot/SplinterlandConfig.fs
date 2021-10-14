namespace Functional.SplinterBots

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Configuration.UserSecrets

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

    let test: UserSecretsIdAttribute = new UserSecretsIdAttribute("")

    let getConfiguration (args: string array)  =
        let builder  = 
            new ConfigurationBuilder()
                |> fun config -> config.AddJsonFile "config.json"
                |> fun config -> config.AddCommandLine args
        let intermediate  = builder.Build()
        builder
            |> fun config -> config.AddUserSecrets(intermediate.["userSecretId"])
            |> fun config -> config.Build()
            |> fun config -> 
                let transferSettigs = 
                    let sentTo = config.GetValue<string>("sentTo")
                    config.GetSection("accounts").Get<UserConfig array>()
                    |> Array.map (fun userInfo -> TransferDetails.bind sentTo userInfo)
                {
                    browser = config.GetSection("browser").Get<Browser>()
                    sentCards = config.GetValue<bool> "sentCards"
                    claimWeeklyReward = config.GetValue<bool> "claimWeeklyReward" 
                    claimSeasonReward = config.GetValue<bool> "claimSeasonReward"
                    transferDetails = transferSettigs
                }
