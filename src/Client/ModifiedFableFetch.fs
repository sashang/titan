module ModifiedFableFetch

open Domain
open Fable.Core.JsInterop
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Thoth.Json


type FetchResult =
    | Success of Response
    | BadStatus of Response
    | NetworkError

let fetch (url: string) (init: RequestProperties list) : JS.Promise<FetchResult> =
    GlobalFetch.fetch (RequestInfo.Url url, requestProps init)
    |> Promise.map (fun response ->
        if response.Ok then
            Success response
        else
            if response.Status < 200 || response.Status >= 300 then
                BadStatus response
            else
                NetworkError
    )

let post_record (url: string) (body: string) (properties: RequestProperties list) =
    let defaultProps =
      [ RequestProperties.Method HttpMethod.POST
        RequestProperties.Credentials RequestCredentials.Include
        requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                         HttpRequestHeaders.Accept "application/json" ]
        RequestProperties.Body !^(body) ]

    List.append defaultProps properties
    |> fetch url
    |> Promise.bind(fun result ->
        promise {
            match result with
            | Success response -> return response
            | BadStatus response ->
                let! body_text = response.text ()
                return failwith ("eek! " + body_text + string response.Status + " " + response.StatusText + " for URL " + response.Url)
            | NetworkError ->
                return failwith "network error"
        }
    )

let unwrap_response (response : Result<APIError,string>) ex_to_raise = 
    match response with
    | Ok result ->
        match result.Codes with
        | [] -> ()
        | APICode.Success::_  -> ()
        | _ ->
            raise (ex_to_raise result)
    | Error e ->
        Browser.console.warn ("Error: " + e)
        raise (ex_to_raise (APIError.init [APICode.Fetch] [e]))


let make_request (count : int) (data : 'a) =
    let body = Encode.Auto.toString (count, data)
    let props =
        [ RequestProperties.Method HttpMethod.POST
          RequestProperties.Credentials RequestCredentials.Include
          requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                           HttpRequestHeaders.Accept "application/json"]
          RequestProperties.Body !^(body) ] 
    props