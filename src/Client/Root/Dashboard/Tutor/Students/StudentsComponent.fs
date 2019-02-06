/// The students component shows those enrolled
module StudentsComponent

open CustomColours
open Domain
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fable.PowerPack
open Fable.FontAwesome
open Fulma
open ModifiedFableFetch
open Thoth.Json
open Client.Shared


type Model =
    { Students : Domain.Student list
      LoadStudentsState : LoadingState
      //result of interaction with the api
      Error : APIError option}


exception GetAllStudentsEx of APIError
exception DismissStudentEx of APIError


type Msg =
    | LoadStudentsSuccess of Student list
    | LoadStudentsFailure  of exn
    | GetAllStudents
    | DismissStudent of Student
    | DismissStudentSuccess of unit
    | DismissStudentFailure of exn

let dismiss_student (student : DismissStudentRequest) = promise {
    let request = make_post 1 student
    let decoder = Decode.Auto.generateDecoder<APIError option>()
    let! response = Fetch.tryFetchAs "/api/dismiss-student" decoder request
    return map_api_error_result response DismissStudentEx
}

let get_all_students () = promise {
    let decoder = Decode.Auto.generateDecoder<Domain.GetAllStudentsResult>()
    let! response = Fetch.tryFetchAs "/api/get-all-students" decoder make_get
    match response with
    | Ok result ->
        match result.Error with
        | Some error ->
            Browser.console.error ("get_all_students: " + (List.head error.Messages))
            return (raise (GetAllStudentsEx error))
        | None ->
            return result.Students
    | Error e ->
        return raise (GetAllStudentsEx (APIError.init [APICode.Fetch] [e]))
}

let init () =
    { LoadStudentsState = Loading; Students = [ ]; Error = None },
    Cmd.ofPromise get_all_students () LoadStudentsSuccess LoadStudentsFailure

let update (model : Model) (msg : Msg) =
    match msg with
    | GetAllStudents ->
        model, Cmd.ofPromise get_all_students () LoadStudentsSuccess LoadStudentsFailure
        
    | LoadStudentsSuccess students ->
        {model with LoadStudentsState = Loaded; Students = students}, Cmd.none
    | LoadStudentsFailure e ->
        match e with 
        | :? GetAllStudentsEx as ex ->
            Browser.console.warn "Received GetAllStudentsEx"
            { model with Error = Some ex.Data0 }, Cmd.none
        | e ->
            Browser.console.warn ("Received general exception: " + e.Message)
            model, Cmd.none
    | DismissStudent student ->
        model, Cmd.ofPromise dismiss_student {Email = student.Email} (DismissStudentSuccess) (DismissStudentFailure)
        
    | DismissStudentSuccess () ->
        model, Cmd.ofPromise get_all_students () LoadStudentsSuccess LoadStudentsFailure
        
    | DismissStudentFailure e ->
        match e with 
        | :? DismissStudentEx as ex ->
            Browser.console.warn "Received DismissStudentEx"
            { model with Error = Some ex.Data0 }, Cmd.none
        | e ->
            Browser.console.warn ("Received general exception: " + e.Message)
            model, Cmd.none


let private card_footer (student : Student) (dispatch : Msg -> unit) =
    [ Card.Footer.div [ ]
        [ Button.button [ Button.Color IsTitanInfo
                          Button.Props [ OnClick (fun _ -> dispatch (DismissStudent student)) ] ]
            [ Icon.icon [ ]
                [ Fa.i [ Fa.Solid.Trash ]
                    [ ] ] ] ] ]
    
let private student_content (student : Student) =
      Text.div
        [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ] [
            Columns.columns [] [
                Column.column [] [
                    Label.label [ ] [ str "Email" ]
                ]
                Column.column [] [
                    Text.div [ ] [ str student.Email ]
                ]
                Column.column [] [
                    Label.label [ ] [ str "Phone" ]
                ]
                Column.column [] [
                    Text.div [ ] [ str student.Phone ]
                ]
            ]
        ]

let private single_student model dispatch (student : Domain.Student) = 
    Column.column [ ]
      [ Card.card [ ] 
          [ Card.header [ Modifiers [ Modifier.BackgroundColor IsTitanSecondary ] ]
              [ Card.Header.title 
                    [ Card.Header.Title.Modifiers [ Modifier.TextColor IsWhite ] ] 
                    [ str (student.FirstName + " " + student.LastName) ] ]
            Card.content [  ] [ yield student_content student ]
            Card.footer [ ]
                [ yield! card_footer student dispatch ] ] ]

let private render_all_students (model : Model) (dispatch : Msg->unit) =
    match model.Students with
    | [] ->
        [nothing]
    | _ ->
       [ for x in model.Students do
           yield single_student model dispatch x] 

let private students_level =
    Level.level [ ] 
        [ Level.left [ ]
            [ Level.title [ Common.Modifiers [ Modifier.TextTransform TextTransform.UpperCase
                                               Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            Common.Props [ Style [ CSSProp.FontFamily "'Montserrat', sans-serif" ]] ] [ str "students" ] ] ]

let view (model : Model) (dispatch : Msg -> unit) =
    [ Box.box' [ ] 
        [ yield! (match model.LoadStudentsState with
                   | Loaded -> List.append [students_level] [yield! render_all_students model dispatch] 
                   | Loading -> Client.Style.loading_view) ] ] 