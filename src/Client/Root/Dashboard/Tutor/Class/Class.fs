/// A class in the school
module Class

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
open System
open Client.Shared
open Thoth.Json

type LiveState =
    | On
    | Off

type Model =
    { Students : Student list
      OTI : OpenTokInfo option
      StartTime : DateTimeOffset option
      Error : APIError option
      Session : obj option
      Live : LiveState
      Email : string //tutor's email
      EndTime : DateTimeOffset  option }

type Msg =
    | GoLive
    | StopLive
    | SignOut
    | GetSessionSuccess of OpenTokInfo
    | GetSessionFailure of exn


exception GetSessionEx of APIError

let private get_live_session_id () = promise {
    let request = make_get 
    let decoder = Decode.Auto.generateDecoder<OTIResponse>()
    let! response = Fetch.tryFetchAs "/api/get-session-id" decoder request
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


let init email =
    { Session = None; Students = []; StartTime = None; Live = Off
      EndTime = None; OTI = None; Error = None; Email = email},
      Cmd.ofPromise get_live_session_id () GetSessionSuccess GetSessionFailure

let update (model : Model) (msg : Msg) =
    match model, msg with

    | model, GetSessionSuccess oti ->
        Browser.console.info ("Got session id")
        let session = OpenTokJSInterop.init_session oti.Key oti.SessionId
        OpenTokJSInterop.on_streamcreate_subscribe session 640 480
        if session = null then failwith "failed to get js session"
        {model with OTI = Some oti; Session = Some session; Error = None}, Cmd.none

    |  model,GetSessionFailure e ->
        match e with
        | :? GetSessionEx as ex ->
            Browser.console.warn ("Failed to get session: " + List.head ex.Data0.Messages)
            {model with Error = Some ex.Data0} , Cmd.none
        | e ->
            Browser.console.warn ("Failed to get session: " + e.Message)
            model, Cmd.none

    |  {Live = Off; OTI = Some oti; Session = Some session}, GoLive ->
        Browser.console.info (sprintf "Clicked GoLive...initialzing publisher with session id = %s" model.OTI.Value.SessionId)
        let publisher = OpenTokJSInterop.init_pub "publisher" "1280x720" model.Email
        OpenTokJSInterop.connect_session_with_pub session publisher model.OTI.Value.Token
        {model with Live = On; Session = Some session}, Cmd.none

    | _, GoLive ->
        Browser.console.error ("Bad state for GoLive message")
        model, Cmd.none


    | {Live = On }, StopLive ->
        match model.Session with
        | Some session ->
            OpenTokJSInterop.disconnect session
            {model with Live = Off}, Cmd.none
        | None ->
            model, Cmd.none

    | _, StopLive ->
        Browser.console.warn "Clicked StopLive ... but we are not live."
        model, Cmd.none

    | _, SignOut ->
        Browser.console.info "Received signout msg"
        match model.Session with
        | Some session ->
            OpenTokJSInterop.disconnect session
            {model with Live = Off; Session = None}, Cmd.none
        | None ->
            model, Cmd.none


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
            | Off, Some _ , Some _ ->
                Client.Style.button dispatch GoLive "Go Live!" [ ]
            | _ -> nothing)
        ]
    ]

let private students_in_room (model : Model) =
    match model.Students with
    | [] -> str "Nobody here"
    | students -> nothing

let private video = 
    div [ HTMLAttr.Id "videos"] [
        div [ HTMLAttr.Id "layoutContainer"] [
        ]
        div [ HTMLAttr.Id "publisher" ] [

        ]
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    div [ ] [
        classroom_level model dispatch
        video
    ]
