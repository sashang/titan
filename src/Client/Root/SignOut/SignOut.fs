module SignOut

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
        match sign_out_result.code with
        | SignOutCode.Success :: _ ->
            Fable.Import.Browser.console.info "Successfully called /api/sign-out"
            return sign_out_result
        | _ -> return raise (SignOutEx "Failed to logout")
    | Error e -> return failwithf "fail: %s" e
}

let update (msg : Msg) : Cmd<Msg> =
    match msg with
    | SignOut ->
        Fable.Import.Browser.console.info "SignOut.update received SignOut"
        Cmd.ofPromise sign_out () SignOutSuccess Failure
    | SignOutSuccess result -> 
        Fable.Import.Browser.console.info "SignOut.update received SignOutSuccess"
        Navigation.newUrl (Pages.to_path Pages.Home)
    | Failure ex -> failwith ("Failed to sign out " + ex.Message)


