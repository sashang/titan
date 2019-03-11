module Student.Class

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

type LiveState =
    | On
    | Off


type Msg =
    | GoLive
    | SignOut
    | GetSessionSuccess of OpenTokInfo
    | Failure of exn
    | StopLive

type Model =
    { Session : obj option
      School : School 
      Live : LiveState
      StudentEmail: string
      OTI : OpenTokInfo option
      Error : APIError option}

exception GetSessionEx of APIError

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

let private  nav_item_stop_button (dispatch : Msg -> unit) =
    Navbar.Item.div [ ]
        [ Button.button 
            [ Button.Color IsDanger
              Button.OnClick (fun e -> dispatch StopLive)  ]
            [ str "Stop" ] ]

let private classroom_level model dispatch =
    Level.level [ ] [ 
        Level.left [ ]
            [ Level.title [ Common.Modifiers [ Modifier.TextTransform TextTransform.UpperCase
                                               Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            Common.Props [ Style [ CSSProp.FontFamily "'Montserrat', sans-serif" ]] ] [ str "classroom" ] ]
        Level.right [ ] [
            (match model.Live, model.Session, model.OTI with
            | On, Some _, Some _ ->
                nav_item_stop_button dispatch
            | Off, Some _ , Some _ -> //only show the go live button when we have values for session and oti and we are off
                Client.Style.button dispatch GoLive "Go Live!" [ ]
            | _ -> nothing)
        ]
    ]

let init school student_email = 
    {School = school; StudentEmail = student_email; Session = None; OTI = None; Error = None; Live = Off},
    Cmd.ofPromise get_live_session_id {Email = school.Email} GetSessionSuccess Failure

let update (model : Model) (msg : Msg) =
    //TODO: map this to the OTI record based on the email
    match model, msg with
    | {OTI = Some oti; Session = Some session; Live = Off}, GoLive ->
        Browser.console.info (sprintf "received GoLive for student at school %s with session = %s" model.School.SchoolName oti.SessionId)
        let publisher = OpenTokJSInterop.init_pub "publisher" "640x480" model.StudentEmail
        OpenTokJSInterop.connect_session_with_pub model.Session.Value publisher oti.Token
        {model with Live = On}, Cmd.none

    | _, GoLive ->
        Browser.console.error ("Bad state for GoLive message")
        model, Cmd.none

    | {Session = Some session; Live = On}, StopLive ->
        Browser.console.info (sprintf "received StopLive for student at school %s" model.School.SchoolName)
        OpenTokJSInterop.disconnect session
        {model with Live = Off}, Cmd.none

    | {Session = Some session}, SignOut ->
        Browser.console.info (sprintf "received StopLive for student at school %s" model.School.SchoolName)
        OpenTokJSInterop.disconnect session
        {model with Live = Off; Session = None; OTI = None}, Cmd.none

    | _, StopLive ->
        Browser.console.error ("Bad state for StopLive message")
        model, Cmd.none

    | model, GetSessionSuccess oti ->
        //TODO: need to fix this to work with multiple schools
        Browser.console.info ("Student.Live: Got session id")
        let session = OpenTokJSInterop.init_session oti.Key oti.SessionId
        OpenTokJSInterop.on_streamcreate_subscribe_filter session 640 480 model.School.Email
        if session = null then failwith "failed to get js session"
        {model with OTI = Some oti; Session = Some session; Error = None}, Cmd.none

    | model, Failure e ->
        match e with
        | :? GetSessionEx as ex ->
            Browser.console.warn ("Student.Live: Failed to get session: " + List.head ex.Data0.Messages)
            {model with Error = Some ex.Data0} , Cmd.none
        | e ->
            Browser.console.warn ("Student.Live: Failed to get session: " + e.Message)
            model, Cmd.none


let private video = 
    div [ HTMLAttr.Id "videos"] [
        div [ HTMLAttr.Id "publisher" ] [

        ]
        div [ HTMLAttr.Id "subscriber" ] [

        ]
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    div [ ] [
        classroom_level model dispatch
        video
        // Box.box' [ Common.Props [ HTMLAttr.Id ""
        //                           Style [ CSSProp.Height "100%" ] ] ]
        //     [ video ] 
    ]
