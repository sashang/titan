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
      LoadSchoolsState : LoadingState } //list of enroled schools

type Msg =
    | GetEnroledSchoolsSuccess of School list
    | GetEnroledSchoolsFailure of exn

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
    {Schools = []; LoadSchoolsState = Loading}, Cmd.ofPromise get_enroled_schools () GetEnroledSchoolsSuccess GetEnroledSchoolsFailure

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match model, msg with
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

let private your_schools model dispatch =
    Level.level [ ] 
        [ Level.left [ ]
            [ Level.title [ Common.Modifiers [ Modifier.TextTransform TextTransform.UpperCase
                                               Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            Common.Props [ Style [ CSSProp.FontFamily "'Montserrat', sans-serif" ]] ] [ str "your schools" ] ] ]

//render the schools that this student is enroled in
let view (model : Model) (dispatch : Msg -> unit) =
        Box.box' [ ] [
            yield! (match model.LoadSchoolsState with
                    | Loaded ->
                        List.append
                            [your_schools model dispatch ]
                            [ for school in model.Schools do
                                yield render_school school dispatch ]
                    | Loading ->
                        Client.Style.loading_view)
        ]
