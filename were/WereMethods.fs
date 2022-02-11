module WereMethods

open System.Text.RegularExpressions
open System.IO
open System
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
                        | Replace (_, _) -> boolToInt (containingList.Contains Replace)
                        | _ -> failwith "Error: Unknown action")
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
                    | _ -> failwith "Error: Unknown action"
    let filterFunc =
        function
        | Delete _ -> true
        | Count _ -> true
        | Replace (_, _) -> true
        | _ -> false
    
    let mapFunc =
        function
        | Delete x -> Regex x
        | Count x -> Regex x
        | Replace (x, y) -> Regex x
        | _ -> failwith "Error: Unknown action"
            
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
    
    let determineAction results =
        results
        |> List.filter filterFunc

    let zippedExprAndAction results = 
        let rgxList = determineAction results
                      |> List.map mapFunc
        List.zip results rgxList
    
    let doAction (input: string) (inputList: (Arguments * Regex) list)  : (int * unit) list =
        let write data = writeToFile (results.GetResult Path) data
        inputList 
        |> List.map (function 
                        | (Count _, expr) ->
                            (expr.Matches(input).Count, ())
                        | (Delete _, expr) ->
                            (0, (write (expr.Replace(input, ""))))
                        | (Replace (_, y), expr) ->
                            (0, (write (expr.Replace(input, y))))
                        | _ -> failwith "Error: Unknown action")
            
    let actionReturn (inputList: (Arguments * Regex) list) (data: string) =
        inputList
        |> doAction data
        |> List.iter (function
                        | (num, expr) when num > 0 ->
                            Console.WriteLine $"{num} occurences of the expression {expr}"
                        | (_, action) ->
                            action
                            Console.WriteLine $"Succesfully wrote to file {results.GetResult Path}"
                        | _ -> Console.WriteLine() )

    actionReturn (zippedExprAndAction (results.GetAllResults())) data