module Client.FirstTime

open Fulma
open Elmish
open Elmish.Browser.Navigation
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fable.Core.JsInterop
open Client.Style


type Character =
| Pupil
| Teacher

type Model = {
    character : Character
}

type Msg =
| ClickContinue
| SelectCharacter of Character

let init () =
    {character = Pupil}

let update (msg : Msg) (model : Model) : Model*Cmd<Msg> =
    match msg with
    
    //if we click continue
    | ClickContinue ->
        //who did we choose?
        match model.character with
        | Teacher ->
        //then navigate to the teacher page
            model, Navigation.newUrl (Client.Pages.to_path Client.Pages.NewTeacher)
        | Pupil ->
        //navigate the pupil page
            model, Navigation.newUrl (Client.Pages.to_path Client.Pages.NewPupil)

    | SelectCharacter c ->
        { character = c}, Cmd.none

let select_character dispatch =
    fun (ev : React.FormEvent) ->
        let c = !!ev.target?value
        printfn "Character %s selected" c
        if (c = "teacher") then
            dispatch (SelectCharacter Teacher)
        else
            dispatch (SelectCharacter Pupil)

let view model dispatch =
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
                        Box.box' [ ] [
                            Label.label [] [ words 20 "It looks like this is your first time here. Are you a pupil or a teacher?" ]
                            Field.div [] [
                                Control.div [] [
                                    Select.select [ Select.Props [ OnChange (select_character dispatch) ] ] [
                                        select [ DefaultValue "pupil" ] [
                                            option [ Value "teacher" ] [ str "Teacher" ]
                                            option [ Value "pupil" ] [ str "Pupil" ]
                                        ]
                                    ]
                                ]
                            ]
                            Button.button [
                                Button.Color IsPrimary
                                Button.IsFullWidth
                                Button.OnClick (fun _ -> dispatch ClickContinue)
                                Button.CustomClass "is-large is-block"
                            ] [ str "Submit" ]
                        ]
                    ]
                ]
            ]
        ]
    ]
