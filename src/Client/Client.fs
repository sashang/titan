module Client.Main

open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Elmish.React


open Fable.Import
open Fable.Import.Browser

open Shared

let handleNotFound (model: SinglePageState) =
    Browser.console.error("Error parsing url: " + Browser.window.location.href)
    ( model, Navigation.modifyUrl (Client.Pages.toPath Client.Pages.PageType.Login) )

/// The navigation logic of the application given a page identity parsed from the .../#info
/// information in the URL.
let url_update (result : Client.Pages.PageType option) (model : SinglePageState) =
    match result with
    | None ->
        handleNotFound model

    | Some Client.Pages.PageType.Home ->
        { model with page = HomeModel; username = None }, Cmd.none

    | Some Client.Pages.PageType.FirstTime ->
        { model with page = FirstTimeModel({pupil = false; teacher = false}); username = None }, Cmd.none

    | Some Client.Pages.PageType.Login ->
        { model with page = LoginModel; username = None }, Cmd.none

let init _ : SinglePageState * Cmd<Msg> =
    {page = HomeModel; username = None}, Cmd.none

(*
    have a look at the parent-child description at
    https://elmish.github.io/elmish/parent-child.html to understand how update messages
    propagate from the child to parent. It's more subtle than it appears from surface.
*)
let update (msg : Msg) (model : SinglePageState) : SinglePageState * Cmd<Msg> =
    match msg, model with
    | Init, _ -> model, Cmd.none

    //When the user logs in redirect to the first time page for now.
    //TODO: Change this when we identify the user properly.
    | LoginMsg _, _ ->
        {page = FirstTimeModel({pupil = false; teacher = false}); username = None}, Cmd.none

    //When the user clicks the background we get this message
    //which means they just want to escape so change the page to the login page
    | FirstTimeMsg Client.FirstTime.Msg.ClickBackground, _ ->
        {page = LoginModel; username = None}, Cmd.none

    //Redirect this to the appropriate page
    | FirstTimeMsg Client.FirstTime.Msg.ClickContinue, { page = FirstTimeModel ft_model; username = _ } ->
        if ft_model.teacher then
            {page = NewTeacherModel; username = None}, Cmd.none
        else if ft_model.pupil then
            {page = NewPupilModel; username = None}, Cmd.none
        else
            {page = LoginModel; username = None}, Cmd.none

    //any other message from the FirstTime page (basically the TogglePupil/ToggleTeacher messages)
    | FirstTimeMsg msg, {page = FirstTimeModel ft_model; username = _}  ->
        let ft_model', _ = FirstTime.update msg ft_model
        { model with page = FirstTimeModel(ft_model')}, Cmd.none

let show = function
| Some x -> string x
| None -> "Loading..."



#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
|> Program.toNavigable Pages.urlParser url_update
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
