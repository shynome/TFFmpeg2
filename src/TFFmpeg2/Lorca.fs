module Lorca

open System.Diagnostics

let LorcaBin = Utils.getBin "lorca"

let StartGUI port =
    async {
        do! Async.Sleep 500
        let addr = sprintf "http://127.0.0.1:%i" port
        let args = sprintf "%s %i %i" addr 1024 800
        let cmd = Process.Start(LorcaBin, args)
        cmd.WaitForExit()
        System.Environment.Exit 0
        ()
    }
