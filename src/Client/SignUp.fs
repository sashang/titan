module Client.SignUp

open Domain
open Elmish
open Elmish.Browser.Navigation
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.FontAwesome
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Fable.Core.JsInterop
open Fulma
open ModifiedFableFetch
open Thoth.Json

type Msg =
    | ClickSignUp
    | SignUpSuccess of SignUpResult
    | SetUsername of string
    | SetPassword of string
    | SignUpFailure of exn

type Model =
    { email : string
      password : string
      sign_up_result : SignUpResult option}

let init () =
    { email = ""; password = ""; sign_up_result = None }

let sign_up (user_info : Domain.Login) = promise {
    let body = Encode.Auto.toString (2, user_info)
    let! response = post_record "/api/sign-up" body []
    let decoder = Decode.Auto.generateDecoder<Domain.SignUpResult>()
    let! text = response.text ()
    let result = Decode.fromString decoder text
    match result with
    | Ok sign_up_result -> return sign_up_result
    | Error e -> return failwithf "fail: %s" e
}

let update (msg : Msg) (model : Model) : Model*Cmd<Msg> =
    match msg with
    | ClickSignUp ->
        Browser.console.info (sprintf "clicked sign up: %s %s" model.email model.password)
        model, Cmd.ofPromise sign_up {Domain.Login.username = model.email; Domain.Login.password = model.password} SignUpSuccess SignUpFailure
    | SetPassword password ->
        { model with password = password }, Cmd.none
    | SetUsername username ->
        { model with email = username }, Cmd.none
    | SignUpFailure err ->
        Browser.console.info ("Failed to login: " + err.Message)
        model, Cmd.none
    | SignUpSuccess result ->
        match result.code with
        | [] ->
            model, Navigation.newUrl  (Client.Pages.to_path Client.Pages.FirstTime)
        | _ ->
            { model with sign_up_result = Some result }, Cmd.none


let column (model : Model) (dispatch : Msg -> unit) =
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
                Field.div [ ] [
                    Control.div [ ] [
                        Input.email [
                            Input.Size IsLarge
                            Input.Placeholder "Your Email"
                            Input.Props [ 
                                AutoFocus true
                                OnChange (fun ev -> dispatch (SetUsername ev.Value)) 
                            ] 
                        ]
                    ]
                    (match model.sign_up_result with
                    | Some r ->
                        match List.contains SignUpCode.BadUsername r.code with
                        | true ->
                            Help.help [
                                Help.Color IsDanger
                                Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            ] [
                                str "Bad username"
                            ]
                        | false -> p [ ] []
                    | _ ->  p [ ] [ ] )  //not sure how to have a null entry here.
                ]
                Field.div [ ] [
                    Control.div [ ] [
                        Input.password [
                            Input.Size IsLarge
                            Input.Placeholder "Your Password"
                            Input.Props [
                                OnChange (fun ev -> dispatch (SetPassword ev.Value)) 
                            ] 
                        ] 
                    ]
                    (match model.sign_up_result with
                    | Some r ->
                        match List.contains SignUpCode.BadPassword r.code with
                        | true ->
                            Help.help [
                                Help.Color IsDanger
                                Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            ] [
                                str "Bad password"
                            ]
                        | false -> p [ ] []
                    | _ ->  p [ ] [ ] )  //not sure how to have a null entry here.
                ]
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
                [ column model dispatch ] ] ]