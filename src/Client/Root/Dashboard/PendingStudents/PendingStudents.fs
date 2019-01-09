/// Component for students awaiting enrollment completion
module PendingStudents

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
open Shared

open Thoth.Json

type Model =
    { Pending : Domain.Student list
      //result of interaction with the api
      Result : PendingResult option}

exception PendingEx of PendingResult

type Msg =
    | GetPendingSuccess of Student list
    | GetPendingFailure  of exn
    | ApprovePendingSuccess of Student
    | ApprovePendingFailure of exn
    | DismissPendingSuccess of Student
    | DismissPendingFailure of exn

let private get_pending () = promise {
    let props =
        [ RequestProperties.Method HttpMethod.GET
          RequestProperties.Credentials RequestCredentials.Include
          requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                           HttpRequestHeaders.Accept "application/json" ]]
    let decoder = Decode.Auto.generateDecoder<Domain.PendingResult>()
    let! response = Fetch.tryFetchAs "/api/secure/get-pending" decoder props
    match response with
    | Ok result ->
        match result.Codes with
        | APICode.Success::_ -> 
            return result.Students
        | _ ->
            return (raise (PendingEx result))
    | Error e ->
        return (raise (PendingEx {Codes = [APICode.FetchError]; Messages = [e];
            Students = []}))
}

let private remove_student student students =
    students
    |> List.filter (fun x -> x.Email <> student.Email)

let init () =
    { Pending = [ ]; Result = None },
    Cmd.ofPromise get_pending () GetPendingSuccess GetPendingFailure

let update (model : Model) (msg : Msg) =
    match msg with
    | ApprovePendingSuccess student ->
        {model with Pending = model.Pending |> remove_student student }, Cmd.none
    | ApprovePendingFailure e ->
        Browser.console.warn ("Failed to approve pending student: " + e.Message)
        match e with 
        | :? PendingEx as ex ->
            Browser.console.warn "Received PendingEx"
            { model with Result = Some ex.Data0 }, Cmd.none
        | e ->
            Browser.console.warn "Received general exception"
            { model with Result = Some { Codes = [APICode.Failure]; Messages = ["Unknown errror"]; Students = [ ] }}, Cmd.none

    | GetPendingSuccess students ->
        {model with Pending = students}, Cmd.none
    | GetPendingFailure e ->
        match e with 
        | :? PendingEx as ex ->
            Browser.console.warn "Received PendingEx"
            { model with Result = Some ex.Data0 }, Cmd.none
        | e ->
            Browser.console.warn "Received general exception"
            { model with Result = Some { Codes = [APICode.Failure]; Messages = ["Unknown errror"]; Students = [ ] }}, Cmd.none

    | DismissPendingSuccess student ->
        {model with Pending = model.Pending |> remove_student student }, Cmd.none
    | DismissPendingFailure e ->
        match e with 
        | :? PendingEx as ex ->
            Browser.console.warn ("Failed to dismiss pending student: " + e.Message)
            { model with Result = Some ex.Data0 }, Cmd.none
        | e ->
            Browser.console.warn "Received general exception"
            { model with Result = Some { Codes = [APICode.Failure]; Messages = ["Unknown errror"]; Students = [ ] }}, Cmd.none

let private student_content (student : Student) =
      [ Text.div [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ] [ str student.Email ] ]

let private single_student model dispatch student = 
    Column.column [ Column.Width (Screen.All, Column.Is3) ]
        [ Card.card [ ] 
            [ ]
          Card.header [ Modifiers [ Modifier.BackgroundColor IsTitanSecondary ] ]
            [ Card.Header.title [ Card.Header.Title.Modifiers [ Modifier.TextColor IsWhite ] ]  [ str (student.FirstName + " " + student.LastName) ] ]
          Card.content [  ]
            [ yield! student_content student ] ] 

let view (model : Model) (dispatch : Msg -> unit) =
    [ Box.box' [ ] 
        [ Columns.columns [ ]
            [ yield! (model.Pending |> List.map (single_student model dispatch))] ] ]