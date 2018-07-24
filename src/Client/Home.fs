module Client.Home

open Fulma
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props

open Style

let view () =
    Hero.hero [
        Hero.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ]
        Hero.Color IsWhite
        Hero.IsHalfHeight
    ] [
        Hero.head [ ] [
            client_header
        ]
        Hero.body [ ] [
            Container.container [ Container.IsFluid ] [
                Heading.h1 [ Heading.Modifiers [ Modifier.TextColor IsBlack ] ] [
                   str "Need to grow your classroom?"
                ]
                Heading.h3 [ Heading.Modifiers [ Modifier.TextColor IsBlack ] ] [
                  str "Add students to your class virtually"
                ]
            ]
        ]
    ]
