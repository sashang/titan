namespace Shared

/// The composed model for the different possible page states of the application
type PageModel =
| LoginModel
| FirstTimeModel of Client.FirstTime.Model
| NewTeacherModel
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
