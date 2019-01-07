module Pages

open Elmish.Browser.UrlParser

[<RequireQualifiedAccess>]
type DashboardPageType =
    /// Main dashboard page
    | Main
    /// Page to create a school
    | School

/// The different pages of the application. If you add a new page, then add an entry here.
type PageType =
    | Home
    | Login
    | SignUp
    | Dashboard of DashboardPageType


let to_path =
    function
    | PageType.Home -> "#home"
    | PageType.Login -> "#login"
    | PageType.SignUp -> "#sign-up"
    | PageType.Dashboard(DashboardPageType.Main) -> "#dashboard"
    | PageType.Dashboard(DashboardPageType.School) -> "#school"


/// The URL is turned into a Result.
let page_parser : Parser<PageType -> _,_> =
    oneOf
        [ map PageType.Home (s "home")
          map PageType.Login (s "login")
          map PageType.SignUp (s "sign-up")
          map (PageType.Dashboard DashboardPageType.School) (s "school")
          map (PageType.Dashboard DashboardPageType.Main) (s "dashboard") ]

let url_parser location = parseHash page_parser location
