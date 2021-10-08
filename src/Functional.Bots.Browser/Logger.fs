module Logger

let logger message context =
    async {
        printfn message
        return ()
    }