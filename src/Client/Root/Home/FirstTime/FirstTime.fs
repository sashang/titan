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
    | ClickTutor
    | ClickStudent
    | Toggle

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

let private input_field label placeholder text on_change =
    [ Field.div [ ] 
        [ Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
            [ Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ str label ] ]
          Control.div [ ]
            [ Input.text 
                [ Input.Value text
                  Input.Placeholder placeholder
                  Input.OnChange on_change ] ] ] ]
let inner_content (category_form : CategoryForm ) dispatch =
    Box.box' [ ]
        [ input_field "First Name" "" cate.]

let content (model : Model) (dispatch : Msg -> unit) =
    div [ ] [
        Columns.columns []
            [ Column.column [ ]
                [ button model dispatch "Tutor" ClickTutor ]
              Column.column [ ]
                [ button model dispatch "Student" ClickStudent ] ]
                
        Columns.columns [ ] 
            [ inner_connent model dispatch ]
    ]
            


let view (model : Model) (dispatch : Msg -> unit) =
    div [ ] [
      Modal.modal [ Modal.IsActive model.Active ]
            [ Modal.background [ Props [ ] ] [ ]
              Modal.Card.card [ ]
                [ Modal.Card.head [ Common.Modifiers [ Modifier.BackgroundColor IsTitanPrimary ] ]
                    [ Modal.Card.title [Common.Modifiers [ Modifier.TextColor IsWhite ]  ]
                        [ str "Welcome!" ]
                      Delete.delete [ ] [ ] ]
                  Modal.Card.body [ ]
                    [ content model dispatch ]
                  Modal.Card.foot [ ]
                    [ Button.button [ Button.Color IsTitanInfo ]
                        [ str "Go!" ]
                      Button.button [ ]
                        [ str "Cancel" ] ] ] ] ]

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match model, msg with
    | model, Toggle -> {model with Active = not model.Active}, Cmd.none

    | model, ClickStudent ->
        {model with SubForm = Some (StudentForm {StudentFormModel.FirstName = ""; LastName = ""})}, Cmd.none

    | model, ClickTutor ->
        {model with SubForm = Some (TutorForm {TutorForm.FirstName = ""; LastName = ""; SchoolName = ""})}, Cmd.none