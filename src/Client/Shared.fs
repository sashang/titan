module Client.Shared

open Domain

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
| SignUpModel of Client.SignUp.Model

type Msg =
| FirstTimeMsg of Client.FirstTime.Msg //message from the first time modal page
| LoginMsg of Client.Login.Msg //message from the login page
| NewTeacherMsg of Client.NewTeacher.Msg
| NewPupilMsg of Client.NewPupil.Msg
| MainSchoolMsg of Client.MainSchool.Msg
| AddClassMsg of Client.AddClass.Msg
| Init
| SignUpMsg of Client.SignUp.Msg
| SignOutMsg of Client.SignOut.Msg


type SinglePageState = {
    page : PageModel //which page I'm at
    session : Session option //who I am
}

let view_page sps dispatch =
    match sps.page with
    | HomeModel -> Client.Home.view (SignOutMsg >> dispatch) sps.session
    | LoginModel model -> Client.Login.view (LoginMsg >> dispatch) model 
    | FirstTimeModel model -> Client.FirstTime.view model (FirstTimeMsg >> dispatch) sps.session
    | NewTeacherModel model -> Client.NewTeacher.view model (NewTeacherMsg >> dispatch) sps.session
    | NewPupilModel -> Client.NewPupil.view (NewPupilMsg >> dispatch) 
    | MainSchoolModel model -> Client.MainSchool.view model (MainSchoolMsg>> dispatch) sps.session
    | HowItWorksModel -> Client.HowItWorks.view ()
    | AddClassModel model -> Client.AddClass.view model (AddClassMsg >> dispatch) sps.session
    | SignUpModel model -> Client.SignUp.view model (SignUpMsg >> dispatch)


