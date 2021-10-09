﻿namespace Functional.SplinterBots

module Splinterland =

    open System
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

    let (|LowerThanMinimal|_|) input =
        if input < 0.001 then 
            Some true
        else
            None

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

    let close context = 
        context |> closeBrowser

    let login (config: TransferDetails) = 
        [|
            clickBySelector "#log_in_button > button"
            typeBySelector "#email" config.username
            typeBySelector "#password" config.password
            pressKey Keys.Enter 
        |]

    let logout () =
        [|
            evaluate "SM.Logout();"
        |]
        
    let closePopUp () =
        [| 
            pressKey Keys.Escape 
        |]

    let private approvePayment selector password context =
        async {
            do! Logger.logger "Open dialog" context
            do! closeConfirmationDialogWhenAppear() context              
            do! clickBySelector selector context
            do! Logger.logger "waiting for dialog approval" context
            do! Logger.logger "Continue with sending the DEC" context
            do! typeBySelector "#active_key" password context
            do! Logger.logger "Password provided now time for accept" context
            do! clickBySelector "#approve_tx" context
            do! pressKey Keys.Escape context 
            ()
        }
    let private round number =
        Math.Floor(1000.0M * number) / 1000.0M

    let private sentDec config context =
        let transferDecToUser username password (amount: double) context = 
            async {
                do! evaluate "SM.ShowDialog('dec_info');" context
                do! typeBySelector "#dec_amount" (amount.ToString("0.000")) context
                do! selectOptionBySelector "#dec_wallet_type" "player" context
                do! typeBySelector "input[name=playerName]" username context
                do! approvePayment "#transfer_out_btn" password context
                return ()
            }
        async {
            do! evaluate "SM.ShowDialog('dec_info');" context

            let! decAmount = 
                readValueFromSelector "#game_balance" ReadValuesKeys.GameBalance context 

            let amount = double(decAmount) |> round 
            let donationAmount = 
                match (amount * 0.01) with
                | 0.0 -> 0.0
                | LowerThanMinimal amount -> 0.001
                | _ as x -> x
            let userAmount = amount - donationAmount

            if donationAmount > 0.0 then 
                do! transferDecToUser "assassyn" config.password donationAmount context
            if userAmount > 0.0 then 
                do! transferDecToUser config.destinationAccount config.password userAmount context

            do! pressKey Keys.Escape context
            return ()
        }
    let transferDec config =
        [|
            sentDec config 
        |]
        
    let private sentSPS config context =
        let transferSPS user password (amount: double) context =
            async {
                do! evaluate "SM.ShowDialog('sps_management/transfer');" context
                do! typeBySelector "#sps_transfer_amount" (amount.ToString("0.000")) context
                do! selectOptionBySelector "#transfer_dest_select" "player" context
                do! typeBySelector "#txtPlayerToSend" user context
                do! approvePayment "#btnTransferOut" password context
                do! pressKey Keys.Escape context 
                return ()
            }
        async {
            let! _ = evaluate "SM.ShowHomeView('sps_management');" context
        
            let! spsAmount = 
                readValueFromSelector "#player_ingame_value" ReadValuesKeys.GameBalance context 

            let amount = double(spsAmount) |> round 
            let donationAmount = 
                match (amount * 0.01) with
                | 0.0 -> 0.0
                | LowerThanMinimal amount -> 0.001
                | _ as x -> x
            let userAmount = amount - donationAmount

            if donationAmount > 0.0 then 
                do! transferSPS "assassyn" config.password donationAmount context
            if userAmount  > 0.0 then 
                do! transferSPS config.destinationAccount config.password userAmount context
                
            do! pressKey Keys.Escape context

            return ()
        }   
    let transferSPS transferDetails =
        [|
            evaluate "SM.ShowHomeView('sps_management');"
            closeConfirmationDialogWhenAppear()
            clickBySelector "#claim_btn_hive"
            pressKey Keys.Escape
            clickBySelector "#claim_btn_binance"
            pressKey Keys.Escape
            clickBySelector "#claim_btn_wax"
            pressKey Keys.Escape
            clickBySelector "#claim_btn_eth"
            pressKey Keys.Escape
            clickBySelector "#claim_btn_steem"
            pressKey Keys.Escape
            clickBySelector "#claim_btn_tron"
            pressKey Keys.Escape
            sentSPS transferDetails
        |]
