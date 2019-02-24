module FAQ

open Client.Shared
open CustomColours
open Domain
open Elmish
open Elmish.Browser.Navigation
open Fulma
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch
open ReactMarkdown
open Thoth.Json
open ValueDeclarations


type Model =
    { FAQ : string }

type Msg =
    | LoadFAQSuccess of string
    | Failure of exn

let load_pp () = promise {
    Browser.console.info "load_faq"
    let request = [ RequestProperties.Method HttpMethod.GET ]
    try
        let! response = Fetch.fetch "/docs/faq.md" request
        return! response.text ()
    with 
        | e -> return failwith (e.Message)
}

let init () =
    //Client.Shared.PP.wait_for_dom ()
    {FAQ = ""}, Cmd.ofPromise load_pp () LoadFAQSuccess Failure

let update (model : Model) (msg : Msg) : Model * Cmd<Msg> =
    match model, msg with 
    | model, LoadFAQSuccess pp ->
        Browser.console.info ("loaded pp")
        {model with FAQ = pp}, Cmd.none
    | model, Failure e ->
        Browser.console.error ("Failure in FAQ: " + e.Message)
        model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    Container.container [ Container.CustomClass "markdown" ] [
        reactMarkdown [ Source model.FAQ ]  []
    ]