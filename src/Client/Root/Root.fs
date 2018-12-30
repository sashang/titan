/// The root of the client application. It's where the logging in and signing up 
/// processing is done. Messages to child pages are routed from here.
module Root

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
open Client.Style
open Fable.Helpers.React


type RootMsg =
    | LoginMsg of Login.Msg
    | GotoSignUpPage
    | GotoLoginPage
    | ClickSignOut
    | ClickTitle
    | SignOutMsg of SignOut.Msg
    | SignUpMsg of SignUp.Msg
    | UrlUpdatedMsg of Client.Pages.PageType
    | ChildMsg

type PageModel =
    | LoginModel of Login.Model
    | SignUpModel of SignUp.Model
    | HomeModel
and
    State = {
        Child : PageModel //which child page I'm at
        Session : Session option //who I am
    }


let url_update (result : Client.Pages.PageType option) (model : State) =
    match result, model with
    | result, {Session = None} ->
        match result with
        | None ->
            model, Cmd.none

        | Some Client.Pages.PageType.Home->
            { model with Child = HomeModel }, Cmd.none

        | Some Client.Pages.PageType.Login ->
            { model with Child = LoginModel Login.init}, Cmd.none

        | _  ->
            model, Cmd.none
    // session token present so we can go to the page.
    | result, {Session = Some session} ->
        match result with
        | None ->
            model, Cmd.none

        | Some Client.Pages.PageType.Home ->
            { model with Child = HomeModel }, Cmd.none

        | Some Client.Pages.PageType.Login ->
            { model with Child = LoginModel Login.init}, Cmd.none


let init _ : State * Cmd<RootMsg> =
    {Child = HomeModel; Session = None}, Cmd.none

let private nav_item_button (text : string) (msg : RootMsg) dispatch =
    Navbar.Item.div [ ] [ Button.button [ Button.OnClick (fun _ -> (dispatch msg)) ] [ str text ] ]

let view model dispatch =
    Hero.hero [
        Hero.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Option.Centered) ]
        Hero.Color IsWhite
        Hero.IsHalfHeight ] [
        Hero.head [ ] [
            Navbar.navbar [ ] [
                Container.container [ ] [
                    Navbar.Brand.div [ ] [
                        Navbar.Item.a [ Navbar.Item.Props [ Href "#" ] ] [
                            img [ Style [ Width "2.5em" ]
                                  Src "https://via.placeholder.com/350" ] ]
                        Navbar.Item.div [ Navbar.Item.Props [ OnClick (fun e -> dispatch ClickTitle) ] ] [
                            Heading.h3 [ Heading.IsSubtitle] [ str "The New Kid" ]
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
        Hero.body [ ] [
            (match model.Child with
            | LoginModel login_model -> 
              Login.view login_model (LoginMsg >> dispatch)
            | SignUpModel sign_up_model ->
              SignUp.view sign_up_model (SignUpMsg >> dispatch)
            | HomeModel ->
              Home.view)
        ]
    ]

let update (msg : RootMsg) (state : State) : State * Cmd<RootMsg> =
    match msg, state with    
    //here we have a login message and we are not logged in (no session)
    | LoginMsg login_msg, {Child = LoginModel model; Session = None} ->
        let next_model, cmd, ext = Login.update login_msg model
        match ext with
        | Login.ExternalMsg.SignedIn session ->
            {state with Child = LoginModel next_model; Session = Some session}, Cmd.map LoginMsg cmd
        | Login.ExternalMsg.Nop ->
            {state with Child = LoginModel next_model}, Cmd.map LoginMsg cmd

    | SignOutMsg sign_out, state ->
        let cmd = SignOut.update sign_out
        //assume that signing out worked so we delete the sesison
        { Child = HomeModel; Session = None}, Cmd.map SignOutMsg cmd

    | ClickTitle, state ->
        //move to the home page
        {state with Child = HomeModel}, Cmd.none

    | ClickSignOut, state ->
        let cmd = SignOut.update SignOut.SignOut
        state, Cmd.map SignOutMsg cmd

    | GotoSignUpPage, state ->
        {state with Child = SignUpModel (SignUp.init ())}, Cmd.none

    | GotoLoginPage, state ->
        {state with Child = LoginModel Login.init}, Cmd.none

    | SignUpMsg signup_msg, {Child = SignUpModel model; Session = None} ->
        let new_model, cmd = SignUp.update signup_msg model
        {state with Child = SignUpModel new_model}, Cmd.map SignUpMsg cmd 

    | _, {Session = None} ->
        //we don't pass these on, user is not logged in
        state, Cmd.none
        
    //here we are logging in and we are already logged in
    | LoginMsg login_msg, {Session = Some session} ->
        state, Cmd.none

    //pass these messages on to the children
    | ChildMsg, {Session = Some session} ->
        state, Cmd.none