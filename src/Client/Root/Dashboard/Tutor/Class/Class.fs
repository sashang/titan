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
open Shared
open System
open Thoth.Json

type Model =
    { Students : Student list
      StartTime : DateTimeOffset option
      EndTime : DateTimeOffset  option }

type Msg =
    | Next

let init () =
    { Students = []; StartTime = None; EndTime = None }, Cmd.none

let update (model : Model) (msg : Msg) =
    match msg with
    | Next -> model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    [ Box.box' [ ] 
        [ ] ]

