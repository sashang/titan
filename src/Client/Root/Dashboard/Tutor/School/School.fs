module School

open Client.Style
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
open ModifiedFableFetch
open Shared
open Thoth.Json

exception SaveEx of APIError

type Model =
    { SchoolName : string
      FirstName : string
      LastName : string
      Error : APIError option}

type Msg =
    | SetSchoolName of string
    | SetFirstName of string
    | SetLastName of string
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
        | _ ->
            return failwith "no school"
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
        | _ ->
            return failwith "no user details"
    | Error e ->
        return failwith "no user details"
}

let private save data = promise {
    let request = make_post 3 data
    let decoder = Decode.Auto.generateDecoder<APIError option>()
    let! response = Fetch.tryFetchAs "/api/save-tutor" decoder request
    match response with
    | Ok result ->
        match result with
        | Some err -> return  raise (SaveEx err)
        | None -> return ()
    | Error msg ->
        return (failwith msg)
}

let init () : Model*Cmd<Msg> =
    {SchoolName = ""; Error = None
     FirstName = ""; LastName = ""},
     Cmd.batch [Cmd.ofPromise load_school () Success Failure
                Cmd.ofPromise load_user () LoadUserSuccess Failure]


let private of_load_school_result (code : APICode) (result : LoadSchoolResult) =
    List.fold2
        (fun acc the_code the_message -> if code = the_code then acc + " " + the_message else acc)
        "" result.Error.Codes result.Error.Messages

let private std_label text = 
    Label.label 
        [ Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
        [ str text ]

let private make_error_from_load_school_result (result : LoadSchoolResult option) (code : APICode) =
    match result with
    | Some result ->
        Help.help [
            Help.Color IsDanger
            Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is5) ]
        ] [
            str (of_load_school_result code result)
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
    [ Image.image [ ]
        [ img [ Src url ] ] ]

let school_content (model : Model) (dispatch : Msg->unit) = 
    [ Columns.columns [ ]
        [ Column.column [ ]
            [ yield! input_field model.Error APICode.FirstName "First Name" model.FirstName (fun e -> dispatch (SetFirstName e.Value))
              yield! input_field model.Error APICode.LastName "Last Name" model.LastName (fun e -> dispatch (SetLastName e.Value))
              yield! input_field model.Error APICode.SchoolName "School Name" model.SchoolName (fun e -> dispatch (SetSchoolName e.Value)) ] ] ]


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
          Card.footer [ ] 
            [ Card.Footer.div [ ] 
                [ Level.level [ ]
                    [ Level.left [ ]
                        [ Level.item [ ]
                            [ save_button dispatch ClickSave "Save" ] ] ] ] ] ] ]

let update  (model : Model) (msg : Msg): Model*Cmd<Msg> =
    match msg with
    | ClickSave ->
        model, Cmd.ofPromise save model SaveSuccess Failure
    | SaveSuccess () ->
        model, Cmd.none
    | SetFirstName name ->
        {model with FirstName = name}, Cmd.none
    | SetLastName name ->
        {model with LastName = name}, Cmd.none
    | SetSchoolName name ->
        {model with SchoolName = name}, Cmd.none
    | Success result ->
        {model with SchoolName = result.SchoolName}, Cmd.none
    | LoadUserSuccess result ->
        {model with FirstName = result.FirstName; LastName = result.LastName}, Cmd.none
    | Failure e ->
        match e with
        | :? SaveEx as ex ->
            { model with Error = Some ex.Data0 }, Cmd.none
        | e ->
            { model with Error = Some { Codes = [APICode.Failure]; Messages = ["Unknown errror"] }}, Cmd.none