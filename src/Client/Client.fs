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
        { model with page = LoginModel; username = None }, Cmd.none

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
    //When the user logs in redirect to the first time page for now.
    //TODO: Change this when we identify the user properly.
    | LoginMsg msg, {page = LoginModel; username = _} ->
        let login_model', msg' = Login.update msg
        {page = FirstTimeModel (FirstTime.init ()); username = None},
        Navigation.newUrl (Client.Pages.to_path Client.Pages.FirstTime)

    | FirstTimeMsg msg, {page = FirstTimeModel ft_model; username = _}  ->
        let ft_model', msg' = FirstTime.update msg ft_model
        { sps with page = FirstTimeModel(ft_model')}, Cmd.map FirstTimeMsg msg'

    | NewTeacherMsg msg, {page = NewTeacherModel nt_model; username = _} ->
        let nt_model', msg' = NewTeacher.update msg nt_model
        {sps with page = NewTeacherModel(nt_model')}, Cmd.map NewTeacherMsg msg'

    | MainSchoolMsg msg, {page = MainSchoolModel main_school_model; username = _} ->
        let main_school_model', msg' = MainSchool.update msg main_school_model
        {sps with page = MainSchoolModel main_school_model'}, Cmd.map MainSchoolMsg msg'

    | AddClassMsg msg, {page = AddClassModel model; username = _} ->
        let model', msg' = AddClass.update msg model
        {sps with page = AddClassModel model'}, Cmd.map AddClassMsg msg'

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
