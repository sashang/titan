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
module R = Fable.Helpers.React


type RootMsg =
    | LoginMsg of Login.Msg
    | SignOutMsg of SignOut.Msg
    | UrlUpdatedMsg of Client.Pages.PageType
    | ChildMsg

type PageModel =
    | LoginModel of Login.Model
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
let view model dispatch =
    Hero.hero [
        Hero.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ]
        Hero.Color IsWhite
        Hero.IsHalfHeight ] [
        Hero.head [ ] [
          Section.section [ ] [
              Level.level [ ] [
                  Level.left [ ] [
                      Level.item [ ] [
                          viewLink Client.Pages.Home "The New Kid"
                      ]
                  ]
                  Level.right [ ] [
                      Level.item [ ] [
                          (match model.Session with
                          | None -> viewLink Client.Pages.Login "Sign In"
                          | Some session -> SignOut.view (SignOutMsg >> dispatch))
                      ]
                  ]
              ]
          ]
        ]
        Hero.body [ ] [
          Section.section [] [
              (match model.Child with
              | LoginModel login_model -> 
                    Login.view (LoginMsg >> dispatch) login_model
              | HomeModel ->
                    Home.view)
          ]
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
        { Child = HomeModel; Session = None}, Cmd.map SignOutMsg cmd

    | _, {Session = None} ->
        //we don't pass these on, user is not logged in
        state, Cmd.none

    //here we are logging in and we are already logged in
    | LoginMsg login_msg, {Session = Some session} ->
        state, Cmd.none

    //pass these messages on to the children
    | ChildMsg, {Session = Some session} ->
        state, Cmd.none