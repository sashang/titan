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
      EndTime : DateTimeOffset  option }

type Msg =
    | GoLive
    | StopLive
    | GetSessionSuccess of OpenTokInfo
    | GetSessionFailure of exn


exception GetSessionEx of APIError

let private get_live_session_id () = promise {
    let request = make_get 
    let decoder = Decode.Auto.generateDecoder<OTIResponse>()
    let! response = Fetch.tryFetchAs "/api/go-live" decoder request
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


let init () =
    { Session = None; Students = []; StartTime = None; Live = Off
      EndTime = None; OTI = None; Error = None},
      Cmd.ofPromise get_live_session_id () GetSessionSuccess GetSessionFailure

let update (model : Model) (msg : Msg) =
    match model, msg with

    | model, GetSessionSuccess oti ->
        Browser.console.info ("Got session id")
        let session = OpenTokJSInterop.init_session oti.Key oti.SessionId
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

    |  {Live = Off}, GoLive ->
            Browser.console.info (sprintf "Clicked GoLive...initialzing publisher with session id = %s" model.OTI.Value.SessionId)
            let publisher = OpenTokJSInterop.init_pub "publisher" "1280x720"
            OpenTokJSInterop.connect_session_with_pub model.Session.Value publisher model.OTI.Value.Token
            OpenTokJSInterop.add_subscriber model.Session.Value
            {model with Live = On}, Cmd.none

    | _, GoLive ->
        Browser.console.warn "Clicked GoLive...but we are still live."
        model, Cmd.none


    | {Live = On }, StopLive ->
        OpenTokJSInterop.disconnect model.Session.Value
        {model with Live = Off}, Cmd.none

    | _, StopLive ->
        Browser.console.warn "Clicked StopLive ... but we are not live."
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
            (match model.Live with
            | On ->
                nav_item_stop_button dispatch
            | Off ->
                Client.Style.button dispatch GoLive "Go Live!" [ ])
        ]
    ]

let private students_in_room (model : Model) =
    match model.Students with
    | [] -> str "Nobody here"
    | students -> nothing

let private video = 
    div [ HTMLAttr.Id "videos"] [
        div [ HTMLAttr.Id "publisher"
              Style [ ] ] [

        ]
        div [ HTMLAttr.Id "subscriber" ] [

        ]
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    [  classroom_level  model dispatch
       Box.box' [ Common.Props [ HTMLAttr.Id ""
                                 Style [ CSSProp.Height "100%" ]  ] ]
        [ video ] ]
