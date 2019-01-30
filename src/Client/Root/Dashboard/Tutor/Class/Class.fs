/// A class in the school
module Class

open CustomColours
open Domain
open Elmish
open Elmish.Browser.Navigation
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Fable.Core.JsInterop
open Fulma
open System
open Client.Shared
open Thoth.Json

type Model =
    { Students : Student list
      StartTime : DateTimeOffset option
      OTI : OpenTokInfo option
      Session : obj option
      Publisher : obj option
      EndTime : DateTimeOffset  option }

type Msg =
    | StopLive
    | GoLive of OpenTokInfo

let init () =
    { Students = []; StartTime = None; EndTime = None; OTI = None;
      Publisher = None; Session = None }, Cmd.none


let update (model : Model) (msg : Msg) =
    match msg with

    | GoLive open_tok_info ->
        Browser.console.info ("Going live")
        match model.Session with
        | None ->
            let session = OpenTokJSInterop.init_session open_tok_info.Key open_tok_info.SessionId
            if session = null then failwith "failed to get js session"
            let publisher = OpenTokJSInterop.init_pub "publisher"
            OpenTokJSInterop.connect_session_with_pub session publisher open_tok_info.Token
            {model with OTI = Some open_tok_info; Session = Some session; Publisher = Some publisher }, Cmd.none

        | Some session ->
            OpenTokJSInterop.connect session model.OTI.Value.Token
            model, Cmd.none

    | StopLive ->
        match model.Session with
        | Some session ->
            Browser.console.info ("Stopping live stream")
            OpenTokJSInterop.disconnect session
            {model with OTI = None; Session = None; Publisher = None}, Cmd.none
        | _ ->
            {model with OTI = None}, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    [ Box.box' [ Common.Props [ HTMLAttr.Id "publisher"
                                Style [ CSSProp.Height "100%" ]  ] ]
        [ ] ]

