module WereMethods

open System.Text.RegularExpressions
open System.IO
open Argu

type Arguments =
    | [<Mandatory>] FileName of filname:string
    | [<Unique>] Count of expr:string
    | [<Unique>] Delete of expr:string
    | [<Unique>] Replace of expr:string * replacement:string
    | [<Unique>] Path of path:string

    interface IArgParserTemplate with
        member s.Usage = 
            match s with
            | FileName _ -> "specify a text file"
            | Count _ -> "specify an expression"
            | Delete _ -> "specify an expression"
            | Replace _ -> "specify an expression and a replacement"
            | Path _ -> "specify an existing or non-existing file to place the output (only needed for Delete and Replace commands)"

module HelperMethods =
    let boolToInt = function true -> 1 | false -> 0
    let containsMoreThan (containingList: ParseResults<_>) listOfElements sumOfContaining =
        listOfElements
        |> List.sumBy (function 
                        | Delete _ -> boolToInt (containingList.Contains Delete) 
                        | Count _ -> boolToInt (containingList.Contains Count)
                        | Replace (_, _) -> boolToInt (containingList.Contains Replace) )
        |> fun x -> (( x <= sumOfContaining) || x = 0)

    let rec findItem (containingList: ParseResults<_>) listOfElements =
        match listOfElements with
        | [] -> failwith "Unexpected Error"
        | x::xs -> match x with
                    | Delete x -> if containingList.Contains Delete then 
                                        Delete (containingList.GetResult Delete) 
                                  else 
                                        findItem containingList xs 
                    | Count x -> if containingList.Contains Count then 
                                        Count (containingList.GetResult Count) 
                                 else 
                                        findItem containingList xs 
                    | Replace (x, y) -> if containingList.Contains Replace then 
                                            Replace (containingList.GetResult Replace)
                                        else
                                            findItem containingList xs
            
            

open HelperMethods

let parser = ArgumentParser.Create<Arguments>(programName = "were.exe")

let usage = parser.PrintUsage()

let parseArgs args =
    let results = parser.Parse (args)
    results.GetAllResults()

let executeActions (results: ParseResults<Arguments>) =
    let data = File.ReadAllText (results.GetResult FileName)

    let writeToFile path data = 
        try
            File.WriteAllText (path, data)
        with 
            | :? IOException as e -> failwithf "IO Exception! %s" (e.Message)
    
    let determineAction =
        let listOfElements = [Delete ""; Count ""; Replace ("","")]
        if containsMoreThan results listOfElements 2 then
            failwith "There must be at least on of --delete, --count, or --replace, and no more of them together."
        else    
            findItem results listOfElements

    let rgxExpr = 
        match determineAction with
        | Delete x -> Regex x
        | Count x -> Regex x
        | Replace (x, y) -> Regex x
    
    let doAction (action: Arguments) (input: string) (expr: Regex) =
        let write data = writeToFile (results.GetResult Path) data
        match action with
        | Count x ->
            (expr.Matches(input).Count, ())
        | Delete x ->
            (0, (write (expr.Replace(input, ""))))
        | Replace (x, y) ->
            (0, (write (expr.Replace(input, y))))
            
    let actionReturn (action: Arguments) (expr: Regex) =
        match action with
        | Count x ->
            $"{fst (doAction (Count x) data expr)} occurences of the expression {x}"
        | Delete x ->
            snd (doAction (Delete x) data expr)
            $"Succesfully wrote to file {results.GetResult Path}"
        | Replace (x,y) ->
            snd (doAction (Replace (x,y)) data expr)
            $"Succesfully wrote to file {results.GetResult Path}"

    actionReturn determineAction rgxExpr