module Student.Dashboard

open Client.Shared
open Domain
open Elmish
open Fable.Import
open Fable.PowerPack
open Fable.Helpers.React
open Fulma
open ModifiedFableFetch
open Enrolled
open Thoth.Json

type PageModel =
    | EnrolledModel of Enrolled.Model //page of schools this student is enroled in
    | ClassModel of Student.Class.Model //live view of the classroom
    | HomeModel 

type Model =
    { EnrolledSchools : School list
      Child : PageModel
      Claims : TitanClaim
      Error : APIError option }

type Msg =
    | ClickClassroom of School
    | ClickEnrol
    | ClickAccount
    | SignOut
    | GetEnrolledSchoolsSuccess of School list
    | ClassMsg of Student.Class.Msg
    | EnrolledMsg of Enrolled.Msg
    | Failure of exn

exception GetEnrolledSchoolsEx of APIError

let private get_enroled_schools () = promise {
    let request = make_get
    let decoder = Decode.Auto.generateDecoder<GetAllSchoolsResult>()
    let! response = Fetch.tryFetchAs "/api/get-enrolled-schools" decoder request
    Browser.console.info "received response from get-enroled-schools"
    match response with
    | Ok result ->
        match result.Error with
        | Some api_error -> return raise (GetEnrolledSchoolsEx api_error)
        | None ->  return result.Schools
    | Error e ->
        return raise (GetEnrolledSchoolsEx (APIError.init [APICode.Fetch] [e]))
}



let init claims = 
    {Claims = claims; Error = None; Child = HomeModel; EnrolledSchools = [] },
     Cmd.ofPromise get_enroled_schools () GetEnrolledSchoolsSuccess Failure

let update model msg =
    match model, msg with
    | {Child = ClassModel child}, ClassMsg msg ->
        Browser.console.info ("ClassMsg message")
        let new_model, new_cmd = Student.Class.update child msg
        {model with Child = ClassModel new_model}, Cmd.map ClassMsg new_cmd

    | {Child = ClassModel child}, SignOut ->
        Browser.console.info ("SignOut message")
        let new_model, new_cmd = Student.Class.update child Student.Class.SignOut
        {model with Child = ClassModel new_model}, Cmd.map ClassMsg new_cmd

    | _, ClassMsg _ ->
        Browser.console.error ("Received ClassMsg when child page is not ClassModel")
        model, Cmd.none

    | {Child = EnrolledModel child}, EnrolledMsg  msg ->
        Browser.console.info ("Enrolled message")
        let new_model, new_cmd = Enrolled.update child msg
        {model with Child = EnrolledModel new_model }, Cmd.map EnrolledMsg new_cmd

    | _, EnrolledMsg _ ->
        Browser.console.error ("Received EnrolledMsg when child page is not EnrolledModel")
        model, Cmd.none

    | model, GetEnrolledSchoolsSuccess schools ->
        {model with EnrolledSchools = schools}, Cmd.none
    | model, ClickClassroom school ->
        let new_state, new_cmd = Student.Class.init school model.Claims.Email
        {model with Child = ClassModel new_state}, Cmd.map ClassMsg new_cmd
    
    | model, ClickEnrol ->
        let new_state, new_cmd = Enrolled.init ()
        {model with Child = EnrolledModel new_state}, Cmd.map EnrolledMsg new_cmd

    | model, ClickAccount ->
        model, Cmd.none

    | model, Failure e ->
        match e with
        | :? GetEnrolledSchoolsEx as ex ->
            Browser.console.warn ("GetEnrolledSchoolsEx: " + ex.Message)
            {model with Error = Some ex.Data0}, Cmd.none
        | e ->
            Browser.console.warn ("Failed to get_all_schools: " + e.Message)
            model, Cmd.none

    
// Helper to generate a menu item
let menu_item label isActive dispatch msg =
    Menu.Item.li [ Menu.Item.IsActive isActive
                   Menu.Item.OnClick (fun e -> dispatch msg)
                   Menu.Item.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
       [ str label ]

// Helper to generate a sub menu
let sub_menu label isActive children =
    li [ ]
       [ Menu.Item.a [ Menu.Item.IsActive isActive ]
            [ str label ]
         ul [ ]
            children ]

let view (model : Model) (dispatch : Msg -> unit) =
    Columns.columns [ ] [
        Column.column [ Column.Width (Screen.All, Column.Is1) ] [
            Menu.menu [ ] [
                Menu.list [ ] [
                    sub_menu "Classrooms" false [
                    yield! [ for school in model.EnrolledSchools do
                                yield menu_item school.SchoolName false dispatch (ClickClassroom school) ]
                    ]
                    menu_item "Enrol" false dispatch ClickEnrol
                    menu_item "Account" false dispatch ClickAccount
                ]
            ]
        ]
        Column.column [ ] [
            (match model.Child with
            | EnrolledModel child ->
                Enrolled.view child (EnrolledMsg >> dispatch)
            | ClassModel child ->
                Student.Class.view child (ClassMsg >> dispatch)
            | HomeModel -> nothing)
        ]
    ]
    
