module Dashboard

open Domain
open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Elmish.React
open Fable.Helpers.React.Props
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.PowerPack
open Elmish.Browser.Navigation
open Fulma
open Fulma.Extensions
open Fable.Helpers.React

type Model =
    { TheSchool : School option }

type Msg =
    | ClickCreateSchool

let init =
    {TheSchool = None}

let view (model : Model) (dispatch : Msg -> unit) =
     Container.container
        [ Container.IsFullHD
          Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
        [ Tile.ancestor []
            [ Tile.parent [ ]
                [ Tile.child [ ]
                    [ Box.box' [ Common.Props [ Style [ Height "100%" ] ] ]
                        [ Heading.p [ ] [ str "Live" ] ] ] ] ] ]

let update (model : Model) (msg : Msg) =
    match model, msg with
    | _, _ -> model, Cmd.none