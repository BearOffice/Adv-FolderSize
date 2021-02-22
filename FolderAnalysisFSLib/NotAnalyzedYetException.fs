namespace FolderAnalysisFSLib

open System

[<Class>]
type NotAnalyzedYetException =
    inherit Exception

    new() = { inherit Exception() }

    new(message) = { inherit Exception(message) }

    new(message, (innerException: Exception)) = { inherit Exception(message, innerException) }