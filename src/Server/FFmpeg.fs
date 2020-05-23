module FFmpeg

open System.IO
open FFmpeg.NET
open Fable.RemoteData
open Shared
open System.Threading
open FSharp.Control.Reactive

let internalFFmpegPath () =
    let basedir = System.AppDomain.CurrentDomain.BaseDirectory
    let basedir = basedir + "external-bin/"
    if not (Directory.Exists basedir) then "" else
    let dirs = Directory.GetDirectories(basedir)
    let dir = dirs.[0]
    dir

let getFFmpegTryPaths () =
    let pwd = Directory.GetCurrentDirectory()
    seq {
        yield internalFFmpegPath()
        yield pwd
        yield Path.Combine(pwd, "external-bin/")
    }

let FFmpegBin =
    let dirs = getFFmpegTryPaths ()
    let dirs =
        dirs
        |> Seq.filter (fun d -> d <> "")
        |> Seq.map (fun d -> Path.Combine(d, "ffmpeg"))
    let dir = dirs |> Seq.tryFind (fun path->File.Exists(path))
    match dir with
    | Some path -> path
    | None -> ""

let getVideMetatdata (file:string) =
    async {
        let f = Engine FFmpegBin
        let mutable err = "no metadata"
        f.Error.Add(fun e -> err <- e.Exception.ToString())
        let! a =
            MediaFile file
            |> f.GetMetaDataAsync
            |> Async.AwaitTask
        return
            if isNull a then Failure err else
            let d2 = a.Duration.Ticks
            let d = a.Duration.ToString()
            let d = d.Split(".").[0]
            let size = a.VideoData.FrameSize.Split("x")
            let (w,h) = int size.[0],int size.[1]
            Success { Width = w; Height = h; Duration = d; Duration2 = d2; }
    }

let getOutpath (f:string) =
    let filename = Path.GetFileName f
    let dir = Path.GetDirectoryName f
    let outdir = Path.Combine(dir, "t-video-out")
    Directory.CreateDirectory outdir |> ignore
    Path.Combine(outdir, filename)

let mutable cts = Map.empty<string, CancellationTokenSource * System.Reactive.Subjects.BehaviorSubject<Progress>>

let cancelTransformVideo path =
    let c = cts.TryFind path
    match c with
    | Some (t, s) ->
        cts <- cts.Remove path
        s.OnNext ("手动取消" |> Error)
        s.OnCompleted ()
        t.Cancel()
    | None -> ()

let transformVideo (v:Video) =
    let s = Subject<Progress>.behavior Ready
    let t = new CancellationTokenSource()
    let f = Engine FFmpegBin
    let d =
        match v.Metadata with
        | Success m -> float m.Duration2
        | _ -> float 1
    f.Complete.Add <| fun _ ->
        s.OnNext Finished
        s.OnCompleted ()
    f.Error.Add <| fun e ->
        let e = e.Exception
        s.OnNext (e.ToString() |> Error)
        s.OnError e
    f.Progress.Add <| fun e ->
        let p = float e.ProcessedDuration.Ticks
        let p = p / d
        let p = p * 100.0
        let p = System.Math.Round(p, 2)
        s.OnNext (Progress p)

    let outpath = getOutpath v.Path

    let cmd =
        let cmd = "-y -pix_fmt yuv420p"
        cmd
    let cmd = sprintf "-i %s %s %s" v.Path cmd outpath

    f.ExecuteAsync (cmd, t.Token) |> ignore
    cts <- cts.Add (v.Path, (t,s))
    s
