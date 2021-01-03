module ElmishBridgeServer

open Elmish
open Elmish.Bridge
open ElmishBridgeModel
open global.Giraffe 
open Microsoft.AspNetCore.Http

let server_hub = ServerHub<Model, ServerMsg, ClientMsg>()

let init dispatch () =
    eprintfn "server init"
    dispatch ClientInitialize
    None, Cmd.none
    //model, Cmd.none

let update dispatch msg model =
    let is_student (model : Model) : bool =
        match model with
        | Some Student -> 
            true
        | _ -> false

    let is_tutor (model : Model) : bool =
        match model with
        | Some Tutor ->
            true
        | _ -> false

    match msg with
    | ClientIs user -> 
        Some user, Cmd.none

    //tutor started their live stream
    | TutorGoLive ->
        server_hub.SendClientIf (fun x -> is_student x) ClientTutorGoLive
        model, Cmd.none

    //tutor stopped their live stream
    | TutorStopLive ->
        server_hub.SendClientIf (fun x -> is_student x) ClientTutorStopLive
        model, Cmd.none
    
    //a student wants to know if the tutor has started.
    | StudentRequestLiveState ->
        server_hub.SendClientIf (fun x -> is_tutor x) ClientStudentRequestLiveState
        model, Cmd.none

    | TutorLiveState state ->
        match state with
        | On -> server_hub.SendClientIf (fun x -> is_student x) ClientTutorGoLive
        | Off -> server_hub.SendClientIf (fun x -> is_student x) ClientTutorStopLive
        model, Cmd.none

let endpoint = "/socket"

let server : HttpFunc -> HttpContext -> HttpFuncResult=
    Bridge.mkServer endpoint init update
    |> Bridge.withConsoleTrace
    |> Bridge.withServerHub server_hub
    |> Bridge.run Giraffe.server
