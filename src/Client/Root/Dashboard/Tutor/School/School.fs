module School

open Client
open Client.Style
open CustomColours
open Domain
open Elmish
open Fable.Helpers.React
open Fable.Import
open Fable.Helpers.React.Props
open Fable.PowerPack
open Fulma
open Fulma
open Fulma
open ModifiedFableFetch
open Thoth.Json

exception SaveEx of APIError
exception LoadSchoolEx of APIError
exception LoadUserEx of APIError

type Model =
    { SchoolName : string
      FirstName : string
      Subjects : string
      LastName : string
      Info : string
      Error : APIError option}

type Msg =
    | SetSchoolName of string
    | SetFirstName of string
    | SetLastName of string
    | SetInfo of string
    | ClickSave
    | SaveSuccess of unit
    | Success of SchoolResponse
    | LoadUserSuccess of UserResponse
    | Failure of exn

let private load_school () = promise {
    let request = make_get 
    let decoder = Decode.Auto.generateDecoder<SchoolResponse>()
    let! response = Fetch.tryFetchAs "/api/load-school" decoder request
    match response with
    | Ok result ->
        match result.Error with
        | None -> 
            return result
        | Some api_error ->
            return raise (LoadSchoolEx api_error)
    | Error e ->
        return failwith "no school"
}

let private load_user () = promise {
    let request = make_get 
    let decoder = Decode.Auto.generateDecoder<UserResponse>()
    let! response = Fetch.tryFetchAs "/api/load-user" decoder request
    match response with
    | Ok result ->
        match result.Error with
        | None -> 
            return result
        | Some api_error ->
            return raise (LoadUserEx api_error)
    | Error e ->
        return failwith "no user details"
}

let private save (data : SaveRequest) = promise {
    let request = make_post 5 data
    Browser.console.info (sprintf "request.Subjects = %A" data.Subjects)
    Browser.console.info (sprintf "request.info = %A" data.Info)
    let decoder = Decode.Auto.generateDecoder<APIError option>()
    let! response = Fetch.tryFetchAs "/api/save-tutor" decoder request
    return map_api_error_result response SaveEx
}

let init () : Model*Cmd<Msg> =
    {SchoolName = ""; Error = None; Subjects = ""
     FirstName = ""; LastName = ""; Info = ""},
     Cmd.batch [Cmd.ofPromise load_school () Success Failure
                Cmd.ofPromise load_user () LoadUserSuccess Failure]


let private of_api_error (result : APIError) =
    List.fold2
        (fun acc the_code the_message -> if acc <>  "" then acc + " " + the_message else acc)
        "" result.Codes result.Messages
        
let private of_load_school_result (code : APICode) (result : APIError) =
    List.fold2
        (fun acc the_code the_message -> if code = the_code then acc + " " + the_message else acc)
        "" result.Codes result.Messages

let private std_label text = 
    Label.label 
        [ Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
        [ str text ]

let private make_error (result : APIError option) =
    match result with
    | Some error ->
        Help.help [
            Help.Color IsDanger
            Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is5) ]
        ] [
            str (of_api_error error)
        ]
    | _ ->  nothing

let private help_first_time_user (result : LoadSchoolResult option) =
    match result with
    | Some result ->
        match List.contains APICode.NoSchool result.Error.Codes with
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
        match List.contains APICode.NoSchool result.Error.Codes with
        | true ->
            Help.help
                [ Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is6)
                                   Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                [ str "Enter the name of your school." ]
        | false -> std_label "School Name"
    | _ -> std_label "School Name"

    
let private image_holder url =
    [ Image.image [ Image.Is128x128 ]
        [ img [ Src url ] ] ]

let school_content (model : Model) (dispatch : Msg->unit) = 
    [ Columns.columns [ ]
        [ Column.column [ ]
            [ yield! input_field model.Error APICode.FirstName "First Name" model.FirstName (fun e -> dispatch (SetFirstName e.Value))
              yield! input_field model.Error APICode.LastName "Last Name" model.LastName (fun e -> dispatch (SetLastName e.Value))
              yield! input_field model.Error APICode.SchoolName "School Name" model.SchoolName (fun e -> dispatch (SetSchoolName e.Value))
              yield text_area_without_error "Info" model.Info (fun (e : React.FormEvent) -> dispatch (SetInfo e.Value)) ] ] ]


let private save_button dispatch msg text =
    Button.button [
        Button.Color IsTitanInfo
        Button.OnClick (fun _ -> (dispatch msg))
    ] [ str text ]

let view (model : Model) (dispatch : Msg -> unit) = 
    [ Card.card [ ] 
        [ Card.header [ ]
            [ Card.Header.title [ ] [ ] ]
          Card.image [ ]
            [ yield! image_holder "Images/school.png" ]
          Card.content [ ]
            [ yield! school_content model dispatch ] 
          Card.footer [ ] [
              Level.level [] [
                  Level.left [ ] [
                      Level.item [] [
                          Card.Footer.div [ ] [
                              save_button dispatch ClickSave "Save"
                          ]
                      ]
                  ]
              ]
              make_error model.Error 
          ]
        ]
    ]

let update  (model : Model) (msg : Msg): Model*Cmd<Msg> =
    match msg with
    | ClickSave ->
        let save_request = {SaveRequest.init with FirstName = model.FirstName
                                                  LastName = model.LastName; Info = model.Info; Subjects = ""
                                                  SchoolName = model.SchoolName}
        model, Cmd.ofPromise save save_request SaveSuccess Failure
    | SaveSuccess () ->
        model, Cmd.none
    | SetFirstName name ->
        {model with FirstName = name}, Cmd.none
    | SetLastName name ->
        {model with LastName = name}, Cmd.none
    | SetInfo info ->
        {model with Info = info}, Cmd.none
    | SetSchoolName name ->
        {model with SchoolName = name}, Cmd.none
    | Success result ->
        {model with SchoolName = result.SchoolName; Info = result.Info; Subjects = result.Subjects}, Cmd.none
    | LoadUserSuccess result ->
        {model with FirstName = result.FirstName; LastName = result.LastName}, Cmd.none
    | Failure e ->
        match e with
        | :? SaveEx as ex ->
            { model with Error = Some ex.Data0 }, Cmd.none
        | :? LoadUserEx as ex ->
            { model with Error = Some ex.Data0 }, Cmd.none
        | :? LoadSchoolEx as ex ->
            { model with Error = Some ex.Data0 }, Cmd.none
        | e ->
            { model with Error = Some { Codes = [APICode.Failure]; Messages = ["Unknown errror"] }}, Cmd.none