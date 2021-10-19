module Types

type Keys =
    | Enter
    | Escape
    | Tab

type Context = PuppeteerSharp.Page

type Username = string

type RetryCount = int

type Log = string -> Context -> Async<unit>
