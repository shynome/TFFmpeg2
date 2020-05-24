open System.IO

open System
open Saturn
open Shared

open Elmish
open Elmish.Bridge
open System.Net.Sockets
open System.Net

let tryGetEnv key =
    match Environment.GetEnvironmentVariable key with
    | x when String.IsNullOrWhiteSpace x -> None
    | x -> Some x

let publicPath =
#if DEBUG
    Path.GetFullPath "../Client/deploy/www"
#else
    let basedir = System.AppDomain.CurrentDomain.BaseDirectory
    let basedir = System.IO.Path.Combine (basedir, "./www")
    Path.GetFullPath basedir
#endif

let getRandomPort () =
    let listener = TcpListener(IPAddress.Any, 0)
    listener.Start()
    let port = (listener.LocalEndpoint :?> IPEndPoint).Port
    listener.Stop()
    port

let port =
#if DEBUG
    "SERVER_PORT"
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us
#else
    getRandomPort()
#endif

/// Connect the Elmish functions to an endpoint for websocket connections
let webApp =
    Bridge.mkServer "/socket/init" App.init App.update
    |> Bridge.run Giraffe.server

#if DEBUG
#else
Lorca.StartGUI port |> Async.Start
#endif

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    use_router webApp
    memory_cache
    use_static publicPath
    use_json_serializer(Thoth.Json.Giraffe.ThothSerializer())
    app_config Giraffe.useWebSockets
    use_gzip
}

run app
