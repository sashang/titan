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
    | GotoSignUpPage
    | GotoLoginPage
    | ClickSignOut
    | ClickTitle
    | SignOutMsg of SignOut.Msg
    | SignUpMsg of SignUp.Msg
    | DashboardMsg of Dashboard.Msg
    | UrlUpdatedMsg of Pages.PageType
    | HomeMsg of Home.Msg

let string_of_root_msg = function
    | LoginMsg _ -> "loginmsg"
    | GotoSignUpPage -> "GotoSignUpPage"
    | GotoLoginPage -> "GotoLoginPage"
    | ClickSignOut -> "ClickSignOut"
    | ClickTitle -> "ClickTitle"
    | SignOutMsg _ -> "SignOutMsg"
    | SignUpMsg _ -> "SignUpMsg"
    | DashboardMsg _ -> "DashboardMsg"
    | UrlUpdatedMsg _ -> "UrlUpdatedMsg"

type PageModel =
    | LoginModel of Login.Model
    | SignUpModel of SignUp.Model
    | DashboardModel of Dashboard.Model
    | HomeModel of Home.Model
and
    State = {
        Child : PageModel //which child page I'm at
        Session : Session option //who I am
    }

exception ConversionException of string
//helpers to help destructure some of these types
let to_dashboard_model = function
    | DashboardModel model -> model
    | _ -> raise (ConversionException "Failed to convert model")

let to_dashboard_page_model = function
    | {Dashboard.Model.Child = Dashboard.SchoolModel model} -> model
    | _ -> raise (ConversionException "Failed to convert model")

let extract =  to_dashboard_model >> to_dashboard_page_model

let url_update (result : Pages.PageType option) (model : State) =
    match result, model with
    | result, {Session = None} ->
        match result with
        | None ->
            model, Cmd.none //no page mapped from the given url, so leave it where it is.

        | Some Pages.PageType.Home->
            { model with Child = HomeModel (Home.init ()) }, Cmd.none

        | Some Pages.PageType.Login ->
            { model with Child = LoginModel Login.init}, Cmd.none

        | _  ->
            model, Cmd.none

    // session token present so we can go to the page.
    | result, {Session = Some session} ->
        match result with
        | None ->
            model, Cmd.none

        | Some Pages.PageType.Home ->
            { model with Child = HomeModel (Home.init ())}, Cmd.none

        | Some Pages.PageType.Login ->
            { model with Child = LoginModel Login.init}, Cmd.none
            
        | Some (Pages.PageType.Dashboard Pages.DashboardPageType.School) ->
            let new_model, cmd = Dashboard.url_update Pages.DashboardPageType.School
            { model with Child = DashboardModel new_model}, Cmd.map DashboardMsg cmd

        | Some (Pages.PageType.Dashboard Pages.DashboardPageType.Main) ->
            let new_model, cmd = Dashboard.url_update Pages.DashboardPageType.Main
            {model with Child = DashboardModel new_model}, Cmd.map DashboardMsg cmd


let init _ : State * Cmd<RootMsg> =
    {Child = HomeModel (Home.init ()); Session = None}, Cmd.none

let private nav_item_button (text : string) (msg : RootMsg) dispatch =
    Navbar.Item.div [ ]
        [ Button.button 
            [ Button.OnClick (fun _ -> (dispatch msg)) ]
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
                                yield! [nav_item_button "Login" GotoLoginPage dispatch
                                        nav_item_button "Sign Up" GotoSignUpPage dispatch]
                          | Some session -> yield nav_item_button "Logout" ClickSignOut dispatch ]
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
                    yield! (Home.view model (HomeMsg  >> dispatch))
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

    | GotoSignUpPage, state ->
        {state with Child = SignUpModel (SignUp.init ())}, Cmd.none

    | GotoLoginPage, state ->
        {state with Child = LoginModel Login.init}, Cmd.none

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