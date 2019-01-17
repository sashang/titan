module Login

open CustomColours
open Domain
open Elmish
open Elmish.Browser.Navigation
open Fable.FontAwesome
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Fable.Core.JsInterop
open Fulma
open Shared
open Thoth.Json


let login_with_google_button =
    Columns.columns [ ]
        [ Column.column [ Column.Width (Screen.All, Column.Is1) ]
            [ Fa.i [ Fa.Brand.Google; Fa.Size Fa.Fa2x ] [  ] ]
          Column.column [ ] [
              Button.a [
                    Button.Color IsTitanInfo
                    Button.Props [ Href "/secure/signin-google" ]
                ] [ str "sign in with Google" ] ] ]

let column =
    Column.column
        [ Column.Width (Screen.All, Column.Is4)
          Column.Offset (Screen.All, Column.Is4) ]
        [ Heading.h3
            [ Heading.Modifiers [ Modifier.TextColor IsBlack ] ]
            [ str "Login" ]
          Box.box' [ ] [
                Field.div [] [
                    login_with_google_button
                ]
          ]
    ]

let view =
    Container.container [ Container.IsFullHD
                          Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [ column ]

