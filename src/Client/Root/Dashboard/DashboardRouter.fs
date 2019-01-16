module DashboardRouter

open Elmish
open Elmish.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Helpers.React

type PageModel =
    | TutorModel of Tutor.Dashboard.Model
    | StudentModel
    | TitanModel

type Msg =
    | TutorMsg of Tutor.Dashboard.Msg
    | StudentMsg


type Model =
    { Child : PageModel }

let update (model : Model) (msg : Msg) : Model * Cmd<Msg> =
    match model, msg with
    | {Child = TutorModel model}, TutorMsg msg  ->
        let new_model, cmd = Tutor.Dashboard.update model msg
        {Child = TutorModel new_model}, Cmd.map TutorMsg cmd

let view (model : Model) (dispatch : Msg -> unit) =
    match model with
    | {Child = TutorModel model} ->
        Tutor.Dashboard.view model (dispatch << TutorMsg)