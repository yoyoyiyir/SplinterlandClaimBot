namespace Functional.SplinterBots

module DEC = 
    open Browser
    open Config
    open Commons
    open SplinterBotsMath
    open Types
    
    let private transferDecToUser username password (amount: decimal) context = 
        async {
            try
                do! evaluate "SM.ShowDialog('dec_info');" context
                do! waitForASecond context
                do! typeBySelector "#dec_amount" (amount.ToString("0.000")) context
                do! selectOptionBySelector "#dec_wallet_type" "player" context
                do! typeBySelector "input[name=playerName]" username context
                do! approvePayment "#transfer_out_btn" password context
                do! waitForASecond context
            finally
                ()
        }
    let private sentDec log config context =
        async {
            do! evaluate "SM.ShowDialog('dec_info');" context

            let! result = readValueBySelector "#game_balance" context 
            do! waitFor5Seconds context
            let donationAmount, userAmount = result |> calculateAmounts

            if userAmount > 0.001M then 
                do! log $"Transfering DEC" context
                
                do! log $"Sent credits to main account of {userAmount} DEC" context
                do! transferDecToUser config.destinationAccount config.activeKey userAmount context

                do! log $"Sent creator donation of {donationAmount} DEC" context
                do! transferDecToUser donateAccountName config.activeKey donationAmount context
            else
                do! log "Amount is zero no transfer today" context

            do! pressKey Keys.Escape context
        }
    let transferDec log config =
        [|
            log "Checking DEC"
            sentDec log config 
            log "Transfer all applicable DEC"
        |]
        
module SPS = 
    open Browser
    open Config
    open Commons
    open SplinterBotsMath
    open Types

    let private  transferSPSToUser user password (amount: decimal) context =
        async {
            try
                do! evaluate "SM.ShowDialog('sps_management/transfer');" context
                do! waitForASecond context
                do! typeBySelector "#sps_transfer_amount" (amount.ToString("0.000")) context
                do! selectOptionBySelector "#transfer_dest_select" "player" context
                do! typeBySelector "#txtPlayerToSend" user context
                do! approvePayment "#btnTransferOut" password context
                do! waitForASecond context
            finally
                ()
        }
    let private sentSPS log config context =
        async {
            do! evaluate "SM.ShowHomeView('sps_management');" context
        
            let! result = 
                readValueBySelector "#player_ingame_value" context 
            do! waitFor5Seconds context
            let donationAmount, userAmount = result |> calculateAmounts

            if userAmount > 0.001M then 
                do! log $"Transfering SPS" context

                do! log $"Sent credits to main account of {userAmount} SPS" context
                do! transferSPSToUser config.destinationAccount config.activeKey userAmount context
                
                do! waitFor5Seconds context

                do! log $"Sent creator donation of {donationAmount} SPS" context
                do! transferSPSToUser donateAccountName config.activeKey donationAmount context
            else
                do! log "Amount is zero no transfer today" context
            
            do! pressKey Keys.Escape context
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
