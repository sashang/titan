module FirstTime

open Client.Shared
open Client.Style
open CustomColours
open Domain
open Elmish
open Elmish.React
open Elmish.Navigation
open Fable.React
open Fable.React.Props
open Fable.Import
open Fulma
open ModifiedFableFetch
open Thoth.Json
open Thoth.Fetch
open ValueDeclarations

type Category =
    | Tutor
    | Student

type Msg =
    | ClickGo
    | ClickCancel
    | ClickTutor
    | ClickStudent
    | ClickConsent
    | ClickAcceptTerms
    | SetFirstName of string
    | SetLastName of string
    | SetSchoolName of string
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
      Consent : bool //student has parent or guardian consent
      Error : APIError option }
    static member init active claims = 
        {Active = true; Accepted = false; SubForm = None;
         Claims = claims; Error = None; Consent = false}

exception RegisterEx of APIError

let submit_tutor (tutor : TutorRegister) = promise {
    if not tutor.is_valid then
        return (raise (RegisterEx (APIError.init [APICode.SchoolName] ["Name cannot be blank"])))
    else
        let request = make_post 4 tutor
        let decoder = Decode.Auto.generateDecoder<APIError option>()
        let! response = Fetch.tryFetchAs("/api/register-tutor", decoder, request)
        Browser.Dom.console.info "received response from register-tutor"
        return map_api_error_result response RegisterEx
}

let private submit_student (student : StudentRegister) = promise {
    if not student.is_valid then
        return (raise (RegisterEx (APIError.init [APICode.Failure] ["Invalid input"])))
    else
        let request = make_post 3 student
        let decoder = Decode.Auto.generateDecoder<APIError option>()
        let! response = Fetch.tryFetchAs("/api/register-student", decoder, request)
        Browser.Dom.console.info "received response from register-student"
        return map_api_error_result response RegisterEx
}

let init active claims =
    Model.init active claims



let inner_content model dispatch = function
    | TutorForm form ->
        Box.box' [ ]
            [ yield! input_field  model.Error APICode.FirstName
                  "First Name" form.FirstName (fun e -> dispatch (SetFirstName e.Value))
              yield! input_field model.Error APICode.LastName
                "Last Name" form.LastName (fun e -> dispatch (SetLastName e.Value)) 
              yield! input_field model.Error APICode.SchoolName
                "School Name" form.SchoolName (fun e -> dispatch (SetSchoolName e.Value))
              yield! input_field_ro "Email" form.Email
              yield checkbox ACCEPT_TERMS model.Accepted dispatch ClickAcceptTerms  ]

    | StudentForm form ->
        Box.box' [ ]
            [ yield! input_field model.Error APICode.FirstName 
                "First Name" form.FirstName (fun e -> dispatch (SetFirstName e.Value))
              yield! input_field model.Error APICode.LastName
                "Last Name" form.LastName (fun e -> dispatch (SetLastName e.Value))
              yield! input_field_ro "Email" form.Email 
              yield checkbox CONSENT model.Consent dispatch ClickConsent
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

    let is_go_enabled = 
        match model.SubForm with
        | Some (TutorForm _) ->
            model.Accepted
        | Some (StudentForm _) ->
            model.Accepted && model.Consent
        | _ -> false


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
                    [ button_enabled dispatch ClickGo "Go!" is_go_enabled
                      Button.button [ Button.Props [ OnClick (fun e -> dispatch ClickCancel) ] ]
                        [ str "Cancel" ] ]
                  notification APICode.Database model.Error ] ] ]

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match model, msg with

    | model, ClickCancel -> {model with Active = false}, Cmd.none

    | {Accepted = false; SubForm = Some (TutorForm _)} , ClickGo ->
        model, Cmd.none
        
    | {Accepted = true; SubForm = Some (TutorForm tutor_model)} , ClickGo ->
        model, Cmd.OfPromise.either 
                    submit_tutor { TutorRegister.FirstName = tutor_model.FirstName
                                   TutorRegister.LastName = tutor_model.LastName
                                   TutorRegister.Email = tutor_model.Email
                                   TutorRegister.SchoolName = tutor_model.SchoolName }
                    Success
                    Failure

    | {Consent = false; Accepted = _; SubForm = Some (StudentForm _)}, ClickGo ->
        model, Cmd.none

    | {Consent = _; Accepted = false; SubForm = Some(StudentForm _)}, ClickGo ->
        model, Cmd.none

    | {Consent = true; Accepted = true; SubForm = Some(StudentForm student_model)}, ClickGo ->
       {model with Active = true}, 
       Cmd.OfPromise.either submit_student 
          {StudentRegister.FirstName = student_model.FirstName
           StudentRegister.LastName = student_model.LastName
           StudentRegister.Email = student_model.Email } Success Failure

    | model, Success _ ->
        {model with Active = false
                    SubForm = None},
        Navigation.newUrl (Pages.to_path Pages.Home)

    | model, Failure e ->
        match e with
        | :? RegisterEx as ex ->
            List.iter (fun m -> Browser.Dom.console.warn ("RegisterEx:" + m)) ex.Data0.Messages
            {model with Error = Some ex.Data0}, Cmd.none
        | e ->
            Browser.Dom.console.warn ("Failed to submit form " + e.Message)
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
        {model with Accepted = not model.Accepted}, Cmd.none

    | model, ClickConsent ->
        {model with Consent = not model.Consent}, Cmd.none
    
    | _, msg ->
        Browser.Dom.console.warn(sprintf "message not handled %A" msg)
        model,Cmd.none