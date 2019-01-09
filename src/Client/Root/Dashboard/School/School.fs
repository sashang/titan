module School

open CustomColours
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
exception LoadSchoolException of LoadSchoolResult

type Model =
    { TheSchool : School
      Result : CreateSchoolResult option
      LoadSchoolResult : LoadSchoolResult option }

type Msg =
    | SetPrincipalName of string
    | SetSchoolName of string
    | ClickSave
    | Success of CreateSchoolResult
    | LoadSchoolSuccess of Domain.School
    | Failure of exn

let private load_school () = promise {
    let props =
        [ RequestProperties.Method HttpMethod.GET
          RequestProperties.Credentials RequestCredentials.Include
          requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                           HttpRequestHeaders.Accept "application/json" ]]
    let decoder = Decode.Auto.generateDecoder<Domain.LoadSchoolResult>()
    let! response = Fetch.tryFetchAs "/api/secure/load-school" decoder props
    match response with
    | Ok result ->
        match result.Codes with
        | LoadSchoolCode.Success::_ -> 
            return result.TheSchool
        | _ ->
            return (raise (LoadSchoolException result))
    | Error e ->
        return (raise (LoadSchoolException {Codes = [LoadSchoolCode.FetchError]; Messages = [e];
            TheSchool = {School.Principal = ""; School.Name = ""}}))
}

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

let init () : Model*Cmd<Msg> =
    {TheSchool = {Name = ""; Principal = ""}; Result = None; LoadSchoolResult = None},
     Cmd.ofPromise load_school () LoadSchoolSuccess Failure

let private of_create_school_result (code : CreateSchoolCode) (result : CreateSchoolResult) =
    List.fold2
        (fun acc the_code the_message -> if code = the_code then acc + " " + the_message else acc)
        "" result.Codes result.Messages

let private of_load_school_result (code : LoadSchoolCode) (result : LoadSchoolResult) =
    List.fold2
        (fun acc the_code the_message -> if code = the_code then acc + " " + the_message else acc)
        "" result.Codes result.Messages

let private std_label text = 
    Label.label 
        [ Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
        [ str text ]

let private make_error_from_load_school_result (result : LoadSchoolResult option) (code : LoadSchoolCode) =
    match result with
    | Some result ->
        Help.help [
            Help.Color IsDanger
            Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is5) ]
        ] [
            str (of_load_school_result code result)
        ]
    | _ ->  nothing

let private make_error_from_result (result : CreateSchoolResult option) (code : CreateSchoolCode) =
    match result with
    | Some result ->
        match List.contains code result.Codes with
        | true ->
            Help.help [
                Help.Color IsDanger
                Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is6) ]
            ] [
                str (of_create_school_result code result)
            ]
        | false -> nothing 
    | _ ->  nothing

let private help_first_time_user (result : LoadSchoolResult option) =
    match result with
    | Some result ->
        match List.contains LoadSchoolCode.NoSchool result.Codes with
        | true ->
            Help.help
                [ Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is6) 
                                   Modifier.TextAlignment (Screen.All, TextAlignment.Left)] ]
                [ str "Enter your name." ]
        | false -> std_label "Name"
    | _ -> std_label "Name"

let private school_name_help_first_time_user (result : LoadSchoolResult option) =
    match result with
    | Some result ->
        match List.contains LoadSchoolCode.NoSchool result.Codes with
        | true ->
            Help.help
                [ Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is6)
                                   Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                [ str "Enter the name of your school." ]
        | false -> std_label "School Name"
    | _ -> std_label "School Name"

let private field label placeholder =
    [ Field.div [ ] 
        [ Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
            [ Field.p [ ] [ str label ] ]
          Control.div [ ]
            [ Input.text [Input.Placeholder placeholder ] ] ] ]

let private image_holder url =
    [ Image.image [ ]
        [ img [ Src url ] ] ]

let school_content = 
    [ Columns.columns [ ]
        [ Column.column [ ]
            [ yield! field "Headmaster" "eg: Mr Skinner"
              yield! field "School Name" "eg: Sunnydale High" ] ] ]


let private save_button dispatch msg text =
    Button.button [
        Button.Color IsTitanInfo
        Button.OnClick (fun _ -> (dispatch msg))
    ] [ str text ]

let view  (model : Model) (dispatch : Msg -> unit) = 
    [ Card.card [ ] 
        [ Card.header [ ]
            [ Card.Header.title [ ] [ ] ]
          Card.image [ ]
            [ yield! image_holder "Images/school.png" ]
          Card.content [ ]
            [ yield! school_content ] 
          Card.footer [ ] 
            [ Card.Footer.div [ ] 
                [ Level.level [ ]
                    [ Level.left [ ]
                        [ Level.item [ ]
                            [ save_button dispatch ClickSave "Save" ] ] ] ] ] ] ]

let update  (model : Model) (msg : Msg): Model*Cmd<Msg> =
    match msg with
    | ClickSave ->
        model, Cmd.ofPromise submit model.TheSchool Success Failure
    | SetPrincipalName principal_name ->
        {model with TheSchool = {model.TheSchool with Principal = principal_name}}, Cmd.none
    | SetSchoolName school_name ->
        {model with TheSchool = {model.TheSchool with Name = school_name}}, Cmd.none
    | Success _ ->
        model, Cmd.none
    | LoadSchoolSuccess result ->
        {model with Model.TheSchool = result}, Cmd.none
    | Failure e ->
        match e with
        | :? CreateSchoolException as ex -> //TODO: check this with someone who knows more. the syntax is weird, and Data0??
            { model with Result = Some ex.Data0 }, Cmd.none
        | :? LoadSchoolException as ex -> //TODO: check this with someone who knows more. the syntax is weird, and Data0??
            { model with LoadSchoolResult = Some ex.Data0 }, Cmd.none
        | e ->
            { model with Result = Some { Codes = [CreateSchoolCode.Unknown]; Messages = ["Unknown errror"] }}, Cmd.none