open System
open WereMethods
open HelperMethods

[<EntryPoint>]
let main args =
    let results = parser.Parse args
    printfn "%s" (executeActions results)
    0