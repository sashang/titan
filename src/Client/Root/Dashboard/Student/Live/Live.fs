module Live

open CustomColours
open Client.Shared
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

let update (model : Model) (msg : Msg) =
    match model, msg with
    | model, GoLive oti ->
        Browser.console.info "receive GoLive"
        match model.Session with
        //no existing session so init one an opentok session with the tutor
        | None ->
            let js_session = OpenTokJSInterop.init_session oti.Key oti.SessionId
            if js_session = null then failwith "failed to get js session"
            OpenTokJSInterop.connect_session_with_sub js_session oti.Token
            {model with Session = Some js_session}, Cmd.none
        | Some session ->
            OpenTokJSInterop.connect session oti.Token
            model, Cmd.none

    | model, StopLive ->
        Browser.console.info "received StopLive"
        match model.Session with
        | Some session ->
            //unsubscribe js_session js_sub
            OpenTokJSInterop.disconnect session
            model, Cmd.none
        | _ ->
            Browser.console.info ("nothing to disconnect")
            model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    [ Box.box' [ Common.Props [ HTMLAttr.Id "subscriber"
                                Style [ CSSProp.Height "100%" ]  ] ]
        [ ] ]
    

