module Tutor.Dashboard

open Client.Shared
open Domain
open Elmish
open ElmishBridgeModel
open Fable.Import
open Fable.React
open Fulma
open Thoth.Json

type PageModel =
    | SchoolModel of School.Model
    | ClassModel of Class.Model
    | EnrolModel of PendingStudents.Model
    | StudentsModel of StudentsComponent.Model

type Model =
    { Child : PageModel
      Claims : TitanClaim
      CurrentSession : OpenTokInfo option }

type Msg =
    | SchoolMsg of School.Msg
    | EnrolMsg of PendingStudents.Msg
    | ClassMsg of Class.Msg
    | StudentMsg of StudentsComponent.Msg
    | ClickStudents
    | ClickClassroom
    | ClickAccount
    | StudentRequestLiveState
    | ClickEnrol
    | SignOut


let init (claims : TitanClaim) : Model*Cmd<Msg> =
    let class_model, class_cmd = Class.init claims.Email
    Bridge.Bridge.Send(ClientIs(Tutor))
    { Child = ClassModel class_model; CurrentSession = None; Claims = claims },
    Cmd.map ClassMsg class_cmd

// Helper to generate a menu item
let menuItem label isActive dispatch msg =
    Menu.Item.li [ Menu.Item.IsActive isActive
                   Menu.Item.OnClick (fun e -> dispatch msg)
                   Menu.Item.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
       [ str label ]

// Helper to generate a sub menu
let subMenu label isActive children =
    li [ ]
       [ Menu.Item.a [ Menu.Item.IsActive isActive ]
            [ str label ]
         ul [ ]
            children ]


// Menu rendering
let view (model : Model) (dispatch : Msg -> unit) =
     Container.container
        //fluid to take up the width of the screen
        [ Container.IsFluid 
          Container.Modifiers [ Modifier.IsMarginless ]  ] [
            Columns.columns [ ] [
                Column.column [ Column.Width (Screen.All, Column.Is1) ] [
                    Menu.menu [ ] [
                        Menu.list [ ] [
                            menuItem "Classroom" false dispatch ClickClassroom
                            menuItem "Enrol" false dispatch ClickEnrol
                            menuItem "Students" false dispatch ClickStudents
                            menuItem "Account" false dispatch ClickAccount
                        ]
                    ]
                ]
                Column.column [ ] [
                   yield (match model.Child with
                          | ClassModel class_model ->
                               Class.view class_model (ClassMsg >> dispatch) 
                          | EnrolModel model ->
                               PendingStudents.view model (EnrolMsg >> dispatch) 
                          | SchoolModel model ->
                               School.view model (SchoolMsg >> dispatch) 
                          | StudentsModel model ->
                               StudentsComponent.view model (StudentMsg >> dispatch))
                ]
            ]
        ]
            

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match model, msg with
    | {Child = SchoolModel child_model}, SchoolMsg msg ->
        let new_state, cmd = School.update child_model msg
        {model with Child = SchoolModel new_state}, Cmd.map SchoolMsg cmd

    | {Child = SchoolModel child_model}, SignOut ->
        model, Cmd.none

    | _, SchoolMsg _ ->
        Browser.Dom.console.error("Unexpected SchoolMsg in Dashboard.Tutor")
        model, Cmd.none
    | {Child = ClassModel child_model}, ClassMsg msg ->
        let new_state, cmd = Class.update child_model msg
        {model with Child = ClassModel new_state}, Cmd.map ClassMsg cmd

    | {Child = ClassModel child_model}, SignOut ->
        let new_state, new_cmd = Class.update child_model Class.SignOut
        {model with Child = ClassModel new_state}, Cmd.map ClassMsg new_cmd

    | {Child = ClassModel child_model}, StudentRequestLiveState ->
        let new_state, new_cmd = Class.update child_model Class.StudentRequestLiveState
        {model with Child = ClassModel new_state}, Cmd.map ClassMsg new_cmd

    | _, ClassMsg _ ->
        Browser.Dom.console.error("Unexpected ClassMsg in Dashboard.Tutor")
        model, Cmd.none

    | {Child = StudentsModel child_model}, StudentMsg msg ->
        let new_state, cmd = StudentsComponent.update child_model msg
        {model with Child = StudentsModel new_state}, Cmd.map StudentMsg cmd

    | {Child = StudentsModel child_model}, SignOut ->
        let new_state, new_cmd = StudentsComponent.update child_model StudentsComponent.SignOut
        {model with Child = StudentsModel new_state}, Cmd.map StudentMsg new_cmd

    | _, StudentMsg _ ->
        Browser.Dom.console.error("Unexpected StudentMsg in Dashboard.Tutor")
        model, Cmd.none
    | {Child = EnrolModel child_model}, EnrolMsg msg ->
        let new_state, cmd = PendingStudents.update child_model msg
        {model with Child = EnrolModel new_state}, Cmd.map EnrolMsg cmd

    | {Child = EnrolModel child_model}, SignOut ->
        let new_state, cmd = PendingStudents.update child_model PendingStudents.SignOut
        {model with Child = EnrolModel new_state}, Cmd.map EnrolMsg cmd

    | _, EnrolMsg _ ->
        Browser.Dom.console.error("Unexpected EnrolMsg in Dashboard.Tutor")
        model, Cmd.none
    | model, ClickAccount ->
        let new_state, new_cmd = School.init ()
        {model with Child = SchoolModel new_state}, Cmd.map SchoolMsg new_cmd


    | model, ClickEnrol ->
        let new_state, new_cmd = PendingStudents.init ()
        {model with Child = EnrolModel new_state}, Cmd.map EnrolMsg new_cmd
    | model, ClickClassroom ->
        let new_state, new_cmd = Class.init model.Claims.Email
        {model with Child = ClassModel new_state}, Cmd.map ClassMsg new_cmd
    | model, ClickStudents ->
        let new_state, new_cmd = StudentsComponent.init ()
        {model with Child = StudentsModel new_state}, Cmd.map StudentMsg new_cmd
