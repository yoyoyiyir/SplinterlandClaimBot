namespace Functional.SplinterBots

module Splinterland =

    open System
    open Browser
    open Config

    type UserConfig() = 
        member val username = "" with get,set
        member val password = "" with get,set

    type DestinationAccountName = string
    type Username = string
    type Password = string

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
                password = loginDetails.password
            }

    type SplinterLandConfiguration = 
        {
            accounts: TransferDetails array
        }

    type SplinterLandConfiguration = 
        {
            accounts: TransferDetails array
        }

    type SplinterLandConfiguration = 
        {
            accounts: TransferDetails array
        }

    type SplinterLandConfiguration = 
        {
            accounts: TransferDetails array
        }

    type SplinterLandConfiguration = 
        {
            accounts: TransferDetails array
        }

    type SplinterLandConfiguration = 
        {
            accounts: TransferDetails array
        }

    type ReadValuesKeys =
        | GameBalance

    type Log = string -> Context -> Async<unit>
   
    let (|LowerThanMinimal|_|) input =
        if input < 0.001M then 
            Some true
        else
            None

    let getPage () =
        getBrowser false
        |> getNewPage

    let runActions (page:Context) (actions: seq<Context -> Async<'a>>)  =
        actions 
        |> Seq.iter (fun action -> action page |> Async.RunSynchronously |> ignore)

    let loadSplinterlands () = 
        [|
            setViewPortSize 1200 1500
            goTo "https://splinterlands.com/"
        |]

    let close context = 
        context |> closeBrowser

    let login log (config: TransferDetails) = 
        [|
            log $"Trying to log in ..."
            clickBySelector "#log_in_button > button"
            typeBySelector "#email" config.username
            typeBySelector "#password" config.password
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

    let private approvePayment log selector password context =
        async {
            do! closeConfirmationDialogWhenAppear context              
            do! clickBySelector selector context
            do! typeBySelector "#active_key" password context
            do! clickBySelector "#approve_tx" context
            do! pressKey Keys.Escape context 
            ()
        }
    let private round number =
        Math.Floor(1000.0M * number) / 1000.0M

    let private sentDec log config context =
        let transferDecToUser username password (amount: decimal) context = 
            async {
                do! evaluate "SM.ShowDialog('dec_info');" context
                do! typeBySelector "#dec_amount" (amount.ToString("0.000")) context
                do! selectOptionBySelector "#dec_wallet_type" "player" context
                do! typeBySelector "input[name=playerName]" username context
                do! approvePayment log "#transfer_out_btn" password context
                do! waitFor5Seconds context
                return ()
            }
        async {
            do! evaluate "SM.ShowDialog('dec_info');" context

            let! decAmount = 
                readValueFromSelector "#game_balance" ReadValuesKeys.GameBalance context 

            let amount = decimal(decAmount) |> round 
            let donationAmount = 
                match (amount * 0.01M) with
                | 0.0M -> 0.0M
                | LowerThanMinimal amount -> 0.001M
                | _ as x -> x
            let userAmount = amount - donationAmount

            do! log $"Transfer amount of DEC: {amount}" context

            if amount > 0.01M then 
                do! transferDecToUser "assassyn" config.password donationAmount context
                do! transferDecToUser config.destinationAccount config.password userAmount context

            do! pressKey Keys.Escape context
            
            return ()
        }
    let transferDec log config =
        [|
            log "Sending DEC..."
            sentDec log config 
        |]
        
    let private sentSPS log config context =
        let transferSPS user password (amount: decimal) context =
            async {                
                do! evaluate "SM.ShowDialog('sps_management/transfer');" context
                do! typeBySelector "#sps_transfer_amount" (amount.ToString("0.000")) context
                do! selectOptionBySelector "#transfer_dest_select" "player" context
                do! typeBySelector "#txtPlayerToSend" user context
                do! approvePayment log "#btnTransferOut" password context
                do! waitFor5Seconds context
                return ()
            }
        async {
            do! evaluate "SM.ShowHomeView('sps_management');" context
        
            let! spsAmount = 
                readValueFromSelector "#player_ingame_value" ReadValuesKeys.GameBalance context 

            let amount = decimal(spsAmount) |> round 
            let donationAmount = 
                match (amount * 0.01M) with
                | 0.0M -> 0.0M
                | LowerThanMinimal amount -> 0.001M
                | _ as x -> x
            let userAmount = amount - donationAmount

            do! log $"Transfer amount of SPS: {amount}" context

            if amount > 0.01M then 
                do! transferSPS "assassyn" config.password donationAmount context
                do! transferSPS config.destinationAccount config.password userAmount context
                
            do! pressKey Keys.Escape context

            return ()
        }   
    let transferSPS log transferDetails =
        [|
            log "Claiming SPS"
            evaluate "SM.ShowHomeView('sps_management');"
            closeConfirmationDialogWhenAppear
            clickBySelector "#claim_btn_hive"
            pressKey Keys.Escape
            waitFor5Seconds

            log "Sending SPS..."
            sentSPS log transferDetails
        |]
