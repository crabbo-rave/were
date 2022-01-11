open System
open WereMethods
open HelperMethods

[<EntryPoint>]
let main args =
    let results = parser.Parse args
    printfn "%A" (exeuteActions results)
    0