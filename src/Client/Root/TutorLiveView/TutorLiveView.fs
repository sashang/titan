
module Tutor.LiveView

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
open OpenTokReactApp
open ModifiedFableFetch
open System
open Client.Shared
open Thoth.Json


type Msg =
    | Dismiss
    | GetSessionSuccess of OpenTokInfo
    | GetSessionFailure of exn

type Model =
    { Email : string
      Error : APIError option
      OTI : OpenTokInfo option }

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

let init email =
    Browser.console.info ("tutorliveview init")
    {OTI = None; Email = email; Error = None},
     Cmd.ofPromise get_live_session_id () GetSessionSuccess GetSessionFailure

let update model msg =
    match model,msg with
    | model, Dismiss ->
        model, Cmd.none

    | model, GetSessionSuccess oti ->
        Browser.console.info ("Got session id")
        {model with OTI = Some oti; Error = None}, Cmd.none

    |  model,GetSessionFailure e ->
        match e with
        | :? GetSessionEx as ex ->
            Browser.console.warn ("Failed to get session: " + List.head ex.Data0.Messages)
            {model with Error = Some ex.Data0} , Cmd.none
        | e ->
            Browser.console.warn ("Failed to get session: " + e.Message)
            model, Cmd.none

let private video model dispatch = 
    div [ HTMLAttr.Id "videos"] [
        (match model.OTI with
        | Some oti ->
            Session.session [ Session.ApiKey oti.Key; Session.SessionId oti.SessionId;
                              Session.Token oti.Token ] [
                div [ HTMLAttr.Id "subscriber" ] [
                    Streams.streams [ ] [
                        Subscriber.subscriber [ Subscriber.Props [ Name model.Email; Width "100%"; Height "720px"; ] ] [ ]
                    ]
                ]
                div [ HTMLAttr.Id "publisher" ] [
                    Publisher.publisher [ Publisher.Props [ Width "360px"; Height "240px"; Name model.Email ]  ] [ ]
                ]
            ]
        | None ->
            nothing)
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    // Modal.modal [ Modal.IsActive model.Active
    //               Modal.Props [ Style [ CSSProp.MaxWidth "100%"; CSSProp.MaxHeight "100%" ] ] ]  [
    //   Modal.background [ Props [ OnClick (fun ev -> dispatch Dismiss) ] ] [ ]
    //   Modal.content [ Props [ Style [ CSSProp.MaxWidth "100%"; CSSProp.MaxHeight "100%" ] ] ] [
    //       video model
    //   ]
    //   Modal.close [ Modal.Close.Size IsLarge
    //                 Modal.Close.OnClick (fun ev -> dispatch Dismiss)] [ ]
    // ]
    video model dispatch
