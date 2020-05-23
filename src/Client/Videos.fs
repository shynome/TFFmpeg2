module Videos

open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons
open Shared
open Fable.RemoteData
open Fable.Core

[<Emit("$0.toFixed(2)")>]
let p2str p = jsNative

let viewItem (v:Video) disptach =
    let title = v.Path
    let info =
        match v.Metadata with
        | Success s ->
            let s = sprintf "时长: %s 尺寸: %ix%i" s.Duration s.Width s.Height
            let s =
                match v.Progress with
                | Progress p ->
                    let p = p2str p
                    s + (sprintf " 进度: %f"  p) + "%"
                | _ -> s
            Html.text s
        | Failure e ->
            Mui.alert [
                alert.severity.error
                prop.children [
                    Html.text e
                ]
            ]
        | _ -> Html.text "loading"
    let actionBtn =
     match v.Progress with
        | NotReady | Ready ->
            let ready = v.Progress = Ready
            let tip =
                if ready then "开始转换"
                else "等待视频信息完善中"
            let btn =
                Mui.iconButton [
                    iconButton.disabled (not ready)
                    prop.onClick (fun _ ->
                        if not ready then () else
                        disptach (Transform v)
                    )
                    prop.children (playArrowIcon [])
                ]
            Mui.tooltip [
                tooltip.title tip
                prop.children (Html.span [btn])
            ]
        | Progress _ ->
            Mui.tooltip [
                tooltip.title "点击取消"
                prop.children (Html.span [
                    Mui.iconButton [
                        prop.onClick (fun _ -> disptach (CancelTransform v))
                        prop.children [stopIcon []]
                    ]
                ])
            ]
        | Finished ->
            Mui.tooltip [
                tooltip.title "点击重新转换"
                prop.children (Html.span [
                    Mui.iconButton [
                        prop.onClick (fun _ -> disptach (Transform v))
                        prop.children [refreshIcon []]
                    ]
                ])
            ]
        |  Error e ->
            Mui.tooltip [
                tooltip.title (Html.div [
                    Html.text "点击重新转换"
                    Html.br []
                    Html.text "错误原因"
                    Html.pre e
                ])
                prop.children (Html.span [
                    Mui.iconButton [
                        iconButton.color.secondary
                        prop.onClick (fun _ -> disptach (Transform v))
                        prop.children [refreshIcon []]
                    ]
                ])
            ]
    Mui.listItem [
        prop.key v.Path
        listItem.divider true
        prop.children [
            Html.div [
                prop.style [
                    style.position.absolute
                    style.bottom 0
                    style.left 0
                    style.right 0
                ]
                prop.children [
                    match v.Progress with
                    | Progress p ->
                        Mui.linearProgress [
                            linearProgress.value (int p)
                            linearProgress.variant.determinate
                        ]
                    | _ -> Html.none
                ]
            ]
            Mui.listItemText [
                listItemText.primary title
                listItemText.secondary info
            ]
            Mui.listItemSecondaryAction [
                Mui.tooltip [
                    tooltip.title "移除"
                    prop.children (Mui.iconButton [
                        prop.onClick (fun _ -> disptach (UnSelectFile v.Path))
                        prop.children (deleteIcon [])
                    ])
                ]
                actionBtn
            ]
        ]
    ]

let view (model:Model) dispatch =
    let items =
        model.SelectedVideos |> Array.map (fun v -> viewItem v dispatch)
    Mui.list [
        prop.children items
    ]
