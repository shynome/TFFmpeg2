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
                match v.Option with
                | None -> s
                | Some o ->
                    let o = sprintf " 输出尺寸: %ix%i" o.Width o.Height
                    s + o
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
                Mui.tooltip [
                    tooltip.title "编辑输出选项"
                    prop.children (Mui.iconButton [
                        prop.onClick (fun _ -> SetEditVideo (Some v) |> disptach)
                        prop.children (editIcon [])
                    ])
                ]
                actionBtn
            ]
        ]
    ]

let getSize (v:Video) =
    match (v.Option, v.Metadata) with
    | (Some m, _) -> (string m.Width,string m.Height)
    | (_, Success m) -> (string m.Width,string m.Height)
    | (_, _) -> ("-2","-2")
let viewEditVideo = React.functionComponent (fun (p:{|video: Video;dispatch:ServerMsg->unit|})->
    let (v,dispatch) = (p.video,p.dispatch)
    let initSize = React.useMemo((fun () -> getSize(v)), ([||]))
    let ((w,h), setSize) = React.useState(initSize)
    React.useEffect((fun ()->getSize v |> setSize), [|(v.Metadata :> obj)|])
    let onSave _ =
        let o = { Width = int w; Height = int h }
        let v = { v with Option = Some o }
        SaveEditVideo v |> dispatch
        ()
    let (we,he) =(
        (try int w |> ignore; "" with _ -> "宽度需要是数字"),
        (try int h |> ignore; "" with _ -> "高度需要是数字")
    )
    let disableSubmit =
        match (we,he) with
        | ("","") -> false
        | (_,_) -> true
    Html.form [
        prop.onSubmit (fun e->e.preventDefault();if not disableSubmit then onSave())
        prop.children [
            Mui.dialogTitle v.Path
            Mui.dialogContent [
                Mui.grid [
                    grid.container true
                    grid.spacing._1
                    prop.children [
                        Mui.grid [
                            grid.item true
                            grid.xs._6
                            prop.children [
                                let sw = match v.Metadata with Success m -> m.Width | _ -> -2
                                Mui.textField [
                                    textField.fullWidth true
                                    textField.label (sprintf "定宽 (原始宽度: %i)" sw)
                                    textField.error (we <> "")
                                    textField.helperText we
                                    input.type' "number"
                                    input.value w
                                    input.onChange (fun (w:string)->setSize (w, "-2"))
                                ]
                            ]
                        ]
                        Mui.grid [
                            grid.item true
                            grid.xs._6
                            prop.children [
                                let sh = match v.Metadata with Success m -> m.Height | _ -> -2
                                Mui.textField [
                                    textField.fullWidth true
                                    textField.label ((sprintf "定高 (原始高度: %i)" sh))
                                    textField.error (he <> "")
                                    textField.helperText he
                                    input.type' "number"
                                    input.value h
                                    input.onChange (fun (h:string)->setSize ("-2", h))
                                ]
                            ]
                        ]
                    ]
                ]
            ]
            Mui.dialogActions [
                Mui.button [
                    prop.onClick (fun _ -> SetEditVideo None |> dispatch)
                    prop.text "取消"
                ]
                Mui.button [
                    button.type'.submit
                    prop.disabled disableSubmit
                    button.variant.contained
                    button.color.primary
                    prop.text "保存"
                ]
            ]
        ]
    ]
)

let viewEditVideoDialog (video: Video option) (dispatch:ServerMsg->unit) =
    let onClose _ = SetEditVideo None |> dispatch
    let changeSize (w,h) =
        match video with
        | Some v ->
            let o = Some { Width = w; Height = h }
            let v = { v with Option = o }
            SetVideo v |> dispatch
        | None -> ()
    let body =
        match video with
        | None -> Html.none
        | Some v -> viewEditVideo ({| video = v; dispatch = dispatch |})
    Mui.dialog [
        dialog.open' (video <> None)
        dialog.onClose onClose
        prop.children body
    ]

let view (model:Model) dispatch =
    let items =
        model.SelectedVideos |> Array.map (fun v -> viewItem v dispatch)
    React.fragment [
        viewEditVideoDialog model.EditVideo dispatch
        Mui.list [
            prop.children items
        ]
    ]
