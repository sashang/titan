module Client.NewTeacher

open Fulma
open Elmish
open Elmish.Browser.Navigation
open Fable.Helpers.React
open Fable.Import
open Style

type Model = {
    school_name : string
    teacher_name : string
}

type Msg =
| SetSchoolName of string
| Submit

let init () =
    { school_name = ""; teacher_name = "" }

let update (msg : Msg) (model : Model) : Model*Cmd<Msg> =
    match msg with
    | SetSchoolName name ->
        {model with school_name = name}, Cmd.none
    | Submit ->
        model, Navigation.newUrl (Client.Pages.toPath Client.Pages.MainSchool)

let on_change dispatch =
    fun (ev : React.FormEvent) ->
        let sn = ev.Value
        dispatch (SetSchoolName sn)

let on_submit dispatch =
    dispatch Submit

let input_field field_name description on_change =
    Field.div [ ] [
        Label.label [ ] [
            words 20 field_name
        ]
        Control.div [ ] [
            Input.text [
                Input.Size IsLarge
                Input.Color IsPrimary
                Input.Placeholder description
                Input.OnChange on_change
            ]
        ]
    ]

let view model (dispatch : Msg -> unit) =
    Hero.hero [
        Hero.IsBold
        Hero.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ]
        Hero.Color IsWhite
        Hero.IsFullHeight
    ] [
        Hero.head [ ] [
            client_header
        ]
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
                        input_field "School Name" "Give your school a name" (on_change dispatch)
                        input_field "Your Name" "Your name" (on_change dispatch)
                        Button.button [
                            Button.Color IsInfo
                            Button.IsFullWidth
                            Button.CustomClass "is-large is-block"
                            Button.OnClick (fun _ -> on_submit dispatch)
                        ] [
                            str "Submit"
                        ]
                    ]
                ]
            ]
        ]
    ]
