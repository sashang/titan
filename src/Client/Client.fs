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
        { model with page = FirstTimeModel (FirstTime.init ()); username = None }, Cmd.none

    | Some Client.Pages.PageType.Login ->
        { model with page = LoginModel; username = None }, Cmd.none

    | Some Client.Pages.PageType.NewTeacher ->
        { model with page = NewTeacherModel (NewTeacher.init ()); username = None }, Cmd.none

    | Some Client.Pages.PageType.NewPupil ->
        { model with page = NewPupilModel; username = None }, Cmd.none
    
    | Some Client.Pages.PageType.HowItWorks ->
        { model with page = HowItWorksModel; username = None }, Cmd.none

    | Some Client.Pages.PageType.MainSchool->
        { model with page = MainSchoolModel (MainSchool.init "" ""); username = None }, Cmd.none

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
        {page = FirstTimeModel (FirstTime.init ()); username = None}, Cmd.none

    //Redirect this to the appropriate page
    | FirstTimeMsg Client.FirstTime.Msg.ClickContinue, { page = FirstTimeModel ft_model; username = _ } ->
        match ft_model.character with
        | FirstTime.Teacher ->
            {page = NewTeacherModel (Client.NewTeacher.init ()); username = None}, Cmd.none
        | FirstTime.Pupil ->
            {page = NewPupilModel; username = None}, Cmd.none

    //any other message from the FirstTime page
    | FirstTimeMsg msg, {page = FirstTimeModel ft_model; username = _}  ->
        let ft_model', _ = FirstTime.update msg ft_model
        { model with page = FirstTimeModel(ft_model')}, Cmd.none
    
    | NewTeacherMsg Client.NewTeacher.Msg.Submit, {page = NewTeacherModel new_teacher_model; username = _} ->
        {model with page = (MainSchoolModel (Client.MainSchool.init new_teacher_model.teacher_name new_teacher_model.school_name))}, Cmd.none

    | MainSchoolMsg msg, {page = MainSchoolModel main_school_model; username = _} ->
        let main_school_model', _ = MainSchool.update msg main_school_model
        {page = MainSchoolModel main_school_model'; username = None}, Cmd.none

let show = function
| Some x -> string x
| None -> "Loading..."



#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
|> Program.toNavigable Client.Pages.urlParser url_update
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
