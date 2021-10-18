namespace Functional.SplinterBots

module ClaimChest =
    open Browser
    open Config
    open Commons
    
    let claimWeeklyChest log config = 
        [|
            log  "Checking cards to transfer"
            click "li#menu_item_collection a"
            selectOption "#filter-owned" "owned"
            log "finished card trasnfer"
        |]
