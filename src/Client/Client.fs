module Client.Main

open Client.Shared
open Domain
open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Elmish.React
open Fable.Import
open Fable.Import.Browser
open Pages
open Root

let handleNotFound (model: SinglePageState) =
    Browser.console.error("Error parsing url: " + Browser.window.location.href)
    ( model, Navigation.modifyUrl (Pages.to_path Pages.PageType.Login) )

let print_claims (claims : TitanClaim list) =
    claims |>
    List.map (fun x -> "type = " + x.Type + " value = " + x.Value) |>
    List.iter (fun x -> Browser.console.info x)
    
#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

let  [<Literal>]  nav_event = "NavigationEvent"
let url_sub appState : Cmd<_> = 
    [ fun dispatch -> 
        let on_change _ = 
            match url_parser window.location with 
            | Some parsedPage -> dispatch (Root.UrlUpdatedMsg parsedPage)
            | None -> ()
        
        // listen to manual hash changes or page refresh
        window.addEventListener_hashchange(unbox on_change)
        // listen to custom navigation events published by `Urls.navigate [ . . .  ]`
        window.addEventListener(nav_event, unbox on_change) ]  

Program.mkProgram Root.init Root.update Root.view
|> Program.withSubscription url_sub //detect changes typed into the address bar
|> Program.toNavigable Pages.url_parser url_update
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
