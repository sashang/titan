module CreateSchool

open Domain
open Elmish
open Elmish.Browser.Navigation
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Fable.Core.JsInterop
open Fulma
open Shared
open Thoth.Json

exception CreateSchoolException of CreateSchoolResult

type Model =
    { SchoolCreationInfo : School
      Result : CreateSchoolResult option }

type Msg =
    | SetPrincipalName of string
    | SetSchoolName of string
    | ClickSubmit
    | Success of CreateSchoolResult
    | Failure of exn

let private submit (school : School) = promise {
    let body = Encode.Auto.toString(2, school)
    let props =
        [ RequestProperties.Method HttpMethod.POST
          RequestProperties.Credentials RequestCredentials.Include
          requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                           HttpRequestHeaders.Accept "application/json" ]
          RequestProperties.Body !^(body) ]
    let decoder = Decode.Auto.generateDecoder<CreateSchoolResult>()
    let! response = Fetch.tryFetchAs "/api/secure/create-school" decoder props
    match response with
    | Ok result ->
        match result.Codes with
        | CreateSchoolCode.Success::_ -> return result
        | _  -> return (raise (CreateSchoolException result))
    | Error e ->
        return (raise (CreateSchoolException {Codes = [CreateSchoolCode.FetchError]; Messages = [e]}))
}

let init () =
    {SchoolCreationInfo = {Name = ""; Principal = ""}; Result = None}

let private of_create_school_result (code : CreateSchoolCode) (result : CreateSchoolResult) =
    List.fold2
        (fun acc the_code the_message -> if code = the_code then acc + " " + the_message else acc)
        "" result.Codes result.Messages

let private make_error_from_result (result : CreateSchoolResult option) (code : CreateSchoolCode) =
    match result with
    | Some result ->
        match List.contains code result.Codes with
        | true ->
            Help.help [
                Help.Color IsDanger
                Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is5) ]
            ] [
                str (of_create_school_result code result)
            ]
        | false -> nothing 
    | _ ->  nothing

let view  (model : Model) (dispatch : Msg -> unit) = 

    Container.container [ Container.IsFullHD
                          Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [
        Column.column
            [ Column.Width (Screen.All, Column.Is4)
              Column.Offset (Screen.All, Column.Is4) ]
            [ Heading.h3
                [ Heading.Modifiers [ Modifier.TextColor IsGreyDark ] ]
                [ str "Create School" ]
              Heading.h4
                [ Heading.IsSubtitle
                  Heading.Modifiers [ Modifier.TextColor IsInfo ] ]
                [ str "Please enter your name and the name you want to give to your school." ]
              Box.box' [ ] [
                    Field.div [ ]
                        [ Control.div [ ]
                            [ Input.text
                                [ Input.Size IsLarge
                                  Input.Placeholder "Your Name"
                                  Input.Props [ 
                                    AutoFocus true
                                    OnChange (fun ev -> dispatch (SetPrincipalName ev.Value)) ] ] ] ]
                    Field.div [ ] [
                        Control.div [ ]
                            [ Input.text
                                [ Input.Size IsLarge
                                  Input.Placeholder "Your School Name"
                                  Input.Props [
                                    OnChange (fun ev -> dispatch (SetSchoolName ev.Value)) ] ] ] ]
                    make_error_from_result model.Result CreateSchoolCode.SchoolNameInUse
                    Field.div [] [ Client.Style.button dispatch ClickSubmit "Submit" ]
                    make_error_from_result model.Result CreateSchoolCode.DatabaseError
                    make_error_from_result model.Result CreateSchoolCode.Unknown
                    make_error_from_result model.Result CreateSchoolCode.FetchError
              ] 
        ]
    ]

let update  (model : Model) (msg : Msg): Model*Cmd<Msg> =
    match msg with
    | ClickSubmit ->
        model, Cmd.ofPromise submit model.SchoolCreationInfo Success Failure
    | SetPrincipalName principal_name ->
        {model with SchoolCreationInfo = {model.SchoolCreationInfo with Principal = principal_name}}, Cmd.none
    | SetSchoolName school_name ->
        {model with SchoolCreationInfo = {model.SchoolCreationInfo with Name = school_name}}, Cmd.none
    | Success _ ->
        model, Navigation.newUrl (Client.Pages.to_path Client.Pages.Dashboard)
    | Failure e ->
        Browser.console.info "Failed to create school"
        match e with
        | :? CreateSchoolException as ex -> //TODO: check this with someone who knows more. the syntax is weird, and Data0??
            { model with Result = Some ex.Data0 }, Cmd.none
        | e ->
            { model with Result = Some { Codes = [CreateSchoolCode.Unknown]; Messages = ["Unknown errror"] }}, Cmd.none