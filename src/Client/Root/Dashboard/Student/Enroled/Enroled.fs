module Enroled

open CustomColours
open Domain
open Elmish
open Fable.Import
open Fable.PowerPack
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open ModifiedFableFetch
open Thoth.Json
open Client.Shared

type Model =
    { Schools : School list
      AllSchools : School list
      LoadSchoolsState : LoadingState } //list of enroled schools

type Msg =
    | GetAllSchoolsSuccess of School list
    | GetAllSchoolsFailure of exn
    | GetEnroledSchoolsSuccess of School list
    | GetEnroledSchoolsFailure of exn
    | ClickEnrol of string
    | EnrolSuccess of unit
    | EnrolFailure of exn

exception GetEnroledSchoolsEx of APIError
exception GetAllSchoolsEx of APIError
exception EnrolEx of APIError

let private get_enroled_schools () = promise {
    let request = make_get
    let decoder = Decode.Auto.generateDecoder<GetAllSchoolsResult>()
    let! response = Fetch.tryFetchAs "/api/get-enroled-schools" decoder request
    Browser.console.info "received response from get-enroled-schools"
    match response with
    | Ok result ->
        match result.Error with
        | Some api_error -> return raise (GetEnroledSchoolsEx api_error)
        | None ->  return result.Schools
    | Error e ->
        return raise (GetEnroledSchoolsEx (APIError.init [APICode.Fetch] [e]))
}

let private get_all_schools () = promise {
    let request = make_get
    let decoder = Decode.Auto.generateDecoder<GetAllSchoolsResult>()
    let! response = Fetch.tryFetchAs "/api/get-all-schools" decoder request
    Browser.console.info "received response from get-all-schools"
    match response with
    | Ok result ->
        match result.Error with
        | Some api_error -> return raise (GetAllSchoolsEx api_error)
        | None ->  return result.Schools
    | Error e ->
        return raise (GetAllSchoolsEx (APIError.init [APICode.Fetch] [e]))
}

let private enrol_student (er : EnrolRequest) = promise {
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
    {Schools = []; LoadSchoolsState = Loading; AllSchools = []},
    Cmd.batch [Cmd.ofPromise get_enroled_schools () GetEnroledSchoolsSuccess GetEnroledSchoolsFailure
               Cmd.ofPromise get_all_schools () GetAllSchoolsSuccess GetAllSchoolsFailure ]

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match model, msg with
    | model, GetAllSchoolsSuccess schools ->
        {model with AllSchools = schools; LoadSchoolsState = Loaded }, Cmd.none

    | model, ClickEnrol school ->
        model, Cmd.ofPromise enrol_student {SchoolName = school} EnrolSuccess EnrolFailure

    | model, EnrolSuccess () ->
        Browser.console.info ("Student requested enrolment")
        model, Cmd.none

    | model, EnrolFailure e ->
        match e with
        | :? GetAllSchoolsEx as ex ->
            Browser.console.warn ("Failed to enrol: " + List.head ex.Data0.Messages)
            model, Cmd.none
        | e ->
            Browser.console.warn ("Failed to enrol: " + e.Message)
            model, Cmd.none

    | model, GetAllSchoolsFailure e ->
        match e with
        | :? GetAllSchoolsEx as ex ->
            Browser.console.warn ("Failed to get all schools: " + List.head ex.Data0.Messages)
            model, Cmd.none
        | e ->
            Browser.console.warn ("Failed to get all schools: " + e.Message)
            model, Cmd.none

    | model, GetEnroledSchoolsSuccess schools ->
        {model with Schools = schools; LoadSchoolsState = Loaded }, Cmd.none

    | model, GetEnroledSchoolsFailure e ->
        match e with
        | :? GetEnroledSchoolsEx as ex ->
            Browser.console.warn ("Failed to get enroled schools: " + List.head ex.Data0.Messages)
            model, Cmd.none
        | e ->
            Browser.console.warn ("Failed to get enroled schools: " + e.Message)
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

//render the schools that this student is enroled in
let view (model : Model) (dispatch : Msg -> unit) =
        Box.box' [ ] [
            (match model.LoadSchoolsState with
                | Loaded ->
                   div [ ] [
                        yield your_schools model dispatch 
                        yield! [ for school in model.Schools do
                                    yield render_school school dispatch ]
                        yield all_schools model dispatch 
                        yield! [ for school in model.AllSchools do
                                    yield render_school_for_enrolment school dispatch ] ]

                | Loading ->
                    Client.Style.loading_view)
        ]
