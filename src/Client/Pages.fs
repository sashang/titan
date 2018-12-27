module Client.Pages

open Elmish.Browser.UrlParser

type MainSchoolQuery = 
    { school_name : string
      teacher_name : string }
/// The different pages of the application. If you add a new page, then add an entry here.
type PageType =
    | Home
    | Login
    | FirstTime
    | NewTeacher
    | NewStudent
    | MainSchool
    | HowItWorks
    | AddClass
    | SignUp

let to_path =
    function
    | PageType.Home -> "#home"
    | PageType.Login -> "#login"
    | PageType.FirstTime -> "#first-time"
    | PageType.NewTeacher -> "#new-teacher"
    | PageType.NewStudent -> "#new-pupil"
    | PageType.HowItWorks -> "#how-it-works"
    | PageType.AddClass -> "#add-class"
    | PageType.MainSchool -> "#main-school"
    | PageType.SignUp -> "#sign-up"


/// The URL is turned into a Result.
let page_parser : Parser<PageType -> _,_> =
    oneOf
        [ map PageType.Home (s "home")
          map PageType.Login (s "login")
          map PageType.FirstTime (s "first_time")
          map PageType.NewTeacher (s "new-teacher")
          map PageType.NewStudent (s "new-pupil")
          map PageType.HowItWorks (s "how-it-works")
          map PageType.AddClass (s "add-class")
          map PageType.SignUp (s "sign-up")
          map PageType.MainSchool (s "main-school") ]

let url_parser location = parseHash page_parser location
