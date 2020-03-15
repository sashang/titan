module Login

open CustomColours
open Fable.FontAwesome
open Fable.React
open Fable.React.Props
open Fulma


let login_with_google_button =
    Columns.columns [ ]
        [ Column.column [ Column.Width (Screen.All, Column.Is1) ]
            [ Fa.i [ Fa.Brand.Google; Fa.Size Fa.Fa2x ] [  ] ]
          Column.column [ ] [
              Button.a [
                    Button.Color IsTitanInfo
                    Button.Props [ Href "/signin-google" ]
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

