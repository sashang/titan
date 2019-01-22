module DashboardRouter

open Elmish
open Elmish.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Helpers.React

type PageModel =
    | TutorModel of Tutor.Dashboard.Model
    | StudentModel of Student.Dashboard.Model
    | TitanModel

type Msg =
    | TutorMsg of Tutor.Dashboard.Msg
    | StudentMsg of Student.Dashboard.Msg


type Model =
    { Child : PageModel }

let init_tutor = 
    let tutor_model,cmd = Tutor.Dashboard.init ()
    {Child = TutorModel(tutor_model)}, Cmd.map TutorMsg cmd
    
let init_student =
    let student_model, cmd = Student.Dashboard.init ()
    {Child = StudentModel(student_model)}, Cmd.map StudentMsg cmd

let update (model : Model) (msg : Msg) : Model * Cmd<Msg> =
    match model, msg with
    | {Child = TutorModel model}, TutorMsg msg  ->
        let new_model, cmd = Tutor.Dashboard.update model msg
        {Child = TutorModel new_model}, Cmd.map TutorMsg cmd

    | {Child = StudentModel model}, StudentMsg msg  ->
        let new_model, cmd = Student.Dashboard.update model msg
        {Child = StudentModel new_model}, Cmd.map StudentMsg cmd
        
let view (model : Model) (dispatch : Msg -> unit) =
    match model with
    | {Child = TutorModel model} ->
        Tutor.Dashboard.view model (dispatch << TutorMsg)
        
    | {Child = StudentModel model} ->
        Student.Dashboard.view model (dispatch << StudentMsg)
