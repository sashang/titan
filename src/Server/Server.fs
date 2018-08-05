open Giraffe
open Giraffe.Serialization
open Maybe
open Microsoft.Extensions.DependencyInjection
open Saturn
open ServerCode
open System
open System.IO

let publicPath = Path.GetFullPath "../Client/public"
let port = 8085us

let logged_in = pipeline {
    requires_authentication (Giraffe.Auth.challenge "Google")
}

let logged_in_view = router {
    pipe_through logged_in

    get "/" (fun next ctx -> task {
        return! next ctx
    })
}
let default_view = router {
    get "/" (fun next ctx -> task {
        return! next ctx
    })
}
let webApp = router {
    forward "" default_view //Use the default view
    forward "/members-only" logged_in_view
}

let configureSerialization (services:IServiceCollection) =
    let fableJsonSettings = Newtonsoft.Json.JsonSerializerSettings()
    fableJsonSettings.Converters.Add(Fable.JsonConverter())
    services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer fableJsonSettings)

let get_env_var var =
    match Environment.GetEnvironmentVariable(var) with
    | null -> None
    | value -> Some value

let app google_id google_secret = 
    application {
        url ("http://0.0.0.0:" + port.ToString() + "/")
        use_router webApp
        memory_cache
        use_static publicPath
        service_config configureSerialization
        use_gzip
        use_google_oauth google_id google_secret "/oauth_callback_google" []
}

(* Use a maybe computation expression. In the case where one is not defined
it will return None, and pass that None value through to the subsequent
expression. It's basically a nested if then else sequence. See
http://www.zenskg.net/wordpress/?p=187 for an example of how this works in
OCaml. In F# you get more syntactically sugar so you don't have to explicitly
write 'bind' everywhere. For more F# specific implementation details see
https://fsharpforfunandprofit.com/posts/computation-expressions-builder-part1/*)
maybe {
    let! google_id = get_env_var "GOOGLE_ID"
    let! google_secret = get_env_var "GOOGLE_SECRET"
    do run (app google_id google_secret)
} |> ignore
