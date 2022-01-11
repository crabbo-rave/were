open System
open WereMethods
open HelperMethods

[<EntryPoint>]
let main args =
    let results = parser.Parse [| "fsharp.txt"; |]
    printfn "%A" (noneOfElements results [Delete ""; Count ""; Replace ("", "")])
    0