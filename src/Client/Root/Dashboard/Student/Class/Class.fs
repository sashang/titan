module Student.Class

open CustomColours
open Client.Shared
open Domain
open Elmish
open Fable.Import
open Fable.PowerPack
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core.JsInterop
open Fulma
open ModifiedFableFetch
open Student.LiveView
open Thoth.Json


type Msg =
    | GoLive

type Model =
    { School : School 
      StudentEmail: string
      Error : APIError option}

let private classroom_level model dispatch =
    Level.level [ ] [ 
        Level.left [ ]
            [ Level.title [ Common.Modifiers [ Modifier.TextTransform TextTransform.UpperCase
                                               Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            Common.Props [ Style [ CSSProp.FontFamily "'Montserrat', sans-serif" ]] ] [ str "classroom" ] ]
    ]

let init school student_email = 
    {School = school; StudentEmail = student_email; Error = None},
    Cmd.none

let update (model : Model) (msg : Msg) =
    //TODO: map this to the OTI record based on the email
    match model, msg with
    | model, Msg.GoLive ->
        model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    div [ ] [
        classroom_level model dispatch
    ]
