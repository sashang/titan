module ElmishBridgeServer

open Elmish
open Elmish.Bridge
open ElmishBridgeModel
open global.Giraffe 
open Microsoft.AspNetCore.Http

type Msg = Remote of ServerMsg

let init dispatch model =
    dispatch (TheClientMsg (Msg1 "server message"))
    model, Cmd.none

let update dispatch msg model =
    match msg with
    | Remote() -> failwith "unsupported"

let server (a : HttpFunc) (ctx : HttpContext) =
    Bridge.mkServer endpoint init update
    |> Bridge.run Giraffe.server