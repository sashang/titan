module PrivacyPolicy


open Client.Shared
open CustomColours
open Domain
open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Fable.Helpers.React.Props
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Fulma
open Fable.Helpers.React
open ValueDeclarations

type Model =
    { Policy : string }

type Msg =
    | LoadPPSuccess of string
    | Failure of exn

let load_pp () = promise {
    Browser.console.info "load_pp"
    let request = [ RequestProperties.Method HttpMethod.GET ]
    try
        let! response = Fetch.fetch "/docs/privacy-policy.html" request
        return! response.text ()
    with 
        | e -> return failwith (e.Message)
}

let init () =
    Client.Shared.PP.wait_for_dom ()
    {Policy = ""}, Cmd.ofPromise load_pp () LoadPPSuccess Failure

let update (model : Model) (msg : Msg) : Model * Cmd<Msg> =
    match model, msg with 
    | model, LoadPPSuccess pp ->
        Browser.console.info ("loaded pp")
        {model with Policy = pp}, Cmd.none
    | model, Failure e ->
        Browser.console.error ("Failure in PrivacyPolicy: " + e.Message)
        model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    Container.container [ ] [
        div [ HTMLAttr.Id "pp-container" ] [
        ]
    ]