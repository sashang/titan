module Pages

open Elmish.Browser.UrlParser

/// The different pages of the application. If you add a new page, then add an entry here.
type PageType =
    | Home
    | Login
    | FAQ
    | PrivacyPolicy
    | Terms
    | DashboardTutor
    | DashboardStudent
    | DashboardTitan
    | TutorLiveView
    | StudentLiveView


let to_path =
    function
    | PageType.Home -> "#home"
    | PageType.Login -> "#login"
    | PageType.FAQ -> "#faq"
    | PageType.DashboardTutor -> "#dashboard-tutor"
    | PageType.DashboardStudent -> "#dashboard-student"
    | PageType.DashboardTitan -> "#dashboard-titan"
    | PageType.PrivacyPolicy -> "#privacy-policy"
    | PageType.Terms -> "#terms"
    | PageType.TutorLiveView -> "#tutor-live-view"
    | PageType.StudentLiveView -> "#student-live-view"


/// The URL is turned into a Result.
let page_parser : Parser<PageType -> _,_> =
    oneOf
        [ map PageType.Home (s "home")
          map PageType.Login (s "login")
          map PageType.FAQ (s "faq")
          map PageType.PrivacyPolicy (s "privacy-policy")
          map PageType.Terms (s "terms")
          map (PageType.DashboardTutor) (s "dashboard-tutor")
          map (PageType.TutorLiveView) (s "tutor-live-view")
          map (PageType.StudentLiveView) (s "student-live-view")
          map (PageType.DashboardStudent) (s "dashboard-student")
          map (PageType.DashboardTitan) (s "dashboard-titan") ]

let url_parser location = parseHash page_parser location
