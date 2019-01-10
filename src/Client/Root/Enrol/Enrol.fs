/// Front page form for enrolling
module Enrol


open CustomColours
open Domain
open Elmish
open Elmish.Browser.Navigation
open Fable.FontAwesome
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Fable.Core.JsInterop
open Fulma
open ModifiedFableFetch
open Shared
open Thoth.Json
open ValueDeclarations

type Model =
    { FirstName : string
      LastName : string
      Email : string
      Schools : School list
      ActiveSchool : string option
      Error : APIError option
      GoodNotification : string option  }

type Msg =
    | ClickDelNot
    | EnrolSuccess of unit
    | EnrolFailure of exn
    | GetSchoolsSuccess of School list
    | GetSchoolsFailure of exn
    | SetFirstName of string
    | SetLastName of string
    | SetEmail of string
    | SetSchool of string
    | ClickSubmit

exception EnrolEx of APIError

let private get_schools () = promise {
    Browser.console.info "get_schools promise"
    let props =
        [ RequestProperties.Method HttpMethod.GET
          requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                           HttpRequestHeaders.Accept "application/json" ] ]
    let decoder = Decode.Auto.generateDecoder<Domain.School list>()
    let! response = Fetch.tryFetchAs "/api/get-schools" decoder props
    match response with
    | Ok result ->
        Browser.console.info "got response with schools"
        return result
    | Error e ->
        return failwith e
}

let private enroll (model : Model) = promise {
    match model.ActiveSchool with
    | Some schoolname ->
        let data = { Domain.Enrol.init with SchoolName = schoolname; FirstName = model.FirstName; LastName = model.LastName; Email = model.Email}
        let body = Encode.Auto.toString (4, data)
        let props =
            [ RequestProperties.Method HttpMethod.POST
              requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                               HttpRequestHeaders.Accept "application/json" ]
              RequestProperties.Body !^(body) ]
        let decoder = Decode.Auto.generateDecoder<Domain.EnrolResult>()
        let! response = Fetch.tryFetchAs "/api/enroll" decoder props
        match response with
        | Ok result ->
            match result.Error with
            | Some error -> 
                Browser.console.info ("got some error: " + (List.head error.Messages))
                return (raise (EnrolEx error))
            | _ ->
                return ()
        | Error e ->
            Browser.console.info ("got generic error: " + e)
            return (raise (EnrolEx (APIError.init [APICode.FetchError] [e])))
    | None ->
        let message = "Choose a school from the dropdown"
        return (raise (EnrolEx (APIError.init [APICode.FetchError] [message])))

}
let init () =
    { LastName = ""; FirstName = ""; Email = ""; Schools = [];
      GoodNotification = None; Error = None; ActiveSchool = None},
    Cmd.ofPromise get_schools () GetSchoolsSuccess GetSchoolsFailure

let update (model : Model) (msg : Msg) =

    match msg with
    | ClickDelNot ->
        {model with Error = None; GoodNotification = None}, Cmd.none
    | SetSchool school ->
        {model with ActiveSchool = Some school}, Cmd.none
    | ClickSubmit ->
        model, Cmd.ofPromise enroll model EnrolSuccess EnrolFailure
    | SetEmail email ->
        { model with Email = email}, Cmd.none
    | SetFirstName name ->
        { model with FirstName = name}, Cmd.none
    | SetLastName name ->
        { model with LastName = name}, Cmd.none
    | EnrolSuccess () ->
         { model with Error = None; GoodNotification = Some ENROL_SUCCESS}, Cmd.none
    | GetSchoolsSuccess schools ->
        Browser.console.info ("Got schools")
        { model with Schools = schools }, Cmd.none
    | GetSchoolsFailure e ->
        Browser.console.info ("Failed to get schools: " + e.Message)
        match e with
        | :? EnrolEx as enroll_ex ->
            { model with Error = Some enroll_ex.Data0 }, Cmd.none
        | _ ->
            { model with Error = Some (APIError.init [APICode.Failure] [e.Message]) }, Cmd.none

    | EnrolFailure e ->
        Browser.console.info ("Failed to enroll.")
        match e with
        | :? EnrolEx as enroll_ex -> 
            { model with Error = Some enroll_ex.Data0 }, Cmd.none
        | _ ->
            { model with Error = Some (APIError.init [APICode.Failure] [e.Message]) }, Cmd.none

let enroll_button dispatch msg text = 
    Button.button [
        Button.Color IsTitanInfo
        Button.OnClick (fun _ -> (dispatch msg))
        Button.CustomClass "is-large"
    ] [ str text ]

let field autofocus placeholder input_type dispatch msg = 
    Field.div [ ]
        [ Control.div [ ]
            [ Input.text
                [ Input.Size IsLarge
                  Input.Placeholder placeholder
                  Input.Type input_type
                  Input.Props [ 
                    AutoFocus autofocus
                    OnChange (fun ev -> dispatch (msg ev.Value)) ] ] ] ]

let private school_dd_item (school : School) (current_active : string option) (dispatch : Msg -> unit) =
    Dropdown.Item.a [ (match current_active with
                      | Some s -> Dropdown.Item.IsActive (s = school.Name)
                      | None -> Dropdown.Item.IsActive false) ]
        [ Text.div [ Common.Props [ OnClick (fun _ ->  dispatch (SetSchool school.Name) ) ]  ]  [ str school.Name  ] ]

let private dropdown (model : Model) (dispatch : Msg -> unit) = 
    Dropdown.dropdown [ Dropdown.IsHoverable ]
        [ div [ ]
            [ Button.button [ Button.CustomClass "is-large"  ]
                [ span [ ]
                    [ str "Schools" ]
                  Icon.icon [ Icon.Size IsSmall ]
                    [ Fa.i [ Fa.Solid.AngleDown ]
                        [ ] ] ] ]
          Dropdown.menu [ ]
            [ Dropdown.content [ ]
               [ yield! model.Schools |> List.map (fun x -> school_dd_item x model.ActiveSchool dispatch)  ] ] ]

let private of_api_result (result : APIError) =
    List.fold (fun acc the_message -> acc + ":" + the_message + ":" ) "" result.Messages

let private render_notification (model : Model) dispatch =
    match model.GoodNotification with
    | Some message ->
        Notification.notification 
            [ Notification.Modifiers 
                [ Modifier.BackgroundColor IsTitanSuccess ]] 
            [ str message
              Notification.delete [ Common.Props [ OnClick (fun _ -> dispatch ClickDelNot) ] ] [ ] ]
    | None ->
        nothing
let private render_error (model : Model) dispatch =
    match model.Error with
    | Some api_error ->
        Notification.notification 
            [ Notification.Modifiers 
                [ Modifier.TextColor IsWhite  
                  Modifier.BackgroundColor IsTitanError ]] 
            [ str (of_api_result api_error)
              Notification.delete [ Common.Props [ OnClick (fun _ -> dispatch ClickDelNot) ] ] [ ] ]
    | None -> nothing

let private column (model : Model) (dispatch : Msg -> unit) =

    Column.column
        [ Column.Width (Screen.All, Column.Is4)
          Column.Offset (Screen.All, Column.Is4) ]
        [ Heading.h3
            [ Heading.Modifiers [ Modifier.TextColor IsBlack ] ]
            [ str "Enrol" ]
          Box.box' [ ] [
            field true "First Name" Input.Text dispatch SetFirstName
            field false "Last Name" Input.Text dispatch SetLastName
            field true "Email" Input.Email dispatch SetEmail
            Field.div [ ] [ dropdown model dispatch ]
            enroll_button dispatch ClickSubmit "Submit"
          ]
          render_error model dispatch
          render_notification model dispatch
    ]


let view (model : Model) (dispatch : Msg -> unit) =
    [ Container.container [ Container.IsFullHD 
                            Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] 
        [ column model dispatch ] ]