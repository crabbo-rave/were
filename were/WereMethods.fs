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

    let writeToFile (path: string) (data: string) =
        try 
            use FileStream fs = File.Create(path) 
            let info = UTF8Encoding(true).GetBytes(data)
            fs.WriteAllBytes(info, 0, info.Length)
        with 
            | :? IOException -> failwith "Error writing to file"
    
    let determineAction =
        let listOfElements = [Delete (results.GetResult Delete); Count (results.GetResult Count); Replace (results.GetResult Replace)]
        if containsMoreThan results listOfElements 2 then
            failwith "Cannot use --delete, --count, or --replace together. Choose one."
        elif noneOfElements results listOfElements then   
            failwith "At most one of --delete, --count, or --replace is mandatory"
        else    
            findItem results listOfElements

    let rgxExpr = 
        match determineAction with
        | Delete x -> Regex x
        | Count x -> Regex x
        | Replace (x, y) -> Regex x
    
    let doAction (action: Arguments) (input: string) (expr: Regex) =
        match action with
        | Count x ->
            expr.Matches(input).Count
        | Delete x ->
            writeToFile (results.GetResult Path) (expr.Replace(input, ""))
        | Replace (x, y) ->
            writeToFile (results.GetResult Path) (expr.Replace(input, y))
            
    let actionReturn (action: Arguments) (expr: Regex) =
        match action with
        | Count x ->
            $"{doAction (Count x) data expr} occurences of the expression {x}"
        | Delete x ->
            doAction (Delete x) data expr
            $"Succesfully wrote to file {results.GetResult Path}"
        | Replace (x,y) ->
            doAction (Replace (x,y)) data expr
            $"Succesfully wrote to file {results.GetResult Path}"

    actionReturn