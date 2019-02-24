//terms and conditions page
module Terms

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
open ReactMarkdown

type Model =
    { Terms : string }

type Msg =
    | LoadTerms of string
    | Failure of exn

let load_pp () = promise {
    Browser.console.info "load_terms"
    let request = [ RequestProperties.Method HttpMethod.GET ]
    try
        let! response = Fetch.fetch "/docs/terms.md" request
        return! response.text ()
    with 
        | e -> return failwith (e.Message)
}

let init () =
    //Client.Shared.PP.wait_for_dom ()
    {Terms = ""}, Cmd.ofPromise load_pp () LoadTerms Failure

let update (model : Model) (msg : Msg) : Model * Cmd<Msg> =
    match model, msg with 
    | model, LoadTerms pp ->
        Browser.console.info ("loaded pp")
        {model with Terms = pp}, Cmd.none
    | model, Failure e ->
        Browser.console.error ("Failure in Terms: " + e.Message)
        model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    Container.container [ Container.CustomClass "markdown" ] [
        reactMarkdown [ Source model.Terms ]  []
    ]