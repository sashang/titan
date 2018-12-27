module ModifiedFableFetch

open Fable.Core.JsInterop
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch

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