/// The root of the client application. It's where the logging in and signing up 
/// processing is done. Messages to child pages are routed from here.
module Root

open Client.Shared
open CustomColours
open DashboardRouter
open Domain
open Elmish
open Elmish.Navigation
open ElmishBridgeModel
open Fable.React.Props
open Fable.Import
open Fulma
open Fetch
open Fable.React
open Fable.Core
open Thoth.Json
type TF = Thoth.Fetch.Fetch
open ValueDeclarations


//convert string from base64. Needed for reading content section of a JWT.
[<Emit("atob($0)")>]
let from_base64 (s:string) : string = jsNative


type RootMsg =
    | TenSecondsTimer
    | ClickSignOut
    | ClickStopLive
    | ClickTitle
    | FirstTime of TitanClaim
    | LoadPPSuccess of string
    | LoadTermsSuccess of string
    | CheckSessionSuccess of Session
    | CheckSessionFailure of exn
    | SignOutMsg of SignOut.Msg
    | DashboardRouterMsg of DashboardRouter.Msg
    | UrlUpdatedMsg of Pages.PageType
    | HomeMsg of Home.Msg
    | PrivacyPolicyMsg of PrivacyPolicy.Msg
    | FAQMsg of FAQ.Msg
    | TermsMsg of Terms.Msg
    | Success of unit
    | Failure of exn
    | ClickLoadPP
    | ClickLoadTerms
    | Remote of ClientMsg

type BroadcastState =
    | Tutor
    | Student

type PageModel =
    | LoginModel
    | FAQModel of FAQ.Model
    | DashboardRouterModel of DashboardRouter.Model
    | HomeModel of Home.Model
    | PrivacyPolicyModel of PrivacyPolicy.Model
    | TermsModel of Terms.Model
and
    State = {
        Child : PageModel //which child page I'm at
        Session : Session option //who I am
        Claims : TitanClaim option
        BCast : BroadcastState option
    } with
    static member init = {Child = HomeModel (Home.init ()); Session = None; Claims = None; BCast = None } 

let url_update (page : Pages.PageType option) (model : State) : State*Cmd<RootMsg> =
    match page with
    //no page type for some reason (maybe the url to page parser didn't work or the user entered and invalid url)
    //so we just change nothing.
    | None -> model, Cmd.none

    // the following pages require a session token
    | Some Pages.DashboardTutor ->
        match model with
        | {Session = Some session; Claims = Some claims} ->
            let new_model, cmd = Tutor.Dashboard.init claims
            { model with Child = (DashboardRouterModel ({Child = DashboardRouter.TutorModel new_model}))},
                                 Cmd.map (DashboardRouterMsg << DashboardRouter.TutorMsg) cmd
        | {Session = None} ->
            model, Cmd.none
            
    | Some Pages.DashboardStudent ->
        match model with
        | {Session = Some session; Claims = Some claims} ->
            let new_model, cmd = Student.Dashboard.init claims
            { model with Child = (DashboardRouterModel ({Child = DashboardRouter.StudentModel new_model}))},
                                 Cmd.map (DashboardRouterMsg << DashboardRouter.StudentMsg) cmd
        | {Session = None} ->
            model, Cmd.none

    //these pages we don't care about the token. We can go to them with or without one.
    | Some Pages.PageType.Home ->
        { model with Child = HomeModel (Home.init ()) }, Cmd.none

    | Some Pages.PageType.Login ->
        { model with Child = LoginModel}, Cmd.none 

    | Some Pages.PageType.FAQ ->
        let faq_model, cmd = FAQ.init ()
        { model with Child = FAQModel faq_model}, Cmd.map FAQMsg cmd

    | Some Pages.PageType.PrivacyPolicy ->
        let pp_model, cmd = PrivacyPolicy.init ()
        { model with Child = PrivacyPolicyModel pp_model }, Cmd.map PrivacyPolicyMsg cmd

    | Some Pages.PageType.Terms ->
        let terms_model, cmd = Terms.init ()
        { model with Child = TermsModel terms_model }, Cmd.map TermsMsg cmd

    | Some Pages.DashboardTitan ->
        match model with
        | {Session = Some session; Claims = Some claims} ->
            let new_model, cmd = Titan.Dashboard.init claims
            { model with Child = (DashboardRouterModel ({Child = DashboardRouter.TitanModel new_model}))},
                                 Cmd.map (DashboardRouterMsg << DashboardRouter.TitanMsg) cmd
        | {Session = None} ->
            model, Cmd.none

let check_session () = promise {
    Browser.Dom.console.info "check_session"
    let props = [ RequestProperties.Method HttpMethod.GET
                  RequestProperties.Credentials RequestCredentials.Include ]
    let decoder = Decode.Auto.generateDecoder<Session>()
    try
        let! response = TF.fetchAs<Session>("/check-session", decoder, props)
        Browser.Dom.console.info "decoded response"
        return response
    with 
        | e -> return failwith (e.Message)
}

let init _ : State * Cmd<RootMsg> =
    State.init, Cmd.OfPromise.either check_session () CheckSessionSuccess CheckSessionFailure

let private goto_url page e =
    Navigation.newUrl (Pages.to_path page) |> List.map (fun f -> f ignore) |> ignore

let private  nav_item_button (dispatch : RootMsg -> unit) (msg : RootMsg) (text : string) =
    Navbar.Item.div [ ]
        [ Button.button 
            [ Button.Color IsTitanInfo
              Button.OnClick (fun e -> dispatch msg)  ]
            [ str text ] ]


let private nav_item_button_url page (text : string) =
    Navbar.Item.div [ ]
        [ Button.button 
            [ Button.Color IsTitanInfo
              Button.OnClick (goto_url page) ]
            [ str text ] ]

let private nav_item_button_href href (text : string) =
    Navbar.Item.div [ ]
        [ Button.a 
            [ Button.Color IsWhite
              Button.Props [ Props.Href href ] ]
            [ str text ] ]

let private footer model dispatch = 
    Footer.footer [ Common.Modifiers [ Modifier.BackgroundColor IsTitanPrimary
                                       Modifier.TextColor IsWhite
                                       Modifier.TextAlignment (Screen.All, TextAlignment.Left )  ] ]
        [ div [] [ a [ OnClick (fun ev -> dispatch ClickLoadPP) ] [ str "Privacy Policy" ] ]
          div [ ] [ a [ OnClick (fun ev -> dispatch ClickLoadTerms) ] [ str "Terms and Conditions" ] ]]


let view model dispatch =
    Hero.hero [
        Hero.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Option.Centered) ]
        Hero.Color IsWhite ] [
        Hero.head [ ] [
            Navbar.navbar [ Navbar.Modifiers [ Modifier.BackgroundColor IsTitanPrimary ] ] [
                Container.container [ Container.Props [ Style [ ] ] ] [
                    Navbar.Brand.div [ ] [
                        Navbar.Item.div [ Navbar.Item.Props [ OnClick (fun e -> dispatch ClickTitle) ] ] [
                            Image.image [ Image.IsSquare; Image.Is32x32] [
                                img [ Src "Images/favicon_symbol.png" ]
                            ]
                        ]
                        Navbar.Item.div [ Navbar.Item.Props [ OnClick (fun e -> dispatch ClickTitle) ] ] [
                            Heading.h3 
                                [ Heading.IsSubtitle
                                  Heading.Modifiers [ Modifier.TextColor IsWhite ]
                                  Heading.Props [ Style [ CSSProp.FontFamily "'Montserrat', sans-serif" ] ] ] [ str MAIN_NAME ]
                        ]
                        Navbar.Item.div [ Navbar.Item.Props [ OnClick (fun e -> dispatch ClickTitle) ] ] [
                            Heading.h5 
                                [ Heading.IsSubtitle
                                  Heading.Modifiers [ Modifier.TextColor IsWhite ]
                                  Heading.Props [ Style [ CSSProp.FontFamily "'Montserrat', sans-serif" ] ] ] [ str "putting tutors first" ]
                        ]
                    ]
                    Navbar.End.div []
                        [ match model.Session with
                          | None -> 
                                yield nav_item_button_url Pages.Login "Register or Login"
                          | Some session ->
                                yield nav_item_button dispatch ClickSignOut "Sign Out" ]
                ]
            ]
            (match model.Claims with
            | Some claims ->
                match claims.IsApproved || claims.IsTitan with
                | true -> nothing
                | false ->
                    Message.message [ Message.Color IsTitanInfo ] [
                        Message.body [ ] [ str ("Hi " + claims.GivenName + "! Thanks for your interest in Tewtin. We will email you when your application is approved.") ]
                    ]
            | None -> nothing)
        ]
        Hero.body [ Common.Props [ Style [ ] ] ] [ 
            match model.Child with
            | LoginModel -> 
                yield Login.view
            | DashboardRouterModel model ->
                yield DashboardRouter.view model (DashboardRouterMsg >> dispatch) 
            | HomeModel model ->
                yield! Home.view model (HomeMsg  >> dispatch)
            | FAQModel model ->
                yield FAQ.view model (FAQMsg >> dispatch)
            | PrivacyPolicyModel model ->
                yield PrivacyPolicy.view model (PrivacyPolicyMsg >> dispatch)
            | TermsModel model ->
                yield Terms.view model (TermsMsg >> dispatch)
        ]
        Hero.foot [ ] [ footer model dispatch ]
    ]

(*
    have a look at the parent-child description at
    https://elmish.github.io/elmish/parent-child.html to understand how update messages
    propagate from the child to parent. It's more subtle than it appears from surface.
*)
let update (msg : RootMsg) (state : State) : State * Cmd<RootMsg> =
    match msg, state with    
    | Remote(TheClientMsg (Msg1 msg)), state ->
        Browser.Dom.console.info ("received message '" + msg + "' from server over bridge")
        state, Cmd.none

    | TenSecondsTimer, {Child = DashboardRouterModel model} ->
        let model', cmd = DashboardRouter.update model DashboardRouter.TenSecondsTimer
        {state with Child = DashboardRouterModel model'}, Cmd.map DashboardRouterMsg cmd

    | TenSecondsTimer, _ -> //ignore other one second timer messages
        state, Cmd.none

    | SignOutMsg sign_out, state ->
        let cmd = SignOut.update sign_out
        //assume that signing out worked so we delete the sesison
        { Child = HomeModel (Home.init ()); Session = None; Claims = None;
          BCast = None}, Cmd.map SignOutMsg cmd

    | ClickTitle, state ->
        //move to the home page
        state, Navigation.newUrl (Pages.to_path Pages.Home)

    | ClickLoadTerms, state ->
        //move to the terms page
        state, Navigation.newUrl (Pages.to_path Pages.Terms)

    | ClickLoadPP, state ->
        //move to the privacy policy page
        state, Navigation.newUrl (Pages.to_path Pages.PrivacyPolicy)

    | FAQMsg msg, {Child = FAQModel model} ->
        let model', cmd' = FAQ.update model msg
        {state with Child = FAQModel model'}, Cmd.map FAQMsg cmd'

    | _, {Child = FAQModel _} ->
        Browser.Dom.console.error "Received unknown message but child is FAQModel"
        state, Cmd.none

    | PrivacyPolicyMsg msg, {Child = PrivacyPolicyModel model} ->
        let model', cmd' = PrivacyPolicy.update model msg
        {state with Child = PrivacyPolicyModel model'}, Cmd.map PrivacyPolicyMsg cmd'

    | _, {Child = PrivacyPolicyModel _} ->
        Browser.Dom.console.error "Received unknown message but child is PrivacyPolicyModel"
        state, Cmd.none

    | TermsMsg msg, {Child = TermsModel model} ->
        let model', cmd' = Terms.update model msg
        {state with Child = TermsModel model'}, Cmd.map TermsMsg cmd'

    | _, {Child = TermsModel _} ->
        Browser.Dom.console.error "Received unknown message but child is TermsModel"
        state, Cmd.none

    | ClickSignOut, state ->
        match state.Child with 
        | DashboardRouterModel model ->
            let dbr_model, dbr_cmd = DashboardRouter.update model DashboardRouter.SignOut
            let cmd = SignOut.update SignOut.SignOut
            state, Cmd.batch [ Cmd.map DashboardRouterMsg dbr_cmd; Cmd.map SignOutMsg cmd ]
        | _ ->
            let cmd = SignOut.update SignOut.SignOut
            state, Cmd.map SignOutMsg cmd

    | CheckSessionSuccess session, state ->
        let jwt_parts = session.Token.Split '.'
        let jwt_content = from_base64 (Array.get jwt_parts 1)
        Browser.Dom.console.info jwt_content
        let result = Decode.fromString TitanClaim.decoder jwt_content
        match result with
        | Ok claims ->
            match state with
            | {Child = HomeModel home_model} when claims.is_first_time ->
                let new_home_model, home_msg = Home.update home_model Home.FirstTimeUser (Some claims)
                { state with Session = Some session; Claims = Some claims; Child = HomeModel new_home_model}, Cmd.none
            | model when claims.IsTitan ->
                let titan_model, cmd = DashboardRouter.init_titan claims
                { state with Session = Some session; Claims = Some claims;
                             Child = DashboardRouterModel(titan_model)}, Cmd.map DashboardRouterMsg cmd

            | model when claims.IsTutor && claims.IsApproved ->
                let tutor_model, cmd = DashboardRouter.init_tutor claims
                { state with 
                    Session = Some session; Claims = Some claims;
                    Child = DashboardRouterModel(tutor_model)}, Cmd.map DashboardRouterMsg cmd
            | model when claims.IsStudent && claims.IsApproved ->
                let student_model, cmd = DashboardRouter.init_student claims
                { state with 
                    Session = Some session; Claims = Some claims;
                    Child = DashboardRouterModel(student_model)}, Cmd.map DashboardRouterMsg cmd
            | model when not claims.IsApproved ->
                let message = "User needs approval from Tewtin"
                Browser.Dom.console.warn message
                { state with Session = Some session; Claims = Some claims}, Cmd.none
            | _ ->
                let message = "Unknown state or claim."
                Browser.Dom.console.error message
                state, Cmd.none

        | Error e -> 
            Browser.Dom.console.warn e
            {state with Session = None}, Cmd.none 

    | CheckSessionFailure session, state ->
        {state with Session = None}, Cmd.none 

    | HomeMsg home_msg, {Child = HomeModel model} ->
        let new_model, cmd = Home.update model home_msg model.Claims
        {state with Child = HomeModel new_model}, Cmd.map HomeMsg cmd

    | _, {Child = HomeModel model} ->
        Browser.Dom.console.error "Received unknown message but child is HomeModel"
        state, Cmd.none

    | Failure e, state ->
        Browser.Dom.console.error ("Failure: " + e.Message)
        state, Cmd.none

    | DashboardRouterMsg msg, {Child = DashboardRouterModel model} ->
        let new_model, cmd = DashboardRouter.update model msg
        {state with Child = DashboardRouterModel new_model}, Cmd.map DashboardRouterMsg cmd

    | DashboardRouterMsg _, {Child = _} ->
        Browser.Dom.console.error "Received DashboardRouterMsg but child is not DashboarRouterModel"
        state, Cmd.none

    | _, {Child = DashboardRouterModel _} ->
        Browser.Dom.console.error "Received unknown message but child is DashboarRouterModel"
        state, Cmd.none

    | _, {Child = LoginModel} ->
        Browser.Dom.console.error "Received unknown message but child is LoginModel. There should be no messages for this model."
        state, Cmd.none
    
