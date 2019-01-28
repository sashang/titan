module Live

open CustomColours
open Domain
open Elmish
open Fable.Import
open Fable.PowerPack
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core.JsInterop
open Fulma
open ModifiedFableFetch
open Thoth.Json

type Msg =
    | StopLive
    | GoLive of OpenTokInfo

type Model =
    { Session : obj option }

let init () = 
    { Session = None}, Cmd.none

let init_session (key:string) (session_id:string) : obj =
    import "init_session" "../../../../custom.js"

let disconnect_session (session : obj) : unit =
    import "disconnect" "../../../../custom.js"

let connect_subscriber (session:obj) (token:obj) : unit =
    import "connect_subscriber" "../../../../custom.js"

let callback_stream_create (session:obj) : unit =
    import "callback_stream_create" "../../../../custom.js"

let unsubscribe (session:obj) (sub:obj) : unit =
    import "callback_stream_create" "../../../../custom.js"

let update (model : Model) (msg : Msg) =
    match model, msg with
    | model, GoLive oti ->
        Browser.console.info "receive GoLive"
        //init an opentok session with the tutor
        let js_session = init_session oti.Key oti.SessionId
        connect_subscriber js_session oti.Token
        //setup the subscription callback
        //connect_subscriber js_session oti.Token
        {model with Session = Some js_session}, Cmd.none

    | model, StopLive ->
        Browser.console.info "received StopLive"
        match model.Session with
        | Some js_session ->
            //unsubscribe js_session js_sub
            disconnect_session js_session
            {model with Session = None}, Cmd.none
        | _ ->
            model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    [ Box.box' [ Common.Props [ HTMLAttr.Id "subscriber"
                                Style [ CSSProp.Height "100%" ]  ] ]
        [ ] ]
    

