module Client.Main

open Client.Pages
open Domain
open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Elmish.React
open Fable.Import
open Fable.Import.Browser
open Fable.PowerPack
open Shared

let secret = "spadR2dre#u-ruBrE@TepA&*Uf@U"
let issuer = "saturnframework.io"

let handleNotFound (model: SinglePageState) =
    Browser.console.error("Error parsing url: " + Browser.window.location.href)
    ( model, Navigation.modifyUrl (Client.Pages.to_path Client.Pages.PageType.Login) )

/// The navigation logic of the application given a page identity parsed from the .../#info
/// information in the URL. In cases where the session = None then we stay where we are for most pages
/// Login, Home, SignUp are exempt
let url_update (result : Client.Pages.PageType option) (model : SinglePageState) =
    match result, model with
    | result, {session = None} ->
        match result with
        | None ->
            handleNotFound model
        | Some Client.Pages.PageType.Home->
            { model with page = HomeModel }, Cmd.none

        | Some Client.Pages.PageType.Login ->
            { model with page = LoginModel Login.init}, Cmd.none

        | Some SignUp ->
            { model with page = SignUpModel (SignUp.init ())}, Cmd.none

        | _  ->
            model, Cmd.none
    // session token present so we can go to the page.
    | result, {session = Some session} ->
        match result with
        | None ->
            handleNotFound model

        | Some Client.Pages.PageType.MainSchool ->
            {model with page = MainSchoolModel (MainSchool.init "" "" [])}, Cmd.none

        | Some Client.Pages.PageType.AddClass ->
            { model with page = AddClassModel (AddClass.init ())}, Cmd.none

        | Some Client.Pages.PageType.Home ->
            { model with page = HomeModel }, Cmd.none

        | Some Client.Pages.PageType.Login ->
            { model with page = LoginModel Login.init}, Cmd.none

        | Some SignUp ->
            { model with page = SignUpModel (SignUp.init ())}, Cmd.none
        


    
        
let init _ : SinglePageState * Cmd<Msg> =
    {page = HomeModel; session = None}, Cmd.none

let print_claims (claims : TitanClaim list) =
    claims |>
    List.map (fun x -> "type = " + x.Type + " value = " + x.Value) |>
    List.iter (fun x -> Browser.console.info x)
    

let msg_to_string msg = function
| SignUpMsg _ -> "SignUpMsg"
| LoginMsg _ -> "LoginMsg"
| MainSchoolMsg _ -> "MainSchoolMsg"
| AddClassMsg _ -> "AddClassMsg"
| SignOutMsg _ -> "SignOutMsg"
| UrlUpdatedMsg _ -> "UrlUpdatedMsg"
| Init _ -> "Init"

(*
    have a look at the parent-child description at
    https://elmish.github.io/elmish/parent-child.html to understand how update messages
    propagate from the child to parent. It's more subtle than it appears from surface.
*)
let update (msg : Msg) (sps : SinglePageState) : SinglePageState * Cmd<Msg> =
    match msg, sps with    
    | SignUpMsg msg, {page = SignUpModel model} ->
        let model', cmd = SignUp.update msg model
        {sps with page = SignUpModel model'}, Cmd.map SignUpMsg cmd

    | LoginMsg msg, {page = LoginModel login_model; session = None} ->
        //The external message returned tells us if we've signed in or not
        //If we've signed in then unpack it to get the session
        let next_state, cmd', ext_msg = Login.update msg login_model
        match ext_msg with
        | Login.ExternalMsg.SignedIn session ->
            {sps with page = LoginModel next_state; session = Some session}, Cmd.map LoginMsg cmd'
        | Login.ExternalMsg.Nop ->
            {sps with page = LoginModel next_state}, Cmd.map LoginMsg cmd'

    //any other message with no session we don't process
    | msg, {session = None} -> 
        sps, Cmd.none

    //from this point we catch all messages with sessions
    | MainSchoolMsg msg, {page = MainSchoolModel main_school_model; session = Some session} ->
        let main_school_model', cmd' = MainSchool.update msg main_school_model session
        {sps with page = MainSchoolModel main_school_model'}, Cmd.map MainSchoolMsg cmd'

    | AddClassMsg msg, {page = AddClassModel model} ->
        let model', cmd' = AddClass.update msg model
        {sps with page = AddClassModel model'}, Cmd.map AddClassMsg cmd'

    //signout msg can come from any page. 
    | SignOutMsg msg, some_page ->
        let cmd' = SignOut.update msg
        //remove the session token
        Browser.console.info "Removing session token"
        {sps with session = None }, Cmd.map SignOutMsg cmd'

    | UrlUpdatedMsg next_page, state ->
        url_update (Some next_page) state

    //potentially mismatched pages and messages we don't care about
    //i.e an AddClasMsg, page = MainSchoolModel which.  The only example of this is where a signout message can
    //come from any page, but that's handled above
    | any_msg, any_page ->
        Browser.console.info (msg_to_string any_msg)
        failwith "All messages not handled. Should not reach here"

let show = function
| Some x -> string x
| None -> "Loading..."

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

let view model dispatch =
    view_page model dispatch 
let  [<Literal>]  nav_event = "NavigationEvent"
let url_sub appState : Cmd<_> = 
    [ fun dispatch -> 
        let on_change _ = 
            match url_parser window.location with 
            | Some parsedPage -> dispatch (UrlUpdatedMsg parsedPage)
            | None -> ()
        
        // listen to manual hash changes or page refresh
        window.addEventListener_hashchange(unbox on_change)
        // listen to custom navigation events published by `Urls.navigate [ . . .  ]`
        window.addEventListener(nav_event, unbox on_change) ]  

Program.mkProgram init update view
|> Program.withSubscription url_sub //detect changes typed into the address bar
|> Program.toNavigable Client.Pages.url_parser url_update
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
