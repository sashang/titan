module Client.Pages

open Elmish.Browser.UrlParser

/// The different pages of the application. If you add a new page, then add an entry here.
[<RequireQualifiedAccess>]
type PageType =
    | Home
    | Login
    | FirstTime

let toPath =
    function
    | PageType.Home -> "/"
    | PageType.Login -> "/login"
    | PageType.FirstTime -> "/first_time"

/// The URL is turned into a Result.
let pageParser : Parser<PageType -> PageType,_> =
    oneOf
        [ map PageType.Home (s "")
          map PageType.Login (s "login")
          map PageType.FirstTime (s "first_time") ]

let urlParser location = parsePath pageParser location
