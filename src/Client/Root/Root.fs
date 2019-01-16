/// The root of the client application. It's where the logging in and signing up 
/// processing is done. Messages to child pages are routed from here.
module Root

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
open Thoth.Json
open ValueDeclarations


//convert string from base64. Needed for reading content section of a JWT.
[<Emit("atob($0)")>]
let from_base64 (s:string) : string = jsNative

[<CLIMutable>]
type TitanClaim = 
    { Surname : string 
      GivenName : string
      Email : string
      IsTitan : bool 
      IsStudent : bool 
      IsTutor : bool}
    static member init = 
      { Surname = ""
        GivenName = ""
        Email = ""
        IsTitan = false
        IsTutor = false
        IsStudent = false }

    static member decoder : Decode.Decoder<TitanClaim> =
        Decode.object
            (fun get -> 
                { Surname = get.Required.Field "family_name" Decode.string
                  GivenName= get.Required.Field "given_name" Decode.string
                  Email = get.Required.Field "email" Decode.string
                  IsTutor =  get.Optional.Field "IsTutor" Decode.string = Some "true"
                  IsStudent = get.Optional.Field "IsStudent" Decode.string = Some "true"
                  IsTitan = get.Optional.Field "IsTitan" Decode.string = Some "true" })
    member this.is_first_time = not (this.IsStudent || this.IsTitan || this.IsTutor)

type RootMsg =
    | LoginMsg of Login.Msg
    | ClickSignOut
    | ClickTitle
    | FirstTime of TitanClaim
    | CheckSessionSuccess of Session
    | CheckSessionFailure of exn
    | SignOutMsg of SignOut.Msg
    | SignUpMsg of SignUp.Msg
    | DashboardRouterMsg of DashboardRouter.Msg
    | UrlUpdatedMsg of Pages.PageType
    | HomeMsg of Home.Msg
    | EnrolMsg of Enrol.Msg

type PageModel =
    | LoginModel of Login.Model
    | SignUpModel of SignUp.Model
    | DashboardRouterModel of DashboardRouter.Model
    | EnrolModel of Enrol.Model
    | HomeModel of Home.Model
and
    State = {
        Child : PageModel //which child page I'm at
        Session : Session option //who I am
        Claims : TitanClaim
    } with
    static member init = {Child = HomeModel (Home.init ()); Session = None; Claims = TitanClaim.init } 

let url_update (page : Pages.PageType option) (model : State) : State*Cmd<RootMsg> =
    match page with
    //no page type for some reason (maybe the url to page parser didn't work or the user entered and invalid url)
    //so we just change nothing.
    | None -> model, Cmd.none

    // the following pages require a session token
    | Some Pages.DashboardTutor ->
        match model with
        | {Session = Some session} ->
            let new_model, cmd = Tutor.Dashboard.init ()
            { model with Child = (DashboardRouterModel ({Child = DashboardRouter.TutorModel new_model}))}, Cmd.map (DashboardRouterMsg << DashboardRouter.TutorMsg) cmd
        | {Session = None} ->
            model, Cmd.none

    //these pages we don't care about the token. We can go to them with or without one.
    | Some Pages.PageType.Home ->
        { model with Child = HomeModel (Home.init ()) }, Cmd.none

    | Some Pages.PageType.Login ->
        let new_model, cmd = Login.init ()
        { model with Child = LoginModel new_model}, Cmd.map LoginMsg cmd

    | Some Pages.PageType.SignUp ->
        let new_model, cmd = SignUp.init ()
        { model with Child = SignUpModel new_model}, Cmd.map SignUpMsg cmd

    | Some Pages.PageType.Enrol ->
        let new_model, cmd = Enrol.init ()
        { model with Child = EnrolModel new_model}, Cmd.map EnrolMsg cmd

// let b2s : byte[] -> string = import "byte2string" "../custom.js"
// let something () : string = import "something" "../custom.js"
// let echo (s:string) : string = import "echo" "../custom.js"

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
    {Child = HomeModel (Home.init ()); Session = None; Claims = TitanClaim.init},
     Cmd.ofPromise check_session () CheckSessionSuccess CheckSessionFailure

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


let dashboard_button (claims : TitanClaim) =
    if claims.IsTutor then
        nav_item_button_url Pages.DashboardTutor "Dashboard"
    else
        nothing
    

let view model dispatch =
    Hero.hero [
        Hero.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Option.Centered) ]
        Hero.Color IsWhite ] [
        Hero.head [ ] [
            Navbar.navbar [ Navbar.Modifiers [ Modifier.BackgroundColor IsTitanPrimary ] ] [
                Container.container [ Container.Props [ Style [ ] ] ] [
                    Navbar.Brand.div [ ] [
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
                                yield nav_item_button_url Pages.Enrol "Enrol"
                                yield nav_item_button_url Pages.Login "Login"
                          | Some session ->
                                yield dashboard_button model.Claims
                                yield nav_item_button dispatch ClickSignOut "Sign Out" ]
                ]
            ]
        ]
        Hero.body [ ] 
            [ 
                match model.Child with
                | LoginModel login_model -> 
                    yield Login.view login_model (LoginMsg >> dispatch)
                | SignUpModel sign_up_model ->
                    yield SignUp.view sign_up_model (SignUpMsg >> dispatch)
                | DashboardRouterModel model ->
                    yield DashboardRouter.view model (DashboardRouterMsg >> dispatch) 
                | HomeModel model ->
                    yield! Home.view model (HomeMsg  >> dispatch)
                | EnrolModel model ->
                    yield! Enrol.view model (EnrolMsg  >> dispatch)
            ]
        Hero.foot [ ] [ Home.footer ]
    ]

(*
    have a look at the parent-child description at
    https://elmish.github.io/elmish/parent-child.html to understand how update messages
    propagate from the child to parent. It's more subtle than it appears from surface.
*)
let update (msg : RootMsg) (state : State) : State * Cmd<RootMsg> =
    match msg, state with    
    //here we have a login message and we are not logged in (no session)
    | LoginMsg login_msg, {Child = LoginModel model; Session = None} ->
        let next_model, cmd, ext = Login.update model login_msg
        match ext with
        | Login.ExternalMsg.SignedIn session ->
            {state with Child = LoginModel next_model; Session = Some session}, Cmd.map LoginMsg cmd
        | Login.ExternalMsg.Nop ->
            {state with Child = LoginModel next_model}, Cmd.map LoginMsg cmd

    | SignOutMsg sign_out, state ->
        let cmd = SignOut.update sign_out
        //assume that signing out worked so we delete the sesison
        { Child = HomeModel (Home.init ()); Session = None; Claims = TitanClaim.init}, Cmd.map SignOutMsg cmd

    | ClickTitle, state ->
        //move to the home page
        state, Navigation.newUrl (Pages.to_path Pages.Home)

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
                let new_home_model, home_msg = Home.update home_model Home.FirstTime
                { state with Session = Some session; Claims = claims; Child = HomeModel new_home_model}, Cmd.none
            | _ ->
                { state with Session = Some session; Claims = claims}, Cmd.none
        | Error e -> 
            Browser.console.warn e
            {state with Session = None}, Cmd.none 

    | CheckSessionFailure session, state ->
        {state with Session = None}, Cmd.none 

    | SignUpMsg signup_msg, {Child = SignUpModel model; Session = None} ->
        let new_model, cmd = SignUp.update model signup_msg 
        {state with Child = SignUpModel new_model}, Cmd.map SignUpMsg cmd 

    | HomeMsg home_msg, {Child = HomeModel model; Session = None} ->
        let new_model, cmd = Home.update model home_msg
        {state with Child = HomeModel new_model}, Cmd.map HomeMsg cmd

    | EnrolMsg enrol_msg, {Child = EnrolModel model; Session = None} ->
        let new_model, cmd = Enrol.update model enrol_msg
        {state with Child = EnrolModel new_model}, Cmd.map EnrolMsg cmd
    //here we are logging in and we are already logged in
    | LoginMsg login_msg, {Session = Some session} ->
        state, Cmd.none

    | DashboardRouterMsg msg, {Child = DashboardRouterModel model; Session = Some session} ->
        let new_model, cmd = DashboardRouter.update model msg
        {state with Child = DashboardRouterModel new_model}, Cmd.map DashboardRouterMsg cmd
    
    | UrlUpdatedMsg msg, {Child = some_child; Session = Some session} ->
        Browser.console.info "got updatedurlmsg"
        state, Cmd.none

    | msg, state ->
        Browser.console.error ("got unexpected msg ")
        state, Cmd.none