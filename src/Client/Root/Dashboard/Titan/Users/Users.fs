module Users

open CustomColours
open Client.Shared
open Client.Style
open Domain
open Elmish
open Fable.Import
open Fable.PowerPack
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core.JsInterop
open Fulma
open ModifiedFableFetch
open Thoth.Json

type Model =
    { Users : UserForTitan list
      Error : APIError option }

type Email = string
type ClaimType = string

type Msg = 
    | ClickRadio of (Email*ClaimType)
    | ClickUpdate of Email
    | UpdateSuccess of unit
    | GetUsersSuccess of UserForTitan list
    | Failure of exn

exception GetUsersForTitanEx of APIError
exception UpdateUserApprovalEx of APIError

let private get_users_for_titan () = promise {
    let request = make_get
    let decoder = Decode.Auto.generateDecoder<UsersForTitanResponse>()
    let! response = Fetch.tryFetchAs "/api/get-users-for-titan" decoder request
    match response with
    | Ok result ->
        match result.Error with
        | Some error ->
            Browser.console.error ("get_users_for_titan: " + (List.head error.Messages))
            return (raise (GetUsersForTitanEx error))
        | None ->
            return result.Users
    | Error e ->
        return raise (GetUsersForTitanEx (APIError.init [APICode.Fetch] [e]))
}

let private update_user_approval (user : UserForTitan)  = promise {
    let request = make_post 1 user
    Browser.console.info ("Updating user claims for" + user.Email)
    let decoder = Decode.Auto.generateDecoder<APIError option>()
    let! response = Fetch.tryFetchAs "/api/update-user-approval" decoder request
    match response with
    | Ok result ->
        match result with
        | Some error ->
            Browser.console.error ("update_user_approval: " + (List.head error.Messages))
            return (raise (UpdateUserApprovalEx error))
        | None ->
            return ()
    | Error e ->
        return raise (UpdateUserApprovalEx (APIError.init [APICode.Fetch] [e]))
}

let init () =
    {Users = []; Error = None}, Cmd.ofPromise get_users_for_titan () GetUsersSuccess Failure


let render_user (user : UserForTitan) (dispatch : Msg->unit) =
    Columns.columns [ ] [
        Column.column [ Column.Width (Screen.All, Column.IsThreeQuarters) ] [
            form [ ] [
                Columns.columns [ ] [
                    Column.column [ ] [
                        yield! input_field_ro "First Name" user.FirstName
                    ]
                    Column.column [ ] [
                        yield! input_field_ro "Last Name" user.LastName
                    ]
                    Column.column [ ] [
                        yield! input_field_ro "Email" user.Email
                    ]
                    Column.column [ ] [
                        radio_buttons "Approved" "yes" "no" user.IsApproved dispatch (ClickRadio (user.Email, "IsApproved"))
                    ]
                    Column.column [ ] [
                        radio_buttons "Tutor" "yes" "no" user.IsTutor dispatch (ClickRadio (user.Email, "IsTutor"))
                    ]
                    Column.column [ ] [
                        radio_buttons "Student" "yes" "no" user.IsStudent dispatch (ClickRadio (user.Email, "IsStudent"))
                    ]
                    Column.column [ ] [
                        radio_buttons "Titan" "yes" "no" user.IsTitan dispatch (ClickRadio (user.Email, "IsTitan"))
                    ]
                ]
            ]
        ]
        Column.column [] [
            Client.Style.button dispatch (ClickUpdate user.Email) "Update" []
        ]
    ]

let users (model : Model) (dispatch : Msg->unit) = 
    Columns.columns [ ] [
        Column.column [ ] [
           for user in model.Users do
                yield render_user user dispatch
        ]
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    Container.container 
        [ Container.IsFluid
          Container.Modifiers [ ] ] [
        users model dispatch
    ]

let update (model : Model) (msg : Msg) =

    match model, msg with
    | model, ClickUpdate email ->
        //update the user with the given email
        let user_to_update = 
            model.Users
            |> List.find (fun user -> user.Email = email)
        model, Cmd.ofPromise update_user_approval user_to_update UpdateSuccess Failure
    
    | model, UpdateSuccess () ->
        Browser.console.info ("Updated user")
        model, Cmd.none

    | model, ClickRadio (email, claim_type) ->
        Browser.console.info ("changed " + claim_type + " for " + email)
        match claim_type with
        | "IsTutor" ->
            let users' = 
                model.Users
                |> List.map (fun (user : UserForTitan) -> 
                                if user.Email = email then {user with IsTutor = not user.IsTutor} else user)
            {model with Users = users'}, Cmd.none
        | "IsStudent" ->
            let users' = 
                model.Users
                |> List.map (fun (user : UserForTitan) -> 
                                if user.Email = email then {user with IsStudent = not user.IsStudent} else user)
            {model with Users = users'}, Cmd.none
        | "IsApproved" ->
            let users' = 
                model.Users
                |> List.map (fun (user : UserForTitan) -> 
                                if user.Email = email then {user with IsApproved = not user.IsApproved} else user)
            {model with Users = users'}, Cmd.none
        | "IsTitan" ->
            let users' = 
                model.Users
                |> List.map (fun (user : UserForTitan) -> 
                                if user.Email = email then {user with IsTitan = not user.IsTitan} else user)
            {model with Users = users'}, Cmd.none
        | _ ->
            Browser.console.error ("Unknown claim type: " + claim_type)
            model, Cmd.none

    | model, GetUsersSuccess users ->
        {model with Users = users}, Cmd.none

    | model, Failure e ->
        match e with
        | :? UpdateUserApprovalEx as ex ->
            Browser.console.warn ("Failed to update user approval status: " + List.head ex.Data0.Messages)
            {model with Error = Some ex.Data0}, Cmd.none
        | :? GetUsersForTitanEx as ex ->
            Browser.console.warn ("Failed to get users for titan: " + List.head ex.Data0.Messages)
            {model with Error = Some ex.Data0}, Cmd.none
        | e ->
            Browser.console.warn ("Failed to get users for titan" + e.Message)
            model, Cmd.none