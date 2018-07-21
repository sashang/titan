//The page of the teachers school
module Client.MainSchool

open Fulma
open Elmish
open Fable.Helpers.React
open Style

type Msg = None

type Model = {
    teacher_name : string
    school_name : string
}

let update (msg : Msg) (model : Model) : Model*Cmd<Msg> =
    model, Cmd.none

let init tn sn =
    {teacher_name = tn; school_name = sn}


let view model dispatch =
    Hero.hero [
        Hero.Color IsSuccess
        Hero.IsFullHeight
    ] [
        Hero.head [ ] [
            client_header
        ]
        Hero.body [ ] [
            Container.container [
                Container.IsFluid
                Container.Modifiers [
                    Modifier.TextAlignment (Screen.All, TextAlignment.Centered)
                    Modifier.TextColor IsBlack
                ]
            ] [
                Heading.h1[
                    Heading.Modifiers [ Modifier.TextColor IsGrey ]
                ] [
                    str model.school_name
                ]
                Heading.h2 [ ] [
                    str model.teacher_name
                ]
            ]
        ]
    ]
        
