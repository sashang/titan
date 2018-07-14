namespace Shared

/// The composed model for the different possible page states of the application
type PageModel =
| LoginModel
| FirstTimeModel of Client.FirstTime.Model

type SinglePageState = {
    page : PageModel //which page I'm at
    username : string option //who I am
}
