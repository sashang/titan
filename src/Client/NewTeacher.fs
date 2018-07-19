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
        Hero.head [ ] [
            Container.container [ Container.IsFluid ]
                [ Section.section [ ]
                    [ Level.level [ ]
                        [ Level.left [ ]
                            [ Level.item [ ]
                                [ Heading.h1 [ Heading.Is3
                                               Heading.Modifiers [ Modifier.TextColor IsBlack ] ]
                                               [ str "The New Kid" ] ] ]
                          Level.right [ ]
                            [ Level.item [ ]
                                [ Text.span [ Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is3)
                                                          Modifier.TextColor IsLink ] ] [ a [ Href "#how_it_works" ] [ str "How it Works" ] ] ]
                              Level.item [ ]
                                [ viewLink Pages.Login "Login" ] ] ] ]
            ]]
        Hero.body [ ] [
            Container.container [
                Container.Modifiers [
                    Modifier.TextAlignment (Screen.All, TextAlignment.Centered)
                ]
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
                    Box.box' [
                        Modifiers [
                            Modifier.TextAlignment (Screen.All, TextAlignment.Left)
                        ]
                    ] [
                        form [ ] [
                            input_field "School Name" "Give your school a name"
                            input_field "Your Name" "Your name"
                            Button.button [
                                Button.Color IsInfo
                                Button.IsFullWidth
                                Button.CustomClass "is-large is-block"
                            ] [
                                str "Submit"
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
