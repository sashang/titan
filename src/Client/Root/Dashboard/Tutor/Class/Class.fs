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
      EndTime : DateTimeOffset  option }

type Msg =
    | Next
    | GoLive of OpenTokInfo

let init () =
    { Students = []; StartTime = None; EndTime = None; OTI = None }, Cmd.none

let initialize_sesison (key:string) (session_id:string) (token:string) : unit = import "initialize_session" "../../../../custom.js"

let update (model : Model) (msg : Msg) =
    match msg with
    | Next -> model, Cmd.none

    | GoLive open_tok_info ->
        initialize_sesison open_tok_info.Key open_tok_info.SessionId open_tok_info.Token
        {model with OTI = Some open_tok_info }, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    [ Box.box' [ Common.Props [ HTMLAttr.Id "publisher" ]  ]
        [  ] ]

