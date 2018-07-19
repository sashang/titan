module Client.NewTeacher

open Fulma
open Fulma.Extensions
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack
open Fable.PowerPack.Fetch.Fetch_types
open Style

type Model = {
    school_name : string
}

type Msg =
| SetSchoolName

let init () =
    { school_name = "" }

let update (msg : Msg) (model : Model) : Model*Cmd<Msg> =
    model, Cmd.none

let input_field field_name description =
    Field.div [ ] [
        Label.label [ ] [
            words 20 field_name
        ]
        Control.div [ ] [
            Input.text [
                Input.Size IsLarge
                Input.Color IsPrimary
                Input.Placeholder description
            ]
        ]
    ]

let view model (dispatch : Msg -> unit) =
    Hero.hero [
        Hero.Color IsSuccess
        Hero.IsFullHeight
    ] [
        Hero.body [ ] [
            Container.container [
                Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ]
            ] [
                Column.column [
                    Column.Width (Screen.All, Column.Is4)
                    Column.Offset (Screen.All, Column.Is4)
                ] [
                    Heading.h3 [
                        Heading.Modifiers [ Modifier.TextColor IsGrey ]
                    ] [
                        str "Welcome"
                    ]
                    Box.box' [ ] [
                        form [ ] [
                            input_field "School Name" "Give your school a name"
                            input_field "Your Name" "Your name"
                        ]
                    ]
                ]
            ]
        ]
    ]
