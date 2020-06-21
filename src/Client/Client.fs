module Client.Main

open Browser
open Elmish
open Elmish.Bridge
open Elmish.Navigation
open Elmish.React
open Pages
open Root


#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

let  [<Literal>]  nav_event = "NavigationEvent"
let  [<Literal>]  hash_change = "hashchange"
let url_sub appState : Cmd<_> = 
    [ fun dispatch -> 
        let on_change _ = 
            match url_parser window.location with 
            | Some parsedPage -> dispatch (Root.UrlUpdatedMsg parsedPage)
            | None -> ()
        
        // listen to manual hash changes or page refresh
        window.addEventListener(hash_change, unbox on_change)
        // listen to custom navigation events published by `Urls.navigate [ . . .  ]`
        window.addEventListener(nav_event, unbox on_change) ]  

let timer dispatch =
    window.setInterval(fun _ -> 
        dispatch Root.TenSecondsTimer
    , 10000) |> ignore

let subscription _ = Cmd.ofSub timer

Program.mkProgram Root.init Root.update Root.view
|> Program.withSubscription url_sub //detect changes typed into the address bar
//|> Program.withSubscription subscription
|> Program.withBridgeConfig (Bridge.endpoint ElmishBridgeModel.endpoint |> Bridge.withMapping Remote)
//|> Program.withBridge ElmishBridgeModel.endpoint
|> Program.toNavigable Pages.url_parser url_update
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
