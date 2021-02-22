namespace FolderAnalysisFSLib

open System.Collections.Generic
open FolderAnalysisTools

[<Class>]
type FolderAnalysisBase(path) =
    let _dirTreeHistory = new Stack<Dir>()

    member private this.CheckIsCreated() = 
        if _dirTreeHistory.Count = 0 then
            raise <| new NotAnalyzedYetException("Not analyzed yet.")

    member this.StartAnalysis() = 
        if _dirTreeHistory.Count = 0 then
            _dirTreeHistory.Push(dirTree path)

    member this.RedirectTo(dirName) =
        this.CheckIsCreated()

        let peeked =
            _dirTreeHistory.Peek().DirList |> List.tryFind (fun item -> item.Info.Name = dirName)

        match peeked with
        | Some(dirTree) -> 
            _dirTreeHistory.Push(dirTree)
            true
        | None -> false

    member this.Back() =
        this.CheckIsCreated()

        if _dirTreeHistory.Count > 1 then
            _dirTreeHistory.Pop() |> ignore
            true
        else
            false

    member this.GetPrintableTree(depthLimt, dirExpLimt, fileExpLimt) = 
        this.CheckIsCreated()

        printableTree (_dirTreeHistory.Peek()) depthLimt dirExpLimt fileExpLimt
        |> List.toArray

    member private this.GetList(list, top, num) =
        this.CheckIsCreated()

        if top then
            _dirTreeHistory.Peek() |> list |> topSize num |> List.toArray
        else
            _dirTreeHistory.Peek() |> list |> bottomSize num |> List.toArray

    member this.GetDirList(top, num) = 
        this.GetList(dirList, top, num)

    member this.GetFileList(top, num) = 
        this.GetList(fileList, top, num)
