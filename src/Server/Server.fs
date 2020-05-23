open System.IO

open System
open Saturn
open Shared

open Elmish
open Elmish.Bridge

let tryGetEnv key =
    match Environment.GetEnvironmentVariable key with
    | x when String.IsNullOrWhiteSpace x -> None
    | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port =
    "SERVER_PORT"
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

/// Connect the Elmish functions to an endpoint for websocket connections
let webApp =
    Bridge.mkServer "/socket/init" App.init App.update
    |> Bridge.run Giraffe.server

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
