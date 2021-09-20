module ElmishBridgeServer

open Elmish
open Elmish.Bridge
open ElmishBridgeModel
open global.Giraffe 
open Microsoft.AspNetCore.Http

let serverHub = ServerHub<Model, ServerMsg, ClientMsg>()

let init dispatch () =
    eprintfn "server init"
    dispatch ClientInitialize
    None, Cmd.none
    //model, Cmd.none

let update dispatch msg model =
    let isStudent (model : Model) : bool =
        match model with
        | Some Student -> 
            true
        | _ -> false

    let isTutor (model : Model) : bool =
        match model with
        | Some Tutor ->
            true
        | _ -> false

    match msg with
    | ClientIs user -> 
        Some user, Cmd.none

    //tutor started their live stream
    | TutorGoLive ->
        serverHub.SendClientIf isStudent ClientTutorGoLive
        model, Cmd.none

    //tutor stopped their live stream
    | TutorStopLive ->
        serverHub.SendClientIf isStudent ClientTutorStopLive
        model, Cmd.none
    
    //a student wants to know if the tutor has started.
    | StudentRequestLiveState ->
        serverHub.SendClientIf isTutor ClientStudentRequestLiveState
        model, Cmd.none

    | TutorLiveState state ->
        match state with
        | On -> serverHub.SendClientIf isStudent ClientTutorGoLive
        | Off -> serverHub.SendClientIf isStudent ClientTutorStopLive
        model, Cmd.none

let endpoint = "/socket"

let server : HttpFunc -> HttpContext -> HttpFuncResult=
    Bridge.mkServer endpoint init update
    |> Bridge.withConsoleTrace
    |> Bridge.withServerHub serverHub
    |> Bridge.run Giraffe.server
