open System.IO

open Microsoft.Extensions.DependencyInjection
open Giraffe
open Saturn

open Giraffe.Serialization

let publicPath = Path.GetFullPath "../Client/public"
let port = 8085us

(*
let getInitCounter () : Task<Counter> = task { return 42 }

let webApp = scope {
    get "/firstTime" (fun next ctx ->
        task {
            let! counter = getInitCounter()
            return! Successful.OK counter next ctx
        })
}
*)

let webApp = scope {
    get "" (fun next ctx -> task {
        return! next ctx
    })
}

let configureSerialization (services:IServiceCollection) =
    let fableJsonSettings = Newtonsoft.Json.JsonSerializerSettings()
    fableJsonSettings.Converters.Add(Fable.JsonConverter())
    services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer fableJsonSettings)

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    router webApp
    memory_cache
    use_static publicPath
    service_config configureSerialization
    use_gzip
}

run app
