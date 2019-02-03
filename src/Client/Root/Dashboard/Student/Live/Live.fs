module Live

open CustomColours
open Client.Shared
open Domain
open Elmish
open Fable.Import
open Fable.PowerPack
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core.JsInterop
open Fulma
open ModifiedFableFetch
open Thoth.Json

type Msg =
    | GoLive of string
    | GetSessionSuccess of OpenTokInfo
    | GetSessionFailure of exn
    | GetEnroledSchoolsSuccess of School list //unfortunately we need this herer as well to get the tutors emails
    | GetEnroledSchoolsFailure of exn
    | StopLive of string

type Model =
    { Session : obj option
      Schools : School list
      OTI : OpenTokInfo option
      Error : APIError option}

exception GetSessionEx of APIError
exception GetEnroledSchoolsEx of APIError

let private get_live_session_id (tutor_email : EmailRequest) = promise {
    let request = make_post 1 tutor_email
    let decoder = Decode.Auto.generateDecoder<OTIResponse>()
    let! response = Fetch.tryFetchAs "/api/student-get-session" decoder request
    match response with
    | Ok result ->
        match result.Error with
        | None -> 
            match result.Info with
            | Some oti -> return oti
            | None -> return failwith ("Expected opentok info but got nothing")
        | Some api_error ->
            return raise (GetSessionEx api_error)
    | Error msg ->
        return failwith ("Failed to go live: " + msg)
}

//easier to just copy and past this here for now.
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
    { Schools = []; Session = None; OTI = None; Error = None},
     Cmd.ofPromise get_enroled_schools () GetEnroledSchoolsSuccess GetEnroledSchoolsFailure

let update (model : Model) (msg : Msg) =
    //TODO: map this to the OTI record based on the email
    match model, msg with
    | {OTI = Some oti; Session = Some session}, GoLive email ->
        Browser.console.info (sprintf "received JoinLive for tutor %s with session = %s" email oti.SessionId)
        let publisher = OpenTokJSInterop.init_pub "publisher" "640x480"
        OpenTokJSInterop.connect_session_with_pub model.Session.Value publisher oti.Token
        OpenTokJSInterop.add_subscriber session
        model, Cmd.none

    | {Session = Some session}, StopLive email ->
        Browser.console.info (sprintf "received StopLive for tutor %s" email)
        OpenTokJSInterop.disconnect session
        model, Cmd.none

    | model, GetSessionSuccess oti ->

        //TODO: need to fix this to work with multiple schools
        Browser.console.info ("Student.Live: Got session id")
        let session = OpenTokJSInterop.init_session oti.Key oti.SessionId
        if session = null then failwith "failed to get js session"
        {model with OTI = Some oti; Session = Some session; Error = None}, Cmd.none

    | model, GetSessionFailure e ->
        match e with
        | :? GetSessionEx as ex ->
            Browser.console.warn ("Student.Live: Failed to get session: " + List.head ex.Data0.Messages)
            {model with Error = Some ex.Data0} , Cmd.none
        | e ->
            Browser.console.warn ("Student.Live: Failed to get session: " + e.Message)
            model, Cmd.none

    | model, GetEnroledSchoolsFailure e ->
        match e with
        | :? GetEnroledSchoolsEx as ex ->
            Browser.console.warn ("Student.Live: Failed to get enroled schools: " + List.head ex.Data0.Messages)
            model, Cmd.none
        | e ->
            Browser.console.warn ("Student.Live: Failed to get enroled schools: " + e.Message)
            model, Cmd.none

    | model, GetEnroledSchoolsSuccess schools ->
        Browser.console.info ("Student.Live: Got enroled schools %A", schools)
        //get the tutors emails and get opentok session ids for them
        let cmds = schools 
                   |> List.map (fun school -> Cmd.ofPromise get_live_session_id {Email = school.Email} GetSessionSuccess GetSessionFailure)
                   |> Cmd.batch
        {model with Schools = schools }, cmds

let private video = 
    div [ HTMLAttr.Id "videos"
          Style [ CSSProp.Position "relative"
                  CSSProp.Height "100%"; CSSProp.Width "100%"] ] [
        div [ HTMLAttr.Id "publisher" ] [

        ]
        div [ HTMLAttr.Id "subscriber" ] [

        ]
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    [ Box.box' [ Common.Props [ HTMLAttr.Id ""
                                Style [ CSSProp.Height "100%" ] ] ]
        [ video ] ]
