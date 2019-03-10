
module Student.LiveView

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
    | Failure of exn

type Model =
    { School : School 
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


let init school student_email = 
    {School = school; StudentEmail = student_email; OTI = None; Error = None},
    Cmd.ofPromise get_live_session_id {Email = school.Email} GetSessionSuccess Failure


let update model msg =
    match model,msg with
    | model, Dismiss ->
        model, Cmd.none

    | model, GetSessionSuccess oti ->
        //TODO: need to fix this to work with multiple schools
        Browser.console.info ("Student.Live: Got session id")
        {model with OTI = Some oti; Error = None}, Cmd.none

    | model, Failure e ->
        match e with
        | :? GetSessionEx as ex ->
            Browser.console.warn ("Student.Live: Failed to get session: " + List.head ex.Data0.Messages)
            {model with Error = Some ex.Data0} , Cmd.none
        | e ->
            Browser.console.warn ("Student.Live: Failed to get session: " + e.Message)
            model, Cmd.none

let private video model dispatch = 
        (match model.OTI with
        | Some oti ->
            // StudentReactComp.comp [ StudentReactComp.ApiKey oti.Key; StudentReactComp.SessionId oti.SessionId;
            //                         StudentReactComp.TutorEmail model.School.Email; StudentReactComp.Token oti.Token ] [ ]
             Session.session [ Session.ApiKey oti.Key; Session.SessionId oti.SessionId;
                               Session.Token oti.Token ] [
                Columns.columns [] [
                    Column.column [ Column.Props [ Style [ ] ]
                                    Column.Modifiers [ ] ] [
                        Streams.streams [ ]  [
                            // StudentSubscriber.comp [ StudentSubscriber.OTProps [ Width "100%"; Height "90vh"; ]
                            //                          StudentSubscriber.TutorEmail model.School.Email ] [ ]
                            Subscriber.subscriber [ Subscriber.OTProps [ Width "100%"; Height "90vh"; ] ] [ ]
                        ]
                    ]
                ]
                Columns.columns [] [
                    Column.column [ Column.Props [ Style [ PaddingLeft 30; PaddingTop 110 ] ]
                                    Column.Modifiers [ Modifier.IsOverlay ] ] [
                        Publisher.publisher [ Publisher.Props [ Name model.StudentEmail; Width "360px"; Height "240px" ]  ] [ ]
                    ]
                ]
             ]
        | None ->
            nothing)
    //]

let view (model : Model) (dispatch : Msg -> unit) =
    video model dispatch
