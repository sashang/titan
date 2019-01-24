/// The students component shows those enrolled
module StudentsComponent

open CustomColours
open Domain
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fable.PowerPack
open Fulma
open ModifiedFableFetch
open Thoth.Json


type Model =
    { Students : Domain.Student list
      //result of interaction with the api
      Error : APIError option}


exception GetAllStudentsEx of APIError

type Msg =
    | LoadStudentsSuccess of Student list
    | LoadStudentsFailure  of exn
    | GetAllStudents

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
    { Students = [ ]; Error = None },
    Cmd.ofPromise get_all_students () LoadStudentsSuccess LoadStudentsFailure

let update (model : Model) (msg : Msg) =
    match msg with
    | GetAllStudents ->
        model, Cmd.ofPromise get_all_students () LoadStudentsSuccess LoadStudentsFailure
        
    | LoadStudentsSuccess students ->
        {model with Students = students}, Cmd.none
    | LoadStudentsFailure e ->
        match e with 
        | :? GetAllStudentsEx as ex ->
            Browser.console.warn "Received GetAllStudentsEx"
            { model with Error = Some ex.Data0 }, Cmd.none
        | e ->
            Browser.console.warn ("Received general exception: " + e.Message)
            model, Cmd.none


let private student_content (student : Student) =
      [ Text.div
            [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
            [ str student.Email ] ]

let private single_student model dispatch (student : Domain.Student) = 
    Column.column [ Column.Width (Screen.All, Column.Is3) ]
      [ Card.card [ ] 
          [ Card.header [ Modifiers [ Modifier.BackgroundColor IsTitanSecondary ] ]
              [ Card.Header.title 
                    [ Card.Header.Title.Modifiers [ Modifier.TextColor IsWhite ] ] 
                    [ str (student.FirstName + " " + student.LastName) ] ]
            Card.content [  ] [ yield! student_content student ] ] ]

let private render_all_students (model : Model) (dispatch : Msg->unit) =
    match model.Students with
    | [] ->
        [nothing]
    | _ ->
        let l4 = Homeless.chunk 4 model.Students
        [for l in l4 do
                yield Columns.columns [ ]
                    [ for x in l do
                        yield single_student model dispatch x] ]

let private students_level =
    Level.level [ ] 
        [ Level.left [ ]
            [ Level.title [ Common.Modifiers [ Modifier.TextTransform TextTransform.UpperCase
                                               Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            Common.Props [ Style [ CSSProp.FontFamily "'Montserrat', sans-serif" ]] ] [ str "students" ] ] ]

let view (model : Model) (dispatch : Msg -> unit) =
    [ Box.box' [ ] 
        [ yield! List.append [students_level] [yield! render_all_students model dispatch] ] ] 