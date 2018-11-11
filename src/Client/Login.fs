module Client.Login

open Elmish
open Elmish.Browser.Navigation
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Fable.Core.JsInterop
open FableJson
open Fulma
open Shared
open Thoth.Json

type LoginState =
    | LoggedOut
    | LoggedIn

type Msg =
    | LoginSuccess of Domain.Login
    | GetLoginGoogle
    | GotLoginGoogle of UserCredentialsResponse
    | ErrorMsg of exn
    | SetPassword of string
    | SetUsername of string
    | ClickLogin

type Model =
    { login_state : LoginState
      user_info : UserInfo }

let init =
    { login_state = LoggedOut; user_info = {username = ""; password = ""} }

let get_credentials () =
    promise {
        let decoder = Decode.Auto.generateDecoder<UserCredentialsResponse>()
        let! credentials = Fetch.fetchAs<UserCredentialsResponse> ("/api/user-credentials") decoder []
        return credentials
    }

let login (user_info : UserInfo) =
    promise {
        let body = Encode.Auto.toString (2, user_info)
        let props =
            [ Fetch.requestHeaders [
                  HttpRequestHeaders.ContentType "application/json" ]
              RequestProperties.Body !^body ]
        try
            let decoder = Decode.Auto.generateDecoder<Domain.Login>()
            return! Fetch.fetchAs<Domain.Login> "api/login" decoder props
        with _ ->
            return! failwithf "Could not authenticate user."
    }

let update (msg : Msg) (model : Model) : Model*Cmd<Msg> =
    match msg with
    | GetLoginGoogle ->
        Browser.console.error ("requesing auth from Google ")
        { login_state = LoggedOut; user_info = {username = ""; password = ""} }, Cmd.ofPromise get_credentials () GotLoginGoogle ErrorMsg
    | GotLoginGoogle credentials ->
        { model with login_state = LoggedOut}, Navigation.newUrl  (Client.Pages.to_path Client.Pages.FirstTime)
    | ErrorMsg err ->
        Browser.console.error ("Failed to login: " + err.Message)
        { model with login_state = LoggedOut }, Cmd.none
    | SetPassword password ->
        { model with user_info = {username = model.user_info.username; password = password }}, Cmd.none
    | SetUsername username ->
        { model with user_info = {username = username; password = model.user_info.password }}, Cmd.none
    | ClickLogin ->
        Browser.console.info ("clicked login")
        model, Cmd.ofPromise login model.user_info LoginSuccess ErrorMsg


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
          Box.box' [ ]
            [ figure [ Class "avatar" ]
                [ img [ Src "https://placehold.it/128x128" ] ]
              form [ ]
                [ Field.div [ ]
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
                  Button.button
                    [ Button.Color IsPrimary
                      Button.IsFullWidth
                      Button.OnClick (fun _ -> (dispatch ClickLogin))
                      Button.CustomClass "is-large is-block" ]
                    [ str "Login" ] ] ]
          Text.p [ Modifiers [ Modifier.TextColor IsGrey ] ]
            [ a [ ] [ str "Sign Up" ]
              str "\u00A0·\u00A0"
              a [ ] [ str "Forgot Password" ]
              str "\u00A0·\u00A0"
              a [ ] [ str "Need Help?" ] ]
          br [ ] ]


let view (dispatch : Msg -> unit) (model : Model) =
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

