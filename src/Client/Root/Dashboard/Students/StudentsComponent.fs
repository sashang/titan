/// The students component shows those enrolled
module StudentsComponent

open CustomColours
open Domain
open Elmish
open Elmish.Browser.Navigation
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Fable.Core.JsInterop
open Fulma
open Shared
open Thoth.Json


type Model =
    { Students : Domain.Student list
      //result of interaction with the api
      Result : GetAllStudentsResult option}


exception GetAllStudentsEx of GetAllStudentsResult

type Msg =
    | LoadStudents of Student list
    | LoadStudentsFailure  of exn

let private get_all_students () = promise {
    let props =
        [ RequestProperties.Method HttpMethod.GET
          RequestProperties.Credentials RequestCredentials.Include
          requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                           HttpRequestHeaders.Accept "application/json" ]]
    let decoder = Decode.Auto.generateDecoder<Domain.GetAllStudentsResult>()
    let! response = Fetch.tryFetchAs "/api/secure/get-all-students" decoder props
    match response with
    | Ok result ->
        match result.Codes with
        | APICode.Success::_ -> 
            return result.Students
        | _ ->
            return (raise (GetAllStudentsEx result))
    | Error e ->
        return (raise (GetAllStudentsEx {Codes = [APICode.FetchError]; Messages = [e];
            Students = []}))
}
let init () =
    { Students = [ ]; Result = None },
    Cmd.ofPromise get_all_students () LoadStudents LoadStudentsFailure

let update (model : Model) (msg : Msg) =
    match msg with
    | LoadStudents students ->
        {model with Students = students}, Cmd.none
    | LoadStudentsFailure e ->
        match e with 
        | :? GetAllStudentsEx as ex ->
            Browser.console.warn "Received GetAllStudentsEx"
            { model with Result = Some ex.Data0 }, Cmd.none
        | e ->
            Browser.console.warn "Received general exception"
            { model with Result = Some { Codes = [APICode.Failure]; Messages = ["Unknown errror"]; Students = [ ] }}, Cmd.none


let private student_content (student : Student) =
      [ Text.div [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ] [ str student.Email ] ]

//make a list that breaks the original list into one that contains 4 elem list
//of the original value
let list_4 input =
    input
    |> List.fold
         (fun (count,result) value ->
            match result with
            | [[]] -> count+1,[[value]]
            | inner::[] ->
                match count, inner with
                | 4, ys -> 0,[ys;[value]]
                | _, ys -> count+1,[List.append ys [value]] 
            | _ -> failwith "Invalid initial state. Should be (0,[[]])") (0,[[]])
    |> (fun (count, result) -> result) //only interested in the result which is a list of lists

let private single_student model dispatch (student : Domain.Student) = 
    Column.column [ Column.Width (Screen.All, Column.Is3) ]
      [ Card.card [ ] 
          [ Card.header [ Modifiers [ Modifier.BackgroundColor IsTitanSecondary ] ]
              [ Card.Header.title 
                    [ Card.Header.Title.Modifiers [ Modifier.TextColor IsWhite ] ] 
                    [ str (student.FirstName + " " + student.LastName) ] ]
            Card.content [  ] [ yield! student_content student ] ] ]

let private render_all_students (model : Model) (dispatch : Msg->unit) =
    let l4 = model.Students |> list_4
    [for l in l4 do
        yield Columns.columns [ ]
            [ for x in l do
                yield single_student model dispatch x] ]

let private students_level =
    Level.level [ ] 
        [ Level.title  [ ] [ str "student" ] ]

let view (model : Model) (dispatch : Msg -> unit) =
    [ Box.box' [ ] 
        [ yield! List.append [students_level] [yield! render_all_students model dispatch] ] ] 