/// The root of the client application. It's where the logging in and signing up 
/// processing is done. Messages to child pages are routed from here.
module Root

open Client.Shared
open CustomColours
open DashboardRouter
open Domain
open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Fable.Helpers.React.Props
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Fulma
open Fable.Helpers.React
open Fable.Core
open Fable.Core.JsInterop
open ModifiedFableFetch
open Thoth.Json
open Tutor.LiveView
open ValueDeclarations


//convert string from base64. Needed for reading content section of a JWT.
[<Emit("atob($0)")>]
let from_base64 (s:string) : string = jsNative


type RootMsg =
    | ClickSignOut
    | ClickGoLive
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
    | TutorLiveViewMsg of Tutor.LiveView.Msg
    | GetEnrolledSchoolsSuccess of School list
    | StudentLiveViewMsg of Student.LiveView.Msg
    | Success of unit
    | Failure of exn
    | ClickLoadPP
    | ClickLoadTerms

type PageModel =
    | LoginModel
    | FAQModel of FAQ.Model
    | DashboardRouterModel of DashboardRouter.Model
    | HomeModel of Home.Model
    | PrivacyPolicyModel of PrivacyPolicy.Model
    | TermsModel of Terms.Model
    | TutorLiveViewModel of Tutor.LiveView.Model
    | StudentLiveViewModel of Student.LiveView.Model
and
    State = {
        Child : PageModel //which child page I'm at
        Session : Session option //who I am
        IsLive : bool
        Claims : TitanClaim option
        EnrolledSchools : School list //what a mess
    } with
    static member init () = 
        let model, cmd = Home.init ()
        {Child = HomeModel model; IsLive = false;
         Session = None; Claims = None; EnrolledSchools = [] }, Cmd.map HomeMsg cmd


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
        let model', cmd = Home.init ()
        { model with Child = HomeModel model' }, Cmd.map HomeMsg cmd

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
    Browser.console.info "check_session"
    let props = [ RequestProperties.Method HttpMethod.GET
                  RequestProperties.Credentials RequestCredentials.Include ]
    let decoder = Decode.Auto.generateDecoder<Session>()
    try
        let! response = Fetch.fetchAs<Session> "/check-session" decoder props
        Browser.console.info "decoded response"
        return response
    with 
        | e -> return failwith (e.Message)
}

let init _ : State * Cmd<RootMsg> =
    let model', cmd = State.init ()
    model', Cmd.batch [ Cmd.ofPromise check_session () CheckSessionSuccess CheckSessionFailure ]

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

let private  nav_item_stop_button (dispatch : RootMsg -> unit) =
    Navbar.Item.div [ ]
        [ Button.button 
            [ Button.Color IsDanger
              Button.OnClick (fun e -> dispatch ClickStopLive)  ]
            [ str "Stop" ] ]

let private hero_head model dispatch =
    Hero.head [ ] [
        Navbar.navbar [ Navbar.Modifiers [ Modifier.BackgroundColor IsTitanPrimary ] ] [
            Container.container [ Container.Props [ Style [ ] ] ] [
                Navbar.Brand.div [ ] [
                    Navbar.Item.div [ Navbar.Item.Props [ OnClick (fun e -> dispatch ClickTitle) ] ] [
                        Image.image [ Image.IsSquare; Image.Is24x24 ] [
                            img [ Src "Images/tewtin-cube-logo-circle.svg" ]
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
                Navbar.End.div [] [
                    match model.Session with
                      | None -> 
                            yield nav_item_button_url Pages.Login "Register or Login"
                      | Some session ->
                            match model.IsLive with
                              | false -> 
                                    yield nav_item_button dispatch ClickGoLive "Go Live!"
                                    yield nav_item_button dispatch ClickSignOut "Sign Out"
                              | true ->
                                    yield nav_item_stop_button dispatch
                                    yield nav_item_button dispatch ClickSignOut "Sign Out"
                ]
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


let view model dispatch =
    Hero.hero [ Hero.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Option.Centered) ]
                Hero.Color IsWhite ] [
            hero_head model dispatch
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
                | TutorLiveViewModel model ->
                    yield Tutor.LiveView.view model (TutorLiveViewMsg >> dispatch)
                | StudentLiveViewModel model ->
                    yield Student.LiveView.view model (StudentLiveViewMsg >> dispatch)
            ]
            (match model.Child with
            | TutorLiveViewModel _ -> nothing
            | StudentLiveViewModel _ -> nothing
            | _ -> Hero.foot [ ] [ footer model dispatch ])
    ]

(*
    have a look at the parent-child description at
    https://elmish.github.io/elmish/parent-child.html to understand how update messages
    propagate from the child to parent. It's more subtle than it appears from surface.
*)
let update (msg : RootMsg) (state : State) : State * Cmd<RootMsg> =
    match msg, state with    

    | SignOutMsg sign_out, state ->
        let cmd = SignOut.update sign_out
        //assume that signing out worked so we delete the sesison
        let new_model, cmd = Home.init ()
        {Child = HomeModel new_model; IsLive = false;
         EnrolledSchools = []; Session = None; Claims = None}, Cmd.map SignOutMsg cmd

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

    | PrivacyPolicyMsg msg, {Child = PrivacyPolicyModel model} ->
        let model', cmd' = PrivacyPolicy.update model msg
        {state with Child = PrivacyPolicyModel model'}, Cmd.map PrivacyPolicyMsg cmd'

    | TermsMsg msg, {Child = TermsModel model} ->
        let model', cmd' = Terms.update model msg
        {state with Child = TermsModel model'}, Cmd.map TermsMsg cmd'

    | _, {Child = TermsModel model} ->
        Browser.console.info "Invalid message for TermsModel"
        state, Cmd.none

    | ClickSignOut, state ->
        let cmd = SignOut.update SignOut.SignOut
        state, Cmd.map SignOutMsg cmd


    | CheckSessionSuccess session, state ->
        let jwt_parts = session.Token.Split '.'
        let jwt_content = from_base64 (Array.get jwt_parts 1)
        Browser.console.info jwt_content
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
                Browser.console.warn message
                { state with Session = Some session; Claims = Some claims}, Cmd.none
            | _ ->
                let message = "Unknown state or claim."
                Browser.console.error message
                state, Cmd.none

        | Error e -> 
            Browser.console.warn e
            {state with Session = None}, Cmd.none 

    | CheckSessionFailure session, state ->
        {state with Session = None}, Cmd.none 

    | HomeMsg home_msg, {Child = HomeModel model} ->
        let new_model, cmd = Home.update model home_msg model.Claims
        {state with Child = HomeModel new_model}, Cmd.map HomeMsg cmd

    | _, {Child = HomeModel model} ->
        Browser.console.warn "Invalid message for HomeModel"
        state, Cmd.none

    | Failure e, state ->
        Browser.console.error ("Failure: " + e.Message)
        state, Cmd.none

    | ClickGoLive, state ->
        match state.Claims with
        | Some claims ->
            if claims.IsTutor && claims.IsApproved then
                let new_model, cmd = Tutor.LiveView.init state.Claims.Value.Email
                {state with Child = TutorLiveViewModel new_model; IsLive = true}, Cmd.map TutorLiveViewMsg cmd
            else if claims.IsStudent && claims.IsApproved then
                //TODO: need to fix this if student is enrolled in multiple schools
                let new_model, cmd = Student.LiveView.init (List.head state.EnrolledSchools) claims.Email
                {state with Child = StudentLiveViewModel new_model; IsLive = true}, Cmd.map StudentLiveViewMsg cmd
            else
                let new_model, cmd = Home.init ()
                {state with Child = HomeModel new_model; IsLive = false; EnrolledSchools = []}, Cmd.map HomeMsg cmd
        | None ->
            let new_model, cmd = Home.init ()
            {state with Child = HomeModel new_model; IsLive = false; EnrolledSchools = []; Session = None}, Cmd.map HomeMsg cmd

    | ClickStopLive, state ->
        match state.Claims with
        | Some claims ->
            if claims.IsTutor && claims.IsApproved then
               let tutor_model, cmd = DashboardRouter.init_tutor claims
               { state with Child = DashboardRouterModel(tutor_model); IsLive = false}, Cmd.map DashboardRouterMsg cmd
            else if claims.IsStudent  && claims.IsApproved then
               let student_model, cmd = DashboardRouter.init_student claims
               { state with Child = DashboardRouterModel(student_model); IsLive = false}, Cmd.map DashboardRouterMsg cmd
            else
                let new_model, cmd = Home.init ()
                {state with Child = HomeModel new_model; IsLive = false; EnrolledSchools = []}, Cmd.map HomeMsg cmd
        | None ->
            let new_model, cmd = Home.init ()
            {state with Child = HomeModel new_model; IsLive = false; EnrolledSchools = []; Session = None}, Cmd.map HomeMsg cmd

    | DashboardRouterMsg msg, {Child = DashboardRouterModel model; Session = Some session} ->
        let new_model, cmd, ext = DashboardRouter.update model msg
        match ext with
        | DashboardRouter.ExternalMsg.EnrolledSchools schools ->
            //copy the enrolled schools list into this models state --- this is not ideal..but oh well
            {state with Child = DashboardRouterModel new_model; EnrolledSchools = schools}, Cmd.map DashboardRouterMsg cmd
        | DashboardRouter.ExternalMsg.Noop ->
            {state with Child = DashboardRouterModel new_model}, Cmd.map DashboardRouterMsg cmd

    | _, {Child = DashboardRouterModel model} -> //any dashboard router model paired with a non-dashboardrouterMsg is an error
        Browser.console.warn "Invalid message for DashboardRouterModel"
        state, Cmd.none

    | DashboardRouterMsg msg, _ -> //any other dashboard router msg is an error
        Browser.console.warn "Invalid DashboardRouterMsg"
        state, Cmd.none

    | TutorLiveViewMsg msg, {Child = TutorLiveViewModel model} ->
        Browser.console.info "got TutorLiveViewMsg"
        let model', cmd = Tutor.LiveView.update model msg
        {state with Child = TutorLiveViewModel model'}, Cmd.map TutorLiveViewMsg cmd

    | StudentLiveViewMsg msg, {Child = StudentLiveViewModel model} ->
        Browser.console.info "got TutorLiveViewMsg"
        let model', cmd = Student.LiveView.update model msg
        {state with Child = StudentLiveViewModel model'}, Cmd.map TutorLiveViewMsg cmd

    | UrlUpdatedMsg msg, {Child = some_child; Session = Some session} ->
        Browser.console.info "got updatedurlmsg"
        state, Cmd.none
