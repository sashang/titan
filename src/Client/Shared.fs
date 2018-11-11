module Client.Shared

/// The composed model for the different possible page states of the application
type PageModel =
| HomeModel
| LoginModel of Client.Login.Model
| FirstTimeModel of Client.FirstTime.Model
| NewTeacherModel of Client.NewTeacher.Model
| NewPupilModel
| MainSchoolModel of Client.MainSchool.Model
| HowItWorksModel
| AddClassModel of Client.AddClass.Model

type Msg =
| FirstTimeMsg of Client.FirstTime.Msg //message from the first time modal page
| LoginMsg of Client.Login.Msg //message from the login page
| NewTeacherMsg of Client.NewTeacher.Msg
| NewPupilMsg of Client.NewPupil.Msg
| MainSchoolMsg of Client.MainSchool.Msg
| AddClassMsg of Client.AddClass.Msg
| Init


type SinglePageState = {
    page : PageModel //which page I'm at
    username : string option //who I am
}

let view_page sps dispatch =
    match sps.page with
    | HomeModel -> Client.Home.view ()
    | LoginModel model -> Client.Login.view (LoginMsg >> dispatch) model
    | FirstTimeModel model -> Client.FirstTime.view model (FirstTimeMsg >> dispatch)
    | NewTeacherModel model -> Client.NewTeacher.view model (NewTeacherMsg >> dispatch)
    | NewPupilModel -> Client.NewPupil.view (NewPupilMsg >> dispatch)
    | MainSchoolModel model -> Client.MainSchool.view model (MainSchoolMsg>> dispatch)
    | HowItWorksModel -> Client.HowItWorks.view ()
    | AddClassModel model -> Client.AddClass.view model (AddClassMsg >> dispatch)

let view model dispatch =
    view_page model dispatch

