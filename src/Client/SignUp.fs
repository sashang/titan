module Client.SignUp

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

type Msg =
    | ClickSignUp
    | SignUpSuccess of Domain.Login
    | SetUsername of string
    | SetPassword of string
    | ErrorMsg of exn

type Model =
    { email : string
      password : string }

let init () =
    { email = ""; password = "" }

let sign_up (user_info : Domain.Login) =
    promise {
        let body = Encode.Auto.toString (2, user_info)
        let props =
            [ RequestProperties.Method HttpMethod.POST
              Fetch.requestHeaders [
                HttpRequestHeaders.ContentType "application/json" ]
              RequestProperties.Body !^body ]
        try
            let decoder = Decode.Auto.generateDecoder<Domain.Login>()
            return! Fetch.fetchAs<Domain.Login> "/api/sign-up" decoder props
        with exn ->
            return! failwithf "Could not sign up user: %s." exn.Message
    }

let update (msg : Msg) (model : Model) : Model*Cmd<Msg> =
    match msg with
    | ClickSignUp ->
        Browser.console.info (sprintf "clicked sign up: %s %s" model.email model.password)
        model, Cmd.ofPromise sign_up {Domain.Login.username = model.email; Domain.Login.password = model.password} SignUpSuccess ErrorMsg
    | SetPassword password ->
        { model with password = password }, Cmd.none
    | SetUsername username ->
        { model with email = username }, Cmd.none
    | ErrorMsg err ->
        Browser.console.error ("Failed to login: " + err.Message)
        model, Cmd.none
    | SignUpSuccess login ->
        model, Navigation.newUrl  (Client.Pages.to_path Client.Pages.FirstTime)


let column (dispatch : Msg -> unit) =
    Column.column
        [ Column.Width (Screen.All, Column.Is4)
          Column.Offset (Screen.All, Column.Is4) ]
        [ Heading.h3
            [ Heading.Modifiers [ Modifier.TextColor IsGrey ] ]
            [ str "Sign Up" ]
          Heading.p
            [ Heading.Modifiers [ Modifier.TextColor IsGrey ] ]
            [ str "Please enter your details." ]
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
                Field.div [] [
                    Client.Style.button dispatch ClickSignUp "Sign Up"
                ]
        ]
   ]

let view (model : Model) (dispatch : Msg -> unit) =
    Hero.hero
        [ Hero.Color IsSuccess
          Hero.IsFullHeight
          Hero.Color IsWhite ]
        [ Hero.body [ ]
            [ Container.container
                [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                [ column dispatch ] ] ]