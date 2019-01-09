module Pages

open Elmish.Browser.UrlParser

/// The different pages of the application. If you add a new page, then add an entry here.
type PageType =
    | Home
    | Login
    | SignUp
    | Enrol
    | Dashboard


let to_path =
    function
    | PageType.Home -> "#home"
    | PageType.Login -> "#login"
    | PageType.SignUp -> "#sign-up"
    | PageType.Dashboard -> "#dashboard"
    | PageType.Enrol -> "#enroll"


/// The URL is turned into a Result.
let page_parser : Parser<PageType -> _,_> =
    oneOf
        [ map PageType.Home (s "home")
          map PageType.Login (s "login")
          map PageType.SignUp (s "sign-up")
          map PageType.Enrol (s "enroll")
          map (PageType.Dashboard) (s "dashboard") ]

let url_parser location = parseHash page_parser location
