module FolderAnalysisFSLib.ByteMeasure

open System

let private kilo = 1024.0
let private bToKB byte = byte / kilo
let private (|ToUpper|)(measure: string) = measure.ToUpper()
let private round(num: float) = Math.Round(num, 2)

let rec byteToString measure (byte: int64) =
    let num = float byte

    match measure with
    | ToUpper "B" -> (num |> string) + " B"
    | ToUpper "KB" -> (num |> bToKB |> round |> string) + " KB"
    | ToUpper "MB" -> (num |> bToKB |> bToKB |> round |> string) + " MB"
    | ToUpper "GB" -> (num |> bToKB |> bToKB |> bToKB |> round |> string) + " GB"
    | ToUpper "AUTO" ->
        if num < kilo then 
            byteToString "B" byte
        elif num < kilo * kilo then 
            byteToString "KB" byte
        elif num < kilo * kilo * kilo then 
            byteToString "MB" byte
        else 
            byteToString "GB" byte
    | _ -> failwith "Failed with matching measure specified."
