﻿module Home

open Fulma
open Fable.Helpers.React

let view =
    Container.container [ Container.IsFluid ] [
        Heading.h1 [ Heading.Modifiers [ Modifier.TextColor IsBlack ] ] [
            str "Need more space in your classroom?"
        ]
        Heading.h3 [ Heading.Modifiers [ Modifier.TextColor IsGreyDark ] ] [
            str "Add students to your class virtually"
        ]
    ]