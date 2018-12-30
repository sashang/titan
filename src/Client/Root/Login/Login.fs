module Login

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
open Shared
open Thoth.Json

type LoginState =
    | LoggedOut
    | LoggedIn

type LoginEx(msg) =
    inherit System.Exception(msg)

/// Messages that go back to the parent. See https://medium.com/@MangelMaxime/my-tips-for-working-with-elmish-ab8d193d52fd.
/// This basically obviates the need to hijack the session login message in the parent. 
type ExternalMsg =
    | Nop
    | SignedIn of Session

type Msg =
    | Response of Session
    | GetLoginGoogle
    | GotLoginGoogle of UserCredentialsResponse
    | SubmissionFailure of exn
    | SetPassword of string
    | SetUsername of string
    | ClickLogin

type Model =
    { login_state : LoginState
      user_info : Login }

let init =
    { login_state = LoggedOut; user_info = {username = ""; password = ""} }

let get_credentials () =
    promise {
        let props =
            [ RequestProperties.Mode RequestMode.Cors]
        let decoder = Decode.Auto.generateDecoder<UserCredentialsResponse>()
        try
            return! Fetch.fetchAs<UserCredentialsResponse> "/api/user-credentials" decoder props
        with exn ->
            return! failwithf "Could not authenticate user: %s." exn.Message
    }

let login (user_info : Login) =
    promise {
        //2 in toString means 2 fields in the record. in this case it's username and password
        let body = Encode.Auto.toString (2, user_info)
        let! response = post_record "/api/login" body []
        let! text = response.text()
        let decoder = Decode.Auto.generateDecoder<LoginResult>()
        let result = Decode.fromString decoder text
        match result with
        | Ok login ->
            match login.code with
            | LoginCode.Success :: _ ->
                return { username = login.username; token = login.token}
            | _ -> return raise (LoginEx "Failed to login")
        | Error e -> return raise (LoginEx "Failed to dedcode login response")
    }

let update (msg : Msg) (model : Model) : Model*Cmd<Msg>*ExternalMsg =
    match msg with
    | Response session ->
         {model with login_state = LoggedIn},
         //Navigation.newUrl (Client.Pages.to_path Client.Pages.MainSchool),
         Cmd.none,
         SignedIn session //return the session. the parent will see this and can store the session state.
    | GetLoginGoogle ->
        Browser.console.info ("requesing auth from Google ")
        model, Cmd.ofPromise get_credentials () GotLoginGoogle SubmissionFailure, Nop
    | GotLoginGoogle credentials ->
        { model with login_state = LoggedOut}, Navigation.newUrl  (Client.Pages.to_path Client.Pages.MainSchool), Nop
    | SubmissionFailure err ->
        Browser.console.error ("Failed to login: " + err.Message)
        { model with login_state = LoggedOut }, Cmd.none, Nop
    | SetPassword password ->
        { model with user_info = {username = model.user_info.username; password = password }}, Cmd.none, Nop
    | SetUsername username ->
        { model with user_info = {username = username; password = model.user_info.password }}, Cmd.none, Nop
    | ClickLogin ->
        Browser.console.info ("clicked login")
        model, Cmd.ofPromise login model.user_info Response SubmissionFailure, Nop


let column (dispatch : Msg -> unit) =
    Column.column
        [ Column.Width (Screen.All, Column.Is4)
          Column.Offset (Screen.All, Column.Is4) ]
        [ Heading.h3
            [ Heading.Modifiers [ Modifier.TextColor IsGrey ] ]
            [ str "Login" ]
          Heading.p
            [ Heading.Modifiers [ Modifier.TextColor IsGrey ] ]
            [ str "Please login to proceed." ]
          Box.box' [ ] [
                Field.div [ ]
                    [ Control.div [ ]
                        [ Input.email
                            [ Input.Size IsLarge
                              Input.Placeholder "Your Email"
                              Input.Props [ 
                                AutoFocus true
                                OnChange (fun ev -> dispatch (SetUsername ev.Value)) ] ] ] ]
                Field.div [ ]
                    [ Control.div [ ]
                        [ Input.password
                            [ Input.Size IsLarge
                              Input.Placeholder "Your Password"
                              Input.Props [
                                OnChange (fun ev -> dispatch (SetPassword ev.Value)) ] ] ] ]
                Field.div [ ]
                    [ Checkbox.checkbox [ ]
                        [ input [ Type "checkbox" ]
                          str "Remember me" ] ] 
                Field.div [] [
                    Client.Style.button dispatch ClickLogin "Login"
                ]
          ] 
    ]


let view  (model : Model) (dispatch : Msg -> unit) = 
    match model.login_state with
    | LoggedIn ->   
        Hero.hero
            [ Hero.Color IsSuccess
              Hero.IsFullHeight
              Hero.Color IsWhite ]
            [ Hero.body [ ]
                [ div [ Id "greeting"] [
                        h3 [ ClassName "text-center" ] [ str (sprintf "Hi %s!" model.user_info.username) ] ] ] ] 
    | LoggedOut ->
        Hero.hero
            [ Hero.Color IsSuccess
              Hero.IsFullHeight
              Hero.Color IsWhite ]
            [ Hero.body [ ]
                [ Container.container
                    [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ column dispatch ] ] ]

