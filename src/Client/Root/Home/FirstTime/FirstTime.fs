module FirstTime

open Client.Shared
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
open Thoth.Json

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
      Claims : TitanClaim}
    static member init active claims = {Active = true; SubForm = None; Claims = claims}

exception StudentRegisterEx of APIError

let submit_tutor (tutor : TutorRegister) = promise {
    Browser.console.info "start  submit-tutor"
    let body = Encode.Auto.toString (4, tutor)
    let props =
        [ RequestProperties.Method HttpMethod.POST
          RequestProperties.Credentials RequestCredentials.Include
          requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                           HttpRequestHeaders.Accept "application/json"]
          RequestProperties.Body !^(body) ] 
    let decoder = Decode.Auto.generateDecoder<TutorRegisterResult>()
    let! response = Fetch.tryFetchAs "/secure/api/register-tutor" decoder props
    Browser.console.info "received response from register-tutor"
    match response with
    | Ok result ->
        match result.Error with
        | Some error -> 
            Browser.console.info ("got some error: " + (List.head error.Messages))
            return (raise (StudentRegisterEx error))
        | _ ->
            return ()
    | Error e ->
        Browser.console.info ("got fetch error: " + e)
        return (raise (StudentRegisterEx (APIError.init [APICode.FetchError] [e])))
}

let private submit_student (student : StudentRegister) = promise {
    let body = Encode.Auto.toString (3, student)
    let props =
        [ RequestProperties.Method HttpMethod.POST
          RequestProperties.Credentials RequestCredentials.Include
          requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                           HttpRequestHeaders.Accept "application/json"]
          RequestProperties.Body !^(body) ] 
    let decoder = Decode.Auto.generateDecoder<StudentRegisterResult>()
    let! response = Fetch.tryFetchAs "/secure/api/register-student" decoder props
    match response with
    | Ok result ->
        match result.Error with
        | Some error -> 
            Browser.console.info ("got some error: " + (List.head error.Messages))
            return (raise (StudentRegisterEx error))
        | _ ->
            return ()
    | Error e ->
        Browser.console.info ("got fetch error: " + e)
        return (raise (StudentRegisterEx (APIError.init [APICode.FetchError] [e])))
}

let init active claims =
    Model.init active claims

let button (model : Model) (dispatch : Msg -> unit) text msg =
    Button.button [ Button.Color IsTitanInfo
                    Button.Props [ OnClick (fun e -> dispatch msg) ] ] [ str text ] 

let private input_field_ro label text =
    [ Field.div [ ] 
        [ Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
            [ Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ str label ] ]
          Control.div [ ]
            [ Input.text 
                [ Input.Value text
                  Input.IsReadOnly true ]]] ]

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
              yield! input_field "School Name" form.SchoolName (fun e -> dispatch (SetSchoolName e.Value))
              yield! input_field_ro "Email" form.Email  ]

    | StudentForm form ->
        Box.box' [ ]
            [ yield! input_field "First Name" form.FirstName (fun e -> dispatch (SetFirstName e.Value))
              yield! input_field "Last Name" form.LastName (fun e -> dispatch (SetLastName e.Value))
              yield! input_field_ro "Email" form.Email]

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
                    [ button model dispatch "Go!" ClickGo
                      Button.button [ Button.Props [ OnClick (fun e -> dispatch ClickCancel) ] ]
                        [ str "Cancel" ] ] ] ] ]

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match model, msg with
    | model, Toggle -> {model with Active = not model.Active}, Cmd.none

    | model, ClickCancel -> {model with Active = false}, Cmd.none

    | model, SuccessTest test -> 
        Browser.console.info "asdasd"
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