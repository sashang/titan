module StudentSchools

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

type LiveState =
    | On
    | Off

type Model =
    { Schools : (School*LiveState) list } //list of enroled schools

type Msg =
    | JoinLiveSuccess of OpenTokInfo*string
    | JoinLiveFailure of exn
    | GetEnroledSchoolsSuccess of School list
    | GetEnroledSchoolsFailure of exn
    | JoinLive of string
    | StopLive of string
    | Subscribe of OpenTokInfo
    | Unsubscribe

exception GetEnroledSchoolsEx of APIError
exception JoinLiveEx of APIError

let private join_live (join_request : JoinLiveRequest) = promise {

    let request = make_post 1 join_request
    let decoder = Decode.Auto.generateDecoder<GoLiveResponse>()
    let! response = Fetch.tryFetchAs "/api/join-live" decoder request
    Browser.console.info "received response from join-live"
    match response with
    | Ok result ->
        match result.Error with
        | Some api_error -> return raise (JoinLiveEx api_error)
        | None ->
            match result.Info with
            | None -> return raise (JoinLiveEx (APIError.init [APICode.Failure] ["Expected some opentokinfo"]))
            | Some oti -> return (oti,join_request.TutorEmail)
    | Error e ->
        return raise (JoinLiveEx (APIError.init [APICode.Fetch] [e]))
}

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

let init () =
    {Schools = []}, Cmd.ofPromise get_enroled_schools () GetEnroledSchoolsSuccess GetEnroledSchoolsFailure

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match model, msg with
    | model, GetEnroledSchoolsSuccess schools ->
        Browser.console.info ("Got enroled schools %A", schools)
        {model with Schools = schools |> List.map (fun s -> (s,Off)) }, Cmd.none

    | model, JoinLive email ->
        model, Cmd.ofPromise join_live {TutorEmail = email} JoinLiveSuccess JoinLiveFailure

    | model, JoinLiveSuccess (oti,tutor_email) ->
        //mark this school as live
        let update = model.Schools
                      |> List.map (fun (school,state) -> if school.Email = tutor_email then (school,On) else (school,state))
        //we have the session info of the school the student picked to view
        // so tell the live view window to subscribe
        {model with Schools = update}, Cmd.ofMsg (Subscribe oti)

    | model, StopLive tutor_email ->
        //mark this school as off
        let update = model.Schools
                      |> List.map (fun (school,state) -> if school.Email = tutor_email then (school,Off) else (school,state))
        {model with Schools = update}, Cmd.ofMsg Unsubscribe


    | model, JoinLiveFailure e ->
        match e with
        | :? JoinLiveEx as ex ->
            Browser.console.warn ("Failed to join live session: " + List.head ex.Data0.Messages)
            model, Cmd.none
        | e ->
            Browser.console.warn ("Failed to join live session: " + e.Message)
            model, Cmd.none

    | model, GetEnroledSchoolsFailure e ->
        match e with
        | :? GetEnroledSchoolsEx as ex ->
            Browser.console.warn ("Failed to get enroled schools: " + List.head ex.Data0.Messages)
            model, Cmd.none
        | e ->
            Browser.console.warn ("Failed to get enroled schools: " + e.Message)
            model, Cmd.none

let private card_footer (school : School) (state : LiveState) (dispatch : Msg -> unit) =
    [ Card.Footer.div [ ]
        [ (match state with
           | On ->
                (Button.button [ Button.Color IsDanger
                                 Button.Props [ OnClick (fun _ -> dispatch (StopLive school.Email)) ] ]
                [ str "Stop" ])
           | Off ->
                (Button.button [ Button.Color IsTitanInfo
                                 Button.Props [ OnClick (fun _ -> dispatch (JoinLive school.Email)) ] ]
                [ str "Go Live" ])) ] ] 

let private card_content (school:School) (state : LiveState) (dispatch:Msg->unit) =
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

let render_school (school : School) (state : LiveState) (dispatch : Msg -> unit) =
    Card.card [] [
        Card.header [ Modifiers [ Modifier.BackgroundColor IsTitanSecondary
                                  Modifier.TextColor IsWhite
                                  Modifier.TextTransform TextTransform.Capitalized] ] [
            Card.Header.title
                [ Card.Header.Title.Modifiers [ Modifier.TextColor IsWhite ] ]
                [ str school.SchoolName ]
        ]
        Card.content [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ] [
            yield! card_content school state dispatch
        ]
        Card.footer [ ] [
            yield! card_footer school state dispatch 
        ]
    ]

//render the schools that this student is enroled in
let view (model : Model) (dispatch : Msg -> unit) =
    Box.box' [ ] [
        yield! List.append 
                [ Heading.h2 [ ] [ str "Your Schools" ] ]
                [ for (school,live_state) in model.Schools do
                    yield render_school school live_state dispatch ]
    ]
    

