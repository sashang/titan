module ElmishBridgeServer

open Elmish
open Elmish.Bridge
open ElmishBridgeModel
open global.Giraffe 
open Microsoft.AspNetCore.Http

let server_hub = ServerHub<Model, ServerMsg, ClientMsg>()

let init dispatch model =
    dispatch TestMessage
    User(""), Cmd.none

let update dispatch msg model =
    match msg with

    //tutor started their live stream
    | TutorGoLive ->
        eprintfn "Received TutorGoLive"
        eprintfn "Connected clients %A" (server_hub.GetModels().Length)
        server_hub.BroadcastClient(ClientTutorGoLive)
        model, Cmd.none

    //tutor stopped their live stream
    | TutorStopLive ->
        eprintfn "Received TutorStopLive"
        eprintfn "Connected clients %A" (server_hub.GetModels().Length)
        server_hub.BroadcastClient(ClientTutorStopLive)
        model, Cmd.none

let endpoint = "/socket"

let server : HttpFunc -> HttpContext -> HttpFuncResult=
    Bridge.mkServer endpoint init update
    |> Bridge.withConsoleTrace
    |> Bridge.withServerHub server_hub
    |> Bridge.run Giraffe.server