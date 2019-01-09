module Dashboard

open Domain
open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Elmish.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Helpers.React


type Model =
    { School : School.Model
      Class : Class.Model
      Students : StudentsComponent.Model }

type Msg =
    | SchoolMsg of School.Msg
    | ClassMsg of Class.Msg
    | StudentMsg of StudentsComponent.Msg

let init () : Model*Cmd<Msg> =
    let school_model, school_cmd = School.init ()
    let class_model, class_cmd = Class.init ()
    let student_model, student_cmd = StudentsComponent.init ()
    { School = school_model
      Class = class_model
      Students = student_model }, Cmd.batch [ Cmd.map SchoolMsg school_cmd
                                              Cmd.map ClassMsg class_cmd
                                              Cmd.map StudentMsg student_cmd ]
// Helper to generate a menu item
let menuItem label isActive dispatch msg =
    Menu.Item.li [ Menu.Item.IsActive isActive
                   Menu.Item.OnClick (fun e -> dispatch msg)]
       [ Text.p [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left)
                              Modifier.TextSize (Screen.All, TextSize.Is5)
                              Modifier.TextColor Color.IsWhite
                              Modifier.TextTransform TextTransform.UpperCase ] ]
           [ str label ] ]
// Helper to generate a sub menu
let subMenu label isActive children =
    li [ ]
       [ Menu.Item.a [ Menu.Item.IsActive isActive ]
            [ str label ]
         ul [ ]
            children ]

// let menu dispatch = 
//     Box.box' [ Common.Props [ Style [ Height "100%" ] ] ]
//          [ Menu.menu [ Modifiers [ Modifier.BackgroundColor IsPrimary ] ]
//             [ Menu.list [ ]
//                 [ menuItem "School" false dispatch ClickSchool
//                   menuItem "Classes" false dispatch ClickSchool
//                   menuItem "Enrollment" false dispatch ClickEnroll ] ] ]

let main_dashboard (model : Model) (dispatch : Msg -> unit) =
    Tile.ancestor []
        [ Tile.parent [ ]
            [ Tile.child [ ]
                [ Box.box' [ Common.Props [ Style [ Height "100%" ] ] ]
                    [ Heading.p [ ] [ str "Live" ] ] ] ] ]

// Menu rendering
let view (model : Model) (dispatch : Msg -> unit) =
     Container.container
        //fluid to take up the width of the screen
        [ Container.IsFluid ]
        //yield! because the result of the expression is a list.
        //[ yield! (model.ChildElements |> List.map (dispatch_view model dispatch)) ]
        [ Columns.columns [ ]
            [ Column.column [ Column.Width (Screen.All, Column.Is3) ]
                [ yield! School.view model.School (SchoolMsg >> dispatch) ]
              Column.column [ ]
                [ yield! Class.view model.Class (ClassMsg >> dispatch) ] ]
          Columns.columns [ ]
            [ Column.column [ ] 
                [ yield! StudentsComponent.view model.Students (StudentMsg >> dispatch) ] ] ]

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
        {model with Students = new_state}, Cmd.map ClassMsg cmd

