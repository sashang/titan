//The page of the teachers school
module Client.MainSchool

open Fulma
open Elmish
open Fable.Helpers.React
open Style
open System

type Msg =
| ClickAddClass

type Model = {
    teacher_name : string
    school_name : string
    classes : Class.Info list
}

let update (msg : Msg) (model : Model) : Model*Cmd<Msg> =
    match msg with
    | ClickAddClass ->
        {model with classes = List.append model.classes [{Class.Info.date = DateTimeOffset(DateTime.Now); Class.Info.pupils = [] }] }, Cmd.none

let init sn tn classes =
    {teacher_name = tn; school_name = sn; classes = classes}

///Create view for an individual class
let view_class (info : Class.Info) =
    Section.section [] [
        str (info.date.ToString ())
    ]

///Create view for a list of classses
let view_classes model dispatch =
    Section.section [] [
        Heading.h3 [
            Heading.Modifiers [ Modifier.TextColor IsPrimary ]
        ] [
            str "Upcoming classes"
        ]
        Heading.h5 [ 
            Heading.IsSubtitle
            Heading.Modifiers [ Modifier.TextColor IsDark ]] [
            yield (match model.classes with
                   | [] -> str "None"
                   | classes -> List.map view_class classes |> ofList)
        ]
    ]

let view model dispatch =
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
                Columns.columns [
                    Columns.IsCentered
                    Columns.IsMultiline
                ] [
                    Column.column [ Column.Width (Screen.All, Column.IsFourFifths) ] [
                        Heading.h1[
                            Heading.Modifiers [ Modifier.TextColor IsPrimary ]
                        ] [
                            str model.school_name
                        ]
                        Heading.h3 [ 
                            Heading.IsSubtitle
                            Heading.Modifiers [ Modifier.TextColor IsDark ]] [
                            str ("Principal " + model.teacher_name)
                        ]
                        view_classes model dispatch
                    ]
                    Column.column [ ] [
                        Button.button [
                            Button.Color IsPrimary
                            Button.CustomClass "is-large is-block"
                            Button.OnClick (fun ev -> dispatch ClickAddClass)
                        ] [ str "Add Class" ] 
                    ]
                ]
            ]
        ]
    ]
