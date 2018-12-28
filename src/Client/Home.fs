module Client.Home

open Fulma
open Fable.Helpers.React
open Fable.Import

open Style

let view dispatch session =
    Hero.hero [
        Hero.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ]
        Hero.Color IsWhite
        Hero.IsHalfHeight
    ] [
        Hero.head [ ] [
            client_header dispatch session
        ]
        Hero.body [ ] [
            Container.container [ Container.IsFluid ] [
                Heading.h1 [ Heading.Modifiers [ Modifier.TextColor IsBlack ] ] [
                    str "Need more space in your classroom?"
                ]
                Heading.h3 [ Heading.Modifiers [ Modifier.TextColor IsGreyDark ] ] [
                    str "Add students to your class virtually"
                ]
            ]
        ]
    ]
