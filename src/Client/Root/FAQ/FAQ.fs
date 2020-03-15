module FAQ

open Client.Shared
open CustomColours
open Domain
open Elmish
open Fulma
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Fable.Import
open Fetch
open ReactMarkdown
open Thoth.Json
type TF = Thoth.Fetch.Fetch
open ValueDeclarations


type Model =
    { FAQ : string }

type Msg =
    | LoadFAQSuccess of string
    | Failure of exn

let load_pp () = promise {
    Browser.Dom.console.info "load_faq"
    let request = [ RequestProperties.Method HttpMethod.GET ]
    try
        let! response = Fetch.fetch "/docs/faq.md" request
        return! response.text ()
    with 
        | e -> return failwith (e.Message)
}

let init () =
    //Client.Shared.PP.wait_for_dom ()
    {FAQ = ""}, Cmd.OfPromise.either load_pp () LoadFAQSuccess Failure

let update (model : Model) (msg : Msg) : Model * Cmd<Msg> =
    match model, msg with 
    | model, LoadFAQSuccess pp ->
        Browser.Dom.console.info ("loaded FAQ")
        {model with FAQ = pp}, Cmd.none
    | model, Failure e ->
        Browser.Dom.console.error ("Failure in FAQ: " + e.Message)
        model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    Container.container [ Container.CustomClass "markdown" ] [
        reactMarkdown [ Source model.FAQ ]  []
    ]