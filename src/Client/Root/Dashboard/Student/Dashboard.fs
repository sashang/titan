module Student.Dashboard

open CustomColours
open Domain
open Elmish
open Fable.Import
open Fable.PowerPack
open Fable.Helpers.React
open Fulma
open ModifiedFableFetch
open Thoth.Json



type Model =
    { FirstName : string
      LastName : string
      Phone : string
      Email : string
      Result : GetAllSchoolsResult}

type Msg =
    | ClickEnrol of string
    | Success of GetAllSchoolsResult
    | Failure of exn

exception GetAllSchoolsEx of APIError

let private get_all_schools () = promise {
    let request = make_get
    let decoder = Decode.Auto.generateDecoder<GetAllSchoolsResult>()
    let! response = Fetch.tryFetchAs "/api/get-all-schools" decoder request
    Browser.console.info "received response from register-student"
    match response with
    | Ok result ->
        match result.Error with
        | Some api_error -> return raise (GetAllSchoolsEx api_error)
        | None ->  return result
    | Error e ->
        return raise (GetAllSchoolsEx (APIError.init [APICode.Fetch] [e]))
}

let init () = {FirstName = ""; LastName = ""; Phone = "";
               Email =""; Result = GetAllSchoolsResult.init}, Cmd.ofPromise get_all_schools () Success Failure

let update model msg =
    match model, msg with
    | model, Success result ->
        {model with Result = result}, Cmd.none
    | model, Failure e ->
        match e with
        | :? GetAllSchoolsEx as ex ->
            List.iter (fun m -> Browser.console.warn ("GetAllSchoolsEx:" + m)) ex.Data0.Messages
            {model with Result = {model.Result with Error = Some ex.Data0}}, Cmd.none
        | e ->
            Browser.console.warn ("Failed to get_all_schools: " + e.Message)
            model, Cmd.none

let single_row (school : School) (dispatch : Msg -> unit) =
    Box.box'[] [
        Card.card [] [
            Card.header [ Modifiers [ Modifier.BackgroundColor IsTitanSecondary
                                      Modifier.TextColor IsWhite] ] [
                str school.SchoolName
            ]
            Card.content [  ] [
                Columns.columns [ ] [
                    Column.column [ Column.Width (Screen.All, Column.Is2) ] [
                       Heading.h3 [ Heading.IsSubtitle ] [ str "Tutor" ]
                    ]
                    Column.column [ Column.Width (Screen.All, Column.Is2) ] [
                       Heading.h4 [ ] [ str school.FirstName; str " "; str school.LastName]
                    ]
                ]
            ]
            Card.footer [ ] [
                Button.button [ Button.OnClick (fun ev -> dispatch (ClickEnrol school.SchoolName)) ] [
                    str "Enrol"
                ]
            ]
        ]
    ]
    
    
let view (model : Model) (dispatch : Msg -> unit) =
     let schools = model.Result.Schools
     Container.container [ Container.IsFluid ] [
        yield! [ for s in schools do yield single_row s dispatch ]
     ]
    
