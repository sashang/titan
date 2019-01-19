module FirstTime

open Client.Shared
open Client.Style
open CustomColours
open Domain
open Elmish
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Fable.Core.JsInterop
open Fulma
open ModifiedFableFetch
open Thoth.Json
open ValueDeclarations

type Category =
    | Tutor
    | Student

type Msg =
    | ClickGo
    | ClickCancel
    | ClickTutor
    | ClickStudent
    | Toggle
    | ClickAcceptTerms
    | SetFirstName of string
    | SetLastName of string
    | SetSchoolName of string
    | SuccessTest of bool
    | Success of unit
    | Failure of exn

type TutorFormModel =
    { SchoolName : string
      FirstName : string
      LastName : string
      Email : string }
      static member init fn ln email = {FirstName = fn; LastName = ln; Email = email; SchoolName = ""}

type StudentFormModel =
    { FirstName : string
      LastName : string
      Email : string }
      static member init fn ln email = {FirstName = fn; LastName = ln; Email = email}
      
type CategoryForm =
    | TutorForm of TutorFormModel
    | StudentForm of StudentFormModel

type Model =
    { Active : bool
      SubForm : CategoryForm option
      Claims : TitanClaim
      Accepted : bool
      Error : APIError }
    static member init active claims = 
        {Active = true; Accepted = false; SubForm = None; Claims = claims; Error = APIError.init_empty}

exception RegisterEx of APIError

let submit_tutor (tutor : TutorRegister) = promise {
    if not tutor.is_valid then
        return (raise (RegisterEx (APIError.init [APICode.SchoolName] ["Name cannot be blank"])))
    else
        let request = make_request 4 tutor
        let decoder = Decode.Auto.generateDecoder<APIError>()
        let! response = Fetch.tryFetchAs "/api/register-tutor" decoder request
        Browser.console.info "received response from register-tutor"
        return unwrap_response response RegisterEx
}

let private submit_student (student : StudentRegister) = promise {
    if not student.is_valid then
        return (raise (RegisterEx (APIError.init [APICode.Failure] ["Invalid input"])))
    else
        let request = make_request 3 student
        let decoder = Decode.Auto.generateDecoder<APIError>()
        let! response = Fetch.tryFetchAs "/api/register-student" decoder request
        Browser.console.info "received response from register-student"
        return unwrap_response response RegisterEx
}

let init active claims =
    Model.init active claims



let inner_content model dispatch = function
    | TutorForm form ->
        Box.box' [ ]
            [ yield! input_field_with_error "First Name" form.FirstName
                (fun e -> dispatch (SetFirstName e.Value)) APICode.FirstName model.Error
              yield! input_field_with_error "Last Name" form.LastName
                (fun e -> dispatch (SetLastName e.Value)) APICode.LastName model.Error
              yield! input_field_with_error "School Name" form.SchoolName
                (fun e -> dispatch (SetSchoolName e.Value)) APICode.SchoolName model.Error
              yield! input_field_ro "Email" form.Email APICode.Email model.Error
              yield checkbox ACCEPT_TERMS model.Accepted dispatch ClickAcceptTerms  ]

    | StudentForm form ->
        Box.box' [ ]
            [ yield! input_field_with_error "First Name" form.FirstName
                (fun e -> dispatch (SetFirstName e.Value)) APICode.FirstName model.Error
              yield! input_field_with_error "Last Name" form.LastName
                (fun e -> dispatch (SetLastName e.Value)) APICode.LastName model.Error
              yield! input_field_ro "Email" form.Email APICode.Email model.Error
              yield checkbox ACCEPT_TERMS model.Accepted dispatch ClickAcceptTerms ] 

let content (model : Model) (dispatch : Msg -> unit) =
    let is_tutor_form subform =
        match subform with
        | Some (TutorForm _) -> true
        | _ -> false

    let is_student_form subform = 
        match subform with
        | Some (StudentForm _) -> true
        | _ -> false

    div [ ] [
        Columns.columns []
            [ Column.column [ ]
                [ button_enabled dispatch ClickTutor "Tutor" true ]
              Column.column [ ]
                [ button_enabled dispatch ClickStudent "Student" true ] ]
                
        (match model.SubForm with
        | Some form -> inner_content model dispatch form
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
                    [ button_enabled dispatch ClickGo "Go!" model.Accepted
                      Button.button [ Button.Props [ OnClick (fun e -> dispatch ClickCancel) ] ]
                        [ str "Cancel" ] ]
                  notification APICode.Database model.Error ] ] ]

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match model, msg with
    | model, Toggle -> {model with Active = not model.Active}, Cmd.none

    | model, ClickCancel -> {model with Active = false}, Cmd.none

    | model, SuccessTest test -> 
        Browser.console.info "asdasd"
        model, Cmd.none

    | {Accepted = false} , ClickGo ->
        Browser.console.info "user must accept terms before proceeding"
        model, Cmd.none

    | model, ClickGo ->
        Browser.console.info "received click go"
        match model.SubForm with
        | Some (StudentForm student_model) ->
           {model with Active = true}, 
           Cmd.ofPromise submit_student 
              {StudentRegister.FirstName = student_model.FirstName
               StudentRegister.LastName = student_model.LastName
               StudentRegister.Email = student_model.Email } Success Failure

        | Some (TutorForm tutor_model) ->
            Browser.console.info "this is a tutor form"
            model, Cmd.ofPromise
                        submit_tutor 
                        { TutorRegister.FirstName = tutor_model.FirstName
                          TutorRegister.LastName = tutor_model.LastName
                          TutorRegister.Email = tutor_model.Email
                          TutorRegister.SchoolName = tutor_model.SchoolName }
                        Success
                        Failure

        | _ ->
            Browser.console.info "doing nothing"
            model, Cmd.none
    | model, Success () ->
        {model with Active = false}, Cmd.none

    | model, Failure e ->
        match e with
        | :? RegisterEx as ex ->
            List.iter (fun m -> Browser.console.warn ("RegisterEx:" + m)) ex.Data0.Messages
            {model with Error = ex.Data0}, Cmd.none
        | e ->
            Browser.console.warn ("Failed to submit form " + e.Message)
            model, Cmd.none

    | model, ClickStudent ->
        {model with SubForm = Some (StudentForm (StudentFormModel.init model.Claims.GivenName model.Claims.Surname model.Claims.Email))}, Cmd.none

    | model, ClickTutor ->
        {model with SubForm = Some (TutorForm (TutorFormModel.init model.Claims.GivenName model.Claims.Surname model.Claims.Email))}, Cmd.none

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

    | model, ClickAcceptTerms ->
        Browser.console.info "Accepted terms"
        //toggle when the user clicks accept terms
        {model with Accepted = not model.Accepted}, Cmd.none