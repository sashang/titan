module Enrolled

open CustomColours
open Domain
open Elmish
open Fable.Import
open Fable.React
open Fable.React.Props
open Fulma
open ModifiedFableFetch
open Thoth.Json
type TF = Thoth.Fetch.Fetch
open Client.Shared

type Model =
    { EnrolledSchools : School list //list of schools the student is enrolled in 
      AllSchools : School list //list of all schools
      PendingSchools : School list //list of schools that the student has requested enrolment
      PendingLoaded : LoadingState
      EnrolledLoaded : LoadingState
      ActiveEnrolMessage : bool
      AllLoaded : LoadingState }

type Msg =
    | GetPendingSchools of School list
    | GetPendingSchoolsFailure of exn
    | GetUnenrolledSchoolsSuccess of School list
    | GetUnenrolledSchoolsFailure of exn
    | GetEnrolledSchoolsSuccess of School list
    | GetEnrolledSchoolsFailure of exn
    | ClickEnrol of string
    | EnrolSuccess of unit
    | EnrolFailure of exn
    | SignOut

exception GetPendingSchoolsEx of APIError
exception GetEnrolledSchoolsEx of APIError
exception GetUnenrolledSchoolsEx of APIError
exception GetAllSchoolsEx of APIError
exception EnrolEx of APIError

let private get_enrolled_schools () = promise {
    let request = make_get
    let decoder = Decode.Auto.generateDecoder<GetAllSchoolsResult>()
    let! response = TF.tryFetchAs("/api/get-enrolled-schools", decoder, request)
    Browser.Dom.console.info "received response from get-enrolled-schools"
    match response with
    | Ok result ->
        match result.Error with
        | Some api_error -> return raise (GetEnrolledSchoolsEx api_error)
        | None ->  return result.Schools
    | Error e ->
        return raise (GetEnrolledSchoolsEx (APIError.init [APICode.Fetch] [e]))
}

///get the schools that this student has requested enrolment
let private get_pending_schools () = promise {
    let request = make_get
    let decoder = Decode.Auto.generateDecoder<SchoolsResponse>()
    let! response = TF.tryFetchAs("/api/get-pending-schools", decoder, request)
    Browser.Dom.console.info "received response from get-pending-schools"
    match response with
    | Ok result ->
        match result.Error with
        | Some api_error -> return raise (GetPendingSchoolsEx api_error)
        | None ->  return result.Schools
    | Error e ->
        return raise (GetPendingSchoolsEx (APIError.init [APICode.Fetch] [e]))
}

let private get_unenrolled_schools () = promise {
    let request = make_get
    let decoder = Decode.Auto.generateDecoder<GetAllSchoolsResult>()
    let! response = TF.tryFetchAs("/api/get-unenrolled-schools", decoder, request)
    Browser.Dom.console.info "received response from get-unenrolled-schools"
    match response with
    | Ok result ->
        Browser.Dom.console.info "result ok"
        match result.Error with
        | Some api_error ->
            Browser.Dom.console.info "error in result"
            return raise (GetUnenrolledSchoolsEx api_error)
        | None ->
            Browser.Dom.console.info "result value ok"
            return result.Schools
    | Error e ->
        return raise (GetUnenrolledSchoolsEx (APIError.init [APICode.Fetch] [e]))
}

let private enrol_student (er : EnrolRequest) = promise {
    let request = make_post 1 er
    let decoder = Decode.Auto.generateDecoder<APIError option>()
    let! response = TF.tryFetchAs("/api/enrol-student", decoder, request)
    Browser.Dom.console.info "received response from enrol-student"
    match response with
    | Ok result ->
        match result with
        | Some api_error -> return raise (EnrolEx api_error)
        | None -> return ()
    | Error e ->
        return raise (EnrolEx (APIError.init [APICode.Fetch] [e]))
}

let init () =
    {EnrolledSchools = []; PendingSchools = []; AllLoaded = Loading; AllSchools = [];
    EnrolledLoaded = Loading; PendingLoaded = Loading; ActiveEnrolMessage = false},
    Cmd.batch [Cmd.OfPromise.either get_enrolled_schools () GetEnrolledSchoolsSuccess GetEnrolledSchoolsFailure
               Cmd.OfPromise.either get_pending_schools () GetPendingSchools GetPendingSchoolsFailure
               Cmd.OfPromise.either get_unenrolled_schools () GetUnenrolledSchoolsSuccess GetUnenrolledSchoolsFailure ]

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match model, msg with
    | model, GetPendingSchools schools ->
        {model with PendingSchools = schools; PendingLoaded = Loaded }, Cmd.none

    | model, SignOut ->
        Browser.Dom.console.info "Received signout msg" //we don't have to do anything special here.
        model, Cmd.none

    | model, ClickEnrol school ->
        model, Cmd.OfPromise.either enrol_student {SchoolName = school} EnrolSuccess EnrolFailure

    | model, EnrolSuccess () ->
        Browser.Dom.console.info ("Student request for enrolment succeeded")
        {model with ActiveEnrolMessage = true; AllLoaded = Loading; EnrolledLoaded = Loading; PendingLoaded = Loading },
        Cmd.batch [Cmd.OfPromise.either get_enrolled_schools () GetEnrolledSchoolsSuccess GetEnrolledSchoolsFailure
                   Cmd.OfPromise.either get_pending_schools () GetPendingSchools GetPendingSchoolsFailure
                   Cmd.OfPromise.either get_unenrolled_schools () GetUnenrolledSchoolsSuccess GetUnenrolledSchoolsFailure ]

    | model, EnrolFailure e ->
        match e with
        | :? GetAllSchoolsEx as ex ->
            Browser.Dom.console.warn ("Failed to enrol: " + List.head ex.Data0.Messages)
            model, Cmd.none
        | e ->
            Browser.Dom.console.warn ("Failed to enrol: " + e.Message)
            model, Cmd.none

    | model, GetPendingSchoolsFailure e ->
        match e with
        | :? GetPendingSchoolsEx as ex ->
            Browser.Dom.console.warn ("Failed to get pending schools: " + List.head ex.Data0.Messages)
            model, Cmd.none
        | e ->
            Browser.Dom.console.warn ("Failed to get pending schools: " + e.Message)
            model, Cmd.none

    | model, GetUnenrolledSchoolsSuccess schools ->
        Browser.Dom.console.info("handling unenrolled schools")
        schools
        |> List.iter (fun x -> Browser.Dom.console.info (sprintf "%A" x))
        {model with AllSchools = schools; AllLoaded = Loaded }, Cmd.none

    | model, GetUnenrolledSchoolsFailure e ->
        match e with
        | :? GetUnenrolledSchoolsEx as ex ->
            Browser.Dom.console.error ("Failed to get unenrolled schools: " + List.head ex.Data0.Messages)
            model, Cmd.none
        | e ->
            Browser.Dom.console.error ("Failed to get unenrolled schools: " + e.Message)
            model, Cmd.none

    | model, GetEnrolledSchoolsSuccess schools ->
        {model with EnrolledSchools = schools; EnrolledLoaded = Loaded }, Cmd.none

    | model, GetEnrolledSchoolsFailure e ->
        match e with
        | :? GetEnrolledSchoolsEx as ex ->
            Browser.Dom.console.error ("Failed to get enrolled schools: " + List.head ex.Data0.Messages)
            model, Cmd.none
        | e ->
            Browser.Dom.console.error ("Failed to get enrolled schools: " + e.Message)
            model, Cmd.none

let private card_footer (school : School) (dispatch : Msg -> unit) =
     Card.Footer.div [ ]
        [  ] 

let private card_footer_enrol (school : School) (dispatch : Msg -> unit) =
     Card.Footer.div [ ]
        [ Client.Style.button dispatch (ClickEnrol school.SchoolName) "Enrol" [] ]

let private card_content (school:School) (dispatch:Msg->unit) =
    [
        Columns.columns [ ] [
            Column.column [ ] [
               Label.label
                   [ Label.Modifiers [ ] ]
                   [ str "Tutor" ]
               Text.div [  ] [ str school.FirstName; str " "; str school.LastName]
            ]
            Column.column [ ] [
               Label.label
                   [ Label.Modifiers [ ] ]
                   [ str "Email" ]
               Text.div [  ] [ str school.Email ]
            ]
        ]
        Columns.columns [ ] [
            Column.column [ ] [
               Label.label
                   [ Label.Modifiers [ ] ]
                   [ str "Location" ]
               Text.div [  ] [ str school.Location ]
            ]
        ]
    ]

let render_school (school : School)  (dispatch : Msg -> unit) =
    Card.card [] [
        Card.header [ Modifiers [ Modifier.BackgroundColor IsTitanSecondary
                                  Modifier.TextColor IsWhite
                                  Modifier.TextTransform TextTransform.Capitalized] ] [
            Card.Header.title
                [ Card.Header.Title.Modifiers [ Modifier.TextColor IsWhite ] ]
                [ str school.SchoolName ]
        ]
        Card.content [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ] [
            yield! card_content school dispatch
        ]
        Card.footer [ ] [
            card_footer school dispatch 
        ]
    ]

let render_school_for_enrolment (school : School)  (dispatch : Msg -> unit) =
    Card.card [] [
        Card.header [ Modifiers [ Modifier.BackgroundColor IsTitanSecondary
                                  Modifier.TextColor IsWhite
                                  Modifier.TextTransform TextTransform.Capitalized] ] [
            Card.Header.title
                [ Card.Header.Title.Modifiers [ Modifier.TextColor IsWhite ] ]
                [ str school.SchoolName ]
        ]
        Card.content [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ] [
            yield! card_content school dispatch
        ]
        Card.footer [ ] [
            card_footer_enrol school dispatch 
        ]
    ]

let private pending_schools model dispatch =
    Level.level [ ] 
        [ Level.left [ ]
            [ Level.title [ Common.Modifiers [ Modifier.TextTransform TextTransform.UpperCase
                                               Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            Common.Props [ Style [ CSSProp.FontFamily "'Montserrat', sans-serif" ]] ] [ str "Awaiting approval" ] ] ]

let private your_schools model dispatch =
    Level.level [ ] 
        [ Level.left [ ]
            [ Level.title [ Common.Modifiers [ Modifier.TextTransform TextTransform.UpperCase
                                               Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            Common.Props [ Style [ CSSProp.FontFamily "'Montserrat', sans-serif" ]] ] [ str "your schools" ] ] ]

let private all_schools model dispatch =
    Level.level [ ] 
        [ Level.left [ ]
            [ Level.title [ Common.Modifiers [ Modifier.TextTransform TextTransform.UpperCase
                                               Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            Common.Props [ Style [ CSSProp.FontFamily "'Montserrat', sans-serif" ]] ] [ str "all schools" ] ] ]

let private render_all_school_types model dispatch =
    div [ ] [
        div [] [
            yield your_schools model dispatch
            yield! [ for school in model.EnrolledSchools do
                        yield render_school school dispatch ]
        ]
        div [] [
            match model.PendingSchools with
            | [] ->
                yield nothing
            | _ ->
                yield pending_schools model dispatch 
                yield! [ for school in model.PendingSchools do
                            yield render_school school dispatch ]
        ]
        div [] [
            yield all_schools model dispatch 
            yield! [ for school in model.AllSchools do
                        yield render_school_for_enrolment school dispatch ] 
        ]
    ]

//render the schools that this student is enrolled in
let view (model : Model) (dispatch : Msg -> unit) =
        Box.box' [ ] [
            
            (match model.EnrolledLoaded,model.AllLoaded,model.PendingLoaded,model.ActiveEnrolMessage with
                | Loaded, Loaded, Loaded, true ->
                    div [ ] [
                        Message.message [ Message.Color IsTitanSuccess ] [
                            Message.body [ ] [ str ("Your enrolment request has been received. Your tutor will get back to you.") ]
                        ]
                        render_all_school_types model dispatch
                    ]

                | Loaded, Loaded, Loaded, false ->
                    div [ ] [
                        render_all_school_types model dispatch
                    ]
                | _ ->
                    Client.Style.loading_view)
        ]
