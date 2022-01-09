open System
open WereMethods

[<EntryPoint>]
let main args =
    printfn "%A" (executeAction (parseArgs (Array.toList args)))
    0