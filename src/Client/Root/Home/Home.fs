module Home

open CustomColours
open Domain
open Elmish
open Elmish.Browser.Navigation
open Fulma
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Thoth.Json
open ValueDeclarations

type Msg =
    | SetEmail of string
    | ClickRegister
    | RegisterSuccess of unit
    | RegisterFailure of exn

type Model =
    { Email : string 
      BetaRegistrationResult : BetaRegistrationResult option}

exception RegisterException of BetaRegistrationResult
//what the user sees 1st time
let private register_punter (punter : Domain.BetaRegistration) = promise {
    let body = Encode.Auto.toString (1, punter)
    let props =
        [ RequestProperties.Method HttpMethod.POST
          RequestProperties.Credentials RequestCredentials.Include
          requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                           HttpRequestHeaders.Accept "application/json" ]
          RequestProperties.Body !^(body) ]
    let decoder = Decode.Auto.generateDecoder<BetaRegistrationResult>()
    let! response = Fetch.fetchAs<BetaRegistrationResult> "/api/register-punter" decoder props
    match response.Codes with
    | BetaRegistrationCode.Success::_ ->
        return ()
    | _  -> return (raise (RegisterException response))
}
let init () =
    {Email = ""; BetaRegistrationResult = None }

let update  (model : Model) (msg : Msg): Model*Cmd<Msg> =
    match msg with
    | SetEmail email ->
        { model with Email = email}, Cmd.none
    | ClickRegister ->
        model, Cmd.ofPromise register_punter {Domain.BetaRegistration.Email = model.Email} RegisterSuccess RegisterFailure
    | RegisterSuccess () ->
        {model with BetaRegistrationResult = None}, Cmd.none
    | RegisterFailure err ->
        match err with
        | :? RegisterException as login_ex -> //TODO: check this with someone who knows more. the syntax is weird, and Data0??
            { model with BetaRegistrationResult = Some login_ex.Data0 }, Cmd.none
        | _ ->
            { model with BetaRegistrationResult = None }, Cmd.none

let private of_beta_result (code : BetaRegistrationCode) (result : BetaRegistrationResult) =
    List.fold2 (fun acc the_code the_message -> if code = the_code then acc + " " + the_message else acc) "" result.Codes result.Messages

let first_impression =
    Container.container [ Container.IsFullHD ] 
        [ Columns.columns [ ] 
            [ Column.column []  
                  [ Heading.h1 
                        [ Heading.Modifiers [ Modifier.TextColor IsBlack
                                              Modifier.TextTransform TextTransform.Capitalized
                                              Modifier.TextWeight TextWeight.Bold
                                              Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ] 
                        [ Text.div [ ] [ str "Connect" ] ]
                    Heading.h3 
                        [ Heading.Modifiers [ Modifier.TextColor IsGrey
                                              Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                        [ Text.div [ Common.Props [ Props.Style [ Props.CSSProp.FontFamily "'Montserrat', sans-serif" ] ] ] [ str "and rediscover teaching." ] ] ]
              Column.column [] 
                [ Image.image 
                    [ Image.IsSquare ] 
                    [ img [ Props.Src "Images/teacher.png" ] ] ] ] ]

let target_audience =
    Container.container 
        [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
        [ Columns.columns [ ]
            [ Column.column [ ]
                    [ Box.box' [ Common.Props [ Props.Style [ Props.CSSProp.Height "100%" ] ] ]
                        [ Heading.h3 [ Heading.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] 
                            [ str "For Tutors" ]
                          Text.div 
                            [ Common.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is4) ] ]
                            [ str "Powerful tools to manage your students, schedule classes, and live stream lessons" ] ] ]
              Column.column [ ]
                    [ Box.box' [ Common.Props [ Props.Style [ Props.CSSProp.Height "100%" ] ] ]
                        [ Heading.h3 [ Heading.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ]
                            [ str "For Students" ]
                          Text.div 
                            [ Common.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is4) ] ]
                            [ str "Connect with tutors who know your curriculum." ] ] ] ] ]

let testimonials =
    Container.container 
        [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
        [ Heading.h1
            [ Heading.Modifiers [ Modifier.TextColor IsBlack
                                  Modifier.TextWeight TextWeight.Bold ] ]
            [ str "Why tutors love us" ]
          Columns.columns [ Columns.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
            [ Column.column [ Column.Width (Screen.All, Column.IsHalf) ]
                [ Box.box' [] 
                    [ Text.div 
                        [ Common.Modifiers
                            [ Modifier.TextTransform TextTransform.Italic
                              Modifier.TextSize (Screen.All, TextSize.Is4) ] ]
                        [ str (MAIN_NAME + " has allowed me to increase the number of students attending my class") ] ] ] ] ]

let private beta_program model dispatch =
    Container.container
        [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
        [ Columns.columns [ Columns.IsVCentered]
            [ Column.column [ Column.Width (Screen.All, Column.IsHalf) ]
                [ img [ Props.Src "Images/construction.png" ] ]
              Column.column [ ]
                [ Heading.h5
                    [ Heading.Modifiers 
                        [ Modifier.TextColor IsBlack
                          Modifier.TextTransform TextTransform.Italic ] ]
                        [ str "Enter your email and we'll notify you when we are live!" ]
                  Columns.columns [ Columns.IsCentered; Columns.IsGapless ]
                    [ Column.column [ Column.Width (Screen.All, Column.Is6) ]
                        [ Box.box' [ ]
                            [ Input.text
                                [ Input.Type Input.Email
                                  Input.Placeholder "Email"
                                  Input.Props [ Props.OnChange (fun ev -> dispatch (SetEmail ev.Value)) ] ] ] ]
                      Column.column [ Column.Width (Screen.All, Column.Is1) ]
                        [ Button.button 
                            [ Button.Color IsTitanInfo
                              Button.Props [ Props.OnClick (fun ev -> dispatch ClickRegister)
                                             Props.Style [ Props.CSSProp.Margin "20px" ] ] ]
                            [ str "Register" ] ] ]
                  (match model.BetaRegistrationResult with
                  | Some result ->
                        match List.contains BetaRegistrationCode.BadEmail result.Codes with
                        | true ->
                            Help.help [ Help.Color IsDanger
                                        Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is6) ] ] 
                                      [ str (of_beta_result BetaRegistrationCode.BadEmail result) ]
                        | false -> nothing 
                  | _ -> nothing) ] ] ]

let footer = 
    Footer.footer [ Common.Modifiers [ Modifier.BackgroundColor IsTitanPrimary ] ]
        [ Container.container [ ]
            [ ] ]


let view (model : Model) (dispatch : Msg -> unit) =
    [ Section.section 
        [ Section.Modifiers [ ] ]
        [ first_impression ]
      Section.section 
        [ Section.Modifiers [ Modifier.BackgroundColor IsTitanPrimary ] ]
        [ target_audience ]
      Section.section 
        [ Section.Modifiers [ ] ]
        [ beta_program model dispatch ] ]
