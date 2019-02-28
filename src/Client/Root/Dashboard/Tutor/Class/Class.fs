/// A class in the school
module Tutor.Class

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
open Tutor.LiveView
open OpenTokReactApp
open ModifiedFableFetch
open System
open Client.Shared

open Thoth.Json

///used to communicate info back to the parent
type ExternalMsg =
    | GoLive
    | Noop 

type Model =
    { Students : Student list
      OTI : OpenTokInfo option
      StartTime : DateTimeOffset option
      Error : APIError option
      Email : string //tutor's email
      EndTime : DateTimeOffset  option }

type Msg =
    | GoLive

let init email =
    { Students = []; StartTime = None;
      EndTime = None; OTI = None; Error = None; Email = email},
      Cmd.none

let update (model : Model) (msg : Msg) =
    match model, msg with
    | model, GoLive ->
        model, Cmd.none, ExternalMsg.GoLive

let private classroom_level model dispatch =
    Level.level [ ] [ 
        Level.left [ ]
            [ Level.title [ Common.Modifiers [ Modifier.TextTransform TextTransform.UpperCase
                                               Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            Common.Props [ Style [ CSSProp.FontFamily "'Montserrat', sans-serif" ]] ] [ str "classroom" ] ]
    ]


let view (model : Model) (dispatch : Msg -> unit) =
    div [ ] [
        classroom_level model dispatch
    ]
