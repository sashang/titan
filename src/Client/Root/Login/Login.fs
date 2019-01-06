module Login

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
open ModifiedFableFetch
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

type Model =
    { login_state : LoginState
      user_info : Login
      Result : LoginResult option }

/// Exception raised when the request to login is sent and processed on the
/// server, but the return code denotes an error.
exception LoginException of LoginResult
let init =
    { login_state = LoggedOut; user_info = {username = ""; password = ""}; Result = None}

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
            return { username = response.username; token = response.token}
        | _  -> return (raise (LoginException response))
    }

let update  (model : Model) (msg : Msg): Model*Cmd<Msg>*ExternalMsg =
    match msg with
    | Success session ->
        let new_path = Pages.to_path (Pages.Dashboard Pages.DashboardPageType.Main)
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
    | ClickLogin ->
        model, Cmd.ofPromise login model.user_info Success Failure, Nop
    


let column (model : Model) (dispatch : Msg -> unit) =
    let of_login_result (code : LoginCode) (result : LoginResult) =
        List.fold2 (fun acc the_code the_message -> if code = the_code then acc + " " + the_message else acc) "" result.code result.message

    Column.column
        [ Column.Width (Screen.All, Column.Is4)
          Column.Offset (Screen.All, Column.Is4) ]
        [ Heading.h3
            [ Heading.Modifiers [ Modifier.TextColor IsTitanPrimary ] ]
            [ str "Login" ]
          Box.box' [ ] [
                Field.div [ ]
                    [ Control.div [ ]
                        [ Input.text
                            [ Input.Size IsLarge
                              Input.Placeholder "Your Username"
                              Input.Props [ 
                                AutoFocus true
                                OnChange (fun ev -> dispatch (SetUsername ev.Value)) ] ] ] ]
                Field.div [ ] [
                    Control.div [ ]
                        [ Input.password
                            [ Input.Size IsLarge
                              Input.Placeholder "Your Password"
                              Input.Props [
                                OnChange (fun ev -> dispatch (SetPassword ev.Value)) ] ] ]
                    (match model.Result with
                    | Some result ->
                        match List.contains LoginCode.Failure result.code with
                        | true ->
                            Help.help [
                                Help.Color IsDanger
                                Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            ] [
                                str (of_login_result LoginCode.Failure result)
                            ]
                        | false -> nothing 
                    | _ ->  nothing)
                ]
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

    Container.container [ Container.IsFullHD
                          Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [
        (match model.login_state with
        | LoggedIn ->   
            div [ Id "greeting"] [
                  h3 [ ClassName "text-center" ] [ str (sprintf "Hi %s!" model.user_info.username) ] ]
        | LoggedOut ->
            column model dispatch)
    ]

