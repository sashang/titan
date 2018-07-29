module Client.Pages

open Elmish.Browser.UrlParser

type MainSchoolQuery = {
    school_name : string
    teacher_name : string
}
/// The different pages of the application. If you add a new page, then add an entry here.
type PageType =
    | Home
    | Login
    | FirstTime
    | NewTeacher
    | NewPupil
    | MainSchool of (string*string) option
    | HowItWorks

let to_path =
    function
    | PageType.Home -> "#home"
    | PageType.Login -> "#login"
    | PageType.FirstTime -> "#first_time"
    | PageType.NewTeacher -> "#new_teacher"
    | PageType.NewPupil -> "#new_pupil"
    | PageType.HowItWorks -> "#how_it_works"
    | PageType.MainSchool x ->
        match x with
        | Some (sn, tn) -> "#main_school?school_name=" + sn + "&teacher_name=" + tn
        | None -> "#main_school"

/// The URL is turned into a Result.
let page_parser : Parser<PageType -> _,_> =
    oneOf
        [ map PageType.Home (s "home")
          map PageType.Login (s "login")
          map PageType.FirstTime (s "first_time")
          map PageType.NewTeacher (s "new_teacher")
          map PageType.NewPupil (s "new_pupil")
          map PageType.HowItWorks (s "how_it_works")
          map ((fun sn tn ->
                    match sn, tn with
                    | Some sn, Some tn -> MainSchool (Some (sn, tn))
                    | _ -> MainSchool None))
                (s "main_school" <?> stringParam "school_name" <?> stringParam "teacher_name")]

let url_parser location = parseHash page_parser location
