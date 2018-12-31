module Client.Pages

open Elmish.Browser.UrlParser
/// The different pages of the application. If you add a new page, then add an entry here.
type PageType =
    | Home
    | Login
    | MainSchool
    | AddClass
    | SignUp
    | Dashboard

let to_path =
    function
    | PageType.Home -> "#home"
    | PageType.Login -> "#login"
    | PageType.AddClass -> "#add-class"
    | PageType.MainSchool -> "#main-school"
    | PageType.SignUp -> "#sign-up"
    | PageType.Dashboard -> "#dashboard"


/// The URL is turned into a Result.
let page_parser : Parser<PageType -> _,_> =
    oneOf
        [ map PageType.Home (s "home")
          map PageType.Login (s "login")
          map PageType.AddClass (s "add-class")
          map PageType.SignUp (s "sign-up")
          map PageType.Dashboard (s "dashboard")
          map PageType.MainSchool (s "main-school") ]

let url_parser location = parseHash page_parser location
