module Client.Pages

open Elmish.Browser.UrlParser

/// The different pages of the application. If you add a new page, then add an entry here.
type PageType =
| Home
| Login
| FirstTime
| NewTeacher
| NewPupil

let toPath =
    function
    | PageType.Home -> ""
    | PageType.Login -> "#login"
    | PageType.FirstTime -> "#first_time"
    | PageType.NewTeacher -> "#new_teacher"
    | PageType.NewPupil -> "#new_pupil"

/// The URL is turned into a Result.
let pageParser : Parser<PageType -> PageType,_> =
    oneOf
        [ map PageType.Home (s "")
          map PageType.Login (s "login")
          map PageType.FirstTime (s "first_time")
          map PageType.NewTeacher (s "new_teacher")
          map PageType.NewPupil (s "new_pupil")]

let urlParser location = parseHash pageParser location
