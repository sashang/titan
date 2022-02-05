/// The students component shows those enrolled
module StudentsComponent

open CustomColours
open Domain
open Elmish
open Fable.React
open Fable.React.Props
open Fable.Import
open Fable.FontAwesome
open Fulma
open ModifiedFableFetch
open Thoth.Json
type TF = Thoth.Fetch.Fetch
open Client.Shared


type Model =
    { Students : Domain.Student list
      Filtered : Domain.Student list
      LoadStudentsState : LoadingState
      //result of interaction with the api
      Error : APIError option}


exception GetAllStudentsException of APIError
exception DismissStudentException of APIError

type Msg =
    | LoadStudentsSuccess of Student list
    | LoadStudentsFailure  of exn
    | GetAllStudents
    | SignOut
    | DismissStudent of Student
    | DismissStudentSuccess of unit
    | DismissStudentFailure of exn
    | FilterOnChange of string

let dismiss_student (student : DismissStudentRequest) = promise {
    let request = make_post 1 student
    let decoder = Decode.Auto.generateDecoder<APIError option>()
    let! response = TF.tryFetchAs("/api/dismiss-student", decoder, request)
    return map_api_error_result response DismissStudentException
}

let get_all_students () = promise {
    let decoder = Decode.Auto.generateDecoder<Domain.GetAllStudentsResult>()
    let! response = TF.tryFetchAs("/api/get-all-students", decoder, make_get)
    match response with
    | Ok result ->
        match result.Error with
        | Some error ->
            Browser.Dom.console.error ("get_all_students: " + (List.head error.Messages))
            return (raise (GetAllStudentsException error))
        | None ->
            return result.Students
    | Error e ->
        return raise (GetAllStudentsException (APIError.init [APICode.Fetch] [e.ToString()]))
}

let init () =
    { LoadStudentsState = Loading; Students = [ ]; Filtered = []; Error = None },
    Cmd.OfPromise.either get_all_students () LoadStudentsSuccess LoadStudentsFailure

let update (model : Model) (msg : Msg) =
    match msg with
    | GetAllStudents ->
        model, Cmd.OfPromise.either get_all_students () LoadStudentsSuccess LoadStudentsFailure

    | LoadStudentsSuccess students ->
        {model with LoadStudentsState = Loaded; Students = students; Filtered = students}, Cmd.none
    | LoadStudentsFailure e ->
        match e with
        | :? GetAllStudentsException as ex ->
            Browser.Dom.console.warn "Received GetAllStudentsEx"
            { model with Error = Some ex.Data0 }, Cmd.none
        | e ->
            Browser.Dom.console.warn ("Received general exception: " + e.Message)
            model, Cmd.none
    | DismissStudent student ->
        model, Cmd.OfPromise.either dismiss_student {Email = student.Email} (DismissStudentSuccess) (DismissStudentFailure)

    | DismissStudentSuccess () ->
        model, Cmd.OfPromise.either get_all_students () LoadStudentsSuccess LoadStudentsFailure

    | SignOut ->
        Browser.Dom.console.info "Received signout msg" //we don't have to do anything special here.
        model, Cmd.none

    | DismissStudentFailure e ->
        match e with
        | :? DismissStudentException as ex ->
            Browser.Dom.console.warn "Received DismissStudentEx"
            { model with Error = Some ex.Data0 }, Cmd.none
        | e ->
            Browser.Dom.console.warn ("Received general exception: " + e.Message)
            model, Cmd.none

    | FilterOnChange text ->
        let filtered = model.Students |> List.filter (fun x -> x.FirstName.ToUpper().StartsWith(text.ToUpper()) || x.LastName.ToUpper().StartsWith(text.ToUpper()))
        {model with Filtered = filtered}, Cmd.none



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
            Columns.columns [ ] [
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
    Column.column [ Column.Width (Screen.All, Column.Is6) ]
      [ Card.card [ ]
          [ Card.header [ Modifiers [ Modifier.BackgroundColor IsTitanSecondary ] ]
              [ Card.Header.title
                    [ Card.Header.Title.Modifiers [ Modifier.TextColor IsWhite ] ]
                    [ str (student.FirstName + " " + student.LastName) ] ]
            Card.content [  ] [ yield student_content student ]
            Card.footer [ ]
                [ yield! card_footer student dispatch ] ] ]

let rec private render_2_students (model : Model) (dispatch : Msg->unit) (students : Domain.Student list) =
    match students with
    | [] -> nothing
    | x::[] ->
        Columns.columns [ ] [
            single_student model dispatch x
        ]
    | x::y::others ->
        div [ ] [
            Columns.columns [ ] [
                single_student model dispatch x
                single_student model dispatch y
            ]
            render_2_students model dispatch others
        ]

let private render_all_students (model : Model) (dispatch : Msg->unit) =
    match model.Students with
    | [] ->
        nothing
    | _ ->
        render_2_students model dispatch model.Filtered
    //    Columns.columns [ ] [ for x in model.Students do
    //                             yield single_student model dispatch x]

let private students_level =
    Level.level [ ]
        [ Level.left [ ]
            [ Level.title [ Common.Modifiers [ Modifier.TextTransform TextTransform.UpperCase
                                               Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            Common.Props [ Style [ CSSProp.FontFamily "'Montserrat', sans-serif" ]] ] [ str "students" ] ] ]

let private filter dispatch =
    form [] [
        Field.div [] [
            Label.label [ ] [ str "Filter" ]
            Control.div [ ] [
                Input.text [Input.Props [ Props.OnChange (fun ev -> dispatch (FilterOnChange ev.Value)) ]
                            Input.Placeholder "Ex: Govender"]
            ]
        ]
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    Container.container [ Container.IsFluid ] [
        filter dispatch
        Box.box' [ ]
            [ yield (match model.LoadStudentsState with
                       | Loaded -> div [ ] [
                                      students_level
                                      render_all_students model dispatch
                                   ]
                       | Loading -> Client.Style.loading_view) ] ]
