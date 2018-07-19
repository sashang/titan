module Client.Shared

/// The composed model for the different possible page states of the application
type PageModel =
| HomeModel
| LoginModel
| FirstTimeModel of Client.FirstTime.Model
| NewTeacherModel of Client.NewTeacher.Model
| NewPupilModel

type Msg =
| FirstTimeMsg of Client.FirstTime.Msg //message from the first time modal page
| LoginMsg of Client.Login.Msg //message from the login page
| NewTeacherMsg of Client.NewTeacher.Msg
| NewPupilMsg of Client.NewPupil.Msg
| Init


type SinglePageState = {
    page : PageModel //which page I'm at
    username : string option //who I am
}

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Client.Style

let view_page model dispatch =
    match model.page with
    | HomeModel -> Client.Home.view ()
    | LoginModel -> Client.Login.view (LoginMsg >> dispatch)
    | FirstTimeModel model' -> Client.FirstTime.view model' (FirstTimeMsg >> dispatch)
    | NewTeacherModel model' -> Client.NewTeacher.view model' (NewTeacherMsg >> dispatch)
    | NewPupilModel -> Client.NewPupil.view (NewPupilMsg >> dispatch)

let view model dispatch =
    view_page model dispatch

