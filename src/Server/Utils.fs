module Utils

open System.IO
open System

let private internalBinPath () =
    let basedir = AppDomain.CurrentDomain.BaseDirectory
    let basedir = basedir + "external-bin/"
    if not (Directory.Exists basedir) then ""
    else basedir

let private getBinTryPaths () =
    let pwd = Directory.GetCurrentDirectory()
    seq {
        yield internalBinPath()
        yield pwd
        yield Path.Combine(pwd, "external-bin/")
    }

let getBin name =
    let dirs = getBinTryPaths ()
    let dirs =
        dirs
        |> Seq.filter (fun d -> d <> "")
        |> Seq.map (fun d -> Path.Combine(d, name))
    let dir = dirs |> Seq.tryFind (fun path->File.Exists(path))
    match dir with
    | Some path -> path
    | None -> ""
