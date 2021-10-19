namespace Functional.SplinterBots

module DEC = 
    open Browser
    open Config
    open Commons
    open SplinterBotsMath
    open Types
    
    let private transferDecToUser username password (amount: decimal) context = 
        async {
            do! evaluate "SM.ShowDialog('dec_info');" context
            do! ``type`` "#dec_amount" (amount.ToString("0.000")) context
            do! selectOption "#dec_wallet_type" "player" context
            do! ``type`` "input[name=playerName]" username context
            do! approvePayment "#transfer_out_btn" password context
        }
    let private sentDec username password percentage context: Async<SplinterlandStatus> =
        async {
            do! evaluate "SM.ShowDialog('dec_info');" context
            let! result = readValue "#game_balance" context 
            let transferAmount = result |> calculateAmounts percentage

            if transferAmount >= 0.001M then 
                do! transferDecToUser username password transferAmount context
                do! pressKey Keys.Escape context 

                return DECTransfered (username, transferAmount)
            else
                return NoDECToTransfer
        }
    let transferDec config =
        [|
            message DECTransfeStarted
            sentDec donateAccountName config.activeKey 0.001M
            sentDec config.destinationAccount config.activeKey 1M
            message DECTransferFinished
        |]
        
module SPS = 
    open Browser
    open Config
    open Commons
    open SplinterBotsMath
    open Types

    let private  transferSPSToUser user password (amount: decimal) context =
        async {
            do! evaluate "SM.ShowDialog('sps_management/transfer');" context
            do! waitForASecond context
            do! ``type`` "#sps_transfer_amount" (amount.ToString("0.000")) context
            do! selectOption "#transfer_dest_select" "player" context
            do! ``type`` "#txtPlayerToSend" user context
            do! approvePayment "#btnTransferOut" password context
            do! waitForASecond context
        }
    let private sentSPS username password percentage context =
        async {
            do! evaluate "SM.ShowHomeView('sps_management');" context
        
            let! result = 
                readValue "#player_ingame_value" context 
            
            let transferAmount = result |> calculateAmounts percentage

            if transferAmount >= 0.001M then 
                do! transferSPSToUser username password transferAmount context
                do! pressKey Keys.Escape context 

                return SPSTransfered (username, transferAmount)
            else
                return NoSPSToTransfer
        }   
    let transferSPS config =
        [|
            message SPSTransfeStarted
            evaluate "SM.ShowHomeView('sps_management');" >+ ClaimSPS
            closeConfirmationDialogWhenAppear >+ ClaimSPS
            click "#claim_btn_hive" >+ ClaimSPS
            pressKey Keys.Escape >+ ClaimSPS

            sentSPS donateAccountName config.activeKey 0.001M
            sentSPS config.destinationAccount config.activeKey 1M
            message SPSTransferFinished
        |]
