module Client.Home

open Fulma
open Fable.Helpers.React

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
                    str "Need more space in your classroom?"
                ]
                Heading.h3 [ Heading.Modifiers [ Modifier.TextColor IsBlack ] ] [
                    str "Add students to your class virtually"
                ]
            ]
        ]
    ]
