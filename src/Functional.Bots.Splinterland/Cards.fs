namespace Functional.SplinterBots

module Cards =

    open Browser
    open Config
    open Commons
    
    let transferCards log config = 
        let runTransfer context = 
            async {
                let! cardIds = 
                    readMultiplePropertyValueBySelector "div.card" "id"  context

                for cardId in cardIds do 
                    do! clickBySelector "li#menu_item_collection a" context 
                    do! log $"transfering card {cardId}" context
                    do! clickBySelector $"#{cardId}" context 
                    do! clickBySelector "#check_all" context 
                    do! clickBySelector "#btn_send" context 
                    do! typeBySelector "#recipient" config.destinationAccount context
                    do! approvePayment "#btn_send_popup_send" config.activeKey context
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
