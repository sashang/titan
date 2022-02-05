module Home

open Client.Shared
open CustomColours
open Domain
open Elmish
open Elmish.Navigation
open Fulma
open Fetch
open Fable.React
open Fable.React.Props
open Fable.Import
open Thoth.Json
type TF = Thoth.Fetch.Fetch
open ValueDeclarations

type Msg =
    | SetEmail of string
    | ClickRegister
    | ClickDelNot
    | FirstTimeUser
    | Success of unit
    | Failure of exn
    | FirstTimeMsg of FirstTime.Msg
    | RegisterSuccess of unit
    | RegisterFailure of exn

type PageModel =
    | FirstTimeModel of FirstTime.Model

type Model =
    { Email : string
      BetaRegistrationResult : BetaRegistrationResult option
      Claims : TitanClaim option
      Child : PageModel option }

    static member init =
        { Email = ""; BetaRegistrationResult = None; Claims = None; Child = None}

exception RegisterEx of BetaRegistrationResult
//what the user sees 1st time
let private register_punter (punter : Domain.BetaRegistration) = promise {
    let body = Encode.Auto.toString (1, punter)
    let props =
        [ RequestProperties.Method HttpMethod.POST
          Credentials RequestCredentials.Include
          requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                           HttpRequestHeaders.Accept "application/json" ]
          Body (unbox body) ]
    let decoder = Decode.Auto.generateDecoder<BetaRegistrationResult>()
    let! response = TF.tryFetchAs("/api/register-punter", decoder, props)
    match response with
    | Ok codes ->
        return ()
    | Error msg ->
        let error = {Codes = [BetaRegistrationCode.Failure]; Messages = [msg.ToString()]}
        return (raise (RegisterEx error))
}

let init () = Model.init

let update  (model : Model) (msg : Msg) (claims : TitanClaim option): Model*Cmd<Msg> =
    match model, msg, claims with
    | _, SetEmail email, _ ->
        { model with Email = email}, Cmd.none
    | _, ClickDelNot, _ ->
        {model with BetaRegistrationResult = None}, Cmd.none
    | _, ClickRegister, _ ->
        model, Cmd.OfPromise.either register_punter {Domain.BetaRegistration.Email = model.Email} RegisterSuccess RegisterFailure
    | _, RegisterSuccess (), _ ->
        {model with BetaRegistrationResult = None}, Cmd.none
    | _, RegisterFailure err, _ ->
        match err with
        | :? RegisterEx as login_ex -> //TODO: check this with someone who knows more. the syntax is weird, and Data0??
            { model with BetaRegistrationResult = Some login_ex.Data0 }, Cmd.none
        | _ ->
            { model with BetaRegistrationResult = None }, Cmd.none
    | {Child = None}, FirstTimeUser, claims ->
        Browser.Dom.console.info "FirstTimeUser message"
        match claims with
        | Some claims ->
            {model with Child = Some (FirstTimeModel (FirstTime.init true claims))}, Cmd.none
        | None ->
            Browser.Dom.console.error "Need claims for FirstTimeUser otherwise how do we know who this is?"
            model, Cmd.none

    | {Child = Some (FirstTimeModel child) }, FirstTimeMsg msg, _ ->
        Browser.Dom.console.info "FirstTimeMsg message"
        let new_ft_model, new_cmd = FirstTime.update child msg
        {model with Child = Some (FirstTimeModel new_ft_model) }, Cmd.map FirstTimeMsg new_cmd

    | _ ->
        Browser.Dom.console.info "Unknown HomeMsg"
        model, Cmd.none


let private of_beta_result (code : BetaRegistrationCode) (result : BetaRegistrationResult) =
    List.fold2 (fun acc the_code the_message -> if code = the_code then acc + " " + the_message else acc) "" result.Codes result.Messages

let first_impression =
    Container.container [ Container.IsFullHD ]
        [ Columns.columns [ Columns.IsVCentered ]
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
                    [ img [ Props.Src "Images/teacher.png" ] ] ] ]
          Columns.columns [ ] [
              Column.column []  [
                  Heading.h3 [ Heading.Modifiers [ Modifier.TextColor IsBlack
                                                   Modifier.TextTransform TextTransform.Italic ] ] [
                      Text.div [ Common.Props [ Props.Style [ Props.CSSProp.FontFamily "'Montserrat', sans-serif" ] ] ] [
                          str "A platform for independent tutors of school aged kids."
                      ]
                  ]
              ]
          ]
      ]

let private goto_url page e =
    Navigation.newUrl (Pages.to_path page) |> List.map (fun f -> f ignore) |> ignore

let pricing =
    Container.container
        [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
        [ Columns.columns [ Columns.IsVCentered ]
            [ Column.column [ ]
                    [ Image.image
                        [ ]
                        [ img [ Props.Src "Images/greenback.png" ] ] ]
              Column.column [ ]
                    [ Heading.h1 [ Heading.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ]
                        [ str "Pricing" ]
                      Text.div
                        [ Common.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is4)
                                             Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                        [ str "Currently Tewtin is undergoing a trial phase where all services are free of charge!" ] ] ] ]

let curious =
    Container.container
        [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
            [ Columns.columns [ ]
                [ Column.column [
                    Column.Offset (Screen.All, Column.Is4)
                    Column.Width (Screen.All, Column.Is4)
                ] [
                    Box.box' [ ] [
                        Heading.h1 [ Heading.Modifiers
                                               [ Modifier.TextWeight TextWeight.Bold
                                                 Modifier.TextColor IsBlack
                                                 Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                                [ str "Want to know more?" ]
                        Button.button
                            [ Button.Color IsTitanInfo
                              Button.Size IsLarge
                              Button.OnClick (goto_url Pages.FAQ) ]
                            [ str "Click here to find out" ] ] ] ] ]

let archived_video =
    Container.container
        [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
            [ Columns.columns [ Columns.IsVCentered ]
                [ Column.column [ ] [
                      Image.image
                        [ Image.IsSquare ]
                        [ img [ Props.Src "Images/online tutor.png" ] ]
                  ]
                  Column.column [ ] [
                      Heading.h1 [ Heading.Modifiers [ Modifier.TextWeight TextWeight.Bold
                                                       Modifier.TextAlignment  (Screen.All, TextAlignment.Left) ] ]
                        [ str "Deliver private lessons to 100s of students." ]
                      Text.div
                        [ Common.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is4)
                                             Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                        [ str "" ]
                      Text.div
                        [ Common.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is5)
                                             Modifier.TextTransform TextTransform.Italic
                                             Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                        [ str "During the trial phase we artificially limit the numbers." ] ] ]
                  ]

let no_advertising =
    Container.container
        [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
        [ Columns.columns [ ]
            [ Column.column [ ]
                    [ Box.box' [ Common.Props [ Props.Style [ Props.CSSProp.Height "100%" ] ] ]
                        [ Heading.h3 [ Heading.Modifiers [ Modifier.TextWeight TextWeight.Bold
                                                           Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                            [ str "No advertising." ]
                          Text.div
                            [ Common.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is4) ] ]
                            [ str "Education should be free from advertising. Free versions of Skype and Google
                                   Hangouts are reusing student's personal data and conversations for advertising purposes.
                                   We don't do this." ] ] ] ] ]

let target_audience =
    Container.container
        [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
        [ Columns.columns [ ]
            [ Column.column [ ]
                    [ Box.box' [ Common.Props [ Props.Style [ Props.CSSProp.Height "100%" ] ] ]
                        [ Heading.h3 [ Heading.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ]
                            [ str "Grow your classroom." ]
                          Text.div
                            [ Common.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is4) ] ]
                            [ str "Seamless video conferencing with students. No need to dial them in, they just join!" ] ] ]
              Column.column [ ]
                    [ Box.box' [ Common.Props [ Props.Style [ Props.CSSProp.Height "100%" ] ] ]
                        [ Heading.h3 [ Heading.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ]
                            [ str "Manage enrolments" ]
                          Text.div
                            [ Common.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is4) ] ]
                            [ str "A guided enrolment process places you in control." ] ] ]
              Column.column [ ]
                    [ Box.box' [ Common.Props [ Props.Style [ Props.CSSProp.Height "100%" ] ] ]
                        [ Heading.h3 [ Heading.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ]
                            [ str "Free to make contact." ]
                          Text.div
                            [ Common.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is4) ] ]
                            [ str "Students contact tutors free of charge, and vice versa!" ] ] ] ] ]

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

let private render_error (model : Model) dispatch =
    match model.BetaRegistrationResult with
    | Some result ->
        match List.exists (fun x -> x = BetaRegistrationCode.BadEmail
                                    || x = BetaRegistrationCode.DatabaseError
                                    || x = BetaRegistrationCode.Failure) result.Codes with
        | true ->
            Notification.notification
                [ Notification.Modifiers
                    [ Modifier.TextColor IsWhite
                      Modifier.BackgroundColor IsTitanError ]]
                [ str (of_beta_result BetaRegistrationCode.BadEmail result)
                  str (of_beta_result BetaRegistrationCode.Failure result)
                  str (of_beta_result BetaRegistrationCode.DatabaseError result)
                  Notification.delete [ Common.Props [ OnClick (fun _ -> dispatch ClickDelNot) ] ] [ ] ]
        | false -> nothing
    | None -> nothing

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
                  render_error model dispatch ] ] ]


let view (model : Model) (dispatch : Msg -> unit) =
    [ Section.section
        [ Section.Modifiers [ ] ]
        [ first_impression ]
      Section.section
        [ ]
        [ (match model.Child with
          | Some (FirstTimeModel ft_child) -> FirstTime.view ft_child (dispatch << FirstTimeMsg)
          | _ -> nothing) ]
      Section.section
        [ Section.Modifiers [ Modifier.BackgroundColor IsTitanPrimary ] ]
        [ target_audience ]
      Section.section
        [ Section.Modifiers [ Modifier.BackgroundColor IsWhite ] ]
        [ archived_video ]
      Section.section
        [ Section.Modifiers [ Modifier.BackgroundColor IsTitanPrimary ] ]
        [ no_advertising ]
      Section.section
        [ Section.Modifiers [ Modifier.BackgroundColor IsWhite ] ]
        [ pricing ]
      Section.section
        [ Section.Modifiers [ Modifier.BackgroundColor IsTitanPrimary ] ]
        [ curious ]
    ]
