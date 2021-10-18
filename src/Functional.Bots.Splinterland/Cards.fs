namespace Functional.SplinterBots

module Cards =

    open Browser
    open Config
    open Commons
    
    let transferCards log config = 
        let runTransfer context = 
            async {
                let! cardIds = 
                    readProperty "div.card" "id"  context

                for cardId in cardIds do 
                    do! click "li#menu_item_collection a" context 
                    do! log $"transfering card {cardId}" context
                    do! click $"#{cardId}" context 
                    do! click "#check_all" context 
                    do! click "#btn_send" context 
                    do! ``type`` "#recipient" config.destinationAccount context
                    do! approvePayment "#btn_send_popup_send" config.activeKey context
                    do! click "#btn_back_collection" context
                return ()
            }
        [|
            log  "Checking cards to transfer"
            //evaluate "SM.ResetFilters(); SM.ShowCollection();"
            click "li#menu_item_collection a"
            selectOption "#filter-owned" "owned"
            runTransfer
            log "finished card trasnfer"
        |]
