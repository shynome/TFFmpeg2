module Explorer

open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons
open Shared
open Fable.RemoteData
open Fable.Core

let private viewItem (model, path:string, dir:bool) dispatch =
    let root = model.Root
    let selected =
        if dir then false else
        model.SelectedVideos |> Array.tryFind (fun v -> v.Path = path) |> (fun e -> e <> None)
    let handleClick _ =
        if dir then dispatch (GetFiles path) else
        if selected then
            dispatch (UnSelectFile path)
        else
            dispatch (SelectFile path)
    let displayPath = path.[root.Length..]
    let displayPath = if displayPath.StartsWith "/" then displayPath.[1..] else displayPath
    Mui.listItem [
        prop.key path
        listItem.divider true
        listItem.button true
        prop.onClick handleClick
        prop.children [
            Mui.listItemIcon [
                prop.children [
                    if dir then Mui.iconButton [ folderIcon [] ]
                    else Mui.checkbox [
                        checkbox.disabled dir
                        checkbox.checked' selected
                        checkbox.color.primary
                    ]
                ]
            ]
            Mui.listItemText displayPath
        ]
    ]

[<Emit("document.documentElement.scrollTop = 0")>]
let backTop _ = jsNative

[<Emit("window.prompt($0)")>]
let prompt (text:string): string = jsNative

let private viewHeader (root:string) dispatch =
    let paths = root.Split('/')
    let goDir i =
        let path = if i = 0 then "/" else paths.[0..i] |> String.concat "/"
        dispatch (GetFiles path)
    let btns = paths |> Array.mapi (fun i s ->
        let s = s + "/"
        Mui.button [
            prop.key i
            prop.text s
            prop.onClick (fun _ -> goDir i)
            prop.style [ style.textTransform.none ]
        ]
    )
    Mui.appBar [
        appBar.position.sticky
        appBar.color.inherit'
        appBar.elevation 0
        prop.children (Mui.toolbar [
            Mui.buttonGroup [
                prop.children btns
            ]
            Mui.button [
                button.variant.outlined
                prop.style [style.marginLeft 10]
                prop.onClick (fun _ ->
                    let a = prompt "输入地址"
                    match a with
                    | null | "" -> ()
                    | _ -> dispatch (GetFiles a)
                )
                prop.text "输入地址"
            ]
            Mui.typography [
                prop.style [ style.flexGrow 1 ]
            ]
            Mui.button [
                button.variant.outlined
                prop.onClick backTop
                prop.text "返回顶部"
            ]
        ])
    ]

let view (model:Model) dispatch =
    Mui.paper [
        viewHeader model.Root dispatch
        Mui.divider []
        match model.Files with
        | Success (dirs,files) ->
            let dirs = dirs |> Array.map (fun file ->viewItem (model,file,true) dispatch)
            let files = files |> Array.map (fun file ->viewItem (model,file,false) dispatch)
            let list = Array.append dirs files
            Mui.list list
        | Failure e ->
            Mui.alert [
                alert.severity.error
                prop.children [
                    Mui.alertTitle [
                        prop.text "读取文件夹失败"
                    ]
                    Html.pre (Html.code e)
                ]
            ]
        | Loading | _ ->
            Mui.linearProgress []
    ]
