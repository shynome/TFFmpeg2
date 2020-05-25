module Explorer

open System.IO
open Fable.RemoteData

let private filterStartWithDot (p:string) =
    let p = Path.GetFileName p
    not (p.StartsWith ".")

let getFileEntries dir =
    try
        let dirs = Directory.GetDirectories dir |> Array.filter filterStartWithDot
        let files = Directory.GetFiles dir |> Array.filter filterStartWithDot
        Success (dirs, files)
    with e ->
        Failure (e.ToString())
