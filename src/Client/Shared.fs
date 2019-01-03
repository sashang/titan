module Client.Shared

open Domain

/// The composed model for the different possible page states of the application
type PageModel =
| HomeModel
| LoginModel of Login.Model
| SignUpModel of SignUp.Model

type Msg =
| LoginMsg of Login.Msg //message from the login page
| Init
| SignUpMsg of SignUp.Msg
| SignOutMsg of SignOut.Msg
| UrlUpdatedMsg of Pages.PageType


type SinglePageState = {
    page : PageModel //which page I'm at
    session : Session option //who I am
}


