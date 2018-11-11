module Client.Main

open Client.Pages
open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Elmish.React
open Fable.Import
open Fable.Import.Browser
open Fable.PowerPack
open Shared

let handleNotFound (model: SinglePageState) =
    Browser.console.error("Error parsing url: " + Browser.window.location.href)
    ( model, Navigation.modifyUrl (Client.Pages.to_path Client.Pages.PageType.Login) )

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
        { model with page = LoginModel Login.init; username = None }, Cmd.none

    | Some Client.Pages.PageType.NewTeacher ->
        { model with page = NewTeacherModel (NewTeacher.init ()); username = None }, Cmd.none

    | Some Client.Pages.PageType.NewPupil ->
        { model with page = NewPupilModel; username = None }, Cmd.none
    
    | Some Client.Pages.PageType.HowItWorks ->
        { model with page = HowItWorksModel; username = None }, Cmd.none

    | Some (Client.Pages.PageType.MainSchool model_data) ->
        match model_data with
        | Some (sn, tn) ->
            { model with page = MainSchoolModel (MainSchool.init sn tn []); username = None }, Cmd.none
        | None ->
            { model with page = MainSchoolModel (MainSchool.init "" "" []); username = None }, Cmd.none

    | Some AddClass ->
        { model with page = AddClassModel (AddClass.init ()); username = None }, Cmd.none
        
let init _ : SinglePageState * Cmd<Msg> =
    {page = HomeModel; username = None}, Cmd.none

(*
    have a look at the parent-child description at
    https://elmish.github.io/elmish/parent-child.html to understand how update messages
    propagate from the child to parent. It's more subtle than it appears from surface.
*)

let update (msg : Msg) (sps : SinglePageState) : SinglePageState * Cmd<Msg> =
    match msg, sps with    
    | LoginMsg msg, {page = LoginModel login_model; username = _} ->
        let login_model', cmd = Login.update msg login_model
        { sps with page = LoginModel login_model' }, Cmd.map LoginMsg cmd

    | FirstTimeMsg msg, {page = FirstTimeModel ft_model; username = _}  ->
        let ft_model', cmd = FirstTime.update msg ft_model
        { sps with page = FirstTimeModel ft_model' }, Cmd.map FirstTimeMsg cmd

    | NewTeacherMsg msg, {page = NewTeacherModel nt_model; username = _} ->
        let nt_model', cmd = NewTeacher.update msg nt_model
        {sps with page = NewTeacherModel nt_model' }, Cmd.map NewTeacherMsg cmd

    | MainSchoolMsg msg, {page = MainSchoolModel main_school_model; username = _} ->
        let main_school_model', cmd = MainSchool.update msg main_school_model
        {sps with page = MainSchoolModel main_school_model'}, Cmd.map MainSchoolMsg cmd

    | AddClassMsg msg, {page = AddClassModel model; username = _} ->
        let model', cmd = AddClass.update msg model
        {sps with page = AddClassModel model'}, Cmd.map AddClassMsg cmd

let show = function
| Some x -> string x
| None -> "Loading..."



#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
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
