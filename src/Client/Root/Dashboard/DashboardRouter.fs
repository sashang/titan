module DashboardRouter

open Client.Shared
open Elmish
open Elmish.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Import
open Fable.Helpers.React

type PageModel =
    | TutorModel of Tutor.Dashboard.Model
    | StudentModel of Student.Dashboard.Model
    | TitanModel of Titan.Dashboard.Model

type Msg =
    | TutorMsg of Tutor.Dashboard.Msg
    | StudentMsg of Student.Dashboard.Msg
    | TitanMsg of Titan.Dashboard.Msg


type Model =
    { Child : PageModel }

let init_tutor (claims : TitanClaim) = 
    let tutor_model,cmd = Tutor.Dashboard.init claims
    {Child = TutorModel(tutor_model)}, Cmd.map TutorMsg cmd
    
let init_student claims =
    let student_model, cmd = Student.Dashboard.init claims
    {Child = StudentModel(student_model)}, Cmd.map StudentMsg cmd

let init_titan (claims : TitanClaim) =
    let titan_model, cmd = Titan.Dashboard.init claims
    {Child = TitanModel(titan_model)}, Cmd.map TitanMsg cmd


let update (model : Model) (msg : Msg) : Model * Cmd<Msg> =
    match model, msg with
    | {Child = TutorModel model}, TutorMsg msg  ->
        let new_model, cmd = Tutor.Dashboard.update model msg
        {Child = TutorModel new_model}, Cmd.map TutorMsg cmd

    | _, TutorMsg _  ->
        Browser.console.error("Received bad TutorMsg.")
        model, Cmd.none

    | {Child = StudentModel model}, StudentMsg msg  ->
        let new_model, cmd = Student.Dashboard.update model msg
        {Child = StudentModel new_model}, Cmd.map StudentMsg cmd

    | _, StudentMsg _  ->
        Browser.console.error("Received bad StudentMsg.")
        model, Cmd.none

    | {Child = TitanModel model}, TitanMsg msg  ->
        let new_model, cmd = Titan.Dashboard.update model msg
        {Child = TitanModel new_model}, Cmd.map TitanMsg cmd

    | _, TitanMsg _  ->
        Browser.console.error("Received bad TitanMsg.")
        model, Cmd.none
        
let view (model : Model) (dispatch : Msg -> unit) =
    match model with
    | {Child = TutorModel model} ->
        Tutor.Dashboard.view model (dispatch << TutorMsg)
        
    | {Child = StudentModel model} ->
        Student.Dashboard.view model (dispatch << StudentMsg)

    | {Child = TitanModel model } ->
        Titan.Dashboard.view model (dispatch << TitanMsg)
