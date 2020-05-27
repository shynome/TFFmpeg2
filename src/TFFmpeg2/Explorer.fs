module Explorer

open System.IO
open Fable.RemoteData

let private filterStartWithDot (p:string) =
    let p = Path.GetFileName p
    not (p.StartsWith ".")

let normalizePath (p:string) = p.Replace(@"\","/")

let getFileEntries dir =
    try
        let dirs =
            Directory.GetDirectories dir
            |> Array.filter filterStartWithDot
            |> Array.map normalizePath
        let files =
            Directory.GetFiles dir
            |> Array.filter filterStartWithDot
            |> Array.map normalizePath
        Success (dirs, files)
    with e ->
        Failure (e.ToString())
