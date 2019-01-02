module Dashboard

open Domain
open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Elmish.React
open Fable.Helpers.React.Props
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.PowerPack
open Elmish.Browser.Navigation
open Fulma
open Fulma.Extensions
open Fable.Helpers.React

type PageType =

    /// Main dashboard page
    | Main
    /// Page to create a school
    | CreateSchool

type PageModel =
    | Home
    | CreateSchoolModel of CreateSchool.Model

and
    Model =
        { Child : PageModel }

type Msg =
    | ClickCreateSchool
    | ClickEnroll
    | CreateSchoolMsg of CreateSchool.Msg

let init =
    {Child = Home}
// Helper to generate a menu item
let menuItem label isActive dispatch msg =
    Menu.Item.li [ Menu.Item.IsActive isActive
                   Menu.Item.OnClick (fun e -> dispatch msg)]
       [ Text.p [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left)
                              Modifier.TextSize (Screen.All, TextSize.Is4)
                              Modifier.TextColor Color.IsGreyDark ] ]
           [ str label ] ]
// Helper to generate a sub menu
let subMenu label isActive children =
    li [ ]
       [ Menu.Item.a [ Menu.Item.IsActive isActive ]
            [ str label ]
         ul [ ]
            children ]

let menu dispatch = 
    Menu.menu [ ]
        [ Menu.list [ ]
            [ menuItem "School" false dispatch ClickCreateSchool
              menuItem "Classes" false dispatch ClickCreateSchool
              menuItem "Enrollments" false dispatch ClickEnroll ] ] 

let main_dashboard (model : Model) (dispatch : Msg -> unit) =
    Tile.ancestor []
        [ Tile.parent [ ]
            [ Tile.child [ ]
                [ Box.box' [ Common.Props [ Style [ Height "100%" ] ] ]
                    [ Heading.p [ ] [ str "Live" ] ] ] ] ]

// Menu rendering
let view (model : Model) (dispatch : Msg -> unit) =
     Container.container
        //fluid to take up the width of the screen
        [ Container.IsFluid ]
        [ Columns.columns [ ] 
            [ Column.column [ Column.Width (Screen.All, Column.Is2) ] [ aside [ Class "menu" ] [ menu dispatch ] ]
              Column.column [ ] 
                [ (match model.Child with
                  | Home -> main_dashboard model dispatch
                  | CreateSchoolModel model -> CreateSchool.view model (CreateSchoolMsg >> dispatch)) ] ] ]

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match msg,model with
    | ClickCreateSchool, _ ->
        let initial_state, cmd = CreateSchool.init ()
        {model with Child = CreateSchoolModel initial_state}, Cmd.map CreateSchoolMsg cmd
    | CreateSchoolMsg msg, {Child = CreateSchoolModel cs_model}  ->
        let new_state, cmd = CreateSchool.update cs_model msg
        {model with Child = CreateSchoolModel new_state}, Cmd.map CreateSchoolMsg cmd
    | _, {Child = Home}  ->
        model, Cmd.none
