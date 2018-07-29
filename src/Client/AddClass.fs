module Client.AddClass

open Fulma
open Elmish
open Elmish.Browser.Navigation
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Style
open System

type Model = {
    start_time : DateTimeOffset
    end_time : DateTimeOffset
}

type Msg =
| Submit
| SetStartTime of DateTimeOffset
| SetEndTime of DateTimeOffset

let init () =
    { start_time = DateTimeOffset.Now ; end_time = DateTimeOffset.Now }

let update (msg : Msg) (model : Model) : Model*Cmd<Msg> =
    match msg with
    | Submit ->
        model, Navigation.jump -1

let on_blur_end_time dispatch =
    fun (ev : React.FocusEvent) ->
        let end_time = DateTimeOffset()
        dispatch (SetStartTime end_time)

let on_blur_start_time dispatch =
    fun (ev : React.FocusEvent) ->
        let start_time = DateTimeOffset()
        dispatch (SetStartTime start_time)

let on_submit dispatch =
    dispatch Submit
let input_field field_name description on_blur =
    Field.div [ ] [
        Label.label [ ] [
            words 20 field_name
        ]
        Control.div [ ] [
            Input.text [
                Input.Size IsLarge
                Input.Color IsPrimary
                Input.Placeholder description
                Input.Props [ OnBlur on_blur ]
            ]
        ]
    ]

let view model (dispatch : Msg -> unit) =
    Hero.hero [
        Hero.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ]
        Hero.Color IsWhite
        Hero.IsHalfHeight
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
                        str "Enter the start and end times"
                    ]
                    Box.box' [
                        Modifiers [
                            Modifier.TextAlignment (Screen.All, TextAlignment.Left)
                        ]
                    ] [
                        input_field "Start Time" "" (on_blur_start_time dispatch)
                        input_field "End Time" "" (on_blur_end_time dispatch)
                        Button.button [
                            Button.Color IsPrimary
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
