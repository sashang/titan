module SignOut

open Domain
open Elmish
open Elmish.Navigation
open Fetch
open Thoth.Json

type Msg =
| SignOut 
| SignOutSuccess of SignOutResult
| Failure of exn

type SignOutEx(msg : string) =
    inherit System.Exception(msg)

let sign_out () = promise {
    let props =
      [ RequestProperties.Method HttpMethod.GET
        RequestProperties.Credentials RequestCredentials.Include
        requestHeaders [ ] ]
    let! response = Fetch.fetch "/sign-out/sign-out" props
    let decoder = Decode.Auto.generateDecoder<SignOutResult>()
    let! text = response.text ()
    let result = Decode.fromString decoder text
    match result with
    | Ok sign_out_result ->
        match sign_out_result.Code with
        | SignOutCode.Success ->
            Browser.Dom.console.info sign_out_result.Message
            return sign_out_result
        | _ -> return raise (SignOutEx "Failed to logout")
    | Error e -> return failwithf "fail: %s" e
}

let update (msg : Msg) : Cmd<Msg> =
    match msg with
    | SignOut ->
        Browser.Dom.console.info "SignOut.update received SignOut"
        Cmd.OfPromise.either sign_out () SignOutSuccess Failure
    | SignOutSuccess result -> 
        Browser.Dom.console.info "SignOut.update received SignOutSuccess"
        Navigation.newUrl (Pages.to_path Pages.Home)
    | Failure ex -> failwith ("Failed to sign out " + ex.Message)


