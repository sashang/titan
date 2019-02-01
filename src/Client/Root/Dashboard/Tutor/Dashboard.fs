module Tutor.Dashboard

open Domain
open Elmish
open Elmish.React
open Fable.Helpers.React.Props
open Fable.PowerPack
open Fable.Import
open Fable.Helpers.React
open Fulma
open Thoth.Json



type LiveState =
    | On
    | Off

type Model =
    { School : School.Model
      Class : Class.Model
      Pending : PendingStudents.Model
      Students : StudentsComponent.Model
      CurrentSession : OpenTokInfo option
      LiveState : LiveState }

type Msg =
    | GoLive
    | StopLive
    | SchoolMsg of School.Msg
    | PendingMsg of PendingStudents.Msg
    | ClassMsg of Class.Msg
    | StudentMsg of StudentsComponent.Msg


let init () : Model*Cmd<Msg> =
    let school_model, school_cmd = School.init ()
    let class_model, class_cmd = Class.init ()
    let student_model, student_cmd = StudentsComponent.init ()
    let pending_model, pending_cmd = PendingStudents.init ()
    { School = school_model; Class = class_model
      Pending = pending_model; Students = student_model;
      LiveState = Off; CurrentSession = None },
    Cmd.batch [ Cmd.map SchoolMsg school_cmd
                Cmd.map ClassMsg class_cmd
                Cmd.map PendingMsg pending_cmd
                Cmd.map StudentMsg student_cmd ]


let view_in_classroom (model : Model) (dispatch : Msg -> unit) =
    [ Columns.columns [ ]
        [ Column.column [ Column.Width (Screen.All, Column.Is4) ]
            [ yield! School.view model.School (SchoolMsg >> dispatch)
              yield! PendingStudents.view model.Pending (PendingMsg >> dispatch) 
              yield! StudentsComponent.view model.Students (StudentMsg >> dispatch) ]
          Column.column [ ]
            [ yield! Class.view model.Class (ClassMsg >> dispatch) ] ] ]

// Menu rendering
let view (model : Model) (dispatch : Msg -> unit) =
     Container.container
        //fluid to take up the width of the screen
        [ Container.IsFluid ]
        (view_in_classroom model dispatch)
            

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match msg with
    | SchoolMsg msg ->
        let new_state, cmd = School.update model.School msg
        {model with School = new_state}, Cmd.map SchoolMsg cmd
    | ClassMsg msg ->
        let new_state, cmd = Class.update model.Class msg
        {model with Class = new_state}, Cmd.map ClassMsg cmd
    | StudentMsg msg ->
        let new_state, cmd = StudentsComponent.update model.Students msg
        {model with Students = new_state}, Cmd.map StudentMsg cmd
    | PendingMsg msg ->
        match msg with
        | PendingStudents.ApprovePendingSuccess () ->
            let new_student_state, student_cmd = StudentsComponent.update model.Students StudentsComponent.GetAllStudents
            let new_pending_state, pending_cmd = PendingStudents.update model.Pending msg
            {model with Pending = new_pending_state
                        Students = new_student_state},
            Cmd.batch [Cmd.map PendingMsg pending_cmd; Cmd.map StudentMsg student_cmd]
        | _ ->
            let new_pending_state, pending_cmd = PendingStudents.update model.Pending msg
            {model with Pending = new_pending_state}, Cmd.map PendingMsg pending_cmd

    | GoLive ->
        Browser.console.info ("Tutor.Dashboard.GoLive")
        {model with LiveState = LiveState.On}, Cmd.ofMsg (ClassMsg Class.GoLive)

    | StopLive ->
        Browser.console.info ("Tutor.Dashboard.StopLive")
        {model with LiveState = LiveState.Off}, Cmd.ofMsg (ClassMsg Class.StopLive)
        