module Login

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
open Shared
open Thoth.Json

type LoginState =
    | LoggedOut
    | LoggedIn


/// Messages that go back to the parent. See https://medium.com/@MangelMaxime/my-tips-for-working-with-elmish-ab8d193d52fd.
/// This basically obviates the need to hijack the session login message in the parent.
type ExternalMsg =
    | Nop
    | SignedIn of Session

type Msg =
    | Success of Session
    | Failure of exn
    | SetPassword of string
    | SetUsername of string
    | ClickLogin
    | ClickDelNot

type Model =
    { login_state : LoginState
      user_info : Login
      Result : LoginResult option }

/// Exception raised when the request to login is sent and processed on the
/// server, but the return code denotes an error.
exception LoginException of LoginResult
let init () =
    { login_state = LoggedOut; user_info = {username = ""; password = ""}; Result = None}, Cmd.none

let login (user_info : Login) =
    promise {
        //2 in toString means 2 fields in the record. in this case it's username and password
        let body = Encode.Auto.toString (2, user_info)
        let props =
            [ RequestProperties.Method HttpMethod.POST
              RequestProperties.Credentials RequestCredentials.Include
              requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                               HttpRequestHeaders.Accept "application/json" ]
              RequestProperties.Body !^(body) ]
        let decoder = Decode.Auto.generateDecoder<LoginResult>()
        let! response = Fetch.fetchAs<LoginResult> "/api/login" decoder props
        match response.code with
        | LoginCode.Success::_ ->
            return { Session.Username = response.username; Session.Token = response.token}
        | _  -> return (raise (LoginException response))
    }

let update  (model : Model) (msg : Msg): Model*Cmd<Msg>*ExternalMsg =
    match msg with
    | Success session ->
        let new_path = Pages.to_path Pages.Dashboard
        {model with login_state = LoggedIn},
         Navigation.newUrl new_path,
         SignedIn session //return the session. the parent will see this and can store the session state.

    | Failure err ->
        //in the case wher ethe promise failed, it can fail for 2 reasons
        //1: The sumbission to the server didn't work. in that case SystemException is thrown vial failwith
        //2: The submission worked but the response contained an error code
        Browser.console.info ("Failed to login.")
        match err with
        | :? LoginException as login_ex -> //TODO: check this with someone who knows more. the syntax is weird, and Data0??
            { model with login_state = LoggedOut; Result = Some login_ex.Data0 }, Cmd.none, Nop
        | _ ->
            { model with login_state = LoggedOut; Result = None }, Cmd.none, Nop
    | SetPassword password ->
        { model with user_info = {username = model.user_info.username; password = password }}, Cmd.none, Nop
    | SetUsername username ->
        { model with user_info = {username = username; password = model.user_info.password }}, Cmd.none, Nop
    | ClickDelNot ->
        {model with Result = None }, Cmd.none, Nop
    | ClickLogin ->
        model, Cmd.ofPromise login model.user_info Success Failure, Nop


let private of_login_result (code : LoginCode) (result : LoginResult) =
        List.fold2 (fun acc the_code the_message -> if code = the_code then acc + " " + the_message else acc) "" result.code result.message

let private render_error (model : Model) dispatch =
    match model.Result with
    | Some result ->
        match List.contains LoginCode.Failure result.code with
        | true ->
            Notification.notification
                [ Notification.Modifiers
                    [ Modifier.TextColor IsWhite
                      Modifier.BackgroundColor IsTitanError ]]
                [ str (of_login_result LoginCode.Failure result)
                  Notification.delete [ Common.Props [ OnClick (fun _ -> dispatch ClickDelNot) ] ] [ ] ]
        | false -> nothing
    | _ ->  nothing

let login_button dispatch msg text =
    Button.button [
        Button.Color IsTitanInfo
        Button.OnClick (fun _ -> (dispatch msg))
        Button.CustomClass "is-large"
    ] [ str text ]

let login_with_google_button =
    Columns.columns [ ]
        [ Column.column [ Column.Width (Screen.All, Column.Is1) ]
            [ Fa.i [ Fa.Brand.Google; Fa.Size Fa.Fa2x ] [  ] ]
          Column.column [ ] [
              Button.a [
                    Button.Color IsTitanInfo
                    Button.Props [ Href "/secure/signin-google" ]
                ] [ str "sign in with Google" ] ] ]

let column (model : Model) (dispatch : Msg -> unit) =

    Column.column
        [ Column.Width (Screen.All, Column.Is4)
          Column.Offset (Screen.All, Column.Is4) ]
        [ Heading.h3
            [ Heading.Modifiers [ Modifier.TextColor IsBlack ] ]
            [ str "Login" ]
          Box.box' [ ] [
                Field.div [] [
                    login_with_google_button
                ]
                render_error model dispatch
          ]
    ]

let view  (model : Model) (dispatch : Msg -> unit) =

    Container.container [ Container.IsFullHD
                          Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [
        (match model.login_state with
        | LoggedIn ->
            div [ Id "greeting"] [
                  h3 [ ClassName "text-center" ] [ str (sprintf "Hi %s!" model.user_info.username) ] ]
        | LoggedOut ->
            column model dispatch)
    ]

