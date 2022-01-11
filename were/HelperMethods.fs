module HelperMethods

open Argu

let containsMoreThan (containingList: ParseResults<_>) listOfElements sumOfContaining =
    listOfElements
    |> List.sumBy (fun e -> if containingList.Contains e then 1 else 0 )
    |> (<=) sumOfContaining

let noneOfElements (containingList: ParseResults<_>) listOfElements =
    listOfElements
    |> List.sumBy (fun e -> if containingList.Contains e then 1 else 0 )
    |> (=) 0

let rec findItem (containingList: ParseResults<_>) listOfElements =
    match listOfElements with
    | [] -> failwith "Unexpected Error"
    | x::xs ->
        if containingList.Contains x then
            x
        else
            findItem containingList xs 