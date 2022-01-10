module WereMethods

open ANSIConsole
open System
open System.Text.RegularExpressions
open System.IO
open Argu
open HelperMethods

type Arguments =
    | [<Mandatory>] FileName of filname:string
    | Count of expr:string
    | Delete of expr:string
    | Replace of expr:string * replacement:string
    | Path of path:string

    interface IArgParserTemplate with
        member s.Usage = 
            match s with
            | FileName _ -> "specify a text file"
            | Count _ -> "specify an expression"
            | Delete _ -> "specify an expression"
            | Replace _ -> "specify an expression and a replacement"
            | Path _ -> "specify a working directory to place the output file"

let parser = ArgumentParser.Create<Arguments>(programName = "were.exe")

let usage = parser.PrintUsage()

let parseArgs args =
    let results = parser.Parse (args)
    results.GetAllResults()

let exeuteActions (results: ParseResults<Arguments>) =
    let data = File.ReadAllText (results.GetResult FileName)

    let determineAction =
        if containsMoreThan results [Delete; Count; Replace] 2 then
            failwith "Cannot use --delete, --count, or --replace together. Choose one."
        elif noneOfElements results [Delete; Count; Replace] then   
            failwith "At most one of --delete, --count, or --replace is mandatory"
        else    
            findItem

    let rgxExpr = Regex (results.GetResult Expr)

    let writeToFile (path: string) (data: string) =
        use FileStream fs = File.Create(path) 
        let info = UTF8Encoding(true).GetBytes(data)
        fs.WriteAllBytes(info, 0, info.Length)

    let actionReturn =
        ()