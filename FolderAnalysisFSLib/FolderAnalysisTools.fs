module internal FolderAnalysisFSLib.FolderAnalysisTools

open System.IO

type Info = { Name: string; Size: int64 }
(*
                Dir
        /        |        \
     Info     FileList   DirList
              /  |  \      |    \
           Info Info Info Dir   Dir  
*)
type Dir = { Info: Info; FileList: Info list; DirList: Dir list }

let dirTree basePath =
    let enumerationOption =
        new EnumerationOptions(IgnoreInaccessible = true, AttributesToSkip = FileAttributes.System)

    let rec dirTreeSafe path =
        let fileInfoList =
            Directory.GetFiles(path, "*", enumerationOption)
            |> Seq.map
                (fun file ->
                    let fileInfo = FileInfo(file)
                    { Name = fileInfo.Name; Size = fileInfo.Length })
            |> Seq.toList

        let dirList =
            [ for subDir in Directory.GetDirectories(path, "*", enumerationOption) ->
                dirTreeSafe subDir ]

        let sumSize =
            (fileInfoList |> List.fold (fun acc item -> acc + item.Size) 0L)
            + (dirList |> List.fold (fun acc item -> acc + item.Info.Size) 0L)

        { Info = { Name = path; Size = sumSize }; FileList = fileInfoList; DirList = dirList }

    basePath |> dirTreeSafe

// (Type string, Name string, Size string, Depth int) list
// F = Fold
// D = Dir
// FH = Fold hided
// DH = Dir hided
// DF = Dir folded
let printableTree dirTree depthLimt dirExpLimt fileExpLimt =
    let rec printableTreeTR dirTree depthLimt curDepth acc =
        let dirInfo = ("D", dirTree.Info.Name, dirTree.Info.Size, curDepth)

        let fileInfoList =
            let sortedFL = dirTree.FileList |> List.sortByDescending (fun item -> item.Size)

            let fileInfoRevL list =
                list
                |> List.map (fun info -> ("F", info.Name, info.Size, curDepth + 1))
                |> List.rev

            if fileExpLimt >= 0 && dirTree.FileList.Length > fileExpLimt then
                ("FH", $"%d{dirTree.FileList.Length - fileExpLimt}", 0L, curDepth + 1)
                :: (sortedFL.[0..(fileExpLimt - 1)] |> fileInfoRevL)
            else
                sortedFL |> fileInfoRevL

        let dirInfoRevL list =
            List.fold
                (fun ac item -> (printableTreeTR item (depthLimt - 1) (curDepth + 1) acc) @ ac)
                []
                list

        let infoAcc = fileInfoList @ (dirInfo :: acc)

        if depthLimt <> 0 then
            let sortedDL = dirTree.DirList |> List.sortByDescending (fun item -> item.Info.Size)

            if dirExpLimt >= 0 && dirTree.DirList.Length > dirExpLimt then
                (("DH", $"%d{dirTree.DirList.Length - dirExpLimt}", 0L, curDepth)
                 :: (sortedDL.[0..(dirExpLimt - 1)] |> dirInfoRevL))
                @ infoAcc
            elif dirTree.DirList.Length <> 0 then
                (sortedDL |> dirInfoRevL) @ infoAcc
            else
                infoAcc
        else if dirTree.DirList.Length > 0 then
            ("DF", $"%d{dirTree.DirList.Length}", 0L, curDepth) :: infoAcc
        else
            infoAcc

    printableTreeTR dirTree depthLimt 0 [] |> List.rev

let rec dirList dirTree =
    dirTree.Info :: List.foldBack (fun item acc -> (dirList item) @ acc) dirTree.DirList []

let rec fileList dirTree =
    (dirTree.FileList
     |> List.map (fun item -> { item with Name = Path.Combine(dirTree.Info.Name, item.Name) }))
    @ List.foldBack (fun item acc -> (fileList item) @ acc) dirTree.DirList []

let topSize number list =
    (List.sortByDescending (fun item -> item.Size) list).[0..(number - 1)]
    |> List.map (fun item -> (item.Name, item.Size))

let bottomSize number list = 
    (List.sortBy (fun item -> item.Size) list).[0..(number - 1)]
    |> List.map (fun item -> (item.Name, item.Size))