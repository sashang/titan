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

type Model =
    { Info : Enrol
      Schools : School list
      Result : EnrolResult option  }

type Msg =
    | EnrolSuccess of unit
    | EnrolFailure of exn
    | GetSchoolsSuccess of School list
    | GetSchoolsFailure of exn
    | SetFirstName of string
    | SetLastName of string
    | SetEmail of string
    | ClickSubmit

exception EnrolEx of EnrolResult

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
        return result
    | Error e ->
        return failwith e
}

let private enroll (info : Enrol) = promise {
    let body = Encode.Auto.toString (4, info)
    let props =
        [ RequestProperties.Method HttpMethod.POST
          RequestProperties.Credentials RequestCredentials.Include
          requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                           HttpRequestHeaders.Accept "application/json" ]
          RequestProperties.Body !^(body) ]
    let decoder = Decode.Auto.generateDecoder<Domain.EnrolResult>()
    let! response = Fetch.tryFetchAs "/api/enroll" decoder props
    match response with
    | Ok result ->
        match result.Codes with
        | APICode.Success::_ -> 
            return ()
        | _ ->
            return (raise (EnrolEx result))
    | Error e ->
        return (raise (EnrolEx {Codes = [APICode.FetchError]; Messages = [e]}))
}
let init () =
    Browser.console.info "Enrol.init"
    { Info = Enrol.init; Schools = []; Result = None},
    Cmd.ofPromise get_schools () GetSchoolsSuccess GetSchoolsFailure

let update (model : Model) (msg : Msg) =

    match msg with
    | ClickSubmit ->
        model, Cmd.ofPromise enroll model.Info EnrolSuccess EnrolFailure
    | SetEmail email ->
        { model with Info = {model.Info with Email = email}}, Cmd.none
    | SetFirstName name ->
        { model with Info = {model.Info with FirstName = name}}, Cmd.none
    | SetLastName name ->
        { model with Info = {model.Info with LastName = name}}, Cmd.none
    | EnrolSuccess () ->
         model, Cmd.none
    | GetSchoolsSuccess schools ->
        Browser.console.info ("Got schools")
        { model with Schools = schools }, Cmd.none
    | GetSchoolsFailure e ->
        Browser.console.info ("Failed to get schools: " + e.Message)
        { model with Result = None }, Cmd.none
    | EnrolFailure e ->
        Browser.console.info ("Failed to enroll.")
        match e with
        | :? EnrolEx as enroll_ex -> 
            { model with Result = Some enroll_ex.Data0 }, Cmd.none
        | _ ->
            { model with Result = None }, Cmd.none

let enroll_button dispatch msg text = 
    Button.button [
        Button.Color IsTitanInfo
        Button.OnClick (fun _ -> (dispatch msg))
        Button.CustomClass "is-large"
    ] [ str text ]

let field autofocus placeholder dispatch msg = 
    Field.div [ ]
        [ Control.div [ ]
            [ Input.text
                [ Input.Size IsLarge
                  Input.Placeholder placeholder
                  Input.Props [ 
                    AutoFocus autofocus
                    OnChange (fun ev -> dispatch (msg ev.Value)) ] ] ] ]

let private dropdown (model : Model) = 
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
               [ Dropdown.Item.a [ ]
                   [ str "Item n°1" ] ] ] ]


let private column (model : Model) (dispatch : Msg -> unit) =
    let of_api_result (code : APICode) (result : EnrolResult) =
        List.fold2 (fun acc the_code the_message -> if code = the_code then acc + " " + the_message else acc) "" result.Codes result.Messages

    Column.column
        [ Column.Width (Screen.All, Column.Is4)
          Column.Offset (Screen.All, Column.Is4) ]
        [ Heading.h3
            [ Heading.Modifiers [ Modifier.TextColor IsBlack ] ]
            [ str "Enrol" ]
          Box.box' [ ] [
            field true "First Name" dispatch SetFirstName
            field false "Last Name" dispatch SetLastName
            field true "Email" dispatch SetEmail
            dropdown model
          ]
          Field.div [] [
            enroll_button dispatch ClickSubmit "Submit"
          ]
    ]


let view (model : Model) (dispatch : Msg -> unit) =
    [ Container.container [ Container.IsFullHD 
                            Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] 
        [ column model dispatch ] ]