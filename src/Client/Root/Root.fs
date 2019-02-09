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
open Thoth.Json
open ValueDeclarations


//convert string from base64. Needed for reading content section of a JWT.
[<Emit("atob($0)")>]
let from_base64 (s:string) : string = jsNative


type RootMsg =
    | ClickSignOut
    | ClickStopLive
    | ClickTitle
    | FirstTime of TitanClaim
    | CheckSessionSuccess of Session
    | CheckSessionFailure of exn
    | SignOutMsg of SignOut.Msg
    | DashboardRouterMsg of DashboardRouter.Msg
    | UrlUpdatedMsg of Pages.PageType
    | HomeMsg of Home.Msg
    | Success of unit
    | Failure of exn

type BroadcastState =
    | Tutor
    | Student

type PageModel =
    | LoginModel
    | DashboardRouterModel of DashboardRouter.Model
(*    | EnrolModel of Enrol.Model*)
    | HomeModel of Home.Model
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
        | {Session = Some session} ->
            let new_model, cmd = Tutor.Dashboard.init ()
            { model with Child = (DashboardRouterModel ({Child = DashboardRouter.TutorModel new_model}))},
                                 Cmd.map (DashboardRouterMsg << DashboardRouter.TutorMsg) cmd
        | {Session = None} ->
            model, Cmd.none
            
    | Some Pages.DashboardStudent ->
        match model with
        | {Session = Some session} ->
            let new_model, cmd = Student.Dashboard.init ()
            { model with Child = (DashboardRouterModel ({Child = DashboardRouter.StudentModel new_model}))},
                                 Cmd.map (DashboardRouterMsg << DashboardRouter.StudentMsg) cmd
        | {Session = None} ->
            model, Cmd.none


    //these pages we don't care about the token. We can go to them with or without one.
    | Some Pages.PageType.Home ->
        { model with Child = HomeModel (Home.init ()) }, Cmd.none

    | Some Pages.PageType.Login ->
        { model with Child = LoginModel}, Cmd.none 

    | Some Pages.DashboardTitan ->
        let message = "DashboardTitan page not implemented"
        Browser.console.error message
        failwith message

(*    | Some Pages.PageType.Enrol ->
        let new_model, cmd = Enrol.init ()
        { model with Child = EnrolModel new_model}, Cmd.map EnrolMsg cmd*)

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
    State.init, Cmd.ofPromise check_session () CheckSessionSuccess CheckSessionFailure

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



let view model dispatch =
    Hero.hero [
        Hero.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Option.Centered) ]
        Hero.Color IsWhite ] [
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
                    Navbar.End.div []
                        [ match model.Session with
                          | None -> 
                                //yield nav_item_button_url Pages.Enrol "Enrol"
                                //yield nav_item_button_href "/schools.html" "Schools"
                                yield nav_item_button_url Pages.Login "Login"
                          | Some session ->
                                yield nav_item_button dispatch ClickSignOut "Sign Out" ]
                ]
            ]
        ]
        Hero.body [ Common.Props [ Style [ ] ] ] [ 
            match model.Child with
            | LoginModel -> 
                yield Login.view
            | DashboardRouterModel model ->
                yield DashboardRouter.view model (DashboardRouterMsg >> dispatch) 
            | HomeModel model ->
                yield! Home.view model (HomeMsg  >> dispatch)
(*                | EnrolModel model ->
                yield! Enrol.view model (EnrolMsg  >> dispatch)*)
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

    | SignOutMsg sign_out, state ->
        let cmd = SignOut.update sign_out
        //assume that signing out worked so we delete the sesison
        { Child = HomeModel (Home.init ()); Session = None; Claims = None;
          BCast = None}, Cmd.map SignOutMsg cmd

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
                let new_home_model, home_msg = Home.update home_model Home.FirstTimeUser claims
                { state with Session = Some session; Claims = Some claims; Child = HomeModel new_home_model}, Cmd.none
            | model when claims.IsTutor  ->
                let tutor_model, cmd = DashboardRouter.init_tutor
                { state with 
                    Session = Some session; Claims = Some claims;
                    Child = DashboardRouterModel(tutor_model)}, Cmd.map DashboardRouterMsg cmd
            | model when claims.IsStudent  ->
                let student_model, cmd = DashboardRouter.init_student
                { state with 
                    Session = Some session; Claims = Some claims;
                    Child = DashboardRouterModel(student_model)}, Cmd.map DashboardRouterMsg cmd
            | model when claims.IsTitan  ->
                let message = "Titan user update not implemented"
                Browser.console.error message
                failwith message
            | _ ->
                let message = "Unknown state or claim."
                Browser.console.error message
                failwith message

        | Error e -> 
            Browser.console.warn e
            {state with Session = None}, Cmd.none 

    | CheckSessionFailure session, state ->
        {state with Session = None}, Cmd.none 

    | HomeMsg home_msg, {Child = HomeModel model; Claims = Some claims} ->
        Browser.console.info "HomeMsg with claims"
        let new_model, cmd = Home.update model home_msg claims
        {state with Child = HomeModel new_model}, Cmd.map HomeMsg cmd

    | HomeMsg home_msg, {Claims = None} -> //no claims so don't pass it through
        Browser.console.info "HomeMsg with no claims"
        state, Cmd.none

(*    | EnrolMsg enrol_msg, {Child = EnrolModel model; Session = None} ->
        let new_model, cmd = Enrol.update model enrol_msg
        {state with Child = EnrolModel new_model}, Cmd.map EnrolMsg cmd*)

    | DashboardRouterMsg msg, {Child = DashboardRouterModel model; Session = Some session} ->
        let new_model, cmd = DashboardRouter.update model msg
        {state with Child = DashboardRouterModel new_model}, Cmd.map DashboardRouterMsg cmd
    
    | UrlUpdatedMsg msg, {Child = some_child; Session = Some session} ->
        Browser.console.info "got updatedurlmsg"
        state, Cmd.none

    | msg, state ->
        Browser.console.error ("got unexpected msg ")
        state, Cmd.none