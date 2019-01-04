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


type PageModel =
    | MainModel
    | SchoolModel of School.Model

and
    Model =
        { Child : PageModel }

type Msg =
    | ClickSchool
    | ClickEnroll
    | SchoolMsg of School.Msg

let init () =
    {Child = MainModel}
// Helper to generate a menu item
let menuItem label isActive dispatch msg =
    Menu.Item.li [ Menu.Item.IsActive isActive
                   Menu.Item.OnClick (fun e -> dispatch msg)]
       [ Text.p [ Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left)
                              Modifier.TextSize (Screen.All, TextSize.Is5)
                              Modifier.TextColor Color.IsWhite
                              Modifier.TextTransform TextTransform.UpperCase ] ]
           [ str label ] ]
// Helper to generate a sub menu
let subMenu label isActive children =
    li [ ]
       [ Menu.Item.a [ Menu.Item.IsActive isActive ]
            [ str label ]
         ul [ ]
            children ]

let menu dispatch = 
    Box.box' [ Common.Props [ Style [ Height "100%" ] ] ]
         [ Menu.menu [ Modifiers [ Modifier.BackgroundColor IsPrimary ] ]
            [ Menu.list [ ]
                [ menuItem "School" false dispatch ClickSchool
                  menuItem "Classes" false dispatch ClickSchool
                  menuItem "Enrollment" false dispatch ClickEnroll ] ] ]

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
                  | MainModel -> main_dashboard model dispatch
                  | SchoolModel model -> School.view model (SchoolMsg >> dispatch)) ] ] ]

let update (model : Model) (msg : Msg) : Model*Cmd<Msg> =
    match msg,model with
    | ClickSchool, _ ->
        model, Navigation.newUrl (Pages.to_path (Pages.Dashboard Pages.DashboardPageType.School))
    | SchoolMsg msg, {Child = SchoolModel cs_model}  ->
        let new_state, cmd = School.update cs_model msg
        {model with Child = SchoolModel new_state}, Cmd.map SchoolMsg cmd
    | _, {Child = MainModel}  ->
        model, Cmd.none

let url_update  (page : Pages.DashboardPageType) : Model*Cmd<Msg> =
    match page with
    | Pages.DashboardPageType.School ->
        let new_state, cmd = School.init ()
        {Child = SchoolModel new_state}, Cmd.map SchoolMsg cmd

    | Pages.DashboardPageType.Main ->
        {Child = MainModel}, Cmd.none