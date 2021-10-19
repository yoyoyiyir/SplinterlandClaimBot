namespace Functional.SplinterBots

module Commons =
    open System
    open Browser
    open Types

    let donateAccountName = "splinterbots"

    let approvePayment selector password context =
        async {
            do! closeConfirmationDialogWhenAppear context              
            do! click selector context
            do! ``type`` "#active_key" password context
            do! click "#approve_tx" context
            do! pressKey Keys.Escape context 
            ()
        }

    type SplinterlandStatus = 
        | Ignore
        | Error of string

        | BrowserInitialisation
        | SetUpBrowser
        | CloseBrowser
        | ClosePopup

        | LoginStarting
        | LoginInProgress of Username
        | UserIsLoggedIn of Username
        | Logout 

        | DECTransfeStarted
        | DECTransfered of Username * decimal
        | DECTransferFinished
        | NoDECToTransfer

        | ClaimSPS 
        | SPSTransfeStarted
        | SPSTransfered of Username * decimal
        | SPSTransferFinished
        | NoSPSToTransfer

        | ReTry of RetryCount * SplinterlandStatus

    let rec sentStausMessage message action context =
        async {
            try 
                let! result = action context 
                return message
            with 
                | :? Exception as ex -> 
                    match message with 
                    | ReTry (x, msg) when x > 3 -> return (Error ex.Message)
                    | ReTry (x, msg) -> return! sentStausMessage (ReTry (x + 1, msg)) action context
                    | _ -> return! sentStausMessage (ReTry (1, message)) action context
        }
    let (>+) action message =
        sentStausMessage message action

    let message message =
        sentStausMessage message (fun ctx -> async.Return(ctx))

module Repeater = 
    let retryWhenIncorectResult result action context = 
        async {
            let! actionResult = action context
            return () 
        }

module SplinterBotsMath = 
    open System

    let (|LowerThanMinimal|_|) input =
        if input < 0.001M then 
            Some true
        else
            None

    let round number =
        Math.Floor(1000.0M * number) / 1000.0M

    let calculateAmounts percentage (amount: string) = 
        let amount = decimal(amount) |> round 
        match (amount * percentage) with
        | 0.0M -> 0.0M
        | LowerThanMinimal amount -> 0.001M
        | _ as x -> x
        //let donationAmount = 
        //    match (amount * 0.01M) with
        //    | 0.0M -> 0.0M
        //    | LowerThanMinimal amount -> 0.001M
        //    | _ as x -> x
        //let userAmount = amount - donationAmount
        //(donationAmount, userAmount)
