module FirstTime

open CustomColours
open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Elmish.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Helpers.React

type Category =
    | Tutor
    | Student

type Msg =
    | ClickGo
    | ClickCancel
    | ClickTutor
    | ClickStudent
    | Toggle
    | SetFirstName of string
    | SetLastName of string
    | SetSchoolName of string

type TutorFormModel =
    { SchoolName : string
      FirstName : string
      LastName : string }

type StudentFormModel =
    { FirstName : string
      LastName : string }

      
type CategoryForm =
    | TutorForm of TutorFormModel
    | StudentForm of StudentFormModel

type Model =
    { Active : bool
      SubForm : CategoryForm option }


let init active =
    {SubForm = None; Active = active}

let button (model : Model) (dispatch : Msg -> unit) text msg =
    Button.button [ Button.Color IsTitanInfo
                    Button.Props [ OnClick (fun e -> dispatch msg) ] ] [ str text ] 

let private input_field label text on_change =
    [ Field.div [ ] 
        [ Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
            [ Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ str label ] ]
          Control.div [ ]
            [ Input.text 
                [ Input.Value text
                  Input.OnChange on_change ] ] ] ]

let inner_content dispatch = function
    | TutorForm form ->
        Box.box' [ ]
            [ yield! input_field "First Name" form.FirstName (fun e -> dispatch (SetFirstName e.Value))
              yield! input_field "Last Name" form.LastName (fun e -> dispatch (SetLastName e.Value))
              yield! input_field "School Name" form.SchoolName (fun e -> dispatch (SetSchoolName e.Value)) ]

    | StudentForm form ->
        Box.box' [ ]
            [ yield! input_field "First Name" form.FirstName (fun e -> dispatch (SetFirstName e.Value))
              yield! input_field "Last Name" form.LastName (fun e -> dispatch (SetLastName e.Value)) ]

let content (model : Model) (dispatch : Msg -> unit) =
    div [ ] [
        Columns.columns []
            [ Column.column [ ]
                [ button model dispatch "Tutor" ClickTutor ]
              Column.column [ ]
                [ button model dispatch "Student" ClickStudent ] ]
                
        (match model.SubForm with
          | Some form -> inner_content dispatch form
          | None -> nothing)
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    div [ ] [
      Modal.modal [ Modal.IsActive model.Active ]
            [ Modal.background [ Props [ OnClick (fun e -> dispatch ClickCancel)] ] [ ]
              Modal.Card.card [ ]
                [ Modal.Card.head [ Common.Modifiers [ Modifier.BackgroundColor IsTitanPrimary ] ]
                    [ Modal.Card.title [Common.Modifiers [ Modifier.TextColor IsWhite ]  ]
                        [ str "Welcome!" ] ]
                  Modal.Card.body [ ]
                    [ content model dispatch ]
                  Modal.Card.foot [ ]
                    [ Button.button [ Button.Color IsTitanInfo ]
                        [ str "Go!" ]
                      Button.button [ Button.Props [ OnClick (fun e -> dispatch ClickCancel) ] ]
                        [ str "Cancel" ] ] ] ] ]

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match model, msg with
    | model, Toggle -> {model with Active = not model.Active}, Cmd.none

    | model, ClickCancel -> {model with Active = false}, Cmd.none

    | model, ClickStudent ->
        {model with SubForm = Some (StudentForm {FirstName = ""; LastName = ""})}, Cmd.none

    | model, ClickTutor ->
        {model with SubForm = Some (TutorForm {FirstName = ""; LastName = ""; SchoolName = ""})}, Cmd.none

    | {SubForm = Some(StudentForm student_form)}, SetLastName name ->
        {model with SubForm = Some (StudentForm {student_form with StudentFormModel.LastName = name})}, Cmd.none

    | {SubForm = Some(StudentForm student_form)}, SetFirstName name ->
        {model with SubForm = Some (StudentForm {student_form with StudentFormModel.FirstName = name})}, Cmd.none

    | {SubForm = Some(TutorForm tutor_form)}, SetLastName name ->
        {model with SubForm = Some (TutorForm {tutor_form with TutorFormModel.LastName = name})}, Cmd.none

    | {SubForm = Some(TutorForm tutor_form)}, SetFirstName name ->
        {model with SubForm = Some (TutorForm {tutor_form with TutorFormModel.FirstName = name})}, Cmd.none

    | {SubForm = Some(TutorForm tutor_form)}, SetSchoolName name ->
        {model with SubForm = Some (TutorForm {tutor_form with TutorFormModel.SchoolName = name})}, Cmd.none