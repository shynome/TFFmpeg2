module Explorer

open System.IO
open Fable.RemoteData

let getFileEntries dir =
    try
        let dirs = Directory.GetDirectories dir
        let files = Directory.GetFiles dir
        Success (dirs, files)
    with e ->
        Failure (e.ToString())
