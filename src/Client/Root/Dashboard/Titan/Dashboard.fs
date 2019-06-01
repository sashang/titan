module Titan.Dashboard

open Client.Shared
open Domain
open Elmish
open Fable.Import
open Fable.Helpers.React
open Fulma
open Thoth.Json

type PageModel =
    | UsersModel of Users.Model

type Model =
    { Child : PageModel
      Claims : TitanClaim }

type Msg =
    | ClickApprovedUsers
    | ClickUnapprovedUsers
    | SignOut
    | UsersMsg of Users.Msg


let init (claims : TitanClaim) : Model*Cmd<Msg> =
    let (users_model, users_cmd) = Users.init Users.Unapproved
    { Child = UsersModel users_model; Claims = claims},
    Cmd.map UsersMsg users_cmd

// Helper to generate a menu item
let menu_item label isActive dispatch msg =
    Menu.Item.li [ Menu.Item.IsActive isActive
                   Menu.Item.OnClick (fun e -> dispatch msg)
                   Menu.Item.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
       [ str label ]

// Helper to generate a sub menu
let subMenu label isActive children =
    li [ ]
       [ Menu.Item.a [ Menu.Item.IsActive isActive ]
            [ str label ]
         ul [ ]
            children ]


// Menu rendering
let view (model : Model) (dispatch : Msg -> unit) =
     Container.container
        //fluid to take up the width of the screen
        [ Container.IsFluid 
          Container.Modifiers [ Modifier.IsMarginless ]  ] [
            Columns.columns [ ] [
                Column.column [ Column.Width (Screen.All, Column.Is1) ] [
                    Menu.menu [ ] [
                        Menu.list [ ] [
                            menu_item "Unapproved Users" false dispatch ClickUnapprovedUsers
                            menu_item "Approved Users" false dispatch ClickApprovedUsers
                        ]
                    ]
                ]
                Column.column [ ] [
                   (match model.Child with
                    | UsersModel users_model ->
                        Users.view users_model (UsersMsg >> dispatch) )
                ]
            ]
        ]
            

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match model, msg with
    | {Child = UsersModel users_model}, UsersMsg msg ->
        let new_model, new_cmd = Users.update users_model msg
        {model with Child = UsersModel new_model}, Cmd.map UsersMsg new_cmd

    | {Child = UsersModel users_model}, ClickApprovedUsers ->
        Browser.console.info ("ClickApprovedUsers message")
        let new_model, new_cmd = Users.update users_model Users.InitApproved
        {model with Child = UsersModel new_model}, Cmd.map UsersMsg new_cmd

    | {Child = UsersModel users_model}, ClickUnapprovedUsers ->
        Browser.console.info ("ClickUnapprovedUsers message")
        let new_model, new_cmd = Users.update users_model Users.InitUnapproved
        {model with Child = UsersModel new_model}, Cmd.map UsersMsg new_cmd

    | _, SignOut ->
        //let the root handle the signout request.
        model, Cmd.none