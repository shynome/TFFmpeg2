module Store

open System.IO
open Thoth.Json.Net
open Shared

type StoreState = {
    Root: string
    SelectedVideos: string []
    Tab: Tab
}

let getStoreState () =
    let root = System.Environment.GetEnvironmentVariable("HOME")
    let defaultValue = {
        Root = root
        SelectedVideos = [||]
        Tab = Explorer
    }
    try
        let a = File.ReadAllText("state.json")
        let s = Decode.Auto.fromString<StoreState>(a)
        match s with
        | Ok s -> s
        | _ -> defaultValue
    with e ->
        defaultValue

let saveStoreAgent = MailboxProcessor<Model>.Start(fun inbox ->
    let rec messageLoop () = async {
        let! model = inbox.Receive()
        try
            let videos = model.SelectedVideos |> Array.map (fun v -> v.Path)
            let s = {
                Root = model.Root
                SelectedVideos = videos
                Tab = model.Tab
            }
            let s = Encode.Auto.toString(0,s)
            File.WriteAllText("state.json",s)
        with e ->
            printf "%A" e
        return! messageLoop()
    }
    messageLoop()
)

let saveStoreState (model:Model) = saveStoreAgent.Post model