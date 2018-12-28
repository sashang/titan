module Client.SignOut

open Domain
open Elmish
open Elmish.Browser.Navigation
open Fable.Helpers.React.Props
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Thoth.Json
module R = Fable.Helpers.React

type Msg =
| SignOut 
| Success of SignOutResult
| Failure of exn


let sign_out () = promise {
    let props =
      [ RequestProperties.Method HttpMethod.POST
        RequestProperties.Credentials RequestCredentials.Include ]
    let! response = Fetch.fetch "/api/sign-out" props
    let decoder = Decode.Auto.generateDecoder<Domain.SignOutResult>()
    let! text = response.text ()
    let result = Decode.fromString decoder text
    match result with
    | Ok sign_out_result -> return sign_out_result
    | Error e -> return failwithf "fail: %s" e
}

let update (msg : Msg) : Cmd<Msg> =
    match msg with
    | SignOut -> Cmd.ofPromise sign_out () Success Failure
    | Success result -> Navigation.newUrl (Client.Pages.to_path Client.Pages.Home)
    | Failure ex -> failwith ("Failed to sign out " + ex.Message)

let view dispatch session = 
    R.a [
        Style [
            CSSProp.Padding "0 20px"
            CSSProp.TextDecorationLine "underline"
            CSSProp.FontSize 25
        ]
        Href (Client.Pages.to_path Client.Pages.Home)
        OnClick (fun e -> dispatch SignOut)
    ] [ R.str "Sign Out"]

