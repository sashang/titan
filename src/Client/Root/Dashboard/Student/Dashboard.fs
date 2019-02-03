module Student.Dashboard

open CustomColours
open Domain
open Elmish
open Fable.Import
open Fable.PowerPack
open Fable.Helpers.React
open Fulma
open ModifiedFableFetch
open StudentSchools
open Thoth.Json

type Model =
    { FirstName : string
      LastName : string
      Phone : string
      Email : string
      EnrolError : APIError option
      EnroledSchools : StudentSchools.Model
      Live : Live.Model
      Result : GetAllSchoolsResult}

type Msg =
    | ClickEnrol of string
    | GetAllSchoolsSuccess of GetAllSchoolsResult
    | EnrolSuccess of unit
    | StudentSchoolsMsg of StudentSchools.Msg
    | LiveMsg of Live.Msg
    | Failure of exn
    | JoinLive of string
    | StopLive of string

exception GetAllSchoolsEx of APIError
exception EnrolEx of APIError

let private get_all_schools () = promise {
    let request = make_get
    let decoder = Decode.Auto.generateDecoder<GetAllSchoolsResult>()
    let! response = Fetch.tryFetchAs "/api/get-all-schools" decoder request
    Browser.console.info "received response from get-all-schools"
    match response with
    | Ok result ->
        match result.Error with
        | Some api_error -> return raise (GetAllSchoolsEx api_error)
        | None ->  return result
    | Error e ->
        return raise (GetAllSchoolsEx (APIError.init [APICode.Fetch] [e]))
}

let private enrol_student er = promise {
    let request = make_post 1 er
    let decoder = Decode.Auto.generateDecoder<APIError option>()
    let! response = Fetch.tryFetchAs "/api/enrol-student" decoder request
    Browser.console.info "received response from enrol-student"
    match response with
    | Ok result ->
        match result with
        | Some api_error -> return raise (EnrolEx api_error)
        | None -> return ()
    | Error e ->
        return raise (EnrolEx (APIError.init [APICode.Fetch] [e]))
}


let init () = 
    let your_schools, ss_cmd = StudentSchools.init ()
    let live_model, live_cmd = Live.init ()
    {FirstName = ""; LastName = ""; Phone = ""; EnrolError = None;
     Email =""; EnroledSchools = your_schools; Result = GetAllSchoolsResult.init; Live = live_model},
              Cmd.batch [ Cmd.ofPromise get_all_schools () GetAllSchoolsSuccess Failure
                          Cmd.map LiveMsg live_cmd
                          Cmd.map StudentSchoolsMsg ss_cmd ]

let update model msg =
    match model, msg with
    | model, ClickEnrol school_name ->
        model, Cmd.ofPromise enrol_student {SchoolName = school_name} EnrolSuccess Failure
        
    | model, GetAllSchoolsSuccess result ->
        {model with Result = result}, Cmd.none

    //intercept this specific StudentSchoolsMsg, reroute it ot hte live component    
    | {Live = live_model}, StudentSchoolsMsg (StudentSchools.LiveViewGoLive email) ->
        Browser.console.info ("LiveViewGoLive ")
        let new_live_model, new_cmd = Live.update live_model (Live.GoLive email)
        {model with Live = new_live_model}, Cmd.map LiveMsg new_cmd

    //intercept this specific StudentSchoolsMsg, reroute it ot hte live component    
    | {Live = live_model}, StudentSchoolsMsg (StudentSchools.LiveViewStopLive email) ->
        Browser.console.info ("LiveViewStopLive ")
        let new_live_model, new_cmd = Live.update live_model (Live.StopLive email)
        {model with Live = new_live_model}, Cmd.map LiveMsg new_cmd
    
    | {Live = live_model}, LiveMsg msg ->
        Browser.console.info ("Live message")
        let new_model, new_cmd = Live.update live_model msg
        {model with Live = new_model}, Cmd.map LiveMsg new_cmd

    | {EnroledSchools = enroled_model}, StudentSchoolsMsg  msg ->
        Browser.console.info ("STudentSchools message")
        let new_model, new_cmd = StudentSchools.update enroled_model msg
        {model with EnroledSchools = new_model}, Cmd.map StudentSchoolsMsg new_cmd

    | model, EnrolSuccess () ->
        model, Cmd.none
    
    | model, Failure e ->
        match e with
        | :? GetAllSchoolsEx as ex ->
            List.iter (fun m -> Browser.console.warn ("GetAllSchoolsEx:" + m)) ex.Data0.Messages
            {model with Result = {model.Result with Error = Some ex.Data0}}, Cmd.none
        | :? EnrolEx as ex ->
            List.iter (fun m -> Browser.console.warn ("EnrolEx:" + m)) ex.Data0.Messages
            {model with EnrolError = Some ex.Data0}, Cmd.none
        | e ->
            Browser.console.warn ("Failed to get_all_schools: " + e.Message)
            model, Cmd.none

let single_row (school : School) (dispatch : Msg -> unit) =
        Card.card [] [
            Card.header [ Modifiers [ Modifier.BackgroundColor IsTitanSecondary
                                      Modifier.TextColor IsWhite
                                      Modifier.TextTransform TextTransform.Capitalized] ] [
                Card.Header.title
                    [ Card.Header.Title.Modifiers [ Modifier.TextColor IsWhite ] ]
                    [ str school.SchoolName ]
            ]
            Card.content [  ] [
                Columns.columns [  ] [
                    Column.column [ Column.Width (Screen.All, Column.Is1) ] [
                       Label.label
                           [ Label.Modifiers [Modifier.TextAlignment (Screen.All, TextAlignment.Left)] ]
                           [ Text.div [ ] [ str "Tutor" ] ]
                    ]
                    Column.column [ Column.Width (Screen.All, Column.Is2) ] [
                       Text.div [ ] [ str school.FirstName; str " "; str school.LastName]
                    ]
                ]
            ]
            Card.footer [ ] [
                Button.button [ Button.OnClick (fun ev -> dispatch (ClickEnrol school.SchoolName)) ] [
                    str "Enrol"
                ]
            ]
        ]
    
    
let view (model : Model) (dispatch : Msg -> unit) =
     Container.container [ Container.IsFluid ] [
        Columns.columns [ ] [
            Column.column [Column.Width (Screen.All, Column.Is4) ] [
                StudentSchools.view model.EnroledSchools (StudentSchoolsMsg >> dispatch)
            ]
            Column.column [ ] [
                yield! Live.view model.Live (LiveMsg >> dispatch)
            ]
        ]
     ]
    
