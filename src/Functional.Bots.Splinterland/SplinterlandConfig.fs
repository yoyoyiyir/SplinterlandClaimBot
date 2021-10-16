namespace Functional.SplinterBots

module Config = 

    open Microsoft.Extensions.Configuration
    open Microsoft.Extensions.Configuration.UserSecrets

    type DestinationAccountName = string
    type Username = string
    type PostingKey = string
    type ActiveKey = string
    
    type UserConfig () = 
        member val username = "" with get,set
        member val postingKey = "" with get,set
        member val activeKey = "" with get,set

    type TransferDetails = 
       {
           destinationAccount: DestinationAccountName 
           username: Username 
           activeKey: ActiveKey
           postingKey: PostingKey
       }
    module TransferDetails = 
        let bind destinationAccount (loginDetails: UserConfig) = 
           {
               destinationAccount = destinationAccount
               username = loginDetails.username
               activeKey = loginDetails.activeKey
               postingKey = loginDetails.postingKey
           }

    type Browser () = 
        member val headless = true with get,set

    type SplinterBotsCofig = 
        {
            browser: Browser
            transferCards: bool
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
                    transferCards = config.GetValue<bool> "sentCards"
                    claimWeeklyReward = config.GetValue<bool> "claimWeeklyReward" 
                    claimSeasonReward = config.GetValue<bool> "claimSeasonReward"
                    transferDetails = transferSettigs
                }
