namespace Functional.SplinterBots

module Splinterland =

    open Browser
    open Config
    open Types

    type Log = string -> Context -> Async<unit>

    let donateAccountName = "splinterbots"

    let getPage headless =
        getBrowser headless
        |> getNewPage

    let runActions (page:Context) (actions: seq<Context -> Async<'a>>)  =
        actions 
        |> Seq.iter (fun action -> action page |> Async.RunSynchronously |> ignore)

    let loadSplinterlands () = 
        [|
            setViewPortSize 1200 1500
            goTo "https://splinterlands.com/"
        |]

    let close () = 
        let closeBorwser (context: Context) = context.Browser |> closeBrowser
        [|
            closeBorwser
        |]

    let login log (config: TransferDetails) = 
        [|
            log $"Trying to log in ..."
            clickBySelector "#log_in_button > button"
            typeBySelector "#email" config.username
            typeBySelector "#password" config.postingKey
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

    let private sentDec log config context =
        let transferDecToUser username password (amount: decimal) context = 
            async {
                do! evaluate "SM.ShowDialog('dec_info');" context
                do! waitForASecond context
                do! typeBySelector "#dec_amount" (amount.ToString("0.000")) context
                do! selectOptionBySelector "#dec_wallet_type" "player" context
                do! typeBySelector "input[name=playerName]" username context
                do! approvePayment log "#transfer_out_btn" password context
                do! waitForASecond context
                return ()
            }
        async {
            do! evaluate "SM.ShowDialog('dec_info');" context

            let! result = readValueBySelector "#game_balance" context 
            do! waitFor5Seconds context
            let donationAmount, userAmount = result |> SplinterBotsMath.calculateAmounts

            if userAmount > 0.001M then 
                do! log $"Transfering DEC" context

                do! log $"Sent creator donation of {donationAmount} DEC" context
                do! transferDecToUser donateAccountName config.activeKey donationAmount context

                do! log $"Sent credits to main account of {userAmount} DEC" context
                do! transferDecToUser config.destinationAccount config.activeKey userAmount context
            else
                do! log "Amount is zero no transfer today" context

            do! pressKey Keys.Escape context
            
            return ()
        }
    let transferDec log config =
        [|
            log "Checking DEC"
            sentDec log config 
            log "Transfer all applicable DEC"
        |]
        
    let private sentSPS log config context =
        let transferSPS user password (amount: decimal) context =
            async {                
                do! evaluate "SM.ShowDialog('sps_management/transfer');" context
                do! waitForASecond context
                do! typeBySelector "#sps_transfer_amount" (amount.ToString("0.000")) context
                do! selectOptionBySelector "#transfer_dest_select" "player" context
                do! typeBySelector "#txtPlayerToSend" user context
                do! approvePayment log "#btnTransferOut" password context
                do! waitForASecond context
                return ()
            }
        async {
            do! evaluate "SM.ShowHomeView('sps_management');" context
        
            let! result = 
                readValueBySelector "#player_ingame_value" context 
            do! waitFor5Seconds context
            let donationAmount, userAmount = result |> SplinterBotsMath.calculateAmounts

            if userAmount > 0.001M then 
                do! log $"Transfering SPS" context

                do! log $"Sent creator donation of {donationAmount} SPS" context
                do! transferSPS donateAccountName config.activeKey donationAmount context
                
                do! log $"Sent credits to main account of {userAmount} SPS" context
                do! transferSPS config.destinationAccount config.activeKey userAmount context
            else
                do! log "Amount is zero no transfer today" context
            
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
            log "All SPS claimed"
            waitForASecond

            log "Checking SPS"
            sentSPS log transferDetails
            log "Transfer all applicable SPS"
        |]

    let transferCards log config = 
        let runTransfer context = 
            async {
                let! cardIds = 
                    readMultiplePropertyValueBySelector "div.card" "id"  context
                for cardId in cardIds do 
                    do! log $"transfering card {cardId}" context
                    do! clickBySelector $"#{cardId}" context 
                    do! clickBySelector "#check_all" context 
                    do! clickBySelector "#btn_send" context 
                    do! typeBySelector "#recipient" config.destinationAccount context
                    do! approvePayment log "#btn_send_popup_send" config.activeKey context
                    do! clickBySelector "#btn_back_collection" context
                return ()
            }
        [|
            log  "Checking cards to transfer"
            //evaluate "SM.ResetFilters(); SM.ShowCollection();"
            clickBySelector "li#menu_item_collection a"
            selectOptionBySelector "#filter-owned" "owned"
            runTransfer
            log "finished card trasnfer"
        |]
