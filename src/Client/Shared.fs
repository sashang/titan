module Client.Shared

open Domain

/// The composed model for the different possible page states of the application
type PageModel =
| HomeModel
| LoginModel of Client.Login.Model
| MainSchoolModel of Client.MainSchool.Model
| AddClassModel of Client.AddClass.Model
| SignUpModel of Client.SignUp.Model

type Msg =
| LoginMsg of Client.Login.Msg //message from the login page
| MainSchoolMsg of Client.MainSchool.Msg
| AddClassMsg of Client.AddClass.Msg
| Init
| SignUpMsg of Client.SignUp.Msg
| SignOutMsg of Client.SignOut.Msg
| UrlUpdatedMsg of Client.Pages.PageType


type SinglePageState = {
    page : PageModel //which page I'm at
    session : Session option //who I am
}

let view_page sps dispatch =
    match sps.page with
    | HomeModel -> Client.Home.view (SignOutMsg >> dispatch) sps.session
    | LoginModel model -> Client.Login.view (LoginMsg >> dispatch) model 
    | MainSchoolModel model -> Client.MainSchool.view model (MainSchoolMsg>> dispatch) sps.session
    | AddClassModel model -> Client.AddClass.view model (AddClassMsg >> dispatch) sps.session
    | SignUpModel model -> Client.SignUp.view model (SignUpMsg >> dispatch)


