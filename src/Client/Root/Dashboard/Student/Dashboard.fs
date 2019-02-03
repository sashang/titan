module Student.Dashboard

open CustomColours
open Domain
open Elmish
open Fable.Import
open Fable.PowerPack
open Fable.Helpers.React
open Fulma
open ModifiedFableFetch
open StudentSchools
open Thoth.Json

type PageModel =
    | EnroledModel of StudentSchools.Model //page of schools this student is enroled in
    | ClassModel of Student.Class.Model //live view of the classroom
    | HomeModel 

type Model =
    { EnroledSchools : School list
      Child : PageModel
      Error : APIError option }

type Msg =
    | ClickClassroom of School
    | ClickEnrol
    | ClickAccount
    | GetEnroledSchoolsSuccess of School list
    | ClassMsg of Student.Class.Msg
    | EnroledMsg of StudentSchools.Msg
    | Failure of exn

exception GetEnroledSchoolsEx of APIError

let private get_enroled_schools () = promise {
    let request = make_get
    let decoder = Decode.Auto.generateDecoder<GetAllSchoolsResult>()
    let! response = Fetch.tryFetchAs "/api/get-enroled-schools" decoder request
    Browser.console.info "received response from get-enroled-schools"
    match response with
    | Ok result ->
        match result.Error with
        | Some api_error -> return raise (GetEnroledSchoolsEx api_error)
        | None ->  return result.Schools
    | Error e ->
        return raise (GetEnroledSchoolsEx (APIError.init [APICode.Fetch] [e]))
}



let init () = 
    {Error = None; Child = HomeModel; EnroledSchools = [] }, Cmd.ofPromise get_enroled_schools () GetEnroledSchoolsSuccess Failure

let update model msg =
    match model, msg with
    | {Child = ClassModel child}, ClassMsg msg ->
        Browser.console.info ("ClassMsg message")
        let new_model, new_cmd = Student.Class.update child msg
        {model with Child = ClassModel new_model}, Cmd.map ClassMsg new_cmd

    | {Child = EnroledModel child}, EnroledMsg  msg ->
        Browser.console.info ("Enroled message")
        let new_model, new_cmd = StudentSchools.update child msg
        {model with Child = EnroledModel new_model }, Cmd.map EnroledMsg new_cmd

    | model, GetEnroledSchoolsSuccess schools ->
        {model with EnroledSchools = schools}, Cmd.none
    | model, ClickClassroom school ->
        let new_state, new_cmd = Student.Class.init school
        {model with Child = ClassModel new_state}, Cmd.map ClassMsg new_cmd
    
    | model, ClickEnrol ->
        let new_state, new_cmd = StudentSchools.init ()
        {model with Child = EnroledModel new_state}, Cmd.map EnroledMsg new_cmd

    | model, ClickAccount ->
        model, Cmd.none

    | model, Failure e ->
        match e with
        | :? GetEnroledSchoolsEx as ex ->
            Browser.console.warn ("GetEnroledSchoolsEx: " + ex.Message)
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
     Container.container [ Container.IsFluid
                           Container.Modifiers [ Modifier.IsMarginless ] ] [
        Columns.columns [ ] [
            Column.column [ Column.Width (Screen.All, Column.Is1) ] [
                Menu.menu [ ] [
                    Menu.list [ ] [
                        sub_menu "Classrooms" false [
                        yield! [ for school in model.EnroledSchools do
                                    yield menu_item school.SchoolName false dispatch (ClickClassroom school) ]
                        ]
                        menu_item "Enrol" false dispatch ClickEnrol
                        menu_item "Account" false dispatch ClickAccount
                    ]
                ]
            ]
            Column.column [ ] [
                (match model.Child with
                | EnroledModel child ->
                    StudentSchools.view child (EnroledMsg >> dispatch)
                | ClassModel child ->
                    Student.Class.view child (ClassMsg >> dispatch)
                | HomeModel -> nothing)
            ]
        ]
     ]
    
