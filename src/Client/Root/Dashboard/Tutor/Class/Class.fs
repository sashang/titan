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

let init_session (key:string) (session_id:string) (token:string) : obj =
    import "init_session" "../../../../custom.js"

let disconnect_session (session : obj) : unit =
    import "disconnect" "../../../../custom.js"

let init_pub (div_id : string) : obj =
    import "init_pub" "../../../../custom.js"

let connect_session (session:obj) (publisher:obj) (token:string) : unit =
    import "connect_session" "../../../../custom.js"

let update (model : Model) (msg : Msg) =
    match msg with

    | GoLive open_tok_info ->
        Browser.console.info ("Going live")
        let session = init_session open_tok_info.Key open_tok_info.SessionId open_tok_info.Token
        let publisher = init_pub "publisher"
        connect_session session publisher open_tok_info.Token
        {model with OTI = Some open_tok_info; Session = Some session; Publisher = Some publisher }, Cmd.none

    | StopLive ->
        match model.Session with
        | Some session ->
            Browser.console.info ("Stopping live stream")
            disconnect_session session
            {model with OTI = None; Session = None; Publisher = None}, Cmd.none
        | _ ->
            {model with OTI = None}, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    [ Box.box' [ Common.Props [ HTMLAttr.Id "publisher" ]  ]
        [ ] ]

