module WereMethods

open ANSIConsole
open System
open System.Text.RegularExpressions
open System.IO

if not (ANSIInitializer.Init false) then ANSIInitializer.Enabled <- false;

type Arguments = 
    {
        filename: string
        action: string
        word: string
        case: bool
        path: string option
    }

let printUsage = 
    eprintfn "%A - count, delete, or replace all occurences of a specific string in a text file" ("were".Color(ConsoleColor.Green))
    eprintfn "Usage: %A <filename> <action> <word> <case> <output-path>" ("were".Color(ConsoleColor.Yellow))
    eprintfn "%A: the name of the text file you want %A to process" ("<filename>".Color(ConsoleColor.Green)) ("were".Color(ConsoleColor.Yellow))
    eprintfn """%A choices:
           'count' to count all occurences
           'delete' to delete all occurences
           'replace' to replace all occurences with something else""" ("<action>".Color(ConsoleColor.Green))
    eprintfn "%A: the word or regular expression that %A will use" ("<word>".Color(ConsoleColor.Green)) ("were".Color(ConsoleColor.Yellow))
    eprintfn """%A: tells %A whether to be case-insensitive or match the case of you word.
             choices are 'false' for case-insensitive, and 'true' for the opposite.""" ("<case>".Color(ConsoleColor.Green)) ("were".Color(ConsoleColor.Yellow))
    eprintfn "%A: The path to the directory of the output file or an existing text file only for the %A and %A actions" ("<case>".Color(ConsoleColor.Green))

let parseArgs (args: list<string>): Arguments =
    if args.Length <> 4 then
        printUsage
        failwithf "%A wrong number of arguments, expected 4" ("Error:".Color(ConsoleColor.Red))
    {
        filename = args[0]
        action = args[1]
        word = args[2]
        case = bool.Parse args[3]
        path = Some args[4] 
    }

let executeAction (args: Arguments) =
    let regexActions (exprToMatch: Regex) (input : string) (action: string) =
        match action with
        | "count" -> $"{exprToMatch.Matches(input).Count} occurences of the expression {exprToMatch}"
        | "delete" -> 
            
        
    let deleteAll exprToMatch (input: string) =
        Regex.Replace()
    let rgxExpr = Regex args.word
    let fileContents = File.ReadAllText args.filename
    match args.action.ToLower() with
    | "count" ->
        $"{countMatches rgxExpr fileContents} occurences of the expression {rgxExpr}"
    | "delete" -> ""
    | _ -> ""

        