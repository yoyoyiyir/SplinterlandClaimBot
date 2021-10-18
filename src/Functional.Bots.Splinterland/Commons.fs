namespace Functional.SplinterBots

module Commons =
    open Browser
    open Types

    type Log = string -> Context -> Async<unit>

    let donateAccountName = "splinterbots"

    let approvePayment selector password context =
        async {
            do! closeConfirmationDialogWhenAppear context              
            do! clickBySelector selector context
            do! typeBySelector "#active_key" password context
            do! clickBySelector "#approve_tx" context
            do! pressKey Keys.Escape context 
            ()
        }

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

    let calculateAmounts (amount: string) = 
        let amount = decimal(amount) - 0.001M |> round 
        let donationAmount = 
            match (amount * 0.01M) with
            | 0.0M -> 0.0M
            | LowerThanMinimal amount -> 0.001M
            | _ as x -> x
        let userAmount = amount - donationAmount
        (donationAmount, userAmount)
