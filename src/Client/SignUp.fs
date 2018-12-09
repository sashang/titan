module Client.SignUp

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

let sign_up (user_info : Domain.Login) = promise {
    let body = Encode.Auto.toString (2, user_info)
    let! response = post_record "/api/sign-up" body []
    let decoder = Decode.Auto.generateDecoder<Domain.Login>()
    let! text = response.text ()
    let result = Decode.fromString decoder text
    match result with
    | Ok login -> return login
    | Error e -> return failwithf "fail: %s" e
}
    (*
    promise {
        let props =
            [ RequestProperties.Method HttpMethod.POST
              Fetch.requestHeaders [
                HttpRequestHeaders.ContentType "application/json" ]
              RequestProperties.Body !^body ]
        let decoder = Decode.Auto.generateDecoder<Domain.Login>()
        let! result = ModifiedFableFetch.fetch "/api/sign-up" props
        match result with
        | Success response ->
            let! text = response.text ()
            Browser.console.info (sprintf "what we got: %s" text)
            let result = Decode.fromString decoder text
            match result with
            | Ok login -> return login
            | Error e -> return failwithf "fail: %s" e
        | BadStatus response ->
            let message = (string response.Status + " " + response.StatusText + " for URL " + response.Url)
            Browser.console.error message
            return failwith message
        | NetworkError ->
            Browser.console.error "network error"
            return failwith "network error"
        //return! Fetch.fetchAs<Domain.Login> "/api/sign-up" decoder props
    }
    *)

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