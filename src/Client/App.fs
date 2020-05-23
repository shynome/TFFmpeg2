module App

open Feliz
open Feliz.MaterialUI
open Shared
open Elmish
open Elmish.Bridge

let init () =
    None, Cmd.none

type Msg =
    | Remote of ClientMsg
    | Action of ServerMsg

let update msg (model:Model option) =
    match msg with
    | Remote msg ->
        match msg with
        | SyncModel model -> Some(model), Cmd.none
    | Action msg ->
        model, Cmd.bridgeSend msg

let TabMap tab =
    match tab with
    | Explorer -> 0
    | Videos -> 1

let viewApp model dispatch =
    React.fragment [
        Mui.appBar [
            appBar.position.sticky
            prop.children (Mui.toolbar [
                Mui.container [
                    Mui.tabs [
                        tabs.value (TabMap model.Tab)
                        prop.children [
                            Mui.tab [
                                tab.value (TabMap Explorer)
                                tab.label "文件浏览"
                                prop.onClick (fun _ -> dispatch (SetTab Explorer))
                            ]
                            Mui.tab [
                                tab.value (TabMap Videos)
                                tab.label "已选择的视频文件"
                                prop.onClick (fun _ -> dispatch (SetTab Videos))
                            ]
                        ]
                    ]
                ]
            ])
        ]
        Mui.container [
            match model.Tab with
            | Explorer -> Explorer.view model dispatch
            | Videos -> Videos.view model dispatch
        ]
    ]

let view (model) dispatch =
    React.fragment [
        Mui.cssBaseline []
        if model = None then
            Html.text "loading"
        else
            viewApp model.Value (Action >> dispatch)
    ]
