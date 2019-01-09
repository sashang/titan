/// The root of the client application. It's where the logging in and signing up 
/// processing is done. Messages to child pages are routed from here.
module Root

open CustomColours
open Dashboard
open Domain
open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Elmish.React
open Fable.Helpers.React.Props
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.PowerPack
open Elmish.Browser.Navigation
open Fulma
open Fulma.Extensions
open Fable.Helpers.React
open ValueDeclarations

type RootMsg =
    | LoginMsg of Login.Msg
    | ClickSignOut
    | ClickTitle
    | SignOutMsg of SignOut.Msg
    | SignUpMsg of SignUp.Msg
    | DashboardMsg of Dashboard.Msg
    | UrlUpdatedMsg of Pages.PageType
    | HomeMsg of Home.Msg
    | EnrolMsg of Enrol.Msg

let string_of_root_msg = function
    | EnrolMsg _ -> "EnrolMsg"
    | LoginMsg _ -> "loginmsg"
    | ClickSignOut -> "ClickSignOut"
    | ClickTitle -> "ClickTitle"
    | SignOutMsg _ -> "SignOutMsg"
    | HomeMsg _ -> "HomeMsg"
    | SignUpMsg _ -> "SignUpMsg"
    | DashboardMsg _ -> "DashboardMsg"
    | UrlUpdatedMsg _ -> "UrlUpdatedMsg"

type PageModel =
    | LoginModel of Login.Model
    | SignUpModel of SignUp.Model
    | DashboardModel of Dashboard.Model
    | EnrolModel of Enrol.Model
    | HomeModel of Home.Model
and
    State = {
        Child : PageModel //which child page I'm at
        Session : Session option //who I am
    }

let url_update (page : Pages.PageType option) (model : State) =
    match page with
    //no page type for some reason (maybe the url to page parser didn't work or the user entered and invalid url)
    //so we just change nothing.
    | None -> model, Cmd.none

    // the following pages require a session token
    | Some Pages.Dashboard ->
        match model with
        | {Session = Some session} ->
            let new_model, cmd = Dashboard.init ()
            { model with Child = DashboardModel new_model}, Cmd.map DashboardMsg cmd
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
        { model with Child = SignUpModel new_model}, Cmd.map LoginMsg cmd

    | Some Pages.PageType.Enrol ->
        let new_model, cmd = Enrol.init ()
        { model with Child = EnrolModel new_model}, Cmd.map EnrolMsg cmd

let init _ : State * Cmd<RootMsg> =
    {Child = HomeModel (Home.init ()); Session = None}, Cmd.none

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

let view model dispatch =
    Hero.hero [
        Hero.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Option.Centered) ]
        Hero.Color IsWhite ] [
        Hero.head [ ] [
            Navbar.navbar [ Navbar.Modifiers [ Modifier.BackgroundColor IsTitanPrimary ] ] [
                Container.container [ Container.Props [ Style [ ] ] ] [
                    Navbar.Brand.div [ ] [
                        Navbar.Item.a [ Navbar.Item.Props [ Href "#" ] ] [
                            img [ Style [ Width "2.5em" ]
                                  Src "https://via.placeholder.com/50" ] ]
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
                | DashboardModel model ->
                    yield Dashboard.view model (DashboardMsg >> dispatch) 
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
        { Child = HomeModel (Home.init ()); Session = None}, Cmd.map SignOutMsg cmd

    | ClickTitle, state ->
        //move to the home page
        state, Navigation.newUrl (Pages.to_path Pages.Home)

    | ClickSignOut, state ->
        let cmd = SignOut.update SignOut.SignOut
        state, Cmd.map SignOutMsg cmd

    | SignUpMsg signup_msg, {Child = SignUpModel model; Session = None} ->
        let new_model, cmd = SignUp.update model signup_msg 
        {state with Child = SignUpModel new_model}, Cmd.map SignUpMsg cmd 

    | HomeMsg home_msg, {Child = HomeModel model; Session = None} ->
        let new_model, cmd = Home.update model home_msg
        {state with Child = HomeModel new_model}, Cmd.map HomeMsg cmd

    | _, {Session = None} ->
        //we don't pass these on, user is not logged in
        state, Cmd.none
        
    //here we are logging in and we are already logged in
    | LoginMsg login_msg, {Session = Some session} ->
        state, Cmd.none

    | DashboardMsg msg, {Child = DashboardModel model; Session = Some session} ->
        Browser.console.info "got dashboardmsg"
        let new_model, cmd = Dashboard.update model msg
        {state with Child = DashboardModel new_model}, Cmd.map DashboardMsg cmd
    
    | UrlUpdatedMsg msg, {Child = some_child; Session = Some session} ->
        Browser.console.info "got updatedurlmsg"
        state, Cmd.none

    | msg, state ->
        Browser.console.error ("got unexpected msg " + string_of_root_msg msg)
        state, Cmd.none