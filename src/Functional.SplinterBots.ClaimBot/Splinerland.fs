namespace Functional.SplinterBots

module Splinterland =

    open Browser

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

    type ReadValuesKeys =
        | GameBalance

    let getPage () =
        getBrowser false
        |> getNewPage

    let runActions (page:Context) (actions: seq<Context -> Async<unit>>)  =
        actions 
        |> Seq.iter (fun action -> action page |> Async.RunSynchronously |> ignore)

    let loadSplinterlands () = 
        [|
            setViewPortSize 1800 1500
            goTo "https://splinterlands.com/"
        |]

    //let close page = 
    //    (async.Return page)
    //    |> 
    //    |> Async.RunSynchronously

    let login (config: TransferDetails) = 
        [|
            clickBySelector "#log_in_button > button"
            typeBySelector "#email" config.username
            typeBySelector "#password" config.password
            pressKey Keys.Enter 
        |]
        
    let closePopUp () =
        [| 
            pressKey Keys.Escape 
        |]

    //let private confirmWalletTransaction password context =
    //    async {

    //    }

    let transferDecToUser username password percentage context = 
        async {
            let! _ =  evaluate "SM.ShowDialog('dec_info');" context

            let! decAmount = 
                readValueFromSelector "#game_balance" ReadValuesKeys.GameBalance context 
            let transferAmount = 
                (double(decAmount) * percentage)

            if transferAmount > 1.0 then 
                let! _ = typeBySelector "#dec_amount" (transferAmount.ToString("0.000")) context
                let! _ = selectOptionBySelector "#dec_wallet_type" "player" context
                let! _ = typeBySelector "input[name=playerName]" username context
                let! _ = Logger.logger "Open dialog" context
                let! _ = clickBySelector "#transfer_out_btn" context
                let! _ = closeConfirmationDialogWhenAppear() context            
                let! _ = Logger.logger "waiting for dialog approval" context
                let! _ = Logger.logger "Continue with sending the DEC" context
                let! _ = typeBySelector "#active_key" password context
                let! _ = Logger.logger "Password provided now time for accept" context
                let! _ = clickBySelector "#approve_tx" context
                ()

            let! _ = pressKey Keys.Escape context

            return ()
        }

    let transferDec config =
        [|
            transferDecToUser "assassyn" config.password 0.01
            transferDecToUser config.destinationAccount config.password 0.99
        |] 
     
    let claimSPS () =
        [|
            evaluate "SM.ShowHomeView('sps_management');"
            closeConfirmationDialogWhenAppear()
            clickBySelector "#claim_btn_hive"
            pressKey Keys.Enter
            clickBySelector "#claim_btn_binance"
            pressKey Keys.Enter
            clickBySelector "#claim_btn_wax"
            pressKey Keys.Enter
            clickBySelector "#claim_btn_eth"
            pressKey Keys.Enter
            clickBySelector "#claim_btn_steem"
            pressKey Keys.Enter
            clickBySelector "#claim_btn_tron"
            pressKey Keys.Enter
        |]

    let transferSPSToUser username password percentage context =
        async {
            let! _ = evaluate "SM.ShowHomeView('sps_management');" context

            let! spsAmount = 
                readValueFromSelector "#player_ingame_value" ReadValuesKeys.GameBalance context 
            let transferAmount = 
                (double(spsAmount) * percentage)

            //if transferAmount > 1.0 then 
            let! _ = evaluate "SM.ShowDialog('sps_management/transfer');" context
            let! _ = typeBySelector "#sps_transfer_amount" (transferAmount.ToString("0.000")) context
            let! _ = selectOptionBySelector "#transfer_dest_select" "player" context
            let! _ = typeBySelector "#txtPlayerToSend" username context
            let! _ = Logger.logger "Open dialog" context
            let! _ = clickBySelector "#btnTransferOut" context
            let! _ = closeConfirmationDialogWhenAppear() context            
            let! _ = Logger.logger "waiting for dialog approval" context
            let! _ = Logger.logger "Continue with sending the DEC" context
            let! _ = typeBySelector "#active_key" password context
            let! _ = Logger.logger "Password provided now time for accept" context
            let! _ = clickBySelector "#approve_tx" context
            ()

            let! _ = pressKey Keys.Escape context

            return ()
        }
