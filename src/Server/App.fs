module App

open Elmish
open Shared
open Store
open Fable.RemoteData

let syncClient = Cmd.ofMsg Sync'

let getMetadata file =
    async {
        let! metadata = FFmpeg.getVideMetatdata file
        return { Path =file; Metadata = metadata; Progress = Ready; Option = None }
    }

/// Elmish init function with a channel for sending client messages
/// Returns a new state and commands
let init clientDispatch () =
    let s = getStoreState ()
    let model = {
        Root = s.Root
        Files = NotAsked
        SelectedVideos = [||]
        Tab = s.Tab
        EditVideo = None
    }
    let initModel (state:Model) =
        async {
            try
                let! videos =
                    s.SelectedVideos
                    |> Array.map getMetadata
                    |> Async.Parallel
                let files = Explorer.getFileEntries state.Root
                let model = {
                    state with
                        SelectedVideos = videos;
                        Files = files
                }
                return model
            with e ->
                printf "%A" e
                return model
        }
    model, Cmd.OfAsync.perform initModel model SetModel

/// Elmish update function with a channel for sending client messages
/// Returns a new state and commands
let update clientDispatch msg model =
    match msg with
    | Sync' ->
        clientDispatch (SyncModel model)
        saveStoreState model
        model, Cmd.none
    | SetModel model ->
        clientDispatch (SyncModel model)
        model, Cmd.none
    | SetTab tab ->
        {model with Tab = tab}, syncClient
    | GetFiles root ->
        let getFiles () =
            async {
                return Explorer.getFileEntries root
            }
        {model with Root = root; Files = Loading}, Cmd.OfAsync.perform getFiles () SetFiles
    | SetFiles files ->
        { model with Files = files }, syncClient
    | SelectFile file ->
        let video = { Path =file; Metadata = Loading; Progress = NotReady; Option = None }
        let videos = Array.append model.SelectedVideos [|video|]

        let cmd = Cmd.batch [
            syncClient
            Cmd.OfAsync.perform getMetadata file SetVideo
        ]
        {model with SelectedVideos = videos}, cmd
    | SetVideo video ->
        let videos = model.SelectedVideos |> Array.map (fun v ->
            if v.Path <> video.Path then v else
            video
        )
        {model with SelectedVideos = videos}, syncClient
    | SetEditVideo video ->
        {model with EditVideo = video}, syncClient
    | SaveEditVideo video ->
        model, Cmd.batch [
            Cmd.ofMsg (SetVideo video)
            Cmd.ofMsg (SetEditVideo None)
        ]
    | UnSelectFile file ->
        let videos = model.SelectedVideos |> Array.filter (fun v -> v.Path <> file)
        {model with SelectedVideos = videos}, syncClient
// ServerMsg handle
    | Transform v ->
        let s = FFmpeg.transformVideo v
        let v = { v with Progress = s.Value }
        let cmd = Cmd.batch [
            Cmd.ofMsg (SetVideo v)
            Cmd.ofMsg (SubVideoProgress (v,s))
        ]
        model, cmd
    | SubVideoProgress (v, s) ->
        let v = { v with Progress = s.Value }
        let wait' (v,s) =
            async {
                do! Async.Sleep 1000
                return (v,s)
            }
        let cmd =
            match s.Value with
            | Finished | Error _ -> Cmd.none
            | _ -> Cmd.OfAsync.perform wait' (v,s) SubVideoProgress
        let cmd = Cmd.batch [
            Cmd.ofMsg (SetVideo v)
            cmd
        ]
        model, cmd
    | CancelTransform v ->
        FFmpeg.cancelTransformVideo v.Path
        model, Cmd.none
