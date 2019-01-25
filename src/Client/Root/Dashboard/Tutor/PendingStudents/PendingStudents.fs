/// Component for students awaiting enrollment completion
module PendingStudents

open CustomColours
open Domain
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Fable.Core.JsInterop
open Fulma
open Fable.FontAwesome
open ModifiedFableFetch

open Domain
open Thoth.Json

type Model =
    { Pending : Domain.Student list
      //result of interaction with the api
      Error : APIError option}

exception PendingEx of APIError
exception ApprovePendingEx of APIError
exception DismissPendingEx of APIError

type Inter =
    RefreshStudent

type Msg =
    | GetPendingSuccess of Student list
    | GetPendingFailure  of exn
    | ApprovePendingSuccess of unit
    | ApprovePendingFailure of exn
    | DismissPendingSuccess of string
    | DismissPendingFailure of exn
    | DismissPending of Student
    | ApprovePending of Student

let private approve_pending (pending : ApprovePendingRequest) = promise {
    let request = make_post 3 pending
    let decoder = Decode.Auto.generateDecoder<APIError option>()
    let! response = Fetch.tryFetchAs "/api/approve-pending" decoder request
    match response with
    | Ok result ->
        match result with
        | Some error ->
            Browser.console.info ("got some error: " + (List.head error.Messages))
            return (raise (ApprovePendingEx error))
        | None ->
            return ()
    | Error e ->
        Browser.console.info ("got generic error: " + e)
        return raise (ApprovePendingEx (APIError.init [APICode.Fetch] [e]))
}

let private dismiss_pending (pending : DismissPendingRequest) = promise {
    let body = Encode.Auto.toString (3, pending)
    let props =
        [ RequestProperties.Method HttpMethod.POST
          RequestProperties.Credentials RequestCredentials.Include
          requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                           HttpRequestHeaders.Accept "application/json"]
          RequestProperties.Body !^(body) ] 
    let decoder = Decode.Auto.generateDecoder<DismissPendingResult>()
    let! response = Fetch.tryFetchAs "/api/dismiss-pending" decoder props
    match response with
    | Ok result ->
        match result.Error with
        | Some error -> 
            Browser.console.error ("dismiss_pending: " + (List.head error.Messages))
            return (raise (DismissPendingEx error))
        | _ ->
            return pending.Email //return email, use it to id the student to remove from the model
    | Error e ->
        Browser.console.info ("got generic error: " + e)
        return (raise (DismissPendingEx (APIError.init [APICode.Fetch] [e])))
}

let private get_pending () = promise {
    let request = make_get
    let decoder = Decode.Auto.generateDecoder<Domain.PendingResult>()
    let! response = Fetch.tryFetchAs "/api/get-pending" decoder request
    match response with
    | Ok result ->
        match result.Error with
        | Some error ->
            Browser.console.error ("get_pending: " + (List.head error.Messages))
            return (raise (PendingEx error))
        | None ->
            return result.Students
    | Error e ->
        return raise (PendingEx (APIError.init [APICode.Fetch] [e]))
}

let private remove_student (email : string) (students : Student list) =
    students
    |> List.filter (fun x -> x.Email <> email)

let init () =
    { Pending = [ ]; Error = None },
    Cmd.ofPromise get_pending () GetPendingSuccess GetPendingFailure

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match msg with
    | ApprovePending student ->
        model, Cmd.ofPromise approve_pending (ApprovePendingRequest.of_student student)
               ApprovePendingSuccess ApprovePendingFailure
    | ApprovePendingSuccess student ->
        Browser.console.info ("Approved student")
        model, Cmd.ofPromise get_pending () GetPendingSuccess GetPendingFailure
        
    | ApprovePendingFailure e ->
        Browser.console.warn ("Failed to approve pending student: " + e.Message)
        match e with 
        | :? PendingEx as ex ->
            Browser.console.warn ("Received PendingEx: " + ex.Message)
            { model with Error = Some ex.Data0 }, Cmd.none
        | e ->
            Browser.console.warn ("Received general exception: " + e.Message)
            { model with Error = Some (APIError.init [APICode.Failure] [e.Message])}, Cmd.none

    | GetPendingSuccess students ->
        {model with Pending = students}, Cmd.none
    | GetPendingFailure e ->
        match e with 
        | :? PendingEx as ex ->
            Browser.console.warn ("Received PendingEx: " + ex.Message)
            { model with Error = Some ex.Data0 }, Cmd.none
        | e ->
            Browser.console.warn ("Received general exception: " + e.Message)
            { model with Error = Some (APIError.init [APICode.Failure] [e.Message])}, Cmd.none

    | DismissPending student ->
        model, Cmd.ofPromise dismiss_pending (DismissPendingRequest.of_student student)
               DismissPendingSuccess DismissPendingFailure
               
    | DismissPendingSuccess email ->
        {model with Pending = model.Pending |> remove_student email }, Cmd.none
        
    | DismissPendingFailure e ->
        match e with 
        | :? PendingEx as ex ->
            Browser.console.warn ("Received PendingEx: " + ex.Message)
            { model with Error = Some ex.Data0 }, Cmd.none
        | e ->
            Browser.console.warn ("Received general exception: " + e.Message)
            { model with Error = Some (APIError.init [APICode.Failure] [e.Message])}, Cmd.none

let private student_content (student : Student) =
      [
        Text.div [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ] [
            Columns.columns [] [
                Column.column [] [
                    Label.label [ ] [ str "Email" ]
                ]
                Column.column [] [
                    Text.div [ ] [ str student.Email ]
                ]
            ]
            Columns.columns [] [
                Column.column [] [
                    Label.label [ ] [ str "Phone" ]
                ]
                Column.column [] [
                    Text.div [ ] [ str student.Phone ]
                ]
            ]
        ]
      ]

let private card_footer (student : Student) (dispatch : Msg -> unit) =
    [ Card.Footer.div [ ]
        [ Button.button [ Button.Color IsTitanInfo
                          Button.Props [ OnClick (fun _ -> dispatch (ApprovePending student)) ] ]
            [ Icon.icon [ ]
                [ Fa.i [ Fa.Solid.Check ]
                    [ ] ] ] ]
      Card.Footer.div [ ]
        [ Button.button [ Button.Color IsTitanInfo
                          Button.Props [ OnClick (fun _ -> dispatch (DismissPending student)) ] ]
            [ Icon.icon [ ]
                [ Fa.i [ Fa.Solid.Trash ]
                    [ ] ] ] ] ]


let private single_student (dispatch : Msg -> unit) (student : Student) = 
    Column.column [ Column.Width (Screen.All, Column.Is3) ]
        [ Card.card [ ] 
            [ ]
          Card.header [ Modifiers [ Modifier.BackgroundColor IsTitanSecondary ] ]
            [ Card.Header.title [ Card.Header.Title.Modifiers [ Modifier.TextColor IsWhite ] ]  [ str (student.FirstName + " " + student.LastName) ] ]
          Card.content [  ]
            [ yield! student_content student ]
          Card.footer [ ]
            [ yield! card_footer student dispatch ] ]

let private render_all_students (model : Model) (dispatch : Msg->unit) =
    (match model.Pending with
     | [] ->
         [nothing]
     | pending ->
        let l4 = pending |> Homeless.chunk 4
        [for l in l4 do
            yield Columns.columns [ ]
                [ for x in l do
                    yield single_student dispatch x] ])

let private students_level =
    Level.level [ ] 
        [ Level.left [ ]
            [ Level.title [ Common.Modifiers [ Modifier.TextTransform TextTransform.UpperCase
                                               Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            Common.Props [ Style [ CSSProp.FontFamily "'Montserrat', sans-serif" ]] ] [ str "enrol" ] ] ]

let view (model : Model) (dispatch : Msg -> unit) =
    [ Box.box' [ ] 
        [ yield! List.append [students_level] [yield! render_all_students model dispatch] ] ] 