module ModifiedFableFetch

open Domain
open Fable.Core
open Fable.Core.JsInterop
open Fetch
open Fable.Import
open Thoth.Json


type FetchResult =
    | Success of Response
    | BadStatus of Response
    | NetworkError

let fetch (url: string) (init: RequestProperties list) :  JS.Promise<FetchResult> =
    GlobalFetch.fetch (RequestInfo.Url url, requestProps init)
    |> Promise.map (fun (response : Response) ->
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

/// Map the APIError result if it is there to the exeption.
/// Map the error (i.e. the string part in Result) to the exception
let map_api_error_result (response : Result<APIError option,string>) ex_to_raise  = 
    match response with
    | Ok result ->
        match result with
        | Some api_error -> raise (ex_to_raise api_error)
        | None -> ()
    | Error e ->
        raise (ex_to_raise (APIError.init [APICode.Fetch] [e]))

let inline make_post (count : int) (data : 'a) =
    let body = Encode.Auto.toString (count, data)
    let props =
        [ RequestProperties.Method HttpMethod.POST
          RequestProperties.Credentials RequestCredentials.Include
          requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                           HttpRequestHeaders.Accept "application/json"]
          RequestProperties.Body !^(body) ] 
    props

let make_get =
    let props =
        [ RequestProperties.Method HttpMethod.GET
          RequestProperties.Credentials RequestCredentials.Include
          requestHeaders [ HttpRequestHeaders.ContentType "application/json"
                           HttpRequestHeaders.Accept "application/json"] ] 
    props
    
    
