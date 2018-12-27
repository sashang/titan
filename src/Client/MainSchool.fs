﻿//The page of the teachers school
module Client.MainSchool

open Client.Pages
open Domain
open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack
open Fetch.Fetch_types
open Fulma
open ModifiedFableFetch
open Style
open System
open Thoth.Json

type Msg =
| ClickAddClass
| ClickSecure
| Success of string
| Failure of exn

type Model = {
    teacher_name : string
    school_name : string
    classes : Class.Info list
}

let click_secure (session : Session) = promise {
    let props = [
        RequestProperties.Credentials RequestCredentials.Include
        Fetch.requestHeaders [
            HttpRequestHeaders.Authorization ("Bearer " + session.token) ]]
    try
        let! res = Fetch.fetch "/api/secure/validate" props
        return! res.text ()
    with e ->
        return! failwith "could not get secured route"
}

let update (msg : Msg) (model : Model) (session : Session) : Model*Cmd<Msg> =
    match msg with
    | ClickAddClass ->
        model,  Navigation.newUrl (to_path AddClass)
    | ClickSecure ->
        model, Cmd.ofPromise click_secure session Success Failure
    | Success msg ->
        model, Cmd.none
    | Failure e ->
        model, Cmd.none

let init sn tn classes =
    {teacher_name = tn; school_name = sn; classes = classes}

///Create view for an individual class
let view_class (info : Class.Info) =
    Media.media [] [
        Media.left [ ] [
            str (info.date.ToString("HH:mm"))
        ]
        Media.content [] [
            Image.image [ Image.Is128x128 ] [
                img [ Src "https://placehold.it/128x128" ]
            ]
            str "somename"
        ]
        Media.content [] [
            Image.image [ Image.Is128x128 ] [
                img [ Src "https://placehold.it/128x128" ]
            ]
            str "somename"
        ]
    ]

///Create view for a list of classses
let view_classes model dispatch =
    Section.section [] [
        Heading.h2 [
            Heading.Modifiers [ Modifier.TextColor IsPrimary ]
        ] [
            str (DateTime.Today.ToLongDateString())
        ]
        Heading.h5 [ 
            Heading.Modifiers [ Modifier.TextColor IsGreyDark ]] [
            yield (match model.classes with
                   | [] -> str "No classes today!"
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
                            Heading.Modifiers [ Modifier.TextColor IsGreyDark ]] [
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
                    Field.div [] [
                        Client.Style.button dispatch ClickSecure "Secure"
                    ]
                ]
            ]
        ]
    ]
