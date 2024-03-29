module PrivacyPolicy


open Client.Shared
open CustomColours
open Domain
open Elmish
open Elmish.Navigation
open Fable.React.Props
open Fable.Import
open Fulma
open Fable.React
open Fetch
open ValueDeclarations
open ReactMarkdown

type Model =
    { Policy : string }

type Msg =
    | LoadPPSuccess of string
    | Failure of exn

let load_pp () = promise {
    Browser.Dom.console.info "load_pp"
    let request = [ RequestProperties.Method HttpMethod.GET ]
    try
        let! response = Fetch.fetch "/docs/privacy-policy.md" request
        return! response.text ()
    with 
        | e -> return failwith (e.Message)
}

let init () =
    //Client.Shared.PP.wait_for_dom ()
    {Policy = ""}, Cmd.OfPromise.either load_pp () LoadPPSuccess Failure

let update (model : Model) (msg : Msg) : Model * Cmd<Msg> =
    match model, msg with 
    | model, LoadPPSuccess pp ->
        Browser.Dom.console.info ("loaded pp")
        {model with Policy = pp}, Cmd.none
    | model, Failure e ->
        Browser.Dom.console.error ("Failure in PrivacyPolicy: " + e.Message)
        model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    Container.container [ Container.CustomClass "markdown" ] [
        reactMarkdown [ Source model.Policy ]  []
    ]