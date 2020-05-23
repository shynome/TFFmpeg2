module Client

open Elmish
open Elmish.Bridge

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram App.init App.update App.view
|> Program.withBridgeConfig
    (
        Bridge.endpoint "/socket/init"
        |> Bridge.withMapping App.Remote
    )
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
