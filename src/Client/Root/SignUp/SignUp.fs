module SignUp

open CustomColours
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
    | SetRole of string
    | SignUpSuccess of SignUpResult
    | SetUsername of string
    | SetPassword of string
    | SetEmail of string
    | SignUpFailure of exn

type Model =
    { email : string
      password : string
      username : string
      role : TitanRole option
      sign_up_result : SignUpResult option}

let init () =
    { email = ""; password = ""; username = ""; role = None ; sign_up_result = None }

let sign_up (user_info : Domain.SignUp) = promise {
    let body = Encode.Auto.toString (2, user_info)
    let! response = post_record "/api/sign-up" body []
    let decoder = Decode.Auto.generateDecoder<Domain.SignUpResult>()
    let! text = response.text ()
    let result = Decode.fromString decoder text
    match result with
    | Ok sign_up_result -> return sign_up_result
    | Error e -> return failwithf "fail: %s" e
}

let string_to_role role =
    match role with
    | "Student" -> Some TitanRole.Student
    | "Principal" -> Some TitanRole.Principal
    | "Admin" -> Some TitanRole.Admin
    | _ -> None

let update  (model : Model) (msg : Msg): Model*Cmd<Msg> =
    match msg with
    | ClickSignUp ->
        Browser.console.info (sprintf "clicked sign up: %s %s" model.email model.password)
        model, Cmd.ofPromise sign_up
            {Domain.SignUp.email = model.email;
            Domain.SignUp.password = model.password;
            Domain.SignUp.username = model.username;
            Domain.SignUp.role = model.role} SignUpSuccess SignUpFailure
    | SetPassword password ->
        { model with password = password }, Cmd.none
    | SetUsername username ->
        { model with username = username }, Cmd.none
    | SetRole role ->
        Browser.console.info ("Role: " + role)
        { model with role = string_to_role role }, Cmd.none
    | SetEmail email ->
        {model with email = email}, Cmd.none
    | SignUpFailure err ->
        Browser.console.info ("Failed to sign up: " + err.Message)
        model, Cmd.none
    | SignUpSuccess result ->
        match result.code with
        | [] ->
            Browser.console.info ("Sign up success ")
            model, Navigation.newUrl  (Pages.to_path Pages.Login)
        | _ ->
            Browser.console.info ("Sign up fail ")
            { model with sign_up_result = Some result }, Cmd.none


let column (model : Model) (dispatch : Msg -> unit) =

    let of_sign_up_result (code : SignUpCode) (result : SignUpResult) =
        List.fold2 (fun acc the_code the_message -> if code = the_code then acc + " " + the_message else acc) "" result.code result.message

    Column.column
        [ Column.Width (Screen.All, Column.Is4)
          Column.Offset (Screen.All, Column.Is4) ]
        [ Heading.h3
            [ Heading.Modifiers [ Modifier.TextColor IsTitanPrimary ] ]
            [ str "Sign Up" ]
          Box.box' [ ] [
                Field.div [ ] [
                    Control.div [ ] [
                        Input.text [
                            Input.Size IsLarge
                            Input.Placeholder "Your Username"
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
                                str (of_sign_up_result SignUpCode.BadUsername r)
                            ]
                        | false -> nothing 
                    | _ ->  nothing)
                ]
                Field.div [ ] [
                    Control.div [ ] [
                        Input.email [
                            Input.Size IsLarge
                            Input.Placeholder "Your Email"
                            Input.Props [ 
                                OnChange (fun ev -> dispatch (SetEmail ev.Value)) 
                            ] 
                        ]
                    ]
                    (match model.sign_up_result with
                    | Some r ->
                        match List.contains SignUpCode.BadEmail r.code with
                        | true ->
                            Help.help [
                                Help.Color IsDanger
                                Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            ] [
                                str (of_sign_up_result SignUpCode.BadEmail r)
                            ]
                        | false -> nothing
                    | _ ->  nothing)
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
                                str (of_sign_up_result SignUpCode.BadPassword r)
                            ]
                        | false -> nothing
                    | _ -> nothing)
                ]
                Field.div [] [
                    Client.Style.button dispatch ClickSignUp "Sign Up"
                ]
        ]
   ]

let view  (model : Model) (dispatch : Msg -> unit) =
     Container.container
        [ Container.IsFullHD
          Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
        [ column model dispatch ]